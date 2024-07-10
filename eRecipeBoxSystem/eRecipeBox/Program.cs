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
using Foundation;
using RecipeBoxSolutionModel;
using System;
using System.Windows.Forms;
namespace eRecipeBox
{
    static class Program
    {
        [STAThread]
        static void Main()
        {

            //#IMPORTANT DESIGN  CONFIG BUILD.  must disable directX, use windows Default Font, DPI awareness to None(optional), application UI = default
            //if not, fonts mess up only on W11 surface
            try
            {
                Log.Initialize();
                ModelInfo.RegisterModelVersions();

                //#AtmtdTstng
                //Enable automated UI testing support
                //https://docs.devexpress.com/WindowsForms/404045/build-an-application/ui-tests-ui-automation-appium-coded-ui
                WindowsFormsSettings.UseUIAutomation = DevExpress.Utils.DefaultBoolean.True;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                RecipeBoxMDIParent mdiParent = new RecipeBoxMDIParent();

                bool loginSuccess = Authentication.Authenticator.DoLogin();
                if (!loginSuccess)
                    return;
                Application.Run(mdiParent);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
            finally
            {
                Log.Shutdown();
            }
        }
    }
}
