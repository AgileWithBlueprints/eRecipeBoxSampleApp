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
using System.Windows.Forms;

namespace SimpleCRUDFramework
{

    /// <summary>
    /// Parent MDI form for SimpleCRUDFramework.  All forms in the framework are children of this form.
    /// </summary>
    public class MDIParentForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public MDIParentForm()
        {
            this.WindowState = FormWindowState.Maximized;

            //https://docs.devexpress.com/WindowsForms/3451/controls-and-libraries/ribbon-bars-and-menu/ribbon/runtime-capabilities/ribbon-merging
            //https://docs.devexpress.com/WindowsForms/11360/controls-and-libraries/application-ui-manager/examples/how-to-display-documents-using-a-native-mdi            
            this.IsMdiContainer = true;

            fFormStack = new FormStack();
        }

        private FormStack fFormStack;
        public FormStack FormStack
        {
            get { return fFormStack; }
        }
    }
}
