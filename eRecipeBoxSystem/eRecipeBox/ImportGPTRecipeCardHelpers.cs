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
using eRecipeBox.ImportUtils;
using Foundation;
using RecipeBoxSolutionModel;
using System;
using System.Drawing;
namespace eRecipeBox
{
    public partial class ImportGPTRecipeCard
    {
        #region properties
        private XPCollection<Item> allItems;
        #endregion properties

        #region methods

        private void InitializeComponent2(RecipeCard recipeCard,
            XPCollection<Item> allItemsForSuggesting)
        {
            //match font size of instructions.
            memoEdit1.Properties.Appearance.Font = new Font(memoEdit1.Properties.Appearance.Font.FontFamily.Name,
                11, memoEdit1.Properties.Appearance.Font.Style);
            memoEdit1.SpeechRecognitionError += HandleSpeechRecognitionError;
            string subscriptionKey = Encryption.DecryptHexString(AppSettings.GetAppSetting("EncryptedAzureSubscriptionKey"));
            string subscriptionRegion = AppSettings.GetAppSetting("AzureSubscriptionRegion");
            memoEdit1.RegisterAzureSubscriptionKey(subscriptionKey, subscriptionRegion);
            //use the calling RC form's RC, items and UoW
            this.FormHeadBusinessObject = recipeCard;
            UnitOfWork = (UnitOfWork)recipeCard.Session;
            allItems = allItemsForSuggesting;

            //#RQT Shortcut keys - 
            //SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(importBarButtonItem, Keys.Alt, Keys.I);
            SetAllControlNames();
            SetControlConstraints();
        }
        private void HandleSpeechRecognitionError(string errorMessage)
        {
            this.Invoke(new Action(() =>
            {
                instructionsLabelControl.Text = instructionsLabelControl.Text.Replace(" or Say", "");
                statusMessageBarStaticItem.Caption = memoEdit1.SpeechRecognitionErrorMessage;
            }));
        }
        override protected bool ReloadModel()
        {
            //No model for this form
            return true;
        }
        override protected void ModelToView()
        {
            //No model for this form
            return;
        }
        override protected bool SaveModel(out SaveErrorReason? reason)
        {
            //No model for this form
            reason = null;
            return true;
        }
        override protected bool ViewToModel()
        {
            bool isFormValid;
            RecipeCard recipe = (RecipeCard)this.FormHeadBusinessObject;

            UserGPTPrompt = string.Join("\n", memoEdit1.Lines);
            if (string.IsNullOrEmpty(UserGPTPrompt))
                return true;
            string errorMessage = GPTRecipeCardImporter.PromptGPTandLoadRecipeCard(UserGPTPrompt, recipe, allItems);
            if (errorMessage == null)
            {
                isFormValid = true;
            }
            else
            {
                ShowMessageBox(errorMessage, "Ask GPT Error");
                isFormValid = false;
            }

            return isFormValid;
        }

        override protected bool DoCancel(out SaveErrorReason? reason, bool promptToSaveChanges = true)
        {
            reason = null;
            DoHardCancel();
            return false;
        }

        #endregion methods


    }
}
