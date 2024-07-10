/*
* MIT License
* 
* Copyright (C) 2024 SoftArc, LLC
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using DevExpress.Xpo;
using Foundation;
using RecipeBoxSolutionModel;
using System.Collections.Generic;
using System.Text;

namespace eRecipeBox.ImportUtils
{
    public partial class TextRecipeCardImporter
    {
        #region Load parsed text into the RecipeCard model
        private int IndexOf(string[] lines, string search, bool startswith = true, bool contains = false, bool endswith = false)
        {
            string[] searches = new string[1];
            searches[0] = search;

            return IndexOf(lines, searches, startswith, contains, endswith);
        }
        private int IndexOf(string[] lines, string[] searches, bool startswith = true, bool contains = false, bool endswith = false)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < searches.Length; j++)
                {
                    string search = searches[j].ToLower().Trim();

                    if (startswith && lines[i].ToLower().ToLower().StartsWith(search))
                        return i;
                    if (contains && lines[i].ToLower().ToLower().Contains(search))
                        return i;
                    if (endswith && lines[i].ToLower().ToLower().EndsWith(search))
                        return i;
                }
            }
            return -1;
        }
        private string FindField(string[] lines, string fieldName)
        {
            int idx = IndexOf(lines, fieldName + ':');
            if (idx < 0)
                return null;
            else
                return lines[idx].Trim();
        }
        private string ParseIngredient(string line)
        {
            UnitOfWork uow = new UnitOfWork();
            Ingredient ingr = Ingredient.ParseIngredient(line, uow);
            if (ingr == null)
                return null;

            StringBuilder result = new StringBuilder();
            result.Append(PrimitiveUtils.ObjectToString(ingr.Qty)); result.Append(FieldDelimiter);
            result.Append(PrimitiveUtils.ObjectToString(ingr.UoM)); result.Append(FieldDelimiter);
            result.Append(PrimitiveUtils.ObjectToString(ingr.ItemDescription));
            //result.Append(PrimitiveUtils.ObjectToString(ingr.Item));
            return result.ToString();

        }
        private string FindIngredients(string[] lines)
        {
            string[] ingreds = new string[] { "ingredients" };
            List<string> result = new List<string>();
            result.Add("Ingredients:");
            int idx = IndexOf(lines, ingreds);
            if (idx < 0)
                return result.ToString();
            for (int i = idx + 1; i < lines.Length; i++)
            {
                if (lines[i] == null || lines[i].Length == 0)
                    break;
                result.Add(ParseIngredient(lines[i]));
            }
            return string.Join(PrimitiveUtils.newl, result);

        }
        private string FindKeywords(string[] lines)
        {
            //#TODO ENHANCEMENT Import keywords
            return "Keywords:";
        }
        private string FindTextblock(string[] lines, string hdr, string endOfBlockMarker = null)
        {
            string hdr1 = hdr + PrimitiveUtils.newl;
            string[] desc = new string[] { hdr };
            int idx = IndexOf(lines, desc);
            if (idx < 0)
                return hdr1;

            List<string> result = new List<string>();
            for (int i = idx + 1; i < lines.Length; i++)
            {
                if (lines[i] == null || lines[i].Length == 0 || (endOfBlockMarker != null && lines[i].Trim() == endOfBlockMarker))
                    break;
                result.Add(lines[i].Trim());
            }
            return hdr1 + string.Join(" ", result) + PrimitiveUtils.newl;
        }
        private string FindInstructions(string[] lines)
        {
            string endOfInstructionsMarker = "Notes:";
            string[] instrs = new string[] { "instructions", "steps", "preparation", "directions" };
            List<string> result = new List<string>();
            result.Add("Instructions:");
            int idx = IndexOf(lines, instrs);
            if (idx < 0)
                return result.ToString();
            for (int i = idx + 1; i < lines.Length; i++)
            {
                if (lines[i].Trim() == endOfInstructionsMarker)
                    break;
                result.Add(lines[i]);
            }
            return string.Join(PrimitiveUtils.newl, result);

        }

        #endregion Parse text into RecipeCard
    }
}
