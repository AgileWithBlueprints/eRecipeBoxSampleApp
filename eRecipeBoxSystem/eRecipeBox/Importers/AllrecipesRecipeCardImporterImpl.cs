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
using Foundation;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using RecipeBoxSolutionModel;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace eRecipeBox.ImportUtils
{
    partial class AllrecipesRecipeCardImporter
    {

        static public int? ParseDuration(string strVal)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            if (strVal == null)
                return null;
            try
            {
                // PT0D2H25M  PT1H25M  PT25M
                Regex regex = new Regex(@"PT((?<days>\d+)D)?((?<hours>\d+)H)?(?<minutes>\d+)M");
                GroupCollection groups = regex.Match(strVal).Groups;
                Int32.TryParse(groups["days"].Value, out days);
                Int32.TryParse(groups["hours"].Value, out hours);
                Int32.TryParse(groups["minutes"].Value, out minutes);
                TimeSpan result = new TimeSpan(days, hours, minutes, seconds);
                return (int?)result.TotalMinutes;
            }
            catch
            {
                throw new Exception("unexpected duration value");
            }
        }

        static private string GetStringValue(JToken token, string property)
        {

            var val = token[property];
            if (val == null)
                return null;
            else
                return WebUtility.HtmlDecode(val.Value<string>());
        }

        static private void Walk(HtmlNode node, RecipeCard targetRecipeCard)
        {
            var nodeName = node.Name;
            try
            {
                if (nodeName == "script")
                {
                    var a1 = node.GetAttributeValue("id", "").ToLower();
                    var a2 = node.GetAttributeValue("class", "").ToLower();
                    var a3 = node.GetAttributeValue("type", "").ToLower();

                    //#TODO FRAGILE Find a documented, more reliable way to find the recipe JSON
                    //#TODO ENHANCEMENT schema.org/Recipe Detect and support all websites with this.  This is the format that Google uses in its application/ld+json schemas
                    //https://schema.org/Recipe
                    if (a1 == "allrecipes-schema_1-0" && a2.Contains("allrecipes-schema mntl-schema-unified") && a3 == "application/ld+json")
                    {
                        int sortOrder = 0;
                        string content = node.GetDirectInnerText().Trim();
                        JToken parsed = JArray.Parse(content)[0];

                        string name = GetStringValue(parsed, "name");
                        string url = GetStringValue(parsed["mainEntityOfPage"], "@id");
                        string description = GetStringValue(parsed, "description");
                        string prepTime = GetStringValue(parsed, "prepTime");
                        string totalTime = GetStringValue(parsed, "totalTime");
                        var recipeYieldObj = parsed["recipeYield"];
                        string recipeYield = null;
                        if (recipeYieldObj != null)
                            recipeYield = WebUtility.HtmlDecode(recipeYieldObj[0].Value<string>());
                        var aggregateRating = parsed["aggregateRating"];
                        string ratingValue = null;
                        string ratingCount = null;
                        if (aggregateRating != null)
                        {
                            ratingValue = GetStringValue(aggregateRating, "ratingValue");
                            ratingCount = GetStringValue(aggregateRating, "ratingCount");
                        }

                        targetRecipeCard.Title = name;
                        targetRecipeCard.SourceURL = url;
                        targetRecipeCard.Description = description;
                        targetRecipeCard.PrepTime = ParseDuration(prepTime);
                        if (targetRecipeCard.PrepTime == 0)
                            targetRecipeCard.PrepTime = null;
                        targetRecipeCard.TotalTime = ParseDuration(totalTime);
                        if (targetRecipeCard.TotalTime == 0)
                            targetRecipeCard.TotalTime = null;
                        if (targetRecipeCard.PrepTime != null && targetRecipeCard.TotalTime != null)
                            targetRecipeCard.CookTime = targetRecipeCard.TotalTime - targetRecipeCard.PrepTime;
                        targetRecipeCard.Yield = recipeYield;
                        targetRecipeCard.Rating = PrimitiveUtils.TextEditToDecimal(ratingValue);
                        targetRecipeCard.RatingCount = PrimitiveUtils.TextEditToInt32(ratingCount);

                        foreach (var item in parsed["recipeIngredient"])
                        {
                            Ingredient newIng = Ingredient.ParseIngredient(WebUtility.HtmlDecode(item.Value<string>()), targetRecipeCard.Session);
                            newIng.SortOrder = sortOrder;
                            sortOrder++;
                            targetRecipeCard.Ingredients.Add(newIng);
                        }
                        List<string> instrs = new List<string>();
                        int stepNum = 0;
                        foreach (var instruction in parsed["recipeInstructions"])
                        {
                            stepNum++;
                            if (instruction["@type"].Value<string>() == "HowToStep")
                            {
                                string instrText = WebUtility.HtmlDecode(instruction["text"].Value<string>());
                                string step;
                                if (!instrText.ToLower().StartsWith("step "))
                                    step = string.Format("Step {0}. {1}", stepNum, instrText);
                                else
                                    step = instrText;
                                instrs.Add(step);
                            }
                        }
                        targetRecipeCard.Instructions = string.Join(PrimitiveUtils.newl, instrs);
                        targetRecipeCard.Instructions = PrimitiveUtils.InsertLineBreakEachSentence(targetRecipeCard.Instructions);
                    }
                    return;
                }
                if (node.HasChildNodes)
                {
                    foreach (HtmlNode childnode in node.ChildNodes)
                    {
                        Walk(childnode, targetRecipeCard);
                    }
                }
            }
            catch
            {
                throw new Exception("HTML for the Allrecipes recipe is not formatted as expected.");
            }
        }
    }
}
