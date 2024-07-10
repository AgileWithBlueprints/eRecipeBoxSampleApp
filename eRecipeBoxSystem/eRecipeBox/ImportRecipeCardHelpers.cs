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
using Foundation;
using RecipeBoxSolutionModel;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static SimpleCRUDFramework.DevExpressUxUtils;

namespace eRecipeBox
{
    partial class ImportRecipeCard
    {
        #region properties
        private ImportUtils.TextRecipeCardImporter importer;

        #endregion

        #region ctor

        private void InitializeComponent2()
        {
            //tab is our Field delimiter
            //https://supportcenter.devexpress.com/ticket/details/t1087316/memoedit-and-tab
            memoEdit1.Properties.AcceptsTab = true;
            importer = new ImportUtils.TextRecipeCardImporter();

            //#RQT Shortcut keys - Alt-p (parse raw RecipeCard text), Alt-i (import parsed text into RecipeCard and close)            
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(parseBarButtonItem, Keys.Alt, Keys.P);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(importBarButtonItem, Keys.Alt, Keys.I);

            SetAllControlNames();
            SetControlConstraints();
        }
        #endregion ctor

        #region CRUDBusinessObjectForm
        override protected bool ReloadModel()
        {

            NewUnitOfWork();
            int rbID = DataStoreServiceReference.DataStoreServiceReference.MyRecipeBoxOid;
            RecipeBox myRecipeBox = (RecipeBox)XpoService.LoadHeadBusObjByKey(typeof(RecipeBox), rbID, UnitOfWork);
            RecipeCard rec = myRecipeBox.CreateNewRecipeCard(UnitOfWork);
            this.FormHeadBusinessObject = rec;
            if (rec == null)
                return false;
            else
                return true;

        }
        override protected void ModelToView()
        {
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;
            IList<string> lines = importer.UnparseRecipeCard(recipe);
            SetTabWidth(memoEdit1.MaskBox, 5);
            this.memoEdit1.Text = string.Join(PrimitiveUtils.newl, lines);

        }
        override protected bool SaveModel(out SaveErrorReason? reason)
        {
            reason = null;
            return true;
        }
        protected override bool ViewToModel()
        {
            NewUnitOfWork();
            int rbID = DataStoreServiceReference.DataStoreServiceReference.MyRecipeBoxOid;
            RecipeBox myRecipeBox = (RecipeBox)XpoService.LoadHeadBusObjByKey(typeof(RecipeBox), rbID, UnitOfWork);
            this.FormHeadBusinessObject = myRecipeBox.CreateNewRecipeCard(UnitOfWork);
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;

            string[] lines = this.memoEdit1.Text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string errorMessage;
            //#RQT 3. Import the parsed text lines into a RecipeCard by setting RecipeCard's properties with the text.
            this.isFormValid = importer.LoadParsedTextIntoRecipeCard(lines, recipe, this, out errorMessage);
            if (!this.isFormValid)
                ShowMessageBox(errorMessage, "Invalid Form");
            return this.isFormValid;
        }
        protected override bool ValidateForm()
        {
            if (!isFormValid)
                return false;
            else
                return base.ValidateForm();
        }
        #endregion CRUDBusinessObjectForm

    }
}
