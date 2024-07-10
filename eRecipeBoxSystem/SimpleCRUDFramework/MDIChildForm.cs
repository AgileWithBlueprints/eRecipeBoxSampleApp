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
using DevExpress.Utils.Drawing.Helpers;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Foundation;
using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace SimpleCRUDFramework
{
    /// <summary>
    /// All child forms in the SimpleCRUDFramework inherit from this class.
    /// </summary>
    public class MDIChildForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        #region form properties
        public MDIParentForm MDIParentForm
        {
            get
            {
                return (MDIParentForm)this.MdiParent;
            }
        }

        /// <summary>
        /// Simple window navigation.  The invoked child form sets this property to indicate which form
        /// should be shown next.  If null, show the last/top form in the FormStack.
        /// </summary>
        public string NextFormToOpen { get; set; }


        protected DXErrorProvider dxErrorProvider1;
        private System.ComponentModel.IContainer errorProviderComponents;
        #endregion form properties

        #region ctor

        //#NOTE  this is only required so Visual Studio can create an instance of the form in the visual designer 
        public MDIChildForm() : base()
        {
        }

        public MDIChildForm(string formName) : base()
        {
            InitializeBaseComponent();
            //#RQT allow user to tab off an input field that has an error 
            this.AutoValidate = AutoValidate.EnableAllowFocusChange;
            this.StartPosition = FormStartPosition.CenterParent;
            WindowsFormsUtils.SetControlName(this, formName);
        }

        public MDIChildForm(string formName, MDIParentForm parent) : this(formName)
        {
            this.MdiParent = parent;
        }
        #endregion ctor

        #region form methods

        protected void InitializeBaseComponent()
        {
            this.errorProviderComponents = new System.ComponentModel.Container();
            this.dxErrorProvider1 = new DevExpress.XtraEditors.DXErrorProvider.DXErrorProvider(this.errorProviderComponents);
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).BeginInit();
            this.SuspendLayout();
            this.dxErrorProvider1.ContainerControl = this;
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).EndInit();
            this.ResumeLayout(false);
        }
        protected void SetControlConstraints()
        {
            DevExpressUxUtils.SetMaxLengthForAllTextEdits(this);
            DevExpressUxUtils.SetMaskForAllTextEdits(this);
        }

        protected void SetAllControlNames()
        {
            //#AtmtdTstng Set all form field's.Name and AccessibleName properties to the field variable name.
            //Automated testing scripts find the Windows elements using this name.
            var bindingFlags = BindingFlags.Instance |
                   BindingFlags.NonPublic |
                   BindingFlags.Public;
            foreach (FieldInfo fieldInfo in this.GetType().GetFields(bindingFlags))
            {
                PropertyInfo nameProperty = fieldInfo.FieldType.GetProperty("Name");
                if (nameProperty != null)
                {
                    var fieldInstance = fieldInfo.GetValue(this);
                    //field is null
                    if (fieldInstance == null)
                        continue;
                    var fieldNameValue = nameProperty.GetValue(fieldInstance, null);
                    if ((string)fieldNameValue != fieldInfo.Name)
                        nameProperty.SetValue(fieldInstance, fieldInfo.Name, null);

                    PropertyInfo accessibleNameProperty = fieldInfo.FieldType.GetProperty("AccessibleName");
                    accessibleNameProperty?.SetValue(fieldInstance, fieldInfo.Name, null);

                }
            }
        }

        /// <summary>
        /// Enable/disable command buttons based on the state of the form.
        /// </summary>
        protected virtual void UpdateViewState() { }

        protected void DoUpdateViewState()
        {
            //#TRICKY The button enable/disable logic must be executed on the main UI thread.
            base.Invoke((Action)delegate { UpdateViewState(); });
        }

        //#AtmtdTstng Assign names to controls, which are used to create runtime window elements. (and log them).  Automated test scripts refer to these elements.
        public void EndFormLoad()
        {
            ////https://supportcenter.devexpress.com/ticket/details/q401164/controls-find-seems-not-to-work-with-barbuttonitem-and-probably-other-dxp-controls
            //#TRICKY Unfortunatly, Ribbon and its elements are not Controls.  Dump them explicitly.
            //Pages and BarItems are clickable            
            string ribbonName = PrimitiveUtils.Namify("The Ribbon");
            string[] ribbonAnc = new string[] { "Session", ribbonName };
            string[] sessionAnc = new string[] { "Session" };


            Log.LogWindowElement(this.Name, this.GetType().Name, this.Name, sessionAnc);
            //notice this is hardcoded
            if (this.Ribbon != null)
            {
                Log.LogWindowElement(this.Name, this.Ribbon.GetType().Name, ribbonName, sessionAnc);

                //dump the pages on the ribbon
                ArrayList visiblePages = this.Ribbon.TotalPageCategory.GetVisiblePages();
                foreach (RibbonPage page in visiblePages)
                {
                    page.Name = PrimitiveUtils.Namify(page.Text);
                    page.AccessibleName = page.Name;
                    Log.LogWindowElement(this.Name, page.GetType().Name, page.Name, ribbonAnc);
                }

                //Dump all the BarItems            
                foreach (var item in this.Ribbon.Items)
                {
                    if (item is BarItem)
                    {
                        BarItem barItem = (BarItem)item;
                        barItem.Name = PrimitiveUtils.Namify(barItem.Caption);
                        barItem.AccessibleName = barItem.Name;
                        Log.LogWindowElement(this.Name, barItem.GetType().Name, barItem.Name, ribbonAnc);
                    }
                }
            }

            //Log the form and all control names on the form
            WindowsFormsUtils.LogControlNames(this, this.Controls);
        }

        public DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            XtraMessageBoxArgs args = new XtraMessageBoxArgs
            {
                Caption = caption,
                Text = text
            };

            switch (icon)
            {
                case MessageBoxIcon.Question:
                    args.Icon = StockIconHelper.GetWindows8AssociatedIcon(SystemIcons.Question);
                    break;
                case MessageBoxIcon.Error:
                    args.Icon = StockIconHelper.GetWindows8AssociatedIcon(SystemIcons.Error);
                    break;
                case MessageBoxIcon.Information:
                    args.Icon = StockIconHelper.GetWindows8AssociatedIcon(SystemIcons.Information);
                    break;
                case MessageBoxIcon.Exclamation:
                    args.Icon = StockIconHelper.GetWindows8AssociatedIcon(SystemIcons.Exclamation);
                    break;
                case MessageBoxIcon.None:
                    args.Icon = null;
                    break;
            }
            args.Owner = null;
            //https://supportcenter.devexpress.com/ticket/details/t886969/xtramessageboxargs-how-to-convert-messageboxbuttons-enum-to-args-buttons-property
            args.Buttons = XtraMessageBox.MessageBoxButtonsToDialogResults(buttons);
            args.DefaultButtonIndex = 0;
            return ShowMessageBox(args);
        }
        public DialogResult ShowMessageBox(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            XtraMessageBoxArgs args = new XtraMessageBoxArgs
            {
                Caption = caption,
                Text = text,
                Owner = null,
                //https://supportcenter.devexpress.com/ticket/details/t886969/xtramessageboxargs-how-to-convert-messageboxbuttons-enum-to-args-buttons-property
                Buttons = XtraMessageBox.MessageBoxButtonsToDialogResults(buttons),
                DefaultButtonIndex = 0
            };
            return ShowMessageBox(args);
        }
        public DialogResult ShowMessageBox(string text, string caption)
        {
            if (string.IsNullOrEmpty(caption))
                throw new Exception("Caption is required");
            XtraMessageBoxArgs args = new XtraMessageBoxArgs
            {
                Caption = caption,
                Text = text,
                Owner = this.MdiParent,
                Buttons = new DialogResult[] { DialogResult.OK },
                DefaultButtonIndex = 0
            };
            return ShowMessageBox(args);
        }
        public DialogResult ShowMessageBox(XtraMessageBoxArgs args)
        {
            if (string.IsNullOrEmpty(args.Caption))
                throw new Exception("Caption is required");

            //#AtmtdTsting
            if (Log.SystemTestLogging)
            {
                Log.LogWindowElement(PrimitiveUtils.Namify(args.Caption), nameof(XtraMessageBox), PrimitiveUtils.Namify(args.Caption), new string[] { "Session" });
                foreach (DialogResult button in args.Buttons)
                {
                    Log.LogWindowElement(PrimitiveUtils.Namify(args.Caption), nameof(XtraMessageBox), button.ToString(), new string[] { "Session", PrimitiveUtils.Namify(args.Caption) });
                }
            }
            return XtraMessageBox.Show(args);
        }
        public DialogResult ShowDialogForm(XtraDialogArgs args)
        {
            if (string.IsNullOrEmpty(args.Caption))
                throw new Exception("Caption is required");

            //#AtmtdTsting
            if (Log.SystemTestLogging)
            {
                //#TODO need to do same trick as editer form to assign name to the textedit. 
                WindowsFormsUtils.LogFormControls(this);
                Log.LogWindowElement(PrimitiveUtils.Namify(args.Caption), nameof(XtraDialogArgs), PrimitiveUtils.Namify(args.Caption), new string[] { "Session" });
                foreach (DialogResult button in args.Buttons)
                    Log.LogWindowElement(PrimitiveUtils.Namify(args.Caption), nameof(XtraMessageBox), button.ToString(), new string[] { "Session", PrimitiveUtils.Namify(args.Caption) });

            }
            return DevExpress.XtraEditors.XtraDialog.Show(args);
        }

        /// <summary>
        /// Walk all controls, return the first control that has an error, including an error in a cell in a grid
        /// </summary>
        /// <param name="controls"></param>
        /// <returns>First control that has an error.  Null if no errors.</returns>
        public object ValidateControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control.Controls.Count > 0)
                {
                    object result1 = ValidateControls(control.Controls);
                    if (result1 != null)
                        return result1;
                }

                //if an edit control and is bound to a business object and has an error message, then return this control as the first problem to fix.  
                if (control is BaseEdit && ((BaseEdit)control).DataBindings.Count > 0 && !string.IsNullOrEmpty(((BaseEdit)control).ErrorText))
                {
                    return (BaseEdit)control;
                }
                else if (control is GridControl)
                {
                    //https://supportcenter.devexpress.com/ticket/details/q134301/list-of-error-cells
                    GridView gridView1 = (GridView)((GridControl)control).DefaultView;
                    int errorCount = 0;
                    gridView1.BeginDataUpdate();
                    int rowHandle = gridView1.FocusedRowHandle;
                    for (int j = 0; j < gridView1.RowCount; j++)
                    {
                        gridView1.FocusedRowHandle = j;
                        for (int i = 0; i < gridView1.VisibleColumns.Count; i++)
                        {
                            if (gridView1.GetColumnError(gridView1.VisibleColumns[i]) != String.Empty)
                            {
                                errorCount++;
                            }
                        }
                    }
                    gridView1.FocusedRowHandle = rowHandle;
                    gridView1.EndDataUpdate();
                    if (errorCount > 0)
                        return control;
                }
            }
            return null;
        }

        protected virtual bool ValidateForm(out object firstControlWithError)
        {
            firstControlWithError = ValidateControls(this.Controls);
            return firstControlWithError == null;
        }

        protected virtual bool ValidateForm()
        {
            object ignore;
            bool result = ValidateForm(out ignore);
            return result;
        }

        protected bool ValidateFormAndNotify()
        {
            object firstControlWithError;
            if (!ValidateForm(out firstControlWithError))
            {
                ShowMessageBox("Please correct the errors indicated on the form.", "Form Errors");

                //#RQT Set focus to first input control that has an error so user can make corrections.
                if (firstControlWithError != null && firstControlWithError is BaseEdit)
                    ((BaseEdit)firstControlWithError).Focus();
                return false;
            }
            else
                return true;
        }

        //#TODO BUG I think there's a vulnerability. Scenario. Edit form, [another user deletes the BusObject], Refresh.  
        protected virtual void RefreshForm()
        {
        }

        #endregion form methods

    }
}
