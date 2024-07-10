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
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using Foundation;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SimpleCRUDFramework
{
    public partial class WindowsFormsUtils
    {
        static public void SetControlName(Control control, string name)
        {
            control.Name = PrimitiveUtils.Namify(name);
            control.AccessibleName = control.Name;
        }

        static public void LogFormControls(System.Windows.Forms.Form form)
        {
            if (string.IsNullOrEmpty(form.Name) && !string.IsNullOrEmpty(form.AccessibleName))
            {
                form.Name = form.AccessibleName;
            }
            form.Name = PrimitiveUtils.Namify(form.Name);
            form.AccessibleName = PrimitiveUtils.Namify(form.AccessibleName);
            string[] sessionAnc = new string[] { "Session" };
            if (Log.SystemTestLogging)
            {
                Log.LogWindowElement(form.Name, form.GetType().Name, form.Name, sessionAnc);
                LogControlNames(form, form.Controls);
            }
        }

        //#AtmtdTsting
        static public object LogControlNames(Form form, Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                //#TRICKY The Ribbon is child of the Session, not the form.  Ribbon is logged separately in EndFormLoad 
                if (control is RibbonControl)
                    continue;

                if (control.Controls.Count > 0)
                {
                    object result1 = LogControlNames(form, control.Controls);
                    if (result1 != null)
                        return result1;
                }

                string controlName = GetName(control);

                //DevEx sets the element name to Text in this case.
                if (control is XtraTabPage)
                    controlName = PrimitiveUtils.Namify(control.Text);

                //Some test script methods refer to the AccessibleName, set it to the control name.
                if (!string.IsNullOrEmpty(controlName) && string.IsNullOrEmpty(control.AccessibleName))
                    control.AccessibleName = controlName;

                if (Log.SystemTestLogging && !(string.IsNullOrEmpty(controlName) && string.IsNullOrEmpty(control.AccessibleName)))
                    Log.LogWindowElement(form.Name, control.GetType().Name, controlName, GetAncestors(control));

                //#TRICKY the Grid's views don't appear as children, so manually dump them 
                if (control is GridControl)
                {
                    List<string> gridViewAncestors = new List<string>(GetAncestors(control));
                    gridViewAncestors.Add(controlName);
                    foreach (GridView gridView in ((GridControl)control).Views)
                    {
                        gridView.AccessibleName = gridView.Name;
                        Log.LogWindowElement(form.Name, gridView.GetType().Name, gridView.Name, gridViewAncestors.ToArray());
                        List<string> columnAncestors = new List<string>(gridViewAncestors);
                        columnAncestors.Add(gridView.Name);
                        foreach (GridColumn column in gridView.Columns)
                        {
                            if (string.IsNullOrEmpty(column.Caption))
                                column.Caption = column.FieldName;
                            string columnName = column.Caption;
                            if (string.IsNullOrEmpty(columnName))
                                throw new Exception($"All gridview cols must have a caption in order to support automated testing. {gridView.Name}  {column.Name}");
                            //#TRICKY Overwrite the design time name to the Caption to match runtime naming convetions.
                            //eg,  colItemDescription is renamed to Item Description
                            column.Name = columnName;
                            column.AccessibleName = columnName;
                            Log.LogWindowElement(form.Name, gridView.GetType().Name, PrimitiveUtils.Namify(columnName), columnAncestors.ToArray());
                        }
                    }
                }
            }
            return null;
        }

        static public string[] GetAncestors(Control control)
        {

            List<string> result = new List<string>();
            Control current = control.Parent;
            while (current != null)
            {
                string currentName = GetName(current);
                if (!string.IsNullOrEmpty(currentName))
                    result.Add(currentName);

                current = current.Parent;
            }
            result.Reverse();

            bool onRibbonForm = WindowsFormsUtils.IsOnRibbonForm(control);
            //#TRICKY Not sure why, but MDI parent doesn't appear in the hierarchy during runtime.  Replace it with root - Session
            if (onRibbonForm && result.Count >= 1)
                result[0] = "Session";
            else
                result.Insert(0, "Session");

            return result.ToArray();
        }
    }
}
