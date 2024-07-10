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
using DevExpress.Xpo.DB;
using Foundation;
using Newtonsoft.Json;
using System;

namespace RecipeBoxSolutionModel
{

    [MapInheritance(MapInheritanceType.OwnTable)]
    //https://supportcenter.devexpress.com/ticket/details/b4664/can-i-abstract-and-inherit-xpobject-classes
    public class RecipeCard : HeadBusinessObject
    {
        public RecipeCard() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public RecipeCard(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.

        }

        public void Clear()
        {
            Title = null;
            Description = null;
            SourceURL = null;
            SourceIndividualFirstName = null;
            SourceIndividualLastName = null;
            PrepTime = null;
            CookTime = null;
            TotalTime = null;
            Yield = null;
            Rating = null;
            RatingCount = null;
            MyRating = null;
            Instructions = null;
            Notes = null;
            WouldLikeToTryFlag = false;

            while (CookedInstances.Count > 0)
            {
                CookedInstances.Remove(CookedInstances[0]);
            }

            while (Ingredients.Count > 0)
            {
                Ingredients.Remove(Ingredients[0]);
            }


        }

        public override string BusinessObjectDisplayName
        {
            get { return this.Title; }
        }

        /// <summary>
        /// Each Recipe is a recipe card that lives inside a RecipeBox
        /// </summary>
        private RecipeBox fRecipeBox;
        public RecipeBox RecipeBox
        {
            get { return fRecipeBox; }
            set { SetPropertyValue(nameof(RecipeBox), ref fRecipeBox, value); }
        }

        [JsonIgnore]
        [Nullable(false)]
        public DateTime CreatedDate
        {
            get { return fCreatedDate; }
            set { SetPropertyValue(nameof(CreatedDate), ref fCreatedDate, value); }
        }
        DateTime fCreatedDate;

        //Note: Unique Title in a RecipeBox is enforced by DataStore check CK_unique_RecipeCard_Title constraint 
        //#RQT TtlUnq Title must be unique for each RecipeCard within the context of a RecipeBox.
        [Nullable(false)]
        [Indexed]
        [Size(1000)]
        [BusinessKey]
        public string Title
        {
            get { return fTitle; }
            set { SetPropertyValue(nameof(Title), ref fTitle, PrimitiveUtils.Clean(value)); }
        }
        string fTitle;

        [PersistentAlias("Iif(IsNullOrEmpty(SourceURL), Title, '<href=' + SourceURL + '>' + Title + '</href>')")]
        [JsonIgnore]
        public string HyperLinkTitle
        {
            get
            {
                return Convert.ToString(EvaluateAlias(nameof(HyperLinkTitle)));
            }
        }

        [Size(4000)]
        public string Description
        {
            get { return fDescription; }
            set { SetPropertyValue(nameof(Description), ref fDescription, PrimitiveUtils.Clean(value)); }
        }
        string fDescription;

        [Indexed]
        [Size(1000)]
        public string SourceURL
        {
            get { return fSourceURL; }
            set { SetPropertyValue(nameof(SourceURL), ref fSourceURL, PrimitiveUtils.Clean(value)); }
        }
        string fSourceURL;

        [Size(250)]
        public string SourceIndividualFirstName
        {
            get { return fSourceIndividualFirstName; }
            set { SetPropertyValue(nameof(SourceIndividualFirstName), ref fSourceIndividualFirstName, PrimitiveUtils.Clean(value)); }
        }
        string fSourceIndividualFirstName;

        [Size(250)]
        public string SourceIndividualLastName
        {
            get { return fSourceIndividualLastName; }
            set { SetPropertyValue(nameof(SourceIndividualLastName), ref fSourceIndividualLastName, PrimitiveUtils.Clean(value)); }
        }
        string fSourceIndividualLastName;

        [JsonIgnore]
        [PersistentAlias("SourceIndividualLastName + ', ' + SourceIndividualFirstName")]
        public string SourceIndividualFullName
        {
            get { return Convert.ToString(EvaluateAlias(nameof(SourceIndividualFullName))); }
        }

        public int? PrepTime
        {
            get { return fPrepTime; }
            set { SetPropertyValue(nameof(PrepTime), ref fPrepTime, value); }
        }
        int? fPrepTime;

        public int? CookTime
        {
            get { return fCookTime; }
            set { SetPropertyValue(nameof(CookTime), ref fCookTime, value); }
        }
        int? fCookTime;

        public int? TotalTime

        {
            get { return fTotalTime; }
            set { SetPropertyValue(nameof(TotalTime), ref fTotalTime, value); }
        }
        int? fTotalTime;


        [Size(200)]
        public string Yield
        {
            get { return fYield; }
            set { SetPropertyValue(nameof(Yield), ref fYield, PrimitiveUtils.Clean(value)); }
        }
        string fYield;

