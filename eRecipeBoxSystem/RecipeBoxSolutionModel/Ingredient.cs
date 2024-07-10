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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RecipeBoxSolutionModel
{

    /// <summary>
    /// An item included in a Recipe
    /// </summary>
    //https://supportcenter.devexpress.com/ticket/details/t1079544/how-do-i-create-a-unique-multi-field-index-that-includes-oid
    public class Ingredient : BusinessObject
    {
        public Ingredient() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public Ingredient(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        public override string ToString()
        {
            string tab = "" + '\t';
            List<string> parts = new List<string>();
            parts.Add(this.Qty);
            parts.Add(this.UoM);
            parts.Add(this.ItemDescription);
            string result = string.Join(tab, parts);
            return result;
        }

        //#TRICKY [Indexed("SortOrder", Unique = true)]  cannot be unique because xpo uses soft deletes.  add unique check manually after purge
        private RecipeCard fRecipeCard;
        [JsonIgnore]
        [Association("RecipeIngredient"), Aggregated]
        [Indexed("SortOrder")]
        public RecipeCard RecipeCard
        {
            get { return fRecipeCard; }
            set { SetPropertyValue(nameof(RecipeCard), ref fRecipeCard, value); }
        }


        // Store the order of ingredients in the DB. User determines the order.
        //#TODO REFACTOR note: This didn't work. Still get dup key exceptions on this soft delete.
        // try this?https://docs.devexpress.com/XPO/403815/create-a-data-model/indexes
        //[Indexed("RecipeCard;GCRecord", Unique = true)]  
        //
        //#RQT IngUsrOrdr Allow user to set the display order of ingredients. 
        //iRecipeBoxSortOrder index for fast retrieval
        //CK_unique_Ingredient_RecipeCard_SortOrder ensures unique
        [Nullable(false)]
        public int SortOrder
        {
            get { return fSortOrder; }
            set { SetPropertyValue(nameof(SortOrder), ref fSortOrder, value); }
        }
        int fSortOrder;

        [Size(255)]
        public string Qty
        {
            get { return fQty; }
            //Auto-clean all user entered strings entered in the UI -> Trim leading/trailing spaces and set to Null if all whitespace.
            set { SetPropertyValue(nameof(Qty), ref fQty, PrimitiveUtils.Clean(value)); }
        }
        string fQty;

        [Size(255)]
        public string UoM
        {
            get { return fUoM; }
            set { SetPropertyValue(nameof(UoM), ref fUoM, PrimitiveUtils.Clean(value)); }
        }
        string fUoM;

        [Nullable(false)]
        [Indexed]
        [Size(4000)]
        public string ItemDescription
        {
            get { return fItemDescription; }
            set { SetPropertyValue(nameof(ItemDescription), ref fItemDescription, PrimitiveUtils.Clean(value)); }
        }
        string fItemDescription;

        private Item fItem;
        public Item Item
        {
            get { return fItem; }
            set { SetPropertyValue(nameof(Item), ref fItem, value); }
        }

        //#RQT IngSctnHdr Bold a section header that is determined by -
        //#RQT IngSctnHdr 1. No quantity, UoM or Item.
        //#RQT IngSctnHdr 2. Section name in ItemDescription ends with ":"  (eg, Add and Simmer: )      
        [JsonIgnore]
        public bool IsSectionHeader
        {
            get
            {
                return Qty == null && UoM == null && ItemDescription != null && ItemDescription.EndsWith(":");
            }
        }

        //Attempt to suggest an Item based on Item description
        public Item SuggestItem(XPCollection<Item> allItems)
        {
            //#RQT IngSggstItem Attempt to automatically select an Item based on ItemDescription.  Heuristic -  
            //#RQT IngSggstItem 1. Convert all words to their singular form (eg, eggs to egg).
            //#RQT IngSggstItem 2. Search all Items for an exact match.
            //#RQT IngSggstItem 2. If no exact match, search (contains) Items.
            //#RQT IngSggstItem 3. If there is only 1 match result whose Name length is 5 or greater, auto-select it.

            //#TODO ENHANCEMENT SuggestItem Also search with synonyms eg garbanzo(chickpea) 
            //dark red kidney beans as a source...  first try all words, then start removing trying alternate combinations
            //#TODO ENHANCEMENT SuggestItem Search for all distinct prior mappings (low cost learning)
            if (ItemDescription == null || IsSectionHeader)
                return null;
            string testStr = PrimitiveUtils.ReplaceSpecialCharacters(PrimitiveUtils.ToSingularLower(ItemDescription));
            List<Item> candidates = new List<Item>();
            foreach (Item item in allItems)
            {
                if (testStr == PrimitiveUtils.ReplaceSpecialCharacters(item.NameSingularLower))
                    candidates.Add(item);
            }
            if (candidates.Count == 1)
                return candidates[0];

            foreach (Item item in allItems)
            {
                if (testStr.Contains(PrimitiveUtils.ReplaceSpecialCharacters(item.NameSingularLower)))
                    candidates.Add(item);
            }
            if (candidates.Count == 1 && candidates[0].Name.Length > 4)
                return candidates[0];
            else
                return null;
        }


        //#RQT IngPrsItmDscr Parse ingredient string into its parts (Qty,UoM,Item Description) when importing.  Heuristic: Look for a recognizable unit of measure and use it as a parsing anchor.  
        /// <summary>
        /// Parse an unstructured ingredient string into a structured Ingredient (with Qty, UoM, description, Item)
        /// Simple heuristic implementation
        /// </summary>
        /// <param name="val">Unstructured ingredient string</param>
        /// <param name="session"></param>
        /// <returns>Parsed Ingredient</returns>     
        //#TODO ENHANCEMENT Improve the parser with something like this https://github.com/NYTimes/ingredient-phrase-tagger
        static public Ingredient ParseIngredient(string val, Session session)
        {
            string[] uoms = {
            "bag",
            "bags",
            "ball",
            "balls",
            "bar",
            "bars",
            "basket",
            "baskets",
            "batch",
            "batches",
            "block",
            "blocks",
            "bottle",
            "bottles",
            "branch",
            "branches",
            "bulb",
            "bulbs",
            "bunch",
            "bunches",
            "bushy sprigs",
            "can",
            "cans",
            "chunk",
            "chunks",
            "clove",
            "cloves",
            "cluster",
            "clusters",
            "container",
            "containers",
            "cube",
            "cubes",
            "c",
            "C",
            "cup",
            "cups",
            "dash",
            "dashes",
            "dozen",
            "dozen medium",
            "drop",
            "drops",
            "ear",
            "ears",
            "envelope",
            "feet",
            "fluid ounce",
            "fluid ounces",
            "foot",
            "full sprigs",
            "gallon",
            "gallons",
            "gms",
            "gram",
            "grams",
            "handful",
            "handfuls",
            "head",
            "heads",
            "inch",
            "inches",
            "large",
            "lb",
            "lbs",
            "leafy sprig",
            "liter",
            "liters",
            "medium",
            "milliliter",
            "ounce",
            "ounces",
            "oz",
            "part",
            "parts",
            "piece",
            "pieces",
            "pinch",
            "pinches",
            "pint",
            "pints",
            "pod",
            "pods",
            "pound",
            "pounds",
            "qt",
            "qts",
            "quart",
            "quarts",
            "rack",
            "racks",
            "scoop",
            "scoops",
            "segment",
            "segments",
            "shake",
            "shakes",
            "shot",
            "shots",
            "slice",
            "slices",
            "small",
            "spear",
            "spears",
            "splash",
            "splashes",
            "sprig",
            "sprigs",
            "stalk",
            "stalks",
            "stem",
            "stems",
            "stick",
            "sticks",
            "T",
            "tablespoon",
            "tablespoons",
            "tbs",
            "tbsp",
            "tbsps",
            "Tbsp",
            "Tbsps",
            "t",
            "teaspoon",
            "teaspoons",
            "tsp",
            "tsps",
            "vial",
            "vials",
            "wedge",
            "wedges"
            };

            List<string> uomList = new List<string>(uoms);

            string val1 = val.Replace("\t", " ");
            //bcp doesn't convert these unicode fraction chars
            foreach (string frctn in FractionalNumber.UnicodeFractionsTranslation.Keys)
                val1 = val1.Replace(frctn, " " + FractionalNumber.UnicodeFractionsTranslation[frctn]);

            val1 = val1.Trim();
            string lcVal1 = val1.ToLower();
            string[] words = val1.Split(' ');

            Ingredient newIngr = new Ingredient(session);
            bool containsUoM = false;
            //Look for a UoM term
            foreach (string uom in uomList)
            {
                if (lcVal1.Contains(uom))
                {
                    containsUoM = true;
                    break;
                }
            }
            List<string> wordsList = new List<string>(words);

            //attempt to parse a numeric qty
            int wordIdx = 0;
            string qty = null;
            if (words.Length > 1)
                qty = words[wordIdx++];
            decimal decQty;
            //handle the "1 1/2"  case
            if (words.Length > 1 && words[1].Contains(@"/") && !Double.IsNaN(new FractionalNumber(words[1])))
            {
                qty = words[0] + ' ' + words[1];
                wordIdx = 2;
            }

            //#RQT IngTrnslDcmlQty Auto-translate decimal quantities into fractions. eg, .25 is auto-translated to 1/4
            if (decimal.TryParse(qty, out decQty))
                qty = PrimitiveUtils.ConvertToFraction(decQty);

            double qtyAsDouble = new FractionalNumber(qty);
            if (Double.IsNaN(qtyAsDouble))
                qty = null;  //not a number.            

            //Handle special case of "salt and pepper to taste..."  and similar
            if (qty == null && !containsUoM)
            {
                newIngr.ItemDescription = val;
                return newIngr;
            }

            //Look for this special case - (10.75 ounce) can condensed cream of mushroom soup
            //Fortunately, Allrecipes consistently does this 
            //Note: for V1.0, we key off the parens...  #TODO ENHANCEMENT could be improved 
            Regex regex = new Regex(@"\(.*?\)");
            Match match = regex.Match(val1);
            string rest = null;
            if (match.Success)
            {
                string insideParens = val1.Substring(match.Index + 1, match.Value.Length - 2).Trim();
                string[] insideWords = insideParens.Split(' ');  //pick up the next word too

                containsUoM = false;
                //Look for a UoM term
                foreach (string inw in insideWords)
                {
                    if (uomList.Contains(inw.ToLower().Replace(".", "")))
                    {
                        containsUoM = true;
                        break;
                    }
                }

                //looks like we have
                //1 (10.75 ounce) can condensed cream of mushroom soup or 
                //1 (10.75 ounce can) condensed cream of mushroom soup
                if (containsUoM)
                {
                    rest = val1.Substring(match.Index + match.Value.Length).Trim();
                    string[] w = rest.Split(' ');
                    List<string> wList = new List<string>(w);
                    newIngr.Qty = PrimitiveUtils.Clean((qty == null) ? val1.Substring(0, match.Index).Trim() : qty);
                    if (w[0] == "can")
                    {
                        newIngr.UoM = match.Value + " " + w[0];
                        rest = string.Join(" ", wList.GetRange(1, w.Length - 1)).Trim();
                    }
                    else
                        newIngr.UoM = match.Value;

                    newIngr.ItemDescription = rest;
                    return newIngr;
                }
            }

            //Is the 2nd word a UoM?
            if (uomList.Contains(words[wordIdx].ToLower().Replace(".", "")))
            {
                newIngr.UoM = words[wordIdx];

                //qty is not a proper number, so just copy all text before the UoM
                if (qty == null)
                    qty = string.Join(" ", wordsList.GetRange(0, wordIdx)).Trim();

                wordIdx++;
            }
            //no UoM and qty isn't a proper number.
            else if (qty == null)
                wordIdx = 0;

            rest = string.Join(" ", wordsList.GetRange(wordIdx, words.Length - wordIdx)).Trim();
            newIngr.Qty = qty;
            newIngr.ItemDescription = rest;
            return newIngr;

            //#TODO ENHANCEMENT Might be able to improve our parse alg by researching other solutions
            //https://github.com/JedS6391/RecipeIngredientParser
            //https://www.jedsimson.co.nz/blog/2020/06/04/parsing-ingredients-from-online-recipe-articles
            //https://developers.google.com/search/docs/appearance/structured-data/recipe
            //https://www.benawad.com/scraping-recipe-websites/
        }
    }
}