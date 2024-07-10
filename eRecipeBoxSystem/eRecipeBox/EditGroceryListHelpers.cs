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
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Windows.Forms;

namespace eRecipeBox
{
    //#RQT 1EdtGLFrm EditGroceryList behavior -
    //#RQT 1EdtGLFrm Enter GroceryListItem and Add to the GroceryList.
    //#RQT 1EdtGLFrm GroceryListItems can be edited inline in the grid (to modify qty, add preferred brand, etc.)
    //#RQT 1EdtGLFrm To Delete GroceryListItems, select rows (1st column) and DeleteItems.
    //#RQT 1EdtGLFrm Once complete, Email GroceryList to the email address in the user's UserProfile.
    partial class EditGroceryList
    {
        #region ctor
        public void InitializeComponent2()
        {
            this.groceryListGridView.ShownEditor += new System.EventHandler(DevExpressUxUtils.gridView_ShownEditor_SetMaxLength);

            //this is an example how to config the grid in code vs. the designer
            //GLI grid             
            this.groceryListGridView.OptionsView.ShowIndicator = false;
            this.groceryListGridView.OptionsBehavior.Editable = true;
            this.groceryListGridView.OptionsView.ShowAutoFilterRow = false; // filter at top of each row
            this.groceryListGridView.OptionsView.ShowGroupPanel = false;
            this.groceryListGridView.OptionsCustomization.AllowGroup = false;
            this.groceryListGridView.OptionsFind.AllowFindPanel = false;
            this.groceryListGridView.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Default;  //filter panel at bottom
            this.groceryListGridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            this.groceryListGridView.OptionsSelection.MultiSelect = true;
            this.groceryListGridControl.UseEmbeddedNavigator = false;

            //#RQT GLItmsSrtOrdr Sort GroceryListItems by Item so the same Items are grouped together.

            groceryListGridView.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(groceryListGridView.Columns["Item"], DevExpress.Data.ColumnSortOrder.Ascending),
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(groceryListGridView.Columns["ItemDescription"], DevExpress.Data.ColumnSortOrder.Ascending),
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(groceryListGridView.Columns["Oid"], DevExpress.Data.ColumnSortOrder.Ascending)});

            this.LastLookupEditKey = Keys.None;

            //#RQT ShCut Shortcut Keys: Alt-c (save and close), Ctl-s (save), Delete key (delete), Alt-e (email GList), Alt-l (clear all GLItems)
            DevExpressUxUtils.AssignShortcutToButton(barButtonItemSaveAndClose, Keys.Alt, Keys.C);
            DevExpressUxUtils.AssignShortcutToButton(this.barButtonItemSave, Keys.Control, Keys.S);
            DevExpressUxUtils.AssignShortcutToButton(deleteItemBarButtonItem, null, Keys.Delete);
            DevExpressUxUtils.AssignShortcutToButton(emailBarButtonItem, Keys.Alt, Keys.E);
            DevExpressUxUtils.AssignShortcutToButton(clearBarButtonItem, Keys.Alt, Keys.L);

            //#RQT GLItmInptCntns GroceryList Item LookupEdit shows matches that contain the entered text.
            SimpleCRUDFramework.DevExpressUxUtils.InitLookupEditorBusObj(this.itemLookUpEdit, itemsXPBindingSource, "Oid", "Name", DefaultBoolean.False);
            itemLookUpEdit.Properties.SortColumnIndex = 1;
            itemLookUpEdit.Properties.Columns[itemLookUpEdit.Properties.SortColumnIndex].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;