        [JsonIgnore]
        public decimal? Rating
        {
            get { return fRating; }
            set { SetPropertyValue(nameof(Rating), ref fRating, value); }
        }
        decimal? fRating;

        [PersistentAlias(nameof(Rating))]
        [JsonProperty("Rating")]
        public decimal? RatingRounded
        {
            get
            {
                if (fRating == null)
                    return null;
                else
                    return Math.Round((decimal)fRating, 2);
            }
        }

        public static int RatingCountMinValue = 0;
        public int? RatingCount
        {
            get { return fRatingCount; }
            set
            {
                int? trimmedVal = value;
                if (value != null)
                {
                    if (value < RatingCountMinValue)
                        trimmedVal = RatingCountMinValue;
                }
                SetPropertyValue(nameof(RatingCount), ref fRatingCount, trimmedVal);
            }
        }
        int? fRatingCount;

        //formatting tips
        // https://supportcenter.devexpress.com/ticket/details/q321926/sql-error-conversion-failed-when-converting-date-and-or-time-from-character-string-when#

        //this is required when Recipe is used in a XPInstantFeedbackView and other server mode collections
        //https://docs.devexpress.com/CoreLibraries/DevExpress.Data.Filtering.FunctionOperatorType  for ToStr
        //https://docs.devexpress.com/CoreLibraries/4928/devexpress-data-library/criteria-language-syntax  syntax
        //https://docs.devexpress.com/CoreLibraries/DevExpress.Data.Filtering.FunctionOperatorType
        //Note: this expression runs on the server in SQL
        //[PersistentAlias("ToStr(Rating) + ' (' + Iif(RatingCount IS NULL,'',FORMAT(RatingCount, '###,###', 'en-us')) + ')'")]//
        //[PersistentAlias("Iif(IsNull(RatingCount),'n/a',FORMAT(RatingCount , '###,###', 'en-us'))")]
        //[PersistentAlias("FORMAT(Oid, '###,###,###', 'en-us')")]
        //#TODO Book Example bug.. this would be good bug for the book.  original was  [PersistentAlias("ToStr(Rating) + ' (' + ToStr(RatingCount) + ')'")]
        [JsonIgnore]
        [PersistentAlias("ToStr(Round(Rating, 2)) + ' (' + Iif(RatingCount IS NULL, '', ToStr(RatingCount)) + ')'")]
        public string RatingRatingCount
        {
            get { return Convert.ToString(EvaluateAlias(nameof(RatingRatingCount))); }
        }

        int? fMyRating;

        public static int MyRatingMinValue = 1;
        public static int MyRatingMaxValue = 10;
        //#RQT MyRtgRng MyRating score range: 1-10.
        public int? MyRating
        {
            get { return fMyRating; }
            set
            {
                int? trimmedVal = value;
                if (value != null)
                {
                    if (value < MyRatingMinValue)
                        trimmedVal = MyRatingMinValue;
                    else if (value > MyRatingMaxValue)
                        trimmedVal = MyRatingMaxValue;
                }
                SetPropertyValue(nameof(MyRating), ref fMyRating, trimmedVal);
            }
        }

        [Size(4000)]
        public string Instructions
        {
            get { return fInstructions; }
            set { SetPropertyValue(nameof(Instructions), ref fInstructions, PrimitiveUtils.Clean(value)); }
        }
        string fInstructions;

        [Size(4000)]
        public string Notes
        {
            get { return fNotes; }
            set { SetPropertyValue(nameof(Notes), ref fNotes, PrimitiveUtils.Clean(value)); }
        }
        string fNotes;

        [Nullable(false)]
        public bool WouldLikeToTryFlag
        {
            get { return fWouldLikeToTryFlag; }
            set { SetPropertyValue(nameof(WouldLikeToTryFlag), ref fWouldLikeToTryFlag, value); }
        }
        bool fWouldLikeToTryFlag;


