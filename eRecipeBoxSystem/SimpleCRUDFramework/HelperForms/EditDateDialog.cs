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
using System;
using System.Windows.Forms;
namespace SimpleCRUDFramework.HelperForms
{
    public class EditDateDialog
    {
        public DateTime? Date { get; private set; }
        private DateEdit editor;
        public EditDateDialog(string FormCaption, string inputPromptLabel, DateTime? defaultResponse = null, string editMask = null)
        {
            args = new XtraInputBoxArgs();
            args.Caption = FormCaption;
            args.Prompt = inputPromptLabel;
            args.DefaultButtonIndex = 0;
            // initialize a DateEdit editor with custom settings
            editor = new DateEdit();
            //editor.AccessibilityObject
            editor.Properties.CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Default;
            if (editMask == null)
                editor.Properties.Mask.EditMask = "d"; //#TODO REFACTOR shouldnt hardcode this here
            else
                editor.Properties.Mask.EditMask = editMask;
            args.Editor = editor;
            args.Showing += OnArgsShowing;
            args.DefaultResponse = defaultResponse;
        }

        //https://supportcenter.devexpress.com/ticket/details/t689978/xtrainputboxform
        //trick to call showPopup at the right time
        private void OnArgsShowing(object sender, XtraMessageShowingArgs showingArgs)
        {
            //access the form and add event handler
            XtraInputBoxForm form = (XtraInputBoxForm)showingArgs.Form;
            //form.Name = showingArgs.cp; 
            WindowsFormsUtils.LogFormControls(form);
            form.StartPosition = FormStartPosition.CenterParent;
            form.ControlAdded += OnFormOnControlAdded;
            form.Shown += Form_Shown;
        }

        void OnFormOnControlAdded(object sender, ControlEventArgs args)
        {
            if (args.Control is XtraUserControl uc)
            {
                XtraInputBoxForm form = (XtraInputBoxForm)sender;

                foreach (var control in args.Control.Controls)
                {
                    if (control is DateEdit)
                    {
                        DateEdit dateEdit = (DateEdit)control;
                        dateEdit.Name = "dateEdit";
                        dateEdit.AccessibleName = "dateEdit";
                        Log.LogWindowElement(form.Name, dateEdit.GetType().Name, dateEdit.Name, WindowsFormsUtils.GetAncestors(dateEdit));
                        break;
                    }
                }
            }
        }

        private void Form_Shown(object sender, EventArgs e)
        {
            editor.ShowPopup();
        }

        public DialogResult ShowDialog()
        {
            DateTime? response = (DateTime?)XtraInputBox.Show(args);
            if (response == null)
                dialogResult = DialogResult.Cancel;
            else
            {
                dialogResult = DialogResult.OK;
                this.Date = response;
            }
            return dialogResult;
        }
        private XtraInputBoxArgs args;
        private DialogResult dialogResult;
    }
}
