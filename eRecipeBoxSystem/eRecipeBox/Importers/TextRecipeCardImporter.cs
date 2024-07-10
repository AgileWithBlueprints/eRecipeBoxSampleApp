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
using SimpleCRUDFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace eRecipeBox.ImportUtils
{
    public partial class TextRecipeCardImporter
    {
        static public readonly char FieldDelimiter = '\t';

        //Importing a RecipeCard is performed in three steps:
        //1. Create a sequence of lines containing RecipeCard property markers (eg "Title:"), followed by their property values.
        //2. Parse the individual parts of each Ingredients into its properties.  Use '`' as a delimiter.
        //3. Import the parsed text lines into a RecipeCard by setting RecipeCard's properties with the text.

        /// <summary>
        /// Take as input a sequence of lines containing RecipeCard property markers (eg "Title:") and parse the individual parts of each Ingredients into its properties.
        /// </summary>
        /// <param name="lines">Text with property markers</param>
        /// <returns>Parsed set of lines, ready to import.</returns>
        public IList<string> ParseTextAndAddFieldDelimiters(string[] lines)
        {
            List<string> parts = new List<string>
            {
                FindField(lines, "Title")
            };
            string x = this.FindTextblock(lines, "Description:", EndOfDescriptionMarker);
            parts.Add(x);
            parts.Add(EndOfDescriptionMarker);
            parts.Add(FindField(lines, "URL"));
            parts.Add(FindField(lines, "PrepTime"));
            parts.Add(FindField(lines, "CookTime"));
            parts.Add(FindField(lines, "TotalTime"));
            parts.Add(FindField(lines, "Servings"));
            parts.Add(FindField(lines, "Rating"));
            parts.Add(FindField(lines, "RatingCount"));
            parts.Add(FindField(lines, "MyRating"));
            parts.Add(FindIngredients(lines));
            parts.Add(PrimitiveUtils.InsertLineBreakEachSentence(FindInstructions(lines)));
            x = this.FindTextblock(lines, "Notes:");
            parts.Add(x);
            parts.Add(FindKeywords(lines));
            return parts;
        }

        static public readonly string EndOfDescriptionMarker = @"<end of description>";
        public IList<string> UnparseRecipeCard(RecipeCard recipe)
        {
            List<string> result = new List<string>();

            result.Add("Title: " + PrimitiveUtils.ObjectToString(recipe.Title));
            result.Add("Description:");
            result.Add(PrimitiveUtils.ObjectToString(recipe.Description));
            result.Add("");
            result.Add(EndOfDescriptionMarker);
            result.Add("URL: " + PrimitiveUtils.ObjectToString(recipe.SourceURL));
            result.Add("PrepTime: " + PrimitiveUtils.ObjectToString(recipe.PrepTime));
            result.Add("CookTime: " + PrimitiveUtils.ObjectToString(recipe.CookTime));
            result.Add("TotalTime: " + PrimitiveUtils.ObjectToString(recipe.TotalTime));
            result.Add("Servings: " + PrimitiveUtils.ObjectToString(recipe.Yield));
            result.Add("Rating: " + PrimitiveUtils.ObjectToString(recipe.RatingRounded));
            result.Add("RatingCount: " + PrimitiveUtils.ObjectToString(recipe.RatingCount));
            result.Add("MyRating: " + PrimitiveUtils.ObjectToString(recipe.MyRating));

            StringBuilder line = new StringBuilder();
            result.Add("Ingredients:");
            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                line.Clear();
                line.Append(PrimitiveUtils.ObjectToString(ingredient.Qty));
                line.Append(FieldDelimiter);
                line.Append(PrimitiveUtils.ObjectToString(ingredient.UoM));
                line.Append(FieldDelimiter);
                line.Append(PrimitiveUtils.ObjectToString(ingredient.ItemDescription));
                result.Add(line.ToString());
            }
            result.Add("");
            result.Add("Instructions:");
            result.Add(PrimitiveUtils.ObjectToString(recipe.Instructions));
            result.Add("");

            result.Add("Notes:");
            result.Add(PrimitiveUtils.ObjectToString(recipe.Notes));
            result.Add("");

            result.Add("Keywords:");
            foreach (Keyword keyw in recipe.Keywords)
            {
                result.Add(PrimitiveUtils.ObjectToString(keyw.Name));
            }
            return result;

        }

        //#TODO REFACTOR This was done quick and dirty
        public bool LoadParsedTextIntoRecipeCard(string[] lines, RecipeCard targetRecipeCard, MDIChildForm parentForm, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                List<string> propertyValueInput = new List<string>();
                if (!lines[0].ToLower().StartsWith("title"))
                {
                    errorMessage = "1st line must start with Title:";
                    return false;
                }
                targetRecipeCard.Title = lines[0].Replace("Title:", "").Trim();

                if (!lines[1].ToLower().StartsWith("description"))
                {
                    errorMessage = "2nd line must start with Description:";
                    return false;
                }
                int idx = 2;
                for (int i = idx; i < lines.Length; i++)
                {
                    if (lines[idx].ToLower().StartsWith(EndOfDescriptionMarker))
                        break;

                    if (lines[idx].ToLower().StartsWith("URL:"))
                    {
                        errorMessage = EndOfDescriptionMarker + " is missing.";
                        return false;
                    }
                    propertyValueInput.Add(lines[i]);
                    idx++;
                }

                if (idx >= lines.Length || !lines[idx].ToLower().StartsWith(EndOfDescriptionMarker))
                {
                    errorMessage = EndOfDescriptionMarker + " is missing.";
                    return false;
                }

                if (propertyValueInput.Count == 0)
                    targetRecipeCard.Description = null;
                else
                    targetRecipeCard.Description = string.Join(PrimitiveUtils.newl, propertyValueInput);

                propertyValueInput.Clear();

                idx++;

                if (!lines[idx].StartsWith("URL:"))
                {
                    errorMessage = "URL:  must follow Description";
                    return false;
                }

                string val = lines[idx++].Replace("URL:", "").Trim();
                targetRecipeCard.SourceURL = PrimitiveUtils.Clean(val);

                if (!lines[idx].StartsWith("PrepTime:"))
                {
                    errorMessage = "PrepTime:  must follow URL:";
                    return false;
                }

                val = lines[idx++].Replace("PrepTime:", "").Trim();
                targetRecipeCard.PrepTime = PrimitiveUtils.TextEditToInt32(val);

                if (!lines[idx].StartsWith("CookTime:"))
                {
                    errorMessage = "CookTime:  must follow PrepTime:";
                    return false;
                }
                val = lines[idx++].Replace("CookTime:", "").Trim();
                targetRecipeCard.CookTime = PrimitiveUtils.TextEditToInt32(val);

                if (!lines[idx].StartsWith("TotalTime:"))
                {
                    errorMessage = "TotalTime:  must follow CookTime:";
                    return false;
                }
                val = lines[idx++].Replace("TotalTime:", "").Trim();
                targetRecipeCard.TotalTime = PrimitiveUtils.TextEditToInt32(val);

                if (!lines[idx].StartsWith("Servings:"))
                {
                    errorMessage = "missing Servings:";
                    return false;
                }
                val = lines[idx++].Replace("Servings:", "").Trim();
                targetRecipeCard.Yield = PrimitiveUtils.Clean(val);

                if (!lines[idx].StartsWith("Rating:"))
                {
                    errorMessage = "Missing Rating:";
                    return false;
                }
                val = lines[idx++].Replace("Rating:", "").Trim();
                targetRecipeCard.Rating = PrimitiveUtils.TextEditToDecimal(val);

                if (!lines[idx].StartsWith("RatingCount:"))
                {
                    errorMessage = "Missing RatingCount:";
                    return false;
                }
                val = lines[idx++].Replace("RatingCount:", "").Trim();
                targetRecipeCard.RatingCount = PrimitiveUtils.TextEditToInt32(val);

                if (!lines[idx].StartsWith("MyRating:"))
                {
                    errorMessage = "Missing MyRating:";
                    return false;
                }
                val = lines[idx++].Replace("MyRating:", "").Trim();
                targetRecipeCard.MyRating = PrimitiveUtils.TextEditToInt32(val);

                if (!lines[idx].StartsWith("Ingredients:"))
                {
                    errorMessage = "missing Ingredients:";
                    return false;
                }

                idx++;

                string[] cells;
                int sortOrder = 0;
                for (int i = idx; i < lines.Length; i++)
                {
                    if (lines[idx].StartsWith("Instructions:"))
                        break;

                    cells = lines[i].Split(FieldDelimiter);
                    if (cells.Length != 3)
                    {
                        errorMessage = "Each ingredient needs - Qty UoM ItemDescription - " + lines[i];
                        return false;
                    }

                    Ingredient newr = new Ingredient(targetRecipeCard.Session)
                    {
                        Qty = cells[0],
                        UoM = cells[1],
                        ItemDescription = cells[2],
                        SortOrder = sortOrder
                    };
                    sortOrder++;

                    targetRecipeCard.AddIngredient(newr);
                    idx++;
                }
                if (idx == lines.Length - 1)
                {
                    errorMessage = "Missing Instructions:";
                    return false;

                }

                int instructions_idx = idx;
                idx++;
                propertyValueInput.Clear();
                for (int i = idx; i < lines.Length; i++)
                {
                    if (lines[idx].StartsWith("Notes:"))
                        break;

                    propertyValueInput.Add(lines[i]);
                    idx++;
                }
                if (idx == lines.Length - 1)
                {
                    errorMessage = "Missing Notes";
                    return false;

                }
                else if (propertyValueInput.Count() == 0)
                    targetRecipeCard.Instructions = null;
                else
                    targetRecipeCard.Instructions = string.Join(PrimitiveUtils.newl, propertyValueInput);

                int notes_idx = idx;
                idx++;
                propertyValueInput.Clear();
                for (int i = idx; i < lines.Length; i++)
                {
                    if (lines[idx].StartsWith("Keywords:"))
                        break;

                    propertyValueInput.Add(lines[i]);
                    idx++;
                }
                if (idx >= lines.Length || !lines[idx].StartsWith("Keywords:"))
                {
                    errorMessage = "Missing Keywords:";
                    return false;

                }
                else if (propertyValueInput.Count() == 0)
                    targetRecipeCard.Notes = null;
                else
                    targetRecipeCard.Notes = string.Join(PrimitiveUtils.newl, propertyValueInput);
                idx++;

                //
                XPCollection<Keyword> allKeywords = new XPCollection<Keyword>(targetRecipeCard.Session);
                for (int i = idx; i < lines.Length; i++)
                {
                    cells = lines[i].Trim().Split(FieldDelimiter);
                    if (cells.Length != 1)
                    {
                        errorMessage = "Each Keyword should be on its own line - " + lines[i];
                        return false;
                    }

                    Keyword keyword = Keyword.FindKeywordXP(allKeywords, cells[0]);
                    if (keyword == null)
                    {
                        if (
                    parentForm.ShowMessageBox(parentForm, "Add the '" + cells[0].ToString() + "' entry to the list?  ",
                        "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Keyword newk = new Keyword(targetRecipeCard.Session)
                            {
                                Name = cells[0].ToString().Trim()
                            };
                            targetRecipeCard.Keywords.Add(newk);
                        }
                    }
                    else
                        targetRecipeCard.Keywords.Add(keyword);

                    idx++;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Each Keyword should be on its own line - " + ex.Message;
                return false;
            }
            return true;
        }
    }
}
