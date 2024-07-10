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
using System.Text;

namespace RecipeBoxSolutionModel
{

    [MapInheritance(MapInheritanceType.OwnTable)]
    /// <summary>
    /// List of items to purchase at a grocery store... needed for recipes as well as staples to have around the house.
    /// </summary>
    public class GroceryList : HeadBusinessObject
    {
        public GroceryList() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public GroceryList(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        public override string BusinessObjectDisplayName
        {
            get { return $"{this.RecipeBox.BusinessObjectDisplayName} Grocery List"; }
        }

        private RecipeBox fRecipeBox;
        [JsonIgnore]
        [Association("RecipeBoxGroceryList"), Aggregated]
        public RecipeBox RecipeBox
        {
            get { return fRecipeBox; }
            set { SetPropertyValue(nameof(RecipeBox), ref fRecipeBox, value); }
        }

        public void AddGroceryListItem(GroceryListItem gli)
        {
            this.GroceryListItems.Add(gli);
        }

        public void DeleteGroceryListItem(GroceryListItem gli)
        {
            this.GroceryListItems.Remove(gli);
            //#TRICKY item is not null.  Delete unfort saves it so, ensure item isn't null.
            if (gli.Item == null)
                gli.Item = "x";
            gli.Delete();
        }

        [Association("GroceryListItem"), Aggregated]
        public XPCollection<GroceryListItem> GroceryListItems
        {
            get
            {
                XPCollection<GroceryListItem> result = GetCollection<GroceryListItem>(nameof(GroceryListItems));
                result.DisplayableProperties = nameof(GroceryListItem.Item) + ';' + nameof(GroceryListItem.Qty) + ';' + nameof(GroceryListItem.UoM) + ';' + nameof(GroceryListItem.ItemDescription);
                return result;
            }
        }

        //Generate and send an HTML table when emailing the grocery list.
        public string GenerateHTMLtable()
        {
            List<string> lines = new List<string>();
            foreach (GroceryListItem gli in this.GroceryListItems)
            {
                string line = null;
                if (gli.Item == null)
                    line = gli.ItemDescription + ": " + gli.Qty + " " + gli.UoM;
                else
                {
                    if (string.Compare(gli.Item, gli.ItemDescription, ignoreCase: true) == 0 && !string.IsNullOrEmpty(gli.ItemDescription))
                        line = gli.Item + ": " + gli.Qty + " " + gli.UoM;
                    else
                        line = $"{gli.Item}({gli.ItemDescription}): {gli.Qty} {gli.UoM}";
                }
                lines.Add(line);
            }


            //#RQT GLEmlSrt Sort grocery list by Item Name when emailing the grocery list.
            lines.Sort();
            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table border='5' cellpadding='5' cellspacing='0' >");

            //HeaderRow.
            sb.Append("<tr>");
            sb.Append("<th border='5'>Grocery List</th>");
            sb.Append("</tr>");

            //Items
            foreach (string row in lines)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + row + "</td>");
                sb.Append("</tr>");
            }

            //Table end.
            sb.Append("<table>");
            return sb.ToString();

        }



    }

    public class GroceryListItem : BusinessObject
    {
        public GroceryListItem() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public GroceryListItem(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        private GroceryList fGroceryList;
        [Association("GroceryListItem"), Aggregated]
        [JsonIgnore]
        public GroceryList GroceryList
        {
            get { return fGroceryList; }
            set { SetPropertyValue(nameof(GroceryList), ref fGroceryList, value); }
        }

        [Size(255)]
        public string Qty
        {
            get { return fQty; }
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

        [Size(4000)]
        public string ItemDescription
        {
            get { return fItemDescription; }
            set { SetPropertyValue(nameof(ItemDescription), ref fItemDescription, PrimitiveUtils.Clean(value)); }
        }
        string fItemDescription;

        /// <summary>
        /// Note that GroceryListItem merely copies the item name as a string vs. referring to an
        /// instance of Item.  This allows the user to modify the Item in grocery list without updating the master
        /// item.
        /// </summary>
        [Nullable(false)]
        [Size(4000)]
        public string Item
        {
            get { return fItem; }
            set { SetPropertyValue(nameof(Item), ref fItem, PrimitiveUtils.Clean(value)); }
        }
        string fItem;
    }

}