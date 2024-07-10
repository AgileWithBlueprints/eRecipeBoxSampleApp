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
using Foundation;
using SimpleCRUDFramework;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace eRecipeBox
{
    public partial class ImportRecipeCard : CRUDBusinessObjectForm
    {
        #region form properties
        private bool isFormValid = true;
        #endregion form properties

        #region ctor
        public ImportRecipeCard(int? Oid, MDIParentForm parent) : base(Oid, nameof(ImportRecipeCard), parent)
        {
            base.InitializeBaseComponent();
            InitializeComponent();
            InitializeComponent2();
        }
        #endregion ctor

        #region form event handlers
        private void ImportRecipeCard_Load(object sender, EventArgs e)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                ReloadModelAndView();
                EndFormLoad();
            }
        }
        #endregion  

        #region button event handlers

        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.DoCancel();
        }

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            //#RQT Importing a RecipeCard is performed in three steps:
            //#RQT 1. Create a sequence of lines containing RecipeCard property markers (eg "Title:"), followed by their property values.
            //#RQT 2. Parse the individual parts of each Ingredients into its properties.  Use '`' as a delimiter.            
            string[] lines = this.memoEdit1.Text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
            IList<string> parsed = importer.ParseTextAndAddFieldDelimiters(lines);
            this.memoEdit1.Text = string.Join(PrimitiveUtils.newl, parsed);
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            //#Tricky.  Save the view to the model.  SaveModel to data store does nothing and returns success.
            //Tell DoSave not to reload the model after saving because we want to pass this model to
            //the EditRecipeCard form.
            bool valid = DoSave(reloadModelAndViewAfterSave: false);
            if (valid)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        #endregion button event handlers

    }
}