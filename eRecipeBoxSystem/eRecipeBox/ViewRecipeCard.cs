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
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Windows.Forms;

namespace eRecipeBox
{
    public partial class ViewRecipeCard : CRUDBusinessObjectForm
    {
        #region ctor
        public ViewRecipeCard(int? Oid, MDIParentForm parent) : base(Oid, nameof(ViewRecipeCard), parent)
        {
            if (!this.Oid.HasValue)
                throw new Exception("Logic error: Can't view an empty RecipeCard");
            base.InitializeBaseComponent();
            InitializeComponent();
            InitializeComponent2();
        }

        #endregion ctor

        #region form event handlers
        private void ViewRecipe_Load(object sender, EventArgs e)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                ReloadModelAndView();
                RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
                //#AtmtTstng Test needs to know which RecipeCard is being edited.
                this.Text = $"ViewRecipeCard: {recipe.Title}";
                EndFormLoad();
            }
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            FitColumns();
        }

        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            FitColumns();
        }
        #endregion form event handlers

        #region  titleHyperLinkEdit event handlers
        //#TODO REFACTOR create custom control with this behavior 
        //#TIP.  somewhat convoluded way to make hyperlink behave as one would expect - no prompt if there is no url
        //https://supportcenter.devexpress.com/ticket/details/t413528/hyperlinkedit-how-to-not-display-as-hyperlink-when-editvalue-is-null
        private void titleHyperLinkEdit_CustomDisplayText(object sender, CustomDisplayTextEventArgs e)
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            if (recipe == null) return;
            HyperLinkEdit hle = (HyperLinkEdit)sender;
            bool isEmptyVal = string.IsNullOrWhiteSpace((string)e.Value);
            hle.Properties.Appearance.Options.UseFont = isEmptyVal;
            hle.Properties.Appearance.Options.UseForeColor = isEmptyVal;
            e.DisplayText = recipe.Title;  //#TIP for some reason the caption is set to null when we get here.
        }

        #endregion titleHyperLinkEdit event handlers

        #region Gridview1 event handlers

        //#RQT IngSctnHdrBld Bold section headers, indicated by ItemDescription that ends in a colon ':' and other fields empty.
        private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            Ingredient selIngredient = (Ingredient)this.gridView1.GetRow(e.RowHandle);
            if (selIngredient.IsSectionHeader)
            {
                e.Appearance.FontStyleDelta = System.Drawing.FontStyle.Bold;
            }
        }
        #endregion Gridview1 event handlers

        #region button event handlers
        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            DoSaveAndClose();
        }

        //Edit Recipe. Go from view recipe to edit recipe.  Close ViewRecipeCard(me) and set next form to EditRecipeCard
        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.NextFormToOpen = nameof(EditRecipeCard);
            DoSaveAndClose();
        }

        private void scheduleBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            SimpleCRUDFramework.HelperForms.EditDateDialog edd = new SimpleCRUDFramework.HelperForms.EditDateDialog("Schedule RecipeCard", "Date", null);
            DialogResult dr = edd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                CookedDish newCooked = new CookedDish(this.UnitOfWork);
                newCooked.CookedDate = (DateTime)edd.Date;
                recipe.AddCookedDish(newCooked);
                DoSave();
            }
        }
        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            DoDeleteAndClose();
        }

        //Refresh
        private void barButtonItem1_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ReloadModelAndView();
        }
        private void barButtonItem4_ItemClick(object sender, ItemClickEventArgs e)
        {
            int? glOid = DataStoreServiceReference.DataStoreServiceReference.MyGroceryListOid;
            var form = new EditGroceryList(glOid, this.MDIParentForm);
            this.MDIParentForm.FormStack.PushStackAndShowDialogForm(form);
        }
        private void barButtonItem5_ItemClick(object sender, ItemClickEventArgs e)
        {
            int? glOid = DataStoreServiceReference.DataStoreServiceReference.MyGroceryListOid;

            //Prompt user to select which ingredients to add to the GL, then immediately show EditGL form
            SelectIngredientsForGroceryList form = new SelectIngredientsForGroceryList(glOid, (RecipeCard)this.FormHeadBusinessObject, this.MDIParentForm);
            this.MDIParentForm.FormStack.PushStackAndShowDialogForm(form, formClosedAction: () =>
             {
                 if (form.DialogResult == DialogResult.OK)
                 {
                     form.Dispose();
                     form = null;
                     EditGroceryList eglForm = new EditGroceryList(glOid, this.MDIParentForm);
                     this.MDIParentForm.FormStack.PushStackAndShowDialogForm(eglForm);
                 }
             });
        }
        #endregion button event handlers

    }
}