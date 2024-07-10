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
using Newtonsoft.Json.Linq;
using RecipeBoxSolutionModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eRecipeBox.ImportUtils
{
    internal partial class GPTRecipeCardImporter
    {

        /// <summary>
        /// Prompt GPT for a RecipeCard
        /// </summary>
        /// <param name="userRecipePrompt">GPT prompt for a RC</param>
        /// <param name="recipeCard">Returned recipe is loaded into this RC</param>
        /// <param name="allItems">List of Items to use for Ingredient.Item suggestion</param>
        /// <returns>null if no error, otherwise an error message</returns>
        public static string PromptGPTandLoadRecipeCard(string userRecipePrompt, RecipeCard recipeCard, XPCollection<Item> allItems)
        {
            try
            {
                string errorMessage = null;
                string recipeJSON = Task.Run(() => GPTRecipeCardImporter.GetRecipeAsync(userRecipePrompt)).Result;

                //Content is returned as JSON.  use it to populate the RC
                JToken json = JArray.Parse(FixOpenAIBadJSON(recipeJSON))[0];

                recipeCard.Title = json["title"]?.Value<string>();
                recipeCard.Description = json["description"]?.Value<string>();
                recipeCard.Yield = json["yield"]?.Value<string>();
                recipeCard.TotalTime = json["totalCookTime"]?.Value<int>();
                recipeCard.Rating = json["userRating"]?.Value<decimal>();
                string url = json["sourceWebsiteURL"]?.Value<string>();

                //#WORKAROUND gpt returns www.example.com (invalid) for some reason
                if (!string.IsNullOrEmpty(url) && url.ToLower().Contains("example"))
                    recipeCard.SourceURL = null;
                else
                    recipeCard.SourceURL = url;

                int sortOrder = 1;
                foreach (var ing in json["ingredients"])
                {
                    Ingredient newIng = new Ingredient(recipeCard.Session);
                    newIng.SortOrder = sortOrder++;
                    string qty = ing["quantity"]?.Value<string>();
                    decimal decQty;
                    if (decimal.TryParse(qty, out decQty))
                        newIng.Qty = PrimitiveUtils.ConvertToFraction(decQty);
                    else
                        newIng.Qty = qty;
                    if (newIng.Qty != null && newIng.Qty.EndsWith(".0"))
                        newIng.Qty = newIng.Qty.Substring(0, newIng.Qty.Length - 2);

                    newIng.UoM = ing["unitOfMeasure"]?.Value<string>();

                    //we asked for the item separately, us it to suggest...
                    string itemStr = ing["item"]?.Value<string>();
                    newIng.ItemDescription = itemStr;

                    newIng.Item = newIng.SuggestItem(allItems);
                    //Now, set the entire item description
                    string itemDescStr = ing["itemDescription"]?.Value<string>();
                    if (!string.IsNullOrEmpty(itemDescStr))
                        newIng.ItemDescription = $"{itemStr}, {itemDescStr}";
                    recipeCard.AddIngredient(newIng);
                }
                List<string> lines = new List<string>();
                foreach (var step in json["steps"])
                {
                    var x = step.ToString();
                    lines.Add(step.Value<string>());
                }

                recipeCard.Instructions = string.Join(PrimitiveUtils.newl, lines);
                return errorMessage;
            }
            //task.run threw
            catch (AggregateException ex)
            {
                StringBuilder wholeMsg = new StringBuilder();
                foreach (var innerEx in ex.InnerExceptions)
                {
                    wholeMsg.AppendLine(innerEx.Message);
                    Console.WriteLine($"Caught an exception: {innerEx.Message}");
                }
                Log.App.Info(wholeMsg.ToString());
                //#TODO ENHANCEMENT Timeout and cancel exceptions  progress bar with cancel button
                //https://devblogs.microsoft.com/xamarin/getting-started-with-async-await/                
                return wholeMsg.ToString();
            }
            catch (Exception ex)
            {
                string msg = $"GPT unable to find recipe: {ex.Message}";
                Log.App.Info(msg);
                //#TODO ENHANCEMENT Timeout and cancel exceptions  progress bar with cancel button
                //https://devblogs.microsoft.com/xamarin/getting-started-with-async-await/                
                return msg;
            }
        }
    }
}
