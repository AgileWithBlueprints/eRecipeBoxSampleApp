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
using DataStoreUtils;
using DevExpress.Xpo;
using Foundation;
using HtmlAgilityPack;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace eRecipeBox.ImportUtils
{
    //#TODO REFACTOR Remove all UI dependencies in the importers and move them all to RecipeBoxSolutionModel
    //where they belong.
    partial class AllrecipesRecipeCardImporter
    {
        //returns error message, null if success
        static public string LoadRecipeFromWebpage(string url, RecipeCard targetRecipeCard, MDIChildForm parentForm)
        {
            try
            {
                //Response didn't have a recipe
                if (string.IsNullOrEmpty(url) || !url.ToLower().Contains("allrecipes.com"))
                {
                    return $"Error. Provide a recipe from www.allrecipes.com site.";
                }


                //https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel
                //fixes  Could not create SSL/TLS secure channel
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                WebClient wc = new WebClient();
                var web = new HtmlWeb();
                var doc = web.Load(url);

                string html = doc.Text;
                //Get these out of the html instead...
                //primaryTaxonomyNames":"Allrecipes|Recipes|Main Dishes|Pork|Pork Chop Recipes|Braised"
                //looks to be the only place to find keywords
                List<string> keywords = new List<string>();
                List<string> keywords2 = new List<string>();
                Regex regex = new Regex(@"primaryTaxonomyNames"":""((?<taxonomies>.*?)"")");
                Match match = regex.Match(html);
                if (match.Success)
                {
                    GroupCollection groups = match.Groups;
                    var list = groups["taxonomies"].Value;
                    var keyws = list.Split('|');
                    keywords.AddRange(keyws);
                    keywords.Remove("Recipes");
                    keywords.Remove("Cuisine");
                    keywords.Remove("Allrecipes");
                    foreach (string kw in keywords)
                    {
                        keywords2.Add(PrimitiveUtils.Captialize(kw.Replace(" Recipes", "")));
                    }

                    UnitOfWork uow = new UnitOfWork();
                    XPCollection<Keyword> allKeys = new XPCollection<Keyword>(uow);
                    allKeys.CaseSensitive = false;
                    foreach (string strKeyword in keywords2)
                    {
                        if (Keyword.FindKeywordXP(allKeys, strKeyword) == null)
                        {
                            string question = string.Format("Do you want to add Keyword - {0}?", strKeyword);
                            DialogResult answer = parentForm.ShowMessageBox(parentForm, question, "Confirmation",
                                MessageBoxButtons.YesNo);
                            if (answer == DialogResult.Yes)
                            {
                                UnitOfWork uow2 = new UnitOfWork();
                                Keyword newKey = new Keyword(uow2);
                                newKey.Name = strKeyword;
                                XpoService.CommitBodyOfBusObjs(newKey, uow2);
                            }
                        }
                    }
                }
                //    //https://html-agility-pack.net/knowledge-base/22833160/gettig-htmlelement-based-on-htmlagilitypack-htmlnode
                //    //http://www.4guysfromrolla.com/articles/011211-1.aspx
                Walk(doc.DocumentNode, targetRecipeCard);

                //Response didn't have a recipe
                if (string.IsNullOrEmpty(targetRecipeCard.Title))
                {
                    return $"Error. Unable to import recipe from {url}.";
                }

                XPCollection<Keyword> allKeys2 = new XPCollection<Keyword>(targetRecipeCard.Session);
                allKeys2.CaseSensitive = false;
                foreach (string strKeyword in keywords2)
                {
                    Keyword theKey = Keyword.FindKeywordXP(allKeys2, strKeyword);
                    if (theKey != null)
                        targetRecipeCard.Keywords.Add(theKey);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }
    }
}
