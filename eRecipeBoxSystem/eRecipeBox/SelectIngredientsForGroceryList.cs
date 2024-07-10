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
using DevExpress.XtraBars;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace eRecipeBox
{

    public partial class SelectIngredientsForGroceryList : CRUDBusinessObjectForm
    {
        #region form properties        


        //#this serves as our source... analogous to data store
        private RecipeCard[] recipes;

        //#this serves as our model while the user deletes the ones they don't need
        private BindingList<GroceryListItem> items;

        #endregion form properties

        #region ctor

        public SelectIngredientsForGroceryList(int? groceryListOid, RecipeCard[] recipes, MDIParentForm parent) : base(groceryListOid, nameof(SelectIngredientsForGroceryList), parent)
        {
            base.InitializeBaseComponent();
            InitializeComponent();
            InitializeComponent2();
            this.recipes = recipes;
        }
        public SelectIngredientsForGroceryList(int? groceryListOid, RecipeCard recipe, MDIParentForm parent) : base(groceryListOid, nameof(SelectIngredientsForGroceryList), parent)
        {
            base.InitializeBaseComponent();
            InitializeComponent();
            InitializeComponent2();
            this.recipes = new RecipeCard[] { recipe };
        }

        #endregion ctor

        #region form event handlers
        private void SelectRecipeIngForGL_Load(object sender, EventArgs e)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                ReloadModelAndView();
                this.selectIngrGridView.ClearSelection();
                EndFormLoad();
            }
        }
        #endregion form event handlers

        #region grid control event handlers        
        private void gridControl1_ProcessGridKey(object sender, KeyEventArgs e)
        {
            //#RQT ShCut Shortcut keys: Delete, Ctl-D (Delete rows in grid), Ctl-L (clear grid selection), Alt-A (Add Items to GL), Alt-C (Cancel)
            if (e.KeyCode == Keys.Delete || (e.KeyCode == Keys.D && e.Control))
            {
                DeleteSelectedGLItems();
            }

            else if ((e.KeyCode == Keys.L) && e.Control)
            {
                selectIngrGridView.ClearSelection();
            }
        }
        #endregion grid control event handlers

        #region button event handlers
        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            AddItemsToGroceryListAndClose();
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            DoHardCancel();
        }
        #endregion button event handlers

    }
}