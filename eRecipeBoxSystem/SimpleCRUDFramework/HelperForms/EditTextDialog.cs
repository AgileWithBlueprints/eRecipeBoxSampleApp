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
using DevExpress.XtraEditors.Mask;
using Foundation;
using System.Windows.Forms;
namespace SimpleCRUDFramework.HelperForms
{
    //#TODO REFACTOR Copied from EditDateDialog. Refactor and consolidate logic between the two.
    public class EditTextDialog
    {
        public string Text { get; set; }
        //private TextEdit editor;
        public EditTextDialog(string FormCaption, string inputPromptLabel, string defaultResponse = null, string editMask = null)
        {
            args = new XtraInputBoxArgs();
            args.Caption = FormCaption;
            args.Prompt = inputPromptLabel;
            args.DefaultButtonIndex = 0;
            regExpEditMask = editMask;
            args.Showing += OnArgsShowing;
            args.DefaultResponse = defaultResponse;
        }

        //https://supportcenter.devexpress.com/ticket/details/t689978/xtrainputboxform
        //#TRICKY to call showPopup at the right time
        private void OnArgsShowing(object sender, XtraMessageShowingArgs showingArgs)
        {
            //access the form and add event handler
            XtraInputBoxForm form = (XtraInputBoxForm)showingArgs.Form;
            //form.Name = showingArgs.cp; 
            WindowsFormsUtils.LogFormControls(form);
            form.StartPosition = FormStartPosition.CenterParent;
            form.ControlAdded += OnFormOnControlAdded;
        }

        void OnFormOnControlAdded(object sender, ControlEventArgs args)
        {
            if (args.Control is XtraUserControl uc)
            {
                XtraInputBoxForm form = (XtraInputBoxForm)sender;

                foreach (var control in args.Control.Controls)
                {
                    if (control is TextEdit)
                    {
                        TextEdit textEdit = (TextEdit)control;
                        if (regExpEditMask != null)
                        {
                            textEdit.Properties.MaskSettings.Configure<MaskSettings.RegExp>(settings =>
                            {
                                settings.MaskExpression = regExpEditMask;
                            });
                        }
                        textEdit.Name = "textEdit";
                        textEdit.AccessibleName = "textEdit";
                        if (!string.IsNullOrEmpty(Text))
                            textEdit.Text = Text;
                        Log.LogWindowElement(form.Name, textEdit.GetType().Name, textEdit.Name, WindowsFormsUtils.GetAncestors(textEdit));
                        break;
                    }
                }
            }
        }

        public DialogResult ShowDialog()
        {
            string response = (string)XtraInputBox.Show(args);
            if (response == null)
                dialogResult = DialogResult.Cancel;
            else
            {
                dialogResult = DialogResult.OK;
                this.Text = response;
            }
            return dialogResult;
        }
        private string regExpEditMask;
        private XtraInputBoxArgs args;
        private DialogResult dialogResult;
    }
}
