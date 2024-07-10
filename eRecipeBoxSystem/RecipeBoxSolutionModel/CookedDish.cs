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

    /// <summary>
    /// Excecute a recipe on a particular date to produce a cooked dish.
    /// </summary>
    public class CookedDish : BusinessObject
    {
        public CookedDish() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public CookedDish(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        [JsonIgnore]
        [Indexed("RecipeCard;CookedDate", Unique = true)]
        private RecipeCard fRecipeCard;
        [Association("CookedDish"), Aggregated]
        public RecipeCard RecipeCard
        {
            get { return fRecipeCard; }
            set { SetPropertyValue(nameof(RecipeCard), ref fRecipeCard, value); }
        }

        public void EditCookedDishDate(DateTime newDate)
        {
            if (CookedDate == newDate)
                return;

            //#RQT CkdDshDtNoDup Don't add duplicate CookedDish dates for a RecipeCard.  When editing a cooked dish to a date that already exists, process this as a no-op.
            if (RecipeCard.FindCookedDish(newDate) != null)
                //since there is already an instance with this date, remove me so we don't get a duplicate.
                RecipeCard.DeleteCookedDish(this);
            else
                CookedDate = newDate;
        }

        [JsonIgnore]
        [Nullable(false)]
        public DateTime CookedDate
        {
            get { return fCookedDate; }
            set { SetPropertyValue(nameof(CookedDate), ref fCookedDate, value.Date); }
        }
        DateTime fCookedDate;


        //---------------------------
        //#AtmtTstng #TRICKY To support testing against relative dates.
        //Set an anchor date.  CookedDateNumberDaysFromAnchorDate is serialized
        static public void SetAnchorDate(DateTime anchorDate)
        {
            fAnchorDate = anchorDate.Date;
        }

        static private DateTime fAnchorDate;

        //Note: this is used for serializing to JSON for system test.  The test data has data relative to TODAY.
        [NonPersistent]
        public int CookedDateNumberDaysFromAnchorDate
        {
            get { return (int)(CookedDate.Date - fAnchorDate).TotalDays; }
        }
    }
}