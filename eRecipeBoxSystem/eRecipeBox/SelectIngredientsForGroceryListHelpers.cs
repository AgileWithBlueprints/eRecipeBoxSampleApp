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
using DevExpress.Utils;
using DevExpress.Xpo;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace eRecipeBox
{
    //#RQT 1SlctIng4GLFrm SelectIngredientsForGroceryList behavior - 
    //#RQT 1SlctIng4GLFrm eRecipeBox populates SelectIngredientsForGroceryList with all ingredients from all selected RecipeCards and displays them in a grid.
    //#RQT 1SlctIng4GLFrm User removes items not needed.
    //#RQT 1SlctIng4GLFrm User clicks Add Items to Grocery List. 
    partial class SelectIngredientsForGroceryList
    {
        #region CRUDBusinessObjectForm methods
        private void InitializeComponent2()
        {
            DevExpressUxUtils.AssignShortcutToButton(cancelBarButtonItem, Keys.Alt, Keys.C);
            DevExpressUxUtils.AssignShortcutToButton(addIngrToGLbarButtonItem, Keys.Alt, Keys.A);

            //#RQT GrdMltSlct Ingredients grid is read only, multiselect rows.
            this.selectIngrGridView.OptionsSelection.MultiSelect = true;
            this.selectIngrGridView.OptionsView.ShowIndicator = false;
            AutoValidate = AutoValidate.Disable;

            SetAllControlNames();
        }

        protected override bool ReloadModel()
        {
            NewUnitOfWork();
            IList<GroceryListItem> glis = new List<GroceryListItem>();
            foreach (RecipeCard recipe in recipes)
            {
                foreach (Ingredient ingr in recipe.Ingredients)
                {
                    //#RQT IngIgnrSctnHdr Do not add section headers (eg, Spices: ) to the GroceryList.
                    if (ingr.IsSectionHeader && ingr.Item == null)
                        continue;
                    GroceryListItem gli = new GroceryListItem(UnitOfWork);
                    gli.Qty = ingr.Qty;
                    gli.UoM = ingr.UoM;
                    gli.ItemDescription = ingr.ItemDescription;
                    gli.Item = ingr.Item?.Name;
                    //#RQT ItmStIfNull If Item is null, set it to ItemDescription since grid is sorted by Item.
                    if (gli.ItemDescription != null && string.IsNullOrEmpty(gli.Item))
                    {
                        //swap em
                        gli.Item = gli.ItemDescription;
                        gli.ItemDescription = null;
                    }
                    glis.Add(gli);
                }
            }
            //#RQT GrdSrtByItm Sort Ingredients by Item so Items are grouped together.
            items = new BindingList<GroceryListItem>(glis.OrderBy(x => x.Item).ThenBy(y => y.ItemDescription).ThenBy(z => z.Oid).ToList());
            return true;
        }

        protected override void ModelToView()
        {
            this.selectIngrGridControl.DataSource = items;
            this.selectIngrGridView.PopulateColumns();

            //cols           
            foreach (GridColumn col in this.selectIngrGridView.Columns)
            {
                col.Visible = false;
            }
            this.selectIngrGridView.Columns["Qty"].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            this.selectIngrGridView.Columns["Qty"].Visible = true;
            this.selectIngrGridView.Columns["UoM"].Visible = true;
            this.selectIngrGridView.Columns["ItemDescription"].Visible = true;
            this.selectIngrGridView.Columns["Item"].Visible = true;
            this.selectIngrGridView.OptionsBehavior.Editable = false;
            this.selectIngrGridView.OptionsView.ShowAutoFilterRow = false; // filter at top of each row
            this.selectIngrGridView.OptionsView.ShowGroupPanel = false;
            this.selectIngrGridView.OptionsCustomization.AllowGroup = false;
            this.selectIngrGridView.OptionsFind.AllowFindPanel = true;
            this.selectIngrGridView.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Default;  //filter panel at bottom
            this.selectIngrGridControl.UseEmbeddedNavigator = false;
            this.selectIngrGridView.BestFitColumns();
        }

        private void DeleteSelectedGLItems()
        {
            GridView activeView = this.selectIngrGridView;
            if (activeView == null || !activeView.IsFocusedView)
                return;
            ((System.ComponentModel.ISupportInitialize)(activeView)).BeginInit();
            Int32[] selectedRowHandles = selectIngrGridView.GetSelectedRows();
            for (int i = selectedRowHandles.Length - 1; i >= 0; i--)
            {
                int selectedRowHandle = selectedRowHandles[i];
                if (selectedRowHandle >= 0)
                {
                    GroceryListItem gli = (GroceryListItem)selectIngrGridView.GetRow(selectedRowHandle);
                    items.Remove(gli);
                    gli.Delete();
                }
            }
            ((System.ComponentModel.ISupportInitialize)(activeView)).EndInit();
            UpdateViewState();
        }

        #endregion 

        #region form helpers
        private void AddItemsToGroceryListAndClose()
        {
            try
            {
                UnitOfWork uow = new UnitOfWork();
                FormHeadBusinessObject = XpoService.LoadHeadBusObjByKey(typeof(GroceryList), Oid.Value, uow);
                GroceryList groceryList = (GroceryList)FormHeadBusinessObject;
                foreach (GroceryListItem gli in items)
                {
                    GroceryListItem newGLI = new GroceryListItem(uow);
                    newGLI.Qty = gli.Qty;
                    newGLI.UoM = gli.UoM;
                    newGLI.ItemDescription = gli.ItemDescription;
                    newGLI.Item = gli.Item;
                    groceryList.AddGroceryListItem(newGLI);
                }
                this.UnitOfWork = uow;
                DoSaveAndClose(promptToSaveChanges: false);
            }
            catch (Exception ex)
            {
                ShowMessageBox($"Add to GroceryList error: {ex.Message}", "Fatal Error");
            }
        }
        #endregion 
    }
}
