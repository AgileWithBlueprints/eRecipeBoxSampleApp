
namespace eRecipeBox.HelperForms
{
    partial class RestoreDataStore
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RestoreDataStore));
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.dataLayoutControl1 = new DevExpress.XtraDataLayout.DataLayoutControl();
            this.ConnectionStringTextEdit = new DevExpress.XtraEditors.TextEdit();
            this.restoreSchemaPromptsXPBindingSource = new DevExpress.Xpo.XPBindingSource(this.components);
            this.BackupFolderTextEdit = new DevExpress.XtraEditors.ButtonEdit();
            this.Root = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.ItemForSchemaName = new DevExpress.XtraLayout.LayoutControlItem();
            this.ItemForBackupFolder = new DevExpress.XtraLayout.LayoutControlItem();
            this.simpleSeparator1 = new DevExpress.XtraLayout.SimpleSeparator();
            this.okSimpleButton = new DevExpress.XtraEditors.SimpleButton();
            this.cancelSimpleButton = new DevExpress.XtraEditors.SimpleButton();
            this.xtraFolderBrowserDialog1 = new DevExpress.XtraEditors.XtraFolderBrowserDialog(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataLayoutControl1)).BeginInit();
            this.dataLayoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConnectionStringTextEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.restoreSchemaPromptsXPBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BackupFolderTextEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForSchemaName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForBackupFolder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleSeparator1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.dataLayoutControl1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(4);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(906, 102);
            this.panelControl1.TabIndex = 0;
            // 
            // dataLayoutControl1
            // 
            this.dataLayoutControl1.Controls.Add(this.ConnectionStringTextEdit);
            this.dataLayoutControl1.Controls.Add(this.BackupFolderTextEdit);
            this.dataLayoutControl1.DataSource = this.restoreSchemaPromptsXPBindingSource;
            this.dataLayoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataLayoutControl1.Location = new System.Drawing.Point(2, 2);
            this.dataLayoutControl1.Margin = new System.Windows.Forms.Padding(4);
            this.dataLayoutControl1.Name = "dataLayoutControl1";
            this.dataLayoutControl1.Root = this.Root;
            this.dataLayoutControl1.Size = new System.Drawing.Size(902, 98);
            this.dataLayoutControl1.TabIndex = 0;
            this.dataLayoutControl1.Text = "dataLayoutControl1";
            // 
            // ConnectionStringTextEdit
            // 
            this.ConnectionStringTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.restoreSchemaPromptsXPBindingSource, "ConnectionString", true));
            this.ConnectionStringTextEdit.Location = new System.Drawing.Point(232, 12);
            this.ConnectionStringTextEdit.Margin = new System.Windows.Forms.Padding(4);
            this.ConnectionStringTextEdit.Name = "ConnectionStringTextEdit";
            this.ConnectionStringTextEdit.Size = new System.Drawing.Size(658, 22);
            this.ConnectionStringTextEdit.StyleController = this.dataLayoutControl1;
            this.ConnectionStringTextEdit.TabIndex = 4;
            // 
            // restoreSchemaPromptsXPBindingSource
            // 
            this.restoreSchemaPromptsXPBindingSource.ObjectType = typeof(eRecipeBox.HelperForms.RestoreSchemaPrompts);
            // 
            // BackupFolderTextEdit
            // 
            this.BackupFolderTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.restoreSchemaPromptsXPBindingSource, "BackupFolder", true));
            this.BackupFolderTextEdit.Location = new System.Drawing.Point(232, 39);
            this.BackupFolderTextEdit.Margin = new System.Windows.Forms.Padding(4);
            this.BackupFolderTextEdit.Name = "BackupFolderTextEdit";
            this.BackupFolderTextEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.BackupFolderTextEdit.Size = new System.Drawing.Size(658, 22);
            this.BackupFolderTextEdit.StyleController = this.dataLayoutControl1;
            this.BackupFolderTextEdit.TabIndex = 5;
            this.BackupFolderTextEdit.Click += new System.EventHandler(this.BackupFolderTextEdit_Click);
            // 
            // Root
            // 
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlGroup1});
            this.Root.Name = "Root";
            this.Root.Size = new System.Drawing.Size(902, 98);
            this.Root.TextVisible = false;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.AllowDrawBackground = false;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.ItemForSchemaName,
            this.ItemForBackupFolder,
            this.simpleSeparator1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "autoGeneratedGroup0";
            this.layoutControlGroup1.Size = new System.Drawing.Size(882, 78);
            // 
            // ItemForSchemaName
            // 
            this.ItemForSchemaName.Control = this.ConnectionStringTextEdit;
            this.ItemForSchemaName.CustomizationFormText = "New Schema XPO Connection String";
            this.ItemForSchemaName.Location = new System.Drawing.Point(0, 0);
            this.ItemForSchemaName.Name = "ItemForSchemaName";
            this.ItemForSchemaName.Size = new System.Drawing.Size(882, 26);
            this.ItemForSchemaName.Text = "New Schema XPO Connection String";
            this.ItemForSchemaName.TextSize = new System.Drawing.Size(208, 16);
            // 
            // ItemForBackupFolder
            // 
            this.ItemForBackupFolder.Control = this.BackupFolderTextEdit;
            this.ItemForBackupFolder.Location = new System.Drawing.Point(0, 27);
            this.ItemForBackupFolder.Name = "ItemForBackupFolder";
            this.ItemForBackupFolder.Size = new System.Drawing.Size(882, 51);
            this.ItemForBackupFolder.Text = "Backup Folder To Restore";
            this.ItemForBackupFolder.TextSize = new System.Drawing.Size(208, 16);
            // 
            // simpleSeparator1
            // 
            this.simpleSeparator1.AllowHotTrack = false;
            this.simpleSeparator1.Location = new System.Drawing.Point(0, 26);
            this.simpleSeparator1.Name = "simpleSeparator1";
            this.simpleSeparator1.Size = new System.Drawing.Size(882, 1);
            // 
            // okSimpleButton
            // 
            this.okSimpleButton.Location = new System.Drawing.Point(234, 114);
            this.okSimpleButton.Margin = new System.Windows.Forms.Padding(4);
            this.okSimpleButton.Name = "okSimpleButton";
            this.okSimpleButton.Size = new System.Drawing.Size(88, 28);
            this.okSimpleButton.TabIndex = 1;
            this.okSimpleButton.Text = "OK";
            this.okSimpleButton.Click += new System.EventHandler(this.okSimpleButton_Click);
            // 
            // cancelSimpleButton
            // 
            this.cancelSimpleButton.Location = new System.Drawing.Point(360, 114);
            this.cancelSimpleButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelSimpleButton.Name = "cancelSimpleButton";
            this.cancelSimpleButton.Size = new System.Drawing.Size(88, 28);
            this.cancelSimpleButton.TabIndex = 2;
            this.cancelSimpleButton.Text = "Cancel";
            this.cancelSimpleButton.Click += new System.EventHandler(this.cancelSimpleButton_Click);
            // 
            // xtraFolderBrowserDialog1
            // 
            this.xtraFolderBrowserDialog1.SelectedPath = "xtraFolderBrowserDialog1";
            // 
            // RestoreDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(906, 158);
            this.Controls.Add(this.cancelSimpleButton);
            this.Controls.Add(this.okSimpleButton);
            this.Controls.Add(this.panelControl1);
            this.IconOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("RestoreDB.IconOptions.SvgImage")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RestoreDB";
            this.Text = "Restore DB from Backup";
            this.Load += new System.EventHandler(this.RestoreSchema_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataLayoutControl1)).EndInit();
            this.dataLayoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ConnectionStringTextEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.restoreSchemaPromptsXPBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BackupFolderTextEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForSchemaName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForBackupFolder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleSeparator1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraDataLayout.DataLayoutControl dataLayoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.Xpo.XPBindingSource restoreSchemaPromptsXPBindingSource;
        private DevExpress.XtraEditors.TextEdit ConnectionStringTextEdit;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem ItemForSchemaName;
        private DevExpress.XtraLayout.LayoutControlItem ItemForBackupFolder;
        private DevExpress.XtraEditors.SimpleButton okSimpleButton;
        private DevExpress.XtraEditors.SimpleButton cancelSimpleButton;
        private DevExpress.XtraLayout.SimpleSeparator simpleSeparator1;
        private DevExpress.XtraEditors.XtraFolderBrowserDialog xtraFolderBrowserDialog1;
        private DevExpress.XtraEditors.ButtonEdit BackupFolderTextEdit;
    }
}