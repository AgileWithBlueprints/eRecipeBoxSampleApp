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
using System;
using System.Collections.Generic;
using System.Windows.Forms;
namespace SimpleCRUDFramework
{
    /// <summary>
    ///Simple window/form manager design
    ///MDI parent.  Show one maximized child window at a time.  Each child behaves like show model dialog.
    ///Maintain a stack forms.  Open new form minimizes current form and pushes the new form on the stack.
    ///Show new child form maximized.
    ///Upon close window, pop current form from the top of the stack and maximize top(last) form. 
    ///MDI children cannot show modal dialog, so we emulate the model behavior by maximizing the child window and 
    ///not allow user to minimize it.
    /// </summary>
    public class FormStack : Stack<MDIChildForm>
    {
        public FormStack()
        {
        }

        //#WORKAROUND Can't showdialog on MDI children
        //https://stackoverflow.com/questions/41068714/how-allows-showdialog-mdi-children-on-mdi-form-parent

        //#RQT Pushes the form onto the form stack and shows it
        public void PushStackAndShowDialogForm(MDIChildForm dialogForm, Action formClosedAction = null, bool maximizeDialog = true)
        {
            // Dispose the form and maximize top form on the stack
            dialogForm.FormClosed += (object closeSender, FormClosedEventArgs closeE) =>
            {
                //Pop me from top of stack 
                this.Pop();
                //Fire the callback
                if (formClosedAction != null)
                    formClosedAction();

                if (!(dialogForm is null) && !dialogForm.IsDisposed)
                    dialogForm.Dispose();

                dialogForm = null;
                if (this.Count > 0)
                {
                    var currentForm = this.Peek();
                    if (currentForm != null)
                    {
                        currentForm.Visible = true;
                        currentForm.WindowState = FormWindowState.Maximized;
                    }
                }
            };

            if (maximizeDialog)
                dialogForm.WindowState = FormWindowState.Maximized;
            else
                dialogForm.StartPosition = FormStartPosition.CenterParent;

            //#RQT This hides the icons to resize the window when maximized in the parent.
            dialogForm.ControlBox = false;
            dialogForm.MinimizeBox = false;
            dialogForm.MaximizeBox = false;

            if (this.Count > 0)
            {
                var currentForm = this.Peek();
                //maybe have the minimized win not go to the bottom
                //https://social.msdn.microsoft.com/Forums/windows/en-US/d6014e48-2adb-4096-8bea-94c2f3b1c47c/how-to-change-the-location-of-a-minimized-mdichild-form?forum=winforms

                //Hide current form by setting Visible = false
                currentForm.Visible = false;
                currentForm.WindowState = FormWindowState.Minimized;  //#TODO This doesnt seem necessary since Visible = false
            }
            this.Push(dialogForm);
            //my turn
            dialogForm.Show();
        }
    }
}
