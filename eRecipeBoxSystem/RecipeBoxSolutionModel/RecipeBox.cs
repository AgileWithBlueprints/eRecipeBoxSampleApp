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

namespace RecipeBoxSolutionModel
{

    [MapInheritance(MapInheritanceType.OwnTable)]
    /// <summary>
    /// Digitize a RecipeBox, usually one per household.  A set of RecipeCards.
    /// </summary>
    public class RecipeBox : HeadBusinessObject
    {
        public RecipeBox() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public RecipeBox(Session session) : base(session)
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
            get { return this.Name; }
        }

        private string fName;
        [Indexed(Unique = true)]
        [Size(4000)]
        [BusinessKey]
        public string Name
        {
            get { return fName; }
            set
            {
                SetPropertyValue(nameof(Name), ref fName, PrimitiveUtils.Clean(value));
            }
        }

        /// <summary>
        /// The application uses CreateNewRecipeCard to create and initial a new RecipeCard.
        /// The constructors are also used to populate persistent RecipeCard.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public RecipeCard CreateNewRecipeCard(Session session)
        {
            RecipeCard result = new RecipeCard(session);
            //Set DT when recipe card was created.
            result.CreatedDate = DateTime.Now;
            result.RecipeBox = this;
            return result;
        }


        /// <summary>
        /// Associate permissions for UserProfile(s) to a RecipeBox.  Many to Many. UPRB is our association class.
        /// </summary>
        [Association("RecipeBoxUPRB")]
        [JsonIgnore]
        public XPCollection<UserProfileRecipeBox> UPRBs
        {
            get { return GetCollection<UserProfileRecipeBox>(nameof(UPRBs)); }
        }

        /// <summary>
        /// Each RecipeBox has its own Grocery list - typically one per household.
        /// </summary>        
        /// Nullable(false) isn't supported, so we implement it manually in
        /// RecipeBoxDataService.SQL.GenerateConstraintAndIndexesSQL
        /// https://supportcenter.devexpress.com/ticket/details/cs33406/not-null-keyword
        //private GroceryList fGroceryList;
        //[JsonIgnore]
        //public GroceryList GroceryList
        //{
        //    get { return fGroceryList; }
        //    set { SetPropertyValue(nameof(GroceryList), ref fGroceryList, value); }
        //}

        [JsonIgnore]
        [Association("RecipeBoxGroceryList"), Aggregated]
        public XPCollection<GroceryList> GroceryLists
        {
            get
            {
                XPCollection<GroceryList> result = GetCollection<GroceryList>(nameof(GroceryLists));
                return result;
            }
        }

        public void AddGroceryList(GroceryList groceryList)
        {
            GroceryLists.Add(groceryList);
        }

        public void DeleteGroceryList(GroceryList groceryList)
        {
            GroceryLists.Remove(groceryList);
            groceryList.Delete();
        }

    }
}
