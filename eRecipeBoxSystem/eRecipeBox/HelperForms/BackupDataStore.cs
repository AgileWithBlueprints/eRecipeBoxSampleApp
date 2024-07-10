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
using DevExpress.XtraGrid.Views.Grid;
using SimpleCRUDFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace eRecipeBox.HelperForms
{
    public partial class BackupDataStore : MDIChildForm
    {
        public string BackupName { get; set; }
        public bool ResetOids { get; set; }

        public bool ReplicateBusObjBodies { get; set; }
        public int NumberToReplicate { get; set; }

        public string HeadBusinessObject { get; set; }

        public IList<string> ForwardFKsToReplicate { get; set; }

        public IList<string> DontReplicateTheseBodyParts { get; set; }


        private void InitializeGridView(GridView gridView)
        {
            gridView.OptionsView.ShowIndicator = false;
            gridView.OptionsBehavior.Editable = true;
            gridView.OptionsView.ShowAutoFilterRow = false; // filter at top of each row
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsCustomization.AllowGroup = false;
            gridView.OptionsFind.AllowFindPanel = false;
            gridView.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;  //filter panel at bottom            
            gridView.OptionsSelection.MultiSelect = false;
        }
        public BackupDataStore() : base(nameof(BackupDataStore), null)
        {
            InitializeComponent();
            BackupName = null; ResetOids = false; NumberToReplicate = 1; ReplicateBusObjBodies = false;

            InitializeGridView(this.gridView1);
            InitializeGridView(this.gridView2);

            numberToReplictateSpinEdit.Properties.IsFloatValue = false;
            numberToReplictateSpinEdit.Properties.BeepOnError = true;
            numberToReplictateSpinEdit.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.NumericMaskManager));
            numberToReplictateSpinEdit.Properties.MaskSettings.Set("valueAfterDelete", DevExpress.Data.Mask.NumericMaskManager.ValueAfterDelete.Null);
            numberToReplictateSpinEdit.Properties.MaskSettings.Set("valueType", typeof(int));
            numberToReplictateSpinEdit.Properties.MaskSettings.Set("mask", "#######");


        }

        private void okSimpleButton_Click(object sender, EventArgs e)
        {
            if (this.replicateCheckEdit.Checked && this.resetOIdsCheckEdit.Checked)
            {
                XtraMessageBox.Show("Pick RestOIds or Replicate");
                return;
            }
            BackupName = this.nameTextEdit.Text;
            ResetOids = this.resetOIdsCheckEdit.Checked;
            ReplicateBusObjBodies = this.replicateCheckEdit.Checked;
            NumberToReplicate = Convert.ToInt32(this.numberToReplictateSpinEdit.EditValue);
            HeadBusinessObject = this.headBusObjTextEdit.Text;
            ForwardFKsToReplicate = new List<string>();
            foreach (DataRow row in forwardRefsDataTable.Rows)
            {
                var val = row.ItemArray[0];
                if (val is string && !string.IsNullOrEmpty((string)val))
                    ForwardFKsToReplicate.Add((string)val);
            }

            DontReplicateTheseBodyParts = new List<string>();
            foreach (DataRow row in dontReplicateDataTable.Rows)
            {
                var val = row.ItemArray[0];
                if (val is string && !string.IsNullOrEmpty((string)val))
                    DontReplicateTheseBodyParts.Add((string)val);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelSimpleButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void UpdateViewState()
        {
            if (!replicateCheckEdit.Checked)
            {
                headLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                numberLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                forwardLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                dontReplicateBodyPartsLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            }
            else
            {
                headLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                numberLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                forwardLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                dontReplicateBodyPartsLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            }

        }
        private DataTable forwardRefsDataTable;
        private DataTable dontReplicateDataTable;
        private void BackupDataStore_Load(object sender, EventArgs e)
        {
            forwardRefsDataTable = new DataTable();
            forwardRefsDataTable.Columns.Add("Foreign Keys <table>.<col>", typeof(string));
            for (int i = 0; i < 20; i++)
                forwardRefsDataTable.Rows.Add(new object[] { null });
            this.forwardRefsGridControl.DataSource = forwardRefsDataTable;

            dontReplicateDataTable = new DataTable();
            dontReplicateDataTable.Columns.Add("Table Name", typeof(string));
            for (int i = 0; i < 20; i++)
                dontReplicateDataTable.Rows.Add(new object[] { null });
            this.dontRelicateGridControl.DataSource = dontReplicateDataTable;


            UpdateViewState();
        }

        private void replicateCheckEdit_CheckedChanged(object sender, EventArgs e)
        {
            UpdateViewState();
        }
    }
}