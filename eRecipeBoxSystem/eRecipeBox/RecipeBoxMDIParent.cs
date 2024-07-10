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
using Foundation;
using SimpleCRUDFramework;
using System;
using System.Windows.Forms;

namespace eRecipeBox
{
    public partial class RecipeBoxMDIParent : MDIParentForm
    {
        public RecipeBoxMDIParent() : base()
        {
            InitializeComponent();

            //#TODO REFACTOR BEGIN move this to MDIParentForm?  
            this.parentFormRibbonPageGroup.Visible = false; //always use the child ribbon.  merged into nothing
            this.ribbonStatusBar.Visible = false;  //always use the child status bar

            this.documentManager1.MdiParent = this;

            //https://supportcenter.devexpress.com/ticket/details/t1062252/devexpress-xtrabars-barbuttonitem-itemshortcut-has-different-focus-behavior-vs-clicking
            //this is required to get the shortcut keys to work in the mdi children
            //fix https://supportcenter.devexpress.com/ticket/details/t1062695/merged-ribboncontrol-enters-navigation-mode-when-a-shortcut-containing-the-trigger-key
            this.mdiFormRibbon.Manager.UseAltKeyForMenu = false;
            //#TODO REFACTOR END
        }

        //https://www.fmsinc.com/free/NewTips/NET/NETtip54.asp
        //helpful
        //trace all WinMsgs to VS debug output window
        //protected override void WndProc(ref Message m)
        //{
        //    base.WndProc(ref m);

        //    Trace.WriteLine(m.Msg.ToString() + ": " + m.ToString());
        //}


        private void RecipeMDIParent_Load(object sender, EventArgs e)
        {
            this.Text = DataStoreServiceReference.DataStoreServiceReference.MyRecipeBoxName + " RecipeBox";
            SearchRecipeBox f = new SearchRecipeBox(this);
            this.FormStack.PushStackAndShowDialogForm(f);
        }

        private void RecipeBoxMDIParent_FormClosing(object sender, FormClosingEventArgs e)
        {
            //#RQT AppClsPrgSftDel Purge the soft deleted objects in the database when closing the app.
            DataStoreUtils.XpoService.PurgeDeletedObjects();


            bool systemTestLogging = AppSettings.GetBoolAppSetting("SystemTestLogging");
            if (systemTestLogging)
            {
                Log.Shutdown();
                Log.ReportFormsAndTheirElements(Log.App.LogFileFullName);

            }
        }
    }
}