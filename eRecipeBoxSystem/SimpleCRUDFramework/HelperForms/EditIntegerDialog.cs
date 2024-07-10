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
    public class EditIntegerDialog
    {
        public int? IntegerValue { get; private set; }
        private SpinEdit editor;
        public EditIntegerDialog(string FormCaption, string inputPromptLabel, int? minValue, int? maxValue, int? inputPrompt = null)
        {
            args = new XtraInputBoxArgs();
            args.Caption = FormCaption;
            args.Prompt = inputPromptLabel;
            IntegerValue = inputPrompt;
            args.DefaultButtonIndex = 0;

            editor = new SpinEdit();
            editor.Properties.IsFloatValue = false;
            editor.Properties.BeepOnError = true;
            editor.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.NumericMaskManager));
            editor.Properties.MaskSettings.Set("valueAfterDelete", DevExpress.Data.Mask.NumericMaskManager.ValueAfterDelete.Null);
            editor.Properties.MaskSettings.Set("valueType", typeof(int));
            editor.Properties.MaskSettings.Set("mask", "##");

            if (minValue != null)
                editor.Properties.MinValue = (int)minValue;
            if (maxValue != null)
                editor.Properties.MaxValue = (int)maxValue;

            args.Editor = editor;
            args.Showing += OnArgsShowing;
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
        }

        void OnFormOnControlAdded(object sender, ControlEventArgs args)
        {
            if (args.Control is XtraUserControl uc)
            {
                XtraInputBoxForm form = (XtraInputBoxForm)sender;

                foreach (var control in args.Control.Controls)
                {
                    if (control is SpinEdit)
                    {
                        SpinEdit spinEdit = (SpinEdit)control;
                        spinEdit.Name = "spinEdit";
                        spinEdit.AccessibleName = "spinEdit";
                        spinEdit.EditValue = IntegerValue;
                        Log.LogWindowElement(form.Name, spinEdit.GetType().Name, spinEdit.Name, WindowsFormsUtils.GetAncestors(spinEdit));
                        break;
                    }
                }
            }
        }

        public DialogResult ShowDialog()
        {
            var response = XtraInputBox.Show(args);
            if (response == null)
                dialogResult = DialogResult.Cancel;
            else
            {
                dialogResult = DialogResult.OK;
                this.IntegerValue = Convert.ToInt32(editor.EditValue);
            }
            return dialogResult;
        }
        private XtraInputBoxArgs args;
        private DialogResult dialogResult;
    }
}
