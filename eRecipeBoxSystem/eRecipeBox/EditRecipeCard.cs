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
using DevExpress.Xpo;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using eRecipeBox.HelperForms;
using Foundation;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Windows.Forms;

namespace eRecipeBox
{
    public partial class EditRecipeCard : CRUDBusinessObjectForm
    {

        #region ctor  

        public EditRecipeCard(int? Oid, MDIParentForm parent) : base(Oid, nameof(EditRecipeCard), parent)
        {
            base.InitializeBaseComponent();
            InitializeComponent();
            InitializeComponent2();
        }
        #endregion

        #region form event handlers
        private void EditRecipeCard_Load(object sender, EventArgs e)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                ReloadModelAndView();
                RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
                //#AtmtTstng Test needs to know which RecipeCard is being edited so display it the form title
                this.Text = $"EditRecipeCard: {recipe.Title}";
                EndFormLoad();
            }
        }
        #endregion

        #region ingredients gridview event handlers
        private void gridView1_GotFocus(object sender, EventArgs e)
        {
            DoUpdateViewState();
        }

        private void gridView1_LostFocus(object sender, EventArgs e)
        {
            DoUpdateViewState();
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DoUpdateViewState();
        }

        //#RQT IngSctnHdrBld Bold section headers, indicated by ItemDescription that ends in a colon ':' and other fields empty (eg, Toppings: Spices: Dressing: )
        private void gridView1_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            Ingredient selIngredient = (Ingredient)this.ingredientsGridView.GetRow(e.RowHandle);
            if (selIngredient.IsSectionHeader)
            {
                e.Appearance.FontStyleDelta = System.Drawing.FontStyle.Bold;
            }
        }
        #endregion

        #region keywords event handlers
        private void gridView2_GotFocus(object sender, EventArgs e)
        {
            DoUpdateViewState();
        }

        private void gridView2_LostFocus(object sender, EventArgs e)
        {
            DoUpdateViewState();
        }
        #endregion gridview2 

        #region cookedDates event handlers
        private void gridView3_GotFocus(object sender, EventArgs e)
        {
            DoUpdateViewState();
        }

        private void gridView3_LostFocus(object sender, EventArgs e)
        {
            DoUpdateViewState();
        }
        #endregion

        #region keywordlookup event handlers
        private void keywordLookUpEdit_ProcessNewValue(object sender, DevExpress.XtraEditors.Controls.ProcessNewValueEventArgs e)
        {
            LookUpEdit lookUpEdit = (LookUpEdit)sender;
            if ((string)e.DisplayValue == String.Empty)
                return;

            //Remember the new item
            string newKeywordName = (string)e.DisplayValue;

            //https://supportcenter.devexpress.com/ticket/details/t387516/how-to-clear-an-edit-value-in-lookupedit#
            lookUpEdit.EditValue = null;
            lookUpEdit.Text = null;
            e.DisplayValue = null;  //this seems to be only way to clear it.
            e.Handled = true;

            //#RQT KywdAdNw Allow users to add new Keywords by typing a new term into the Keyword lookupedit.  To ensure proper business object consistency, force saving current RecipeCard before adding a new Keyword 
            if (ShowMessageBox(this.MdiParent, "Add new keyword? " + newKeywordName,
                    "Confirm New Keyword", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            bool saved = DoSave(false);
            if (!saved)
            {
                //Things went wrong.  have the user save the form before trying to add the new keyword.
                ShowMessageBox("Unable to save the RecipeCard before adding new Keyword.", "Save RecipeCard Error");
                return;
            }
            try
            {
                Keyword newk = new Keyword(UnitOfWork);
                newk.Name = newKeywordName;
                ((XPCollection<Keyword>)keywordsXPBindingSource.DataSource).Add(newk);

                //need to commit so we obtain an oid for the new Keyword (HeadBusinessObject)
                XpoService.CommitBodyOfBusObjs(newk, UnitOfWork);

                //add to the model
                RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
                recipe.Keywords.Add(newk);
            }
            catch
            {
                //Things went wrong.  have the user save the form before trying to add the new Keyword.
                ShowMessageBox("Unexpected error creating new Keyword.", "Error");
            }
        }
        #endregion

        #region button event handlers
        private void barButtonSave_ItemClick(object sender, ItemClickEventArgs e)
        {
            DoSave();
        }
        private void barButtonItemCancel_ItemClick(object sender, ItemClickEventArgs e)
        {
            DoCancel();
        }

        private void barButtonSaveAndClose_ItemClick(object sender, ItemClickEventArgs e)
        {
            DoSaveAndClose();
        }

        private void deleteRecipeBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            DoDeleteAndClose();
        }

        private void reloadBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            ReloadModelAndView();
        }

        private void moveUpBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            MoveRowUp(this.ingredientsGridView);
        }

        private void moveDownBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            MoveRowDown(this.ingredientsGridView);
        }

        private void deleteRowBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.ingredientsGridView.IsFocusedView)
                DeleteFocusedIngredientRow(ingredientsGridView);
            else if (this.gridView2.IsFocusedView)
                RemoveFocusedKeyword(gridView2);
            else if (this.gridView3.IsFocusedView)
                DeleteFocusedCookedDate(gridView3);

            UpdateViewState();
        }

        private void newRowBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.ingredientsGridView.IsFocusedView)
                InsertFocusedIngredientRow();
        }

        private void addKeywordSimpleButton_Click(object sender, EventArgs e)
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            int? keywordOid = (int?)this.keywordLookUpEdit.EditValue;
            this.keywordLookUpEdit.EditValue = null; //clear
            this.keywordLookUpEdit.Focus();
            if (keywordOid == null)
                return;
            Keyword selectedKey = ((XPCollection<Keyword>)this.keywordsXPBindingSource.DataSource).Lookup(keywordOid);
            recipe.AddKeyword(selectedKey);
        }

        private void importRecipeBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            ImportRecipeCardPrompt importRecipeCardPrompt = new ImportRecipeCardPrompt();
            importRecipeCardPrompt.ShowDialog();

            //#RQT ImprtTxtRC Parse RC text and import it into a new RC
            if (importRecipeCardPrompt.ItemClicked == ImportRecipeCardPrompt.Items.ImportText)
            {
                ImportRecipeCard ir = new ImportRecipeCard(null, this.MDIParentForm);
                this.MDIParentForm.FormStack.PushStackAndShowDialogForm(ir, () =>
                {
                    if (ir.DialogResult == DialogResult.OK)
                    {
                        //treat the import form as a super typist for us.  
                        //the import form creates a uow and populates its RecipeCard
                        //they pass these to our form.  just bind this new model to my view
                        this.UnitOfWork = ir.UnitOfWork;
                        ReloadLookupLists(UnitOfWork);
                        this.FormHeadBusinessObject = ir.FormHeadBusinessObject;
                        this.Oid = ir.Oid;
                        ((RecipeCard)this.FormHeadBusinessObject).SuggestIngredientItems((XPCollection<Item>)itemsXPBindingSource.DataSource);

                        ModelToView();

                        //#TODO ENHANCEMENT Allow user to unparse the RC back to the ImportRecipeCard, fix tabbing and reimport.
                    }
                });
            }
            //#RQT ImprtAllRcps Prompt user for a recipe URL in AllRecipes.com and automatically import it.
            else if (importRecipeCardPrompt.ItemClicked == ImportRecipeCardPrompt.Items.ImportAllRecipes)
            {
                SimpleCRUDFramework.HelperForms.EditTextDialog etd = new SimpleCRUDFramework.HelperForms.EditTextDialog("Import Allrecipes.com", "URL");
                DialogResult dr = etd.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    if (!Uri.IsWellFormedUriString(etd.Text, UriKind.RelativeOrAbsolute))
                    {
                        this.ShowMessageBox("Please enter a valid Allrecipes.com website address.", "Invalid URL");
                        return;
                    }
                    RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
                    string errorMsg = ImportUtils.AllrecipesRecipeCardImporter.LoadRecipeFromWebpage(etd.Text, recipe, this);
                    if (string.IsNullOrEmpty(errorMsg))
                    {
                        recipe.SuggestIngredientItems((XPCollection<Item>)itemsXPBindingSource.DataSource);
                        ModelToView();
                    }
                    else
                    {
                        ShowMessageBox(errorMsg, "Import Allrecipes.com Error");
                        return;
                    }
                }
            }
            //#RQT ImprtGptRC Prompt user for GPT prompt.  Import RC from GPT.
            else if (importRecipeCardPrompt.ItemClicked == ImportRecipeCardPrompt.Items.ImportAI)
            {
                RecipeCard recipeCard = (RecipeCard)this.FormHeadBusinessObject;
                string myIP = WebUtils.GetPublicIPAddress();
                if (myIP == null)
                {
                    ShowMessageBox("Connection to the internet is required to ask GPT.", "Connectivity Error");
                    return;
                }
                ImportGPTRecipeCard askGPT = new ImportGPTRecipeCard(null, this.MDIParentForm, recipeCard,
                    (XPCollection<Item>)itemsXPBindingSource.DataSource);
                this.MDIParentForm.FormStack.PushStackAndShowDialogForm(askGPT, () =>
                {
                    if (askGPT.DialogResult == DialogResult.OK)
                    {
                        this.Oid = askGPT.Oid;
                        ModelToView();
                    }
                });
            }
        }

        //#RQT BtnAdIngs2GL AddToGroceryList button: Populate SelectIngredientsForGroceryList form with all ingredients in the RecipeCard.  User removes ingredients not needed, then adds the remaining to the GroceryList.
        private void addIngToGLButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            int? glOid = DataStoreServiceReference.DataStoreServiceReference.MyGroceryListOid;
            SelectIngredientsForGroceryList form = new SelectIngredientsForGroceryList(glOid, (RecipeCard)this.FormHeadBusinessObject, this.MDIParentForm);

            this.MDIParentForm.FormStack.PushStackAndShowDialogForm(form, () =>
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

        private void editGroceryListBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            int? glOid = DataStoreServiceReference.DataStoreServiceReference.MyGroceryListOid;
            var form = new EditGroceryList(glOid, this.MDIParentForm);
            this.MDIParentForm.FormStack.PushStackAndShowDialogForm(form);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (this.addCookedDateDateEdit.EditValue != null)
            {
                DateTime newDate = (DateTime)this.addCookedDateDateEdit.EditValue;
                RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
                CookedDish newcr = new CookedDish(UnitOfWork);
                newcr.CookedDate = newDate;
                recipe.AddCookedDish(newcr);
                this.addCookedDateDateEdit.Focus();
                this.addCookedDateDateEdit.EditValue = null;
            }
        }

        private void addTodaySimpleButton_Click(object sender, EventArgs e)
        {
            DateTime newDate = DateTime.Today;
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            CookedDish newcr = new CookedDish(UnitOfWork);
            newcr.CookedDate = newDate;
            recipe.AddCookedDish(newcr);
            this.addCookedDateDateEdit.EditValue = null;
            this.addCookedDateDateEdit.Focus();
        }

        private void viewRecipeBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.NextFormToOpen = nameof(ViewRecipeCard);
            DoSaveAndClose();
        }
        //#RQT RCSchCkdDt ScheduleRecipe button: Prompt user for a date to schedule the recipe.  Once scheduled, set focus to the CookedDishes tab so the user sees the new date.
        private void scheduleBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            SimpleCRUDFramework.HelperForms.EditDateDialog edd = new SimpleCRUDFramework.HelperForms.EditDateDialog("Schedule RecipeCard", "Date", null);

            //EditDate edd = new EditDate();
            DialogResult dr = edd.ShowDialog();//owner: this.MdiParent);
            if (dr == DialogResult.OK)
            {
                CookedDish newCooked = new CookedDish(this.UnitOfWork);
                newCooked.CookedDate = (DateTime)edd.Date;
                recipe.AddCookedDish(newCooked);
                this.recipeCardTabControl.SelectedTabPage = this.cookedDishesXtraTabPage;
            }
        }
        //#RQT BtnEdtTtlUrl Edit Title button allows user to edit the title and the RecipeCard SourceURL.
        private void editTitleSimpleButton_Click(object sender, EventArgs e)
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            EditTitle et = new EditTitle(recipe);
            et.ShowDialog(this.MdiParent);
            if (et.DialogResult == DialogResult.OK)
            {
                recipe.Title = et.RecipeCard.Title;
                recipe.SourceURL = et.RecipeCard.SourceURL;
                ModelToView();
            }
        }
        #endregion

    }
}