        #region Keywords        
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if added. false if keyword was already on the RecipeCard so no action taken.</returns>
        public bool AddKeyword(Keyword key)
        {
            //TODO BUG student exercise.  2 apps adding same KW.  have them find this in test.   need SP  
            //#RQT RecpCrdKwdNoDup Don't add the same keyword twice to a RecipeCard.  Adding an existing keyword is effectively a no-op.
            if (this.FindKeyword(key.Name) == null)
            {
                Keywords.Add(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveKeyword(Keyword keyword)
        {
            if (keyword == null)
                return;
            Keywords.Remove(keyword);
        }
        public Keyword FindKeyword(string name)
        {
            foreach (Keyword key in this.Keywords)
            {
                if (key.Name == name)
                    return key;
            }
            return null;
        }

        //#TODO ENHANCEMENT Flag a Keyword as a cuisine.  Then cache cuisines in a property and display it on the filter results
        //Helps see cuisines recently had when meal planning 

        [Association("RecipeCardKeywords", UseAssociationNameAsIntermediateTableName = true)]
        public XPCollection<Keyword> Keywords
        {
            get
            {
                XPCollection<Keyword> result = GetCollection<Keyword>(nameof(Keywords));
                //#NOTE #TIP sorting causes problems for many to many when calling remove. so bind to a grid and sort column within the grid
                //error is collection was reset during object adding
                //result.Sorting = new SortingCollection(new SortProperty("Name", SortingDirection.Ascending));
                result.DisplayableProperties = "Oid;Name";
                return result;
            }
        }

        #endregion Keywords

        #region Ingredients
        public void AddIngredient(Ingredient ing)
        {
            Ingredients.Add(ing);

        }

        public void DeleteIngredient(Ingredient ing)
        {
            if (ing == null)
                return;
            Ingredients.Remove(ing);
            //TRICKY itemDescription is not null.  XPO Delete unfort saves it as a soft delete so, ensure itemDescription isn't null.
            if (ing.ItemDescription == null)
                ing.ItemDescription = "x";
            ing.Delete();

        }

        [Association("RecipeIngredient"), Aggregated]
        public XPCollection<Ingredient> Ingredients
        {
            get
            {
                XPCollection<Ingredient> result = GetCollection<Ingredient>(nameof(Ingredients));

                //#TIP How to sort on server or XPO client
                //per https://docs.devexpress.com/XPO/2037/query-and-shape-data/sorting

                //Store and display ingredients in the same order that the user saved them.
                result.Sorting.Add(new SortingCollection(new SortProperty(nameof(Ingredient.SortOrder), SortingDirection.Ascending)));

                //Ingredient fields column display order in the grid: Qty, UoM, ItemDescription, ItemName
                result.DisplayableProperties = "Qty;UoM;ItemDescription;SortOrder;Item!Key";
                return result;
            }
        }

        public void SuggestIngredientItems(XPCollection<Item> allItems)
        {
            foreach (Ingredient ing in this.Ingredients)
            {
                //#RQT ItmSggst When saving RecipeCard, if an ingredient has a null Item, attempt to auto-suggest an item.
                if (ing.Item == null)
                    ing.Item = ing.SuggestItem(allItems);
                //#RQT ItmDescAtoFll When saving RecipeCard, if an ingredient has a null ItemDescription, set it to the ItemName (because ItemDescription is required).
                if (ing.Item != null && ing.ItemDescription == null)
                    ing.ItemDescription = ing.Item.Name;
            }
        }

        #endregion Ingredients

        #region CookDish

        [Association("CookedDish"), Aggregated]
        public XPCollection<CookedDish> CookedInstances
        {
            get
            {
                XPCollection<CookedDish> result = GetCollection<CookedDish>(nameof(CookedInstances));
                //#TIP XPO doesn't support setting sorting property, then updating the collection
                //https://supportcenter.devexpress.com/ticket/details/q421384/collection-was-reset-during-object-adding
                //result.Sorting = new SortingCollection(new SortProperty("CookedDate", SortingDirection.Descending));
                result.DisplayableProperties = "CookedDate";
                return result;
            }
        }

        public CookedDish LastCookedInstance()
        {
            if (CookedInstances.Count == 0)
                return null;
            CookedDish lastOne = this.CookedInstances[0];
            for (int i = 1; i < this.CookedInstances.Count; i++)
            {
                CookedDish current = this.CookedInstances[i];
                if (current.CookedDate > lastOne.CookedDate)
                    lastOne = current;
            }
            return lastOne;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cr"></param>
        /// <returns>True if actually added. False if would cause a dup, so marked for delete. Provide hint to view if refresh view is needed.</returns>
        public bool AddCookedDish(CookedDish cr)
        {
            //TODO BUG student exercise.  2 apps adding same KW.  have them find this in test.   need SP
            //#RQT Don't add duplicate CookedDish dates for a RecipeCard.  Adding a date that already exists is a no-op.
            if (this.FindCookedDish(cr.CookedDate) == null)
            {
                CookedInstances.Add(cr);
                return true;
            }
            else
            {
                cr.Delete();  //#TRICKY avoid orphans in the DB
                return false;
            }
        }
        public void DeleteCookedDish(CookedDish cr)
        {
            if (cr == null)
                return;
            //#TRICKY must first remove from collection, then delete the child object
            CookedInstances.Remove(cr);
            cr.Delete();
        }

        public CookedDish FindCookedDish(DateTime date)
        {
            foreach (CookedDish cr in this.CookedInstances)
            {
                if (cr.CookedDate.Date == date.Date)
                    return cr;
            }
            return null;
        }

        public CookedDish FindCookedDish(int cookedDishOID)
        {
            foreach (CookedDish cr in this.CookedInstances)
            {
                if (cr.Oid == cookedDishOID)
                    return cr;
            }
            return null;
        }

        #endregion CookDish

    }
}