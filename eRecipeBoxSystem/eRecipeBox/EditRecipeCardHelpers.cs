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
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Windows.Forms;
namespace eRecipeBox
{
    //#RQT 1EdtRcpCrdFrm EditRecipeCard behavior - 
    //#RQT 1EdtRcpCrdFrm User creates/imports/modifies/deletes a single RecipeCard with this form.
    //#RQT 1EdtRcpCrdFrm Users can only import into a new, empty RecipeCard.
    partial class EditRecipeCard
    {

        #region form methods

        private void InitializeComponent2()
        {

            this.ingredientsGridView.ShownEditor += new System.EventHandler(DevExpressUxUtils.gridView_ShownEditor_SetMaxLength);
            this.gridView2.ShownEditor += new System.EventHandler(DevExpressUxUtils.gridView_ShownEditor_SetMaxLength);
            this.gridView3.ShownEditor += new System.EventHandler(DevExpressUxUtils.gridView_ShownEditor_SetMaxLength);
            this.myRatingSpinEdit.Properties.MinValue = RecipeCard.MyRatingMinValue;
            this.myRatingSpinEdit.Properties.MaxValue = RecipeCard.MyRatingMaxValue;
            this.ratingCountTextEdit.Properties.MinValue = RecipeCard.RatingCountMinValue;

            //Hyperlink Title with URL 
            //#RQT TtlTxtHyptxt Title is displayed either as a readonly text label or readonly hyperlink, depending whether there is a URL.  To edit these, user clicks "Edit Title" button.  System presents a pop up dialog to set both the title and URL.
            this.titleTextEdit.ReadOnly = true;

            //#IMPORTANT Note: Set data bindings option for textedits and memoedits to OnPropertyChanged
            //If user types text and hits save without closing the textEditor.  Changes will be saved.

            //#RQT ShCUT Shortcut keys - Alt-c (save and close), Ctl-s (save), Alt-v (view recipeCard), Alt-i (import recipeCard), Alt-s (schedule recipe), Alt-r (refresh), Insert (insert row in grid), Ctl-d or Ctl-delete (delete row in grid), Alt-g (open groceryList), Alt-a (add ingredients to GList)
            this.newRowBarButtonItem.ShortcutKeyDisplayString = "Insert";
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(barButtonSaveAndClose, Keys.Alt, Keys.C); //Ctl-C is copy to clipboard.  Use alt instead
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(barButtonSave, Keys.Control, Keys.S);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(importRecipeBarButtonItem, Keys.Alt, Keys.I);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(deleteRowBarButtonItem, Keys.Control, Keys.Delete);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(deleteRowBarButtonItem, Keys.Control, Keys.D);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(viewRecipeBarButtonItem, Keys.Alt, Keys.V);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(reloadBarButtonItem, Keys.Alt, Keys.R);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(newRowBarButtonItem, null, Keys.Insert);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(editGroceryListBarButtonItem, Keys.Alt, Keys.G);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(addIngToGLButtonItem, Keys.Alt, Keys.A);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(scheduleBarButtonItem, Keys.Alt, Keys.S);

            //Set HorzAlignment.Near
            DevExpressUxUtils.SetTextOptionsForAllTextEditsInGroup(this.layoutControlGroup2, hAlignment: HorzAlignment.Near);
            DevExpressUxUtils.SetTextOptionsForAllTextEditsInGroup(this.layoutControlGroup3, hAlignment: HorzAlignment.Near);
            DevExpressUxUtils.SetTextOptionsForAllTextEditsInGroup(this.layoutControlGroup4, hAlignment: HorzAlignment.Near);

            DevExpressUxUtils.InitializeLookUpEdit(keywordLookUpEdit);

            //Initialize Ingredients grid
            ((System.ComponentModel.ISupportInitialize)(this.ingredientsGridView)).BeginInit();

            //cols
            this.ingredientsGridView.Columns["Qty"].AppearanceCell.Options.UseTextOptions = true;
            this.ingredientsGridView.Columns["Qty"].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            //disable sorting
            foreach (GridColumn col in this.ingredientsGridView.Columns)
                col.OptionsColumn.AllowSort = DefaultBoolean.False;

            //Sort the items in the collection https://supportcenter.devexpress.com/ticket/details/t218041/how-can-i-sort-lookupedit-from-code/
            RepositoryItemLookUpEdit lookupEditItems = new RepositoryItemLookUpEdit();
            DevExpressUxUtils.InitializeRepositoryItemLookUpEdit(lookupEditItems, itemLookUpEdit_ProcessNewValue);
            lookupEditItems.DataSource = itemsXPBindingSource;
            lookupEditItems.DisplayMember = nameof(Item.Name);
            lookupEditItems.KeyMember = nameof(Item.Oid);
            lookupEditItems.ValueMember = nameof(Item.Oid);
            ingredientsGridControl.RepositoryItems.Add(lookupEditItems);
            ingredientsGridView.Columns["Item!Key"].ColumnEdit = lookupEditItems;

            this.ingredientsGridView.OptionsView.ShowIndicator = false;
            this.ingredientsGridView.OptionsBehavior.Editable = true;
            //#RQT IngMltSlct Ingredients grid supports multiple row selection.
            this.ingredientsGridView.FocusRectStyle = DrawFocusRectStyle.CellFocus;
            this.ingredientsGridView.OptionsSelection.MultiSelect = true;
            this.ingredientsGridView.OptionsView.ShowAutoFilterRow = false;
            this.ingredientsGridView.OptionsView.ShowGroupPanel = false;
            this.ingredientsGridView.OptionsCustomization.AllowGroup = false;
            this.ingredientsGridView.OptionsFind.AllowFindPanel = true;
            this.ingredientsGridView.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Default;  //filter panel at bottom
            this.ingredientsGridControl.UseEmbeddedNavigator = false;
            ((System.ComponentModel.ISupportInitialize)(this.ingredientsGridView)).EndInit();


            //Initialize keywords lookupedit
            SimpleCRUDFramework.DevExpressUxUtils.InitLookupEditorBusObj(this.keywordLookUpEdit, this.keywordsXPBindingSource, "Oid", "Name", DefaultBoolean.False);

            //Initialize Keywords grid
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            //#NOTE #TIP sorting causes problems for many to many when calling remove. so bind to a grid and sort column within the grid
            this.gridView2.OptionsView.ShowIndicator = false;
            this.gridView2.OptionsView.ShowAutoFilterRow = false; // filter at top of each row
            this.gridView2.OptionsView.ShowGroupPanel = false;
            this.gridView2.OptionsCustomization.AllowGroup = false;
            this.gridView2.OptionsFind.AllowFindPanel = true;
            //#RQT KywdDsplyAsc Display Keywords in ascending order.
            this.gridView2.Columns["Name"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            this.gridView2.Columns["Name"].OptionsColumn.AllowEdit = false;
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();

            //Initialize Cooked dates grid
            RepositoryItemDateEdit de = new RepositoryItemDateEdit();
            cookedDatesGridControl.RepositoryItems.Add(de);
            ((System.ComponentModel.ISupportInitialize)(this.gridView3)).BeginInit();
            this.gridView3.OptionsView.ShowIndicator = false;
            //#RQT CkdDtDsplyDsc Display CookedDates in descending order.
            this.gridView3.Columns["CookedDate"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
            this.gridView3.Columns["CookedDate"].OptionsColumn.AllowEdit = true;
            this.gridView3.Columns["CookedDate"].ColumnEdit = de;
            this.gridView3.OptionsView.ShowAutoFilterRow = false; // filter at top of each row
            this.gridView3.OptionsView.ShowGroupPanel = false;
            this.gridView3.OptionsCustomization.AllowGroup = false;
            this.gridView3.OptionsFind.AllowFindPanel = true;
            ((System.ComponentModel.ISupportInitialize)(this.gridView3)).EndInit();

            //Connect the form's error provider datasource to our bindingsource
            dxErrorProvider1.DataSource = this.recipeXPBindingSource;

            SetAllControlNames();
            SetControlConstraints();
            UpdateViewState();
        }

        protected void ReloadItemsList(UnitOfWork uow)
        {
            itemsXPBindingSource.DataSource = new XPCollection<Item>(uow, null, new SortProperty("Name", SortingDirection.Ascending));
        }

        protected void ReloadKeywordsList(UnitOfWork uow)
        {
            keywordsXPBindingSource.DataSource = new XPCollection<Keyword>(uow, null, new SortProperty("Name", SortingDirection.Ascending));
        }

        protected void ReloadLookupLists(UnitOfWork uow)
        {
            ReloadItemsList(uow);
            ReloadKeywordsList(uow);
        }
        protected void ResetSortOrders()
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            //loop through ingredients and set the sortOrder to match the order in the grid
            int sortIdx = 0;
            for (int i = 0; i < ingredientsGridView.DataRowCount; i++)
            {
                Ingredient ing = (Ingredient)ingredientsGridView.GetRow(i);
                ing.SortOrder = sortIdx;
                sortIdx++;
            }
            //Loop through and verify
            sortIdx = 0;
            for (int i = 0; i < ingredientsGridView.DataRowCount; i++)
            {
                int test = (int)ingredientsGridView.GetRowCellValue(i, "SortOrder");
                if (sortIdx != test)
                    throw new Exception("dup order");
                sortIdx++;
            }
        }

        //Allow user to create a new Item within the gridview.
        private void itemLookUpEdit_ProcessNewValue(object sender, DevExpress.XtraEditors.Controls.ProcessNewValueEventArgs e)
        {
            LookUpEdit lookUpRI = (LookUpEdit)sender;
            int focusedRow = this.ingredientsGridView.FocusedRowHandle;
            Ingredient ing = (Ingredient)ingredientsGridView.GetRow(focusedRow);
            if (ing == null)
                return;
            if ((string)e.DisplayValue == String.Empty)
                return;

            //Remember the new item
            string newItemName = (string)e.DisplayValue;

            //clear the cell because we need to first save the RecipeCard before adding Item
            lookUpRI.EditValue = null;
            e.DisplayValue = null;
            e.Handled = true;

            if (ShowMessageBox(this, "Add new item? " + newItemName,
                "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            //#TRICKY. Need to first save RC. Will fail if ItemDescription is null.
            ing.ItemDescription = newItemName;

            //#RQT ItmCrNwSvRC Allow new Items to be added to lookupedit.  When adding new Item, first autosave the RecipeCard (without the new Item), then add new Item, then add reference to the new Item.
            bool saved = DoSave(false);
            if (!saved)
            {
                //Things went wrong.  have the user save the form before trying to add the new item.
                ShowMessageBox("Unable to save the RecipeCard before adding new Item.", "Save RecipeCard Error");
                return;
            }
            try
            {
                Item newi = new Item(UnitOfWork);
                newi.Name = newItemName;
                ((XPCollection<Item>)itemsXPBindingSource.DataSource).Add(newi);

                //need to commit so we obtain an oid for the new Item (HeadBusinessObject)
                DataStoreUtils.XpoService.CommitBodyOfBusObjs(newi, UnitOfWork);

                //Set the ingredient item to the new Item
                ing.Item = newi;
            }
            catch (Exception ex)
            {
                Foundation.Log.App.Info(ex.Message);
                //Things went wrong.  have the user save the form before trying to add the new item.
                ShowMessageBox("Unexpected error creating new Item.", "Error");
            }
        }
        #endregion

        #region EditBusObjForm methods

        protected override bool ReloadModel()
        {
            NewUnitOfWork();
            ReloadLookupLists(UnitOfWork);
            if (this.Oid.HasValue)
            {
                FormHeadBusinessObject = DataStoreUtils.XpoService.LoadHeadBusObjByKey(typeof(RecipeCard), Oid.Value, UnitOfWork);
                if (this.FormHeadBusinessObject == null)
                    return false;
            }
            else
            {
                int rbID = DataStoreServiceReference.DataStoreServiceReference.MyRecipeBoxOid;
                RecipeBox myRecipeBox = (RecipeBox)DataStoreUtils.XpoService.LoadHeadBusObjByKey(typeof(RecipeBox), rbID, UnitOfWork);
                RecipeCard rec = myRecipeBox.CreateNewRecipeCard(UnitOfWork);
                this.FormHeadBusinessObject = rec;
            }
            return true;
        }
        protected override void ModelToView()
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;

            //Tricky combination.  errorprovider is connected to titletext, so always show it when title is empty
            //if no url or title, show just the textEdit.
            //in the case of null title, we want to show the error provider 
            if (recipe.SourceURL == null || recipe.Title == null)
            {
                this.ItemForTitle.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                this.ItemForTitleText.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            }
            else
            {
                this.ItemForTitle.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                this.ItemForTitleText.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            }

            //need to manually set the caption for the hyperlink
            this.titleHyperlinkEdit.Properties.Caption = recipe.Title;

            recipeXPBindingSource.DataSource = recipe;
            //#RQT IngBstFtCls Auto-best fit columns in Ingredients grid.
            this.ingredientsGridView.BestFitColumns();

            DoUpdateViewState();
        }
        protected override bool ViewToModel()
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;

            //#WORKAROUND #TODO Report For some reason, this is bound but binding doesn't work, so we view to model it here again. ask DevEx for advice?
            recipe.WouldLikeToTryFlag = this.flagCheckEdit.Checked;

            //#TODO REFACTOR add all active grid CloseEditing to CloseActiveEditor
            this.CloseActiveEditor();
            DevExpressUxUtils.CloseEditing(this.ingredientsGridView);
            DevExpressUxUtils.CloseEditing(this.gridView3);

            ResetSortOrders();

            this.keywordLookUpEdit.EditValue = null;

            recipe.SuggestIngredientItems((XPCollection<Item>)itemsXPBindingSource.DataSource);

            DoUpdateViewState();
            //also clear cooked date input?
            return true;
        }
        protected override bool ProcessDataStoreException(Exception ex)
        {
            bool processed = base.ProcessDataStoreException(ex);
            //if Title has an error, we need to show the ItemForTitleText, so the error message is shown
            if (((RecipeCard)this.FormHeadBusinessObject).DataStoreSavingErrors.ContainsKey(nameof(RecipeCard.Title)))
            {
                this.ItemForTitle.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                this.ItemForTitleText.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            }
            return processed;
        }
        protected override void UpdateViewState()
        {
            //#RQT BtnImprtEnbl Enable import buttons only when creating a new RecipeCard.  This prevents accidently importing on top of an existing RecipeCard.
            if (this.Oid == null)
            {
                this.importRecipeBarButtonItem.Enabled = true;
                this.viewRecipeBarButtonItem.Enabled = false;
            }
            else
            {
                this.importRecipeBarButtonItem.Enabled = false;
                this.viewRecipeBarButtonItem.Enabled = true;
            }

            //#RQT GrdBtnsEnbl Enable grid buttons (insert, move up/down, etc.) only when grid has focus.
            GridView focusedGridView = null;
            if (this.ingredientsGridView.IsFocusedView)
                focusedGridView = this.ingredientsGridView;
            else if (this.gridView2.IsFocusedView)
                focusedGridView = this.gridView2;
            else if (this.gridView3.IsFocusedView)
                focusedGridView = this.gridView3;
            if (focusedGridView != null)
            {
                int focusedRow = focusedGridView.FocusedRowHandle;
                if (focusedRow >= 0 && focusedGridView.RowCount > 0)
                    this.deleteRowBarButtonItem.Enabled = true;

                //#RQT GrdUpDnBtnsEnbl Up/Down buttons only valid for ingredients
                if (this.ingredientsGridView.IsFocusedView)
                {
                    if (focusedRow == 0)
                        this.moveUpBarButtonItem.Enabled = false;
                    else
                        this.moveUpBarButtonItem.Enabled = true;

                    if (focusedRow == ingredientsGridView.RowCount - 1)
                        this.moveDownBarButtonItem.Enabled = false;
                    else
                        this.moveDownBarButtonItem.Enabled = true;

                    this.newRowBarButtonItem.Enabled = true;
                }
            }
            else
            {
                this.newRowBarButtonItem.Enabled = false;
                this.moveUpBarButtonItem.Enabled = false;
                this.moveDownBarButtonItem.Enabled = false;
                this.deleteRowBarButtonItem.Enabled = false;
            }
        }
        #endregion

        #region grid helpers
        private void MoveRowUp(GridView gridView)
        {
            if (!gridView.IsFocusedView)
                return;

            int focusedRow = gridView.FocusedRowHandle;

            //can't move top row up
            if (focusedRow == 0)
                return;

            Ingredient focusedIng = (Ingredient)gridView.GetRow(focusedRow);
            Ingredient previousIng = (Ingredient)gridView.GetRow(focusedRow - 1);
            gridView.BeginSort();
            //swap their sort orders
            int save = focusedIng.SortOrder;
            focusedIng.SortOrder = previousIng.SortOrder;
            previousIng.SortOrder = save;

            RecipeCard recipe = (RecipeCard)FormHeadBusinessObject;
            //force a resort
            recipe.Ingredients.Sorting = new SortingCollection(new SortProperty("SortOrder", SortingDirection.Ascending));
            //place cursor on the new row
            gridView.FocusedRowHandle = focusedRow - 1;
            gridView.EndSort();

            UpdateViewState();
        }

        private void MoveRowDown(GridView gridView)
        {
            if (!gridView.IsFocusedView)
                return;

            int focusedRow = gridView.FocusedRowHandle;

            DevExpress.XtraGrid.Views.Grid.ViewInfo.GridViewInfo info = gridView.GetViewInfo() as DevExpress.XtraGrid.Views.Grid.ViewInfo.GridViewInfo;

            //can't move bottom row down
            if (focusedRow >= info.RowsInfo.Count - 1)
                return;

            Ingredient focusedIng = (Ingredient)gridView.GetRow(focusedRow);
            Ingredient nextIng = (Ingredient)gridView.GetRow(focusedRow + 1);

            gridView.BeginSort();

            //swap their sort orders
            int save = focusedIng.SortOrder;
            focusedIng.SortOrder = nextIng.SortOrder;
            nextIng.SortOrder = save;

            RecipeCard recipe = (RecipeCard)FormHeadBusinessObject;
            //force a resort
            recipe.Ingredients.Sorting = new SortingCollection(new SortProperty("SortOrder", SortingDirection.Ascending));
            //place cursor on the new row
            gridView.FocusedRowHandle = focusedRow + 1;

            gridView.EndSort();

            UpdateViewState();
        }

        private void DeleteFocusedCookedDate(GridView activeView)
        {
            if (activeView == null || !activeView.IsFocusedView)
                return;
            int focusedRow = activeView.FocusedRowHandle;
            CookedDish selectedCookedDish = (CookedDish)activeView.GetRow(focusedRow);
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            recipe.DeleteCookedDish(selectedCookedDish);
        }

        private void RemoveFocusedKeyword(GridView activeView)
        {
            if (activeView == null || !activeView.IsFocusedView)
                return;
            int focusedRow = activeView.FocusedRowHandle;
            Keyword selKeyword = (Keyword)activeView.GetRow(focusedRow);
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            recipe.RemoveKeyword(selKeyword);
        }

        private void DeleteFocusedIngredientRow(GridView activeView)
        {
            if (activeView == null || !activeView.IsFocusedView)
                return;
            int focusedRow = activeView.FocusedRowHandle;
            if (focusedRow < 0)
                return;
            var ingredient = (Ingredient)ingredientsGridView.GetRow(focusedRow);
            ingredient.RecipeCard.DeleteIngredient(ingredient);
        }

        //Insert row in grid at point of current focused row.
        //Design: Append the row to the model, set the SortOrder to the inserted index (adjust others accordingly)
        //and resort the view
        private void InsertFocusedIngredientRow()
        {
            if (!this.ingredientsGridView.IsFocusedView)
                return;

            ingredientsGridView.BeginDataUpdate();
            int focusedIngSortOrder = 0;
            if (this.ingredientsGridView.RowCount > 0)
            {
                int focusedRow = this.ingredientsGridView.FocusedRowHandle;
                focusedIngSortOrder = ((Ingredient)this.ingredientsGridView.GetRow(focusedRow)).SortOrder;
                //reset the sortorder making room for the new ingredient
                int sortIdx = focusedIngSortOrder + 1;
                for (int i = 0; i < ingredientsGridView.DataRowCount; i++)
                {
                    Ingredient ing = (Ingredient)this.ingredientsGridView.GetRow(i);
                    int ingSortOrder = ing.SortOrder;
                    if (ingSortOrder >= focusedIngSortOrder)
                    {
                        ing.SortOrder = sortIdx;
                        sortIdx++;
                    }
                }
            }
            Ingredient newIng = new Ingredient(UnitOfWork);
            //Append the new Ingredient to the model 
            newIng.SortOrder = focusedIngSortOrder;
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            recipe.Ingredients.Add(newIng);
            //forces a resort
            recipe.Ingredients.Sorting = new SortingCollection(new SortProperty("SortOrder", SortingDirection.Ascending));
            ingredientsGridView.EndDataUpdate();

            //place cursor on the new row
            ingredientsGridView.FocusedRowHandle = focusedIngSortOrder;
            //deselect all rows.
            ingredientsGridView.ClearSelection();
            //select the inserted row
            ingredientsGridView.SelectRow(focusedIngSortOrder);

            ResetSortOrders();
        }

        #endregion     
    }
}
