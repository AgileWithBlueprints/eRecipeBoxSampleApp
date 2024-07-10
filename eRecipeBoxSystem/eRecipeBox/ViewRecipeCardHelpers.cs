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
using DevExpress.Utils;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.XtraGrid.Views.Grid;
using Foundation;
using RecipeBoxSolutionModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
namespace eRecipeBox
{
    //#RQT 1VwRecpCrdFrm ViewRecipeCard behavior - 
    //#RQT 1VwRecpCrdFrm ViewRecipeCard supports cooking a recipe - Read only mode.
    //#RQT 1VwRecpCrdFrm User can move the splitter to resize the 2 panels.
    //#RQT 1VwRecpCrdFrm User can Schedule & Delete the RecipeCard.
    //#RQT 1VwRecpCrdFrm User can also Open the GroceryList or Add the Ingredients to the GroceryList from this form.
    partial class ViewRecipeCard
    {
        #region ctor
        private void InitializeComponent2()
        {
            this.instructionsMemoEdit.ReadOnly = true;

            //#RQT Shortcut keys - Alt-c (Close),  Alt-e (editRecipeCard),  Alt-d (deleteRecipeCard),  Alt-g (editGroceryList),  Alt-a (addIngredientsToGroceryList),  Alt-s (scheduleRecipeCard),  
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(closeVarButtonItem, Keys.Alt, Keys.C);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(editRecipeBarButtonItem, Keys.Alt, Keys.E);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(deleteRecipeBarButtonItem, Keys.Alt, Keys.D);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(editGroceryListBarButtonItem, Keys.Alt, Keys.G);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(addIngToGLButtonItem, Keys.Alt, Keys.A);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(scheduleBarButtonItem, Keys.Alt, Keys.S);

            //Ingredients....
            this.VisibleChanged += Form1_VisibleChanged;
            this.ResizeEnd += Form1_ResizeEnd;

            this.gridView1.OptionsView.ShowIndicator = false;
            this.gridView1.FocusRectStyle = DrawFocusRectStyle.RowFullFocus;
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsBehavior.ReadOnly = true;
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsView.ShowAutoFilterRow = false; // filter at top of each row
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.OptionsCustomization.AllowGroup = false;
            this.gridView1.OptionsFind.AllowFindPanel = false;
            this.gridView1.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;  //filter panel at bottom
            this.gridControl1.UseEmbeddedNavigator = false;
            this.gridView1.OptionsView.ShowVerticalLines = DefaultBoolean.False;
            this.gridView1.OptionsView.ShowHorizontalLines = DefaultBoolean.False;

            SetAllControlNames();
        }
        #endregion

        #region CRUDBusinessObjectForm
        protected override void ModelToView()
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;

            //recipe
            this.titleHyperLinkEdit.EditValue = recipe.SourceURL;
            this.titleHyperLinkEdit.Properties.Caption = recipe.Title;
            this.titleHyperLinkEdit.ReadOnly = true;

            //#TODO REFACTOR this is quick and dirty
            //instructions
            List<string> parts = new List<string>();
            string row0 = null;
            string row1 = null;
            string row2 = null;
            string row3 = null;
            string row4 = null;
            string row5 = null;
            row0 = recipe.Title;
            if (recipe.PrepTime != null)
                parts.Add("Prep Time: " + recipe.PrepTime.ToString());
            if (recipe.CookTime != null)
                parts.Add("Cook Time: " + recipe.CookTime.ToString());
            if (recipe.TotalTime != null)
                parts.Add("Total Time: " + recipe.TotalTime.ToString());
            if (parts.Count > 0)
                row1 = string.Join(" ", parts);
            if (row1 != null)
                row1 += " minutes";
            parts.Clear();
            if (recipe.Yield != null)
                parts.Add("Yield: " + recipe.Yield.ToString());
            if (recipe.Rating != null)
                parts.Add("Score: " + recipe.RatingRounded.ToString());
            if (recipe.RatingCount != null)
                parts.Add("Votes: " + recipe.RatingCount.ToString());
            if (parts.Count > 0)
                row2 = string.Join(" ", parts);
            parts.Clear();
            if (recipe.MyRating != null)
                parts.Add("My Score: " + recipe.MyRating.ToString());
            if (parts.Count > 0)
                row3 = string.Join(" ", parts);
            parts.Clear();
            foreach (Keyword k in recipe.Keywords)
            {
                parts.Add(k.Name);
            }
            string keywords = string.Join(", ", parts);
            if (recipe.Keywords.Count > 0)
                row4 = "Keywords: " + keywords;
            if (recipe.CookedInstances.Count > 0)
                row5 = "Last Cooked: " + recipe.LastCookedInstance().CookedDate.ToString("d");

            StringBuilder rhs = new StringBuilder();
            if (row0 != null)
                rhs.Append(row0 + PrimitiveUtils.newl);
            if (row1 != null)
                rhs.Append(row1 + PrimitiveUtils.newl);
            if (row2 != null)
                rhs.Append(row2 + PrimitiveUtils.newl);
            if (row3 != null)
                rhs.Append(row3 + PrimitiveUtils.newl);
            if (row4 != null)
                rhs.Append(row4 + PrimitiveUtils.newl);
            if (row5 != null)
                rhs.Append(row5 + PrimitiveUtils.newl);
            if (recipe.Instructions != null)
            {
                rhs.Append(PrimitiveUtils.newl + "Instructions:" + PrimitiveUtils.newl);
                rhs.Append(recipe.Instructions);
            }
            if (recipe.Description != null)
                rhs.Append(PrimitiveUtils.newl2 + "Description: " + PrimitiveUtils.newl + recipe.Description + PrimitiveUtils.newl);
            if (recipe.Notes != null)
                rhs.Append(PrimitiveUtils.newl + "Notes: " + PrimitiveUtils.newl + recipe.Notes);
            if (rhs.Length > 0)
                this.instructionsMemoEdit.EditValue = rhs.ToString();

            recipeXPBindingSource.DataSource = recipe;
            this.gridView1.Focus();
            DoUpdateViewState();
        }
        protected override bool ReloadModel()
        {
            NewUnitOfWork();
            ReloadLookupLists();
            if (this.Oid.HasValue)
            {
                this.FormHeadBusinessObject = (RecipeCard)DataStoreUtils.XpoService.LoadHeadBusObjByKey(typeof(RecipeCard), Oid.Value, UnitOfWork);
                if (this.FormHeadBusinessObject == null)
                    return false;
            }
            else
                throw new Exception("Logic error: Can't view an empty RecipeCard");
            return true;
        }

        #endregion

        #region form methods
        //#TODO REFACTOR do this for all grids
        private void FitColumns()
        {
            gridView1.BestFitColumns();
            //this.gridView1.Columns["ItemDescription"].BestFit();
            //this.gridView1.Columns["UoM"].BestFit();
            this.gridView1.Columns["Qty"].BestFit();
        }

        protected void ReloadLookupLists()
        {
            itemsXPBindingSource.DataSource = new XPCollection<Item>(UnitOfWork, null, new SortProperty("Name", SortingDirection.Ascending));
        }

        #endregion
    }
}
