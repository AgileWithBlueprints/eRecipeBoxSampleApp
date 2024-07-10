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
using DevExpress.XtraBars;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Windows.Forms;

namespace eRecipeBox
{
    public partial class ImportGPTRecipeCard : CRUDBusinessObjectForm
    {
        #region properties
        public string UserGPTPrompt { get; private set; }
        #endregion properties

        #region ctor

        //for WinForms designer only 
        public ImportGPTRecipeCard()
        {
            base.InitializeBaseComponent();
            InitializeComponent();
        }

        //runtime ctor
        public ImportGPTRecipeCard(int? Oid, MDIParentForm parent, RecipeCard recipeCard,
            XPCollection<Item> allItemsForSuggesting) : base(Oid, nameof(ImportGPTRecipeCard), parent)
        {
            base.InitializeBaseComponent();
            InitializeComponent();
            InitializeComponent2(recipeCard, allItemsForSuggesting);
        }

        #endregion ctor

        #region form event handlers
        private void AskGPTRecipeCard_Load(object sender, EventArgs e)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                ReloadModelAndView();
                EndFormLoad();
            }
        }

        private void AskGPTRecipeCard_FormClosing(object sender, FormClosingEventArgs e)
        {
            memoEdit1.SpeechRecognitionError -= HandleSpeechRecognitionError;
        }

        #endregion form event handlers

        #region button event handlers
        private void cancelBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.DoCancel();
        }

        private void askGPTBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            //#TRICKY  Save the view to the model.  SaveModel to data store does nothing and returns success.
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