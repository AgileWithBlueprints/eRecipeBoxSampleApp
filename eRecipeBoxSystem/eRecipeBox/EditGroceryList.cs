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
using Foundation;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace eRecipeBox
{
    //#TODO REFACTOR move all the image resources to the project level and share them.
    public partial class EditGroceryList : CRUDBusinessObjectForm
    {

        #region form properties
        Keys LastLookupEditKey { get; set; }
        string NewItemToAdd { get; set; }

        #endregion

        #region ctor
        public EditGroceryList(int? glOid, MDIParentForm parent) : base(glOid, nameof(EditGroceryList), parent)
        {
            base.InitializeBaseComponent();
            InitializeComponent();
            InitializeComponent2();
        }
        #endregion

        #region form event handlers
        private void EditGroceryList_Load(object sender, EventArgs e)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                ReloadModelAndView();
                EndFormLoad();
            }
        }
        #endregion

        #region search itemLookUpEdit event handlers
        private void itemLookUpEdit_ProcessNewValue(object sender, ProcessNewValueEventArgs e)
        {
            NewItemToAdd = (string)e.DisplayValue;
            e.DisplayValue = null;

            //#TIP setting this to false only calls this once
            //https://supportcenter.devexpress.com/ticket/details/q303530/lookupedit-processnewvalue-is-called-twice
            e.Handled = false;
        }

        private void itemLookUpEdit_Closed(object sender, ClosedEventArgs e)
        {
            string itemToAdd = (string.IsNullOrWhiteSpace(NewItemToAdd)) ? this.itemLookUpEdit.Text : this.NewItemToAdd;
            if (e.CloseMode == PopupCloseMode.Normal && !string.IsNullOrWhiteSpace(itemToAdd) && LastLookupEditKey == Keys.Enter)
            {
                //this adds and item when hitting enter or tabbing off a suggestion or selecting a suggestion
                this.addSimpleButton.PerformClick();
            }
        }

        private void itemLookUpEdit_KeyDown(object sender, KeyEventArgs e)
        {
            LastLookupEditKey = e.KeyCode;
        }
        #endregion        

        #region GridView event handlers

        private void gridView1_DataSourceChanged(object sender, EventArgs e)
        {
            DisplayRowCount();
            DoUpdateViewState();
        }

        private void gridView1_RowCountChanged(object sender, EventArgs e)
        {
            DisplayRowCount();
            DoUpdateViewState();

        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            DoUpdateViewState();
        }

        #endregion

        #region button event handlers
        private void deleteItemBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            DeleteSelectedGLItems();
        }

        private void barButtonItemSave_ItemClick(object sender, ItemClickEventArgs e)
        {
            AutoSetItems();
            DoSave();
        }

        private void barButtonItemCancel_ItemClick(object sender, ItemClickEventArgs e)
        {
            AutoSetItems();
            DoCancel();
        }

        private void barButtonItemRefresh_ItemClick(object sender, ItemClickEventArgs e)
        {
            ReloadModelAndView();
        }

        private void barButtonItemSaveAndClose_ItemClick(object sender, ItemClickEventArgs e)
        {
            AutoSetItems();
            DoSaveAndClose();
        }

        private void clearBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            //#NOTE this seems to perform better than select all, then clear all selected                       
            using (WaitCursor wc = new WaitCursor())
            {
                ((System.ComponentModel.ISupportInitialize)(this.groceryListGridView)).BeginInit();
                for (int i = groceryListGridView.RowCount - 1; i >= 0; i--)
                {
                    GroceryListItem gli = (GroceryListItem)groceryListGridView.GetRow(i);
                    gli.GroceryList.DeleteGroceryListItem(gli);
                }
                ((System.ComponentModel.ISupportInitialize)(this.groceryListGridView)).EndInit();
            }
        }

        private void emailBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.ViewToModel();

            //#TODO ENHANCEMENT Prompt with all users connected to the RB and ask which email(s) to send if there are multiple.
            this.statusBarStaticItem.Caption = null;
            GroceryList groceryList = (GroceryList)this.FormHeadBusinessObject;
            List<string> lines = new List<string>();

            UserProfile myUserProfile = DataStoreServiceReference.DataStoreServiceReference.MyUserProfile;
            if (string.IsNullOrWhiteSpace(myUserProfile.PersonalEmail))
            {
                ShowMessageBox("Your profile does not have an email address.", "Missing Email");
                return;
            }
            string toemail = myUserProfile.PersonalEmail;
            using (WaitCursor wc = new WaitCursor())
            {
                //#RQT GLEmlHTML Generate an HTML table when emailing GroceryList.
                string t = groceryList.GenerateHTMLtable();
                try
                {
                    //#RQT GLEmlOAth Email GroceryList to the email address in the user's UserProfile.  The email address must be authorized in Google's OAuth2 email service.
                    EmailUtils.SendEmailMessage(toemail, toemail, "grocery list", t);
                    this.statusBarStaticItem.Caption = $"GroceryList sent to email {toemail}";
                }
                catch (Exception ex)
                {
                    if (ex is MissingCredentialsExeption && eRecipeBox.DataStoreServiceReference.DataStoreServiceReference.IsSingleUserDeployment)
                        ShowMessageBox("Email GroceryList is not supported in SingleUser configuration. See Build Dev Environment to deploy eRecipeBox app and configure Gmail service.",
                            "Send grocery list via email failed.");
                    else
                    {
                        ShowMessageBox($"Email failed: {ex.Message}", "Send grocery list via email failed.");
                    }

                }
            }
        }

        private void addSimpleButton_Click(object sender, EventArgs e)
        {
            string itemToAdd = (string.IsNullOrWhiteSpace(NewItemToAdd)) ? this.itemLookUpEdit.Text : this.NewItemToAdd;

            if (string.IsNullOrWhiteSpace(itemToAdd))
                return;

            AddItemToGL(itemToAdd);

            setFocusToInputButtonItem.PerformClick();

            LastLookupEditKey = Keys.None;
            itemLookUpEdit.EditValue = null;
            NewItemToAdd = null;

            //scroll to top  #TODO REFACTOR
            groceryListGridView.ClearSelection();
            groceryListGridView.FocusedRowHandle = 0;
            groceryListGridView.TopRowIndex = 0;

        }

        //#WORKAROUND Create an invisible button that can be clicked programatically to set focus
        private void setFocusToInputButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.itemLookUpEdit.Focus();
        }
        #endregion

    }
}