            SetAllControlNames();
            SetControlConstraints();
            DoUpdateViewState();
        }

        #endregion
        #region CRUDBusinessObjectForm
        protected override bool ReloadModel()
        {
            NewUnitOfWork();
            ConfigureSearchInput();
            FormHeadBusinessObject = DataStoreUtils.XpoService.LoadHeadBusObjByKey(typeof(GroceryList), Oid.Value, UnitOfWork);
            return FormHeadBusinessObject != null;
        }
        protected override void ModelToView()
        {
            GroceryList groceryList = (GroceryList)this.FormHeadBusinessObject;
            groceryListXPBindingSource.DataSource = groceryList;
            this.groceryListGridView.BestFitColumns();
            DoUpdateViewState();
        }
        protected override bool ViewToModel()
        {
            CloseActiveEditor();
            DevExpressUxUtils.CloseEditing(groceryListGridView);
            return true;
        }

        protected override void UpdateViewState()
        {
            if (groceryListGridView.RowCount == 0)
            {
                //#RQT GLItmsBtnEnbl Disable all grid buttons if the grid is empty.
                this.clearBarButtonItem.Enabled = false;
                this.emailBarButtonItem.Enabled = false;
                this.deleteItemBarButtonItem.Enabled = false;
            }
            else
            {
                this.clearBarButtonItem.Enabled = true;
                this.emailBarButtonItem.Enabled = true;
                //#RQT GLItmsBtnGrdFcs Only enable grid buttons when GroceryListItems grid has focus
                if (groceryListGridView.SelectedRowsCount > 0)
                    this.deleteItemBarButtonItem.Enabled = true;
                else
                    this.deleteItemBarButtonItem.Enabled = false;
            }
        }

        #endregion CRUDBusinessObjectForm
        #region searchFilterTermLookupEdit
        private void ConfigureSearchInput()
        {
            //#RQT GLItmsInptSggstLst Populate GroceryListItem LookUpEdit suggested input with all Items, however user can add anything to the GroceryList (e.g., light bulbs).
            itemsXPBindingSource.DataSource = new XPCollection<Item>(UnitOfWork, null, new SortProperty("Name", SortingDirection.Ascending));
            itemLookUpEdit.EditValue = null;
        }
        #endregion
        #region GLItem Gridview
        private void AddItemToGL(string itemDescription)
        {
            if (string.IsNullOrWhiteSpace(itemDescription))
                return;

            GroceryList groceryList = (GroceryList)this.FormHeadBusinessObject;
            GroceryListItem glItem = new GroceryListItem(UnitOfWork);
            glItem.ItemDescription = itemDescription;

            //Use a dummy ingredient to suggest the item
            Ingredient ingr = new Ingredient();
            ingr.ItemDescription = glItem.ItemDescription;
            Item item = ingr.SuggestItem((XPCollection<Item>)itemsXPBindingSource.DataSource);
            //No suggestion, set item to the itemDescription
            if (item == null)
                glItem.Item = itemDescription;
            else
                glItem.Item = item.Name;

            groceryList.GroceryListItems.Add(glItem);

        }
        private void DeleteSelectedGLItems()
        {
            GridView activeView = this.groceryListGridView;
            if (activeView == null || !activeView.IsFocusedView)
                return;
            ((System.ComponentModel.ISupportInitialize)(activeView)).BeginInit();
            Int32[] selectedRowHandles = groceryListGridView.GetSelectedRows();
            for (int i = selectedRowHandles.Length - 1; i >= 0; i--)
            {
                var selectedRowHandle = selectedRowHandles[i];
                if (selectedRowHandle >= 0)
                {
                    GroceryListItem gli = (GroceryListItem)groceryListGridView.GetRow(selectedRowHandle);
                    gli.GroceryList.DeleteGroceryListItem(gli);
                }
            }
            ((System.ComponentModel.ISupportInitialize)(activeView)).EndInit();
            UpdateViewState();
        }
        private void AutoSetItems()
        {
            //#RQT GLItmAtoStItm If Item is empty and ItemDescription has a value, set Item to the same value.
            GroceryList groceryList = (GroceryList)this.FormHeadBusinessObject;
            foreach (GroceryListItem gli in groceryList.GroceryListItems)
            {
                if (gli.Item == null && gli.ItemDescription != null)
                    gli.Item = gli.ItemDescription;
            }
        }
        private void DisplayRowCount()
        {
            //#RQT GLItmGrdDspCnt Display number of GroceryListItem rows in the status bar.
            this.statusBarStaticItem.Caption = groceryListGridView.RowCount.ToString() + " rows";
        }

        #endregion 
    }
}
