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
using DevExpress.XtraEditors;
using SimpleCRUDFramework;
using System;

namespace eRecipeBox.HelperForms
{
    public partial class ImportRecipeCardPrompt : MDIChildForm
    {

        public enum Items { ImportText, ImportAllRecipes, ImportAI }

        public Items? ItemClicked { get; set; }

        public ImportRecipeCardPrompt() : base(nameof(ImportRecipeCardPrompt), null)
        {
            InitializeComponent();
            ItemClicked = null;
        }

        private void ImportTextItem_ItemClick(object sender, TileItemEventArgs e)
        {
            ItemClicked = Items.ImportText;
            this.Close();
        }

        private void ImportAllRecipesItem_ItemClick(object sender, TileItemEventArgs e)
        {
            ItemClicked = Items.ImportAllRecipes;
            this.Close();
        }

        private void ImportGPTItem_ItemClick(object sender, TileItemEventArgs e)
        {
            ItemClicked = Items.ImportAI;
            this.Close();
        }

        private void ImportRecipeCardPrompt_Load(object sender, EventArgs e)
        {
            EndFormLoad();
            this.Text = "Select Import Source";
            if (DataStoreServiceReference.DataStoreServiceReference.IsSingleUserDeployment)
                importGPTItem.Visible = false;
            else
                importGPTItem.Visible = true;
        }
    }
}