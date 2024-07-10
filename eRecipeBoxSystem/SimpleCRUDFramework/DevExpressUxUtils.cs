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
using DevExpress.Utils;
using DevExpress.Xpo;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Customization;
using DevExpress.XtraTab;
using Foundation;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SimpleCRUDFramework
{
    /// <summary>
    /// Reusable DevExpress control utilities.
    /// </summary>
    public class DevExpressUxUtils
    {
        //https://supportcenter.devexpress.com/ticket/details/t349952/how-to-set-maximum-length-for-a-grid-column
        //set maxlength for text cols

        static public void ConfigureHypertextColumn(GridColumn col, string caption, string sortFieldName)
        {
            RepositoryItemHypertextLabel hyperlinkLabel = new RepositoryItemHypertextLabel();
            hyperlinkLabel.Appearance.TextOptions.WordWrap = WordWrap.NoWrap;
            hyperlinkLabel.Appearance.TextOptions.Trimming = Trimming.EllipsisCharacter;

            col.Visible = true;

            //https://docs.devexpress.com/WindowsForms/DevExpress.XtraGrid.Columns.GridColumn.FieldNameSortGroup?utm_source=SupportCenter&utm_medium=mail&utm_campaign=docs-feedback&utm_content=T1153010
            //https://github.com/DevExpress-Examples/how-to-change-columns-sorting-grouping-logic-sort-group-data-against-another-field-e1511
            col.FieldNameSortGroup = sortFieldName;
            col.OptionsFilter.FilterBySortField = DefaultBoolean.True;
            //gridView1.Columns["HyperLinkTitle"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;

            col.Caption = caption;

            //https://docs.devexpress.com/WindowsForms/4874/common-features/html-text-formatting
            //https://supportcenter.devexpress.com/ticket/details/t405329/simple-html-text-editor
            //https://supportcenter.devexpress.com/ticket/details/q230834/html-in-grid
            //https://docs.devexpress.com/WindowsForms/DevExpress.XtraEditors.Repository.RepositoryItemHypertextLabel
            //https://supportcenter.devexpress.com/ticket/details/t654847/bootstrapgridview-how-do-i-sort-by-the-text-value-of-a-hyperlink-column
            //https://supportcenter.devexpress.com/ticket/details/t848344/troubleshooting-how-to-resolve-sorting-and-filtering-issues-in-gridcontrol
            //https://docs.devexpress.com/WindowsForms/114621/controls-and-libraries/data-grid/getting-started/walkthroughs/sorting/tutorial-sorting-by-values-or-display-text
            col.ColumnEdit = hyperlinkLabel;
            col.SortMode = DevExpress.XtraGrid.ColumnSortMode.DisplayText;
            col.AppearanceCell.TextOptions.WordWrap = WordWrap.NoWrap;
        }
        static public void SetControlName(XtraTabControl control, string name)
        {
            control.Name = name;
            control.AccessibleName = name;
        }
        static public void SetControlName(BaseView control, string name)
        {
            control.Name = name;
            control.AccessibleName = name;
        }
        static public void SetControlName(BaseEdit control, string name)
        {
            control.Name = name;
            control.AccessibleName = name;
        }

        static public void SetControlName(XtraTabPage control, string name)
        {
            control.Name = name;
            control.AccessibleName = name;
        }


        static public void gridView_ShownEditor_SetMaxLength(object sender, EventArgs e)
        {
            GridView view = (GridView)sender;
            string collectionPropertyName = view.FocusedColumn.FieldName;  //eg ingredients on RecipeCard
            if (collectionPropertyName is null)
                return;
            if (!(view.GridControl.DataSource is XPBindingSource && view.GridControl.DataSource != null && view.GridControl.DataMember != null))
                return;
            Type boType = ((XPBindingSource)view.GridControl.DataSource).ObjectType; //eg RecipeCard
            Type collectionType = boType.GetProperty(view.GridControl.DataMember).PropertyType; //eg XPCollection<Ingredient>
            if (collectionType.UnderlyingSystemType.GenericTypeArguments.Length != 1)
                return;
            Type gridClassType = collectionType.UnderlyingSystemType.GenericTypeArguments[0]; //eg Ingredient
            //#TODO ... this is the tricky case where lookupedit in grid is Item!Key... we'll need to get the displayname.  maybe just in processnewvalue
            PropertyInfo pi = gridClassType.GetProperty(collectionPropertyName);
            if (pi == null)
                return;
            object[] attrs = pi.GetCustomAttributes(true);
            foreach (var attribute in attrs)
            {
                if (attribute is SizeAttribute)
                {
                    ((TextEdit)view.ActiveEditor).Properties.MaxLength = ((SizeAttribute)attribute).Size;
                    break;
                }
            }
        }
        //get all bindings
        //https://supportcenter.devexpress.com/ticket/details/t848428/list-of-textedits-in-the-form
        //https://supportcenter.devexpress.com/ticket/details/t224799/get-the-databindings-of-all-controlls-on-a-form
        //https://stackoverflow.com/questions/6637679/reflection-get-attribute-name-and-value-on-property
        /// <summary>
        /// #IMPORTANT Quick and dirty.  Assumes XPBindingSource and XPO's SizeAttribute attached directly (in design time) to the property and only for TextEdit
        /// </summary>
        /// <param name="container"></param>
        static public void SetMaxLengthForAllTextEdits(Control container)
        {
            foreach (Control control in container.Controls)
            {
                if (control is TextEdit)
                {
                    foreach (Binding dataBinding in control.DataBindings)
                    {
                        if (!(dataBinding.DataSource is XPBindingSource))
                            continue;

                        var ds = (XPBindingSource)dataBinding.DataSource;
                        Type boType = ds.ObjectType;
                        string dataMember = dataBinding.BindingMemberInfo.BindingField;
                        if (dataMember is null)
                            continue;
                        var attrs = boType.GetProperty(dataMember).GetCustomAttributes(false);
                        foreach (var attribute in attrs)
                        {
                            if (attribute is SizeAttribute)
                            {
                                ((TextEdit)control).Properties.MaxLength = ((SizeAttribute)attribute).Size;
                                break;
                            }
                        }
                    }
                }
                else
                    SetMaxLengthForAllTextEdits(control);
            }
        }
        static public void SetMaskForAllTextEdits(Control container)
        {
            foreach (Control control in container.Controls)
            {
                if (control is TextEdit)
                {
                    foreach (Binding dataBinding in control.DataBindings)
                    {
                        if (!(dataBinding.DataSource is XPBindingSource))
                            continue;
                        Type boType = ((XPBindingSource)dataBinding.DataSource).ObjectType;
                        string dataMem = dataBinding.BindingMemberInfo.BindingField;
                        if (dataMem is null)
                            continue;
                        string dataMember = dataMem.ToLower();
                        if (dataMember.EndsWith("email"))
                        {
                            SetEmailMask((TextEdit)control);
                        }
                        else if (dataMember.EndsWith("cellnumber") || dataMember.EndsWith("mobilenumber") || dataMember.EndsWith("homenumber"))
                        {
                            SetUSPhoneNumberMask((TextEdit)control);
                        }
                        else if (dataMember.EndsWith("zipcode"))
                        {
                            SetZip5Mask((TextEdit)control);
                        }
                    }
                }
                else
                    SetMaskForAllTextEdits(control);
            }
        }
        static public void InitializeLookUpEdit(LookUpEdit lookupEdit)
        {
            lookupEdit.Properties.AcceptEditorTextAsNewValue = DefaultBoolean.True;
            lookupEdit.Properties.TextEditStyle = TextEditStyles.Standard;
            lookupEdit.Properties.SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter;
            lookupEdit.Properties.PopupFilterMode = PopupFilterMode.Contains;
            lookupEdit.Properties.NullText = "";
            lookupEdit.Properties.ShowNullValuePrompt = ShowNullValuePromptOptions.EmptyValue;
            lookupEdit.Properties.AllowNullInput = DefaultBoolean.True;
        }
        static public void InitializeRepositoryItemLookUpEdit(RepositoryItemLookUpEdit lookupEdit, ProcessNewValueEventHandler lookUpEdit_ProcessNewValue)
        {
            lookupEdit.AcceptEditorTextAsNewValue = DefaultBoolean.True;
            if (!(lookUpEdit_ProcessNewValue is null))
                lookupEdit.ProcessNewValue += new DevExpress.XtraEditors.Controls.ProcessNewValueEventHandler(lookUpEdit_ProcessNewValue);
            lookupEdit.TextEditStyle = TextEditStyles.Standard;
            lookupEdit.SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter;
            lookupEdit.PopupFilterMode = PopupFilterMode.Contains;
            lookupEdit.NullText = "";
            lookupEdit.ShowNullValuePrompt = ShowNullValuePromptOptions.EmptyValue;
            lookupEdit.AllowNullInput = DefaultBoolean.True;
        }
        static public void SetTextOptionsForAllTextEditsInGroup(LayoutControlGroup layoutControlGroup,
            HorzAlignment hAlignment = HorzAlignment.Default, VertAlignment vAlignment = VertAlignment.Default,
            WordWrap wordWrap = WordWrap.Default, Trimming trimming = Trimming.Default)
        {
            //#RICK
            //set text align to near for all textedits
            //Note this is slightly risky as LayoutControlWalker is an internal class and may change in the future.
            //https://supportcenter.devexpress.com/ticket/details/t496103/how-to-traverse-through-all-items-in-layoutcontrol-with-respect-to-their-order-in
            LayoutControlWalker walker = new LayoutControlWalker(layoutControlGroup);
            ArrayList arrayList = walker.ArrangeElements(new OptionsFocus(MoveFocusDirection.AcrossThenDown, false));
            foreach (var element in arrayList)
            {
                if (element is LayoutControlItem)
                {
                    LayoutControlItem item = (LayoutControlItem)element;
                    if (item.Control is TextEdit)
                    {
                        TextEdit te = (TextEdit)item.Control;
                        te.Properties.Appearance.Options.UseTextOptions = true;

                        te.Properties.Appearance.TextOptions.HAlignment = hAlignment;
                        te.Properties.Appearance.TextOptions.Trimming = trimming;
                        te.Properties.Appearance.TextOptions.VAlignment = vAlignment;
                        te.Properties.Appearance.TextOptions.WordWrap = wordWrap;
                    }
                }
            }
        }
        static public void CloseEditing(GridView gridView)
        {
            if (gridView.IsEditing)
            {
                gridView.CloseEditor();  //close editor. current cell saves its changes
                gridView.UpdateCurrentRow();
            }
        }
        static public void SetEmailMask(TextEdit textEdit)
        {
            var emailRegularSettings = textEdit.Properties.MaskSettings.Configure<MaskSettings.RegExp>();
            emailRegularSettings.MaskExpression = PrimitiveUtils.EmailMask;
        }
        static public void SetUSPhoneNumberMask(TextEdit textEdit)
        {
            textEdit.Properties.Mask.EditMask = "(999)-000-0000";
            textEdit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Simple;
            textEdit.Properties.Mask.SaveLiteral = false;
            textEdit.Properties.Mask.UseMaskAsDisplayFormat = true;
        }
        static public void SetZip5Mask(TextEdit textEdit)
        {
            textEdit.Properties.Mask.EditMask = "00000";
            textEdit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Simple;
            textEdit.Properties.Mask.SaveLiteral = false;
            textEdit.Properties.Mask.UseMaskAsDisplayFormat = true;
        }
        static public void ClearAllBindings(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                c.DataBindings.Clear();
                ClearAllBindings(c);
            }
        }
        static public void OpenURLinBrowser(object URL)
        {
            if (URL != null && URL is string)
            {
                Process process = new Process();
                process.StartInfo.FileName = (string)URL;
                process.StartInfo.Verb = "open";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                try
                {
                    process.Start();
                }
                catch { }
            }
        }
        //#RQT highlight text in search results
        //old https://supportcenter.devexpress.com/ticket/details/q265210/highlight-specific-word-in-a-grid-cell
        //better https://supportcenter.devexpress.com/ticket/details/e2508/how-to-filter-the-gridview-across-all-columns-and-highlight-the-matched-text
        static public void ScrollToTop(GridView gridView)
        {
            gridView.TopRowIndex = 0;
            gridView.FocusedRowHandle = 0;
        }
        static public void FocusTo(GridView gridView, int? oid)
        {
            gridView.FocusedRowHandle = gridView.LocateByValue(
                "Oid", oid, rowHandle => gridView.FocusedRowHandle = (int)rowHandle);
        }
        static public void AssignShortcutToButton(BarButtonItem barButtonItem, Keys? modifierKey, Keys key)
        {
            if (modifierKey == null)
            {
                BarShortcut theKey = new BarShortcut(key);
                barButtonItem.ItemShortcut = theKey;
                barButtonItem.ShortcutKeyDisplayString = $"{key.ToString()}";
            }
            else
            {
                BarShortcut theKey = new BarShortcut((Keys)modifierKey | key);
                barButtonItem.ItemShortcut = theKey;
                barButtonItem.ShortcutKeyDisplayString = $"{modifierKey.ToString()}+{key.ToString()}";
            }
        }
        static public string GetStringTextEditValue(BaseEdit te)
        {
            if (te.EditValue == null || te.EditValue is System.DBNull)
                return null;
            else
                return ((string)te.EditValue).Trim();
        }
        static public void SetTextEditValue(TextEdit te, object objValue, DefaultBoolean allowNullInput = DefaultBoolean.True)
        {
            te.Properties.AllowNullInput = allowNullInput;
            te.EditValue = objValue;
        }
        static public void SetTimeSpanAsTotalMinutes(TextEdit te, TimeSpan? tsValue, DefaultBoolean allowNullInput = DefaultBoolean.True)
        {
            string txtMin = null;
            if (tsValue != null)
                txtMin = ((TimeSpan)tsValue).TotalMinutes.ToString();
            SetTextEditValue(te, txtMin, allowNullInput);
        }
        static public void InitLookupEditorBusObjNonPersistent(LookUpEdit lookUpEdit, object dataSource, string displayMember,
            XPObject currentBusObject, DefaultBoolean allowNullInput = DefaultBoolean.True, DefaultBoolean acceptEditorTextAsNewValue = DefaultBoolean.True)
        {
            //highlight not supported
            //https://supportcenter.devexpress.com/ticket/details/t638451/repositoryitemlookupedit-does-not-highlight-contains-matches
            //https://supportcenter.devexpress.com/ticket/details/t267582/how-to-highlight-searched-text-in-searchlookupedit#

            //bind to busobjs
            //https://docs.devexpress.com/WindowsForms/116016/controls-and-libraries/editors-and-simple-controls/lookup-editors/advanced-binding-to-business-objects
            lookUpEdit.Properties.PopupFilterMode = PopupFilterMode.Contains;

            //this helped.  seems to know what he is doing
            //https://titanwolf.org/Network/Articles/Article?AID=e844d3d9-7551-4227-b4e3-414ea1cb0cb6#gsc.tab=0
            lookUpEdit.Properties.SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter;
            lookUpEdit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            if (displayMember != null)
            {
                lookUpEdit.Properties.ValueMember = displayMember;
                lookUpEdit.Properties.DisplayMember = displayMember;
            }
            lookUpEdit.Properties.AllowNullInput = allowNullInput;
            //wow esoteric https://supportcenter.devexpress.com/ticket/details/q446357/populatecolumns-does-not-fill-the-columns-collection-lookupcolumninfocollection-for-a
            lookUpEdit.Properties.NullText = "";
            lookUpEdit.Properties.ShowNullValuePrompt = ShowNullValuePromptOptions.EmptyValue;
            lookUpEdit.Properties.AcceptEditorTextAsNewValue = acceptEditorTextAsNewValue;
            lookUpEdit.Properties.DataSource = null;
            lookUpEdit.Properties.DataSource = dataSource;
            lookUpEdit.Properties.ForceInitialize();
            lookUpEdit.Properties.PopulateColumns();
            if (lookUpEdit.Properties.Columns["Oid"] != null)
                lookUpEdit.Properties.Columns["Oid"].Visible = false;

            if (currentBusObject != null)
                lookUpEdit.EditValue = currentBusObject.Oid;
            else
                lookUpEdit.EditValue = null;

        }
        static public void InitLookupEditorBusObj(LookUpEdit lookUpEdit, object dataSource, string keyMember, string displayMember,
              DefaultBoolean allowNullInput = DefaultBoolean.True)
        {
            //highlight not supported
            //https://supportcenter.devexpress.com/ticket/details/t638451/repositoryitemlookupedit-does-not-highlight-contains-matches
            //https://supportcenter.devexpress.com/ticket/details/t267582/how-to-highlight-searched-text-in-searchlookupedit#

            //bind to busobjs
            //https://docs.devexpress.com/WindowsForms/116016/controls-and-libraries/editors-and-simple-controls/lookup-editors/advanced-binding-to-business-objects
            lookUpEdit.Properties.PopupFilterMode = PopupFilterMode.Contains;

            //this helped.  seems to know what he is doing
            //https://titanwolf.org/Network/Articles/Article?AID=e844d3d9-7551-4227-b4e3-414ea1cb0cb6#gsc.tab=0
            lookUpEdit.Properties.SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter;
            //lookUpEdit.Properties.AutoSearchColumnIndex = 1;  didnt help

            lookUpEdit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            lookUpEdit.Properties.KeyMember = keyMember;
            lookUpEdit.Properties.ValueMember = keyMember; // standard mode
            lookUpEdit.Properties.DisplayMember = displayMember;
            lookUpEdit.Properties.AllowNullInput = allowNullInput;
            //wow esoteric https://supportcenter.devexpress.com/ticket/details/q446357/populatecolumns-does-not-fill-the-columns-collection-lookupcolumninfocollection-for-a
            lookUpEdit.Properties.NullText = "";
            lookUpEdit.Properties.ShowNullValuePrompt = ShowNullValuePromptOptions.EmptyValue;
            lookUpEdit.Properties.AcceptEditorTextAsNewValue = DefaultBoolean.True;
            lookUpEdit.Properties.DataSource = null;
            lookUpEdit.Properties.DataSource = dataSource;
            lookUpEdit.Properties.ForceInitialize();
            lookUpEdit.Properties.PopulateColumns();

            //#TODO REFACTOR pull this from Size() lookUpEdit.Properties.MaxLength like we do in the Grid?
            //#TODO lookUpEdit.Properties.MaxLength = 5;   

            //Hide the key column from the user
            if (lookUpEdit.Properties.Columns[keyMember] != null)
                lookUpEdit.Properties.Columns[keyMember].Visible = false;

            lookUpEdit.EditValue = null;
            lookUpEdit.Text = null;
        }

        //This is how to set a tabstop on MemoEdit
        //https://supportcenter.devexpress.com/ticket/details/t707862/memoedit-how-to-add-a-tab-stop
        //https://supportcenter.devexpress.com/ticket/details/q303731/change-size-of-tab-in-memoedit
        public static void SetTabWidth(TextBox textbox, int tabWidth)
        {
            using (Graphics graphics = textbox.CreateGraphics())
            {
                var characterWidth = (int)graphics.MeasureString("M", textbox.Font).Width;
                SendMessage(textbox.Handle, MessageId.SetTabStops, 1,
                            new int[] { tabWidth * characterWidth });
            }
        }
        private enum MessageId { SetTabStops = 0x00CB, }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, MessageId msg, Int32 wParam, Int32[] lParam);

    }
}
