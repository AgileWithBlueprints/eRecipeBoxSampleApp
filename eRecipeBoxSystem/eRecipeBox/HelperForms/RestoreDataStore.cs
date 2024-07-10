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
using DevExpress.Xpo;
using Foundation;
using SimpleCRUDFramework;
using System;
using System.IO;
using System.Windows.Forms;
namespace eRecipeBox.HelperForms
{
    public partial class RestoreDataStore : MDIChildForm
    {
        public RestoreSchemaPrompts RestoreSchemaPrompts { get; private set; }

        public RestoreDataStore() : base(nameof(RestoreDataStore), null)
        {
            InitializeComponent();

            this.AcceptButton = this.okSimpleButton;
            this.RestoreSchemaPrompts = new RestoreSchemaPrompts();
        }

        private void okSimpleButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RestoreSchemaPrompts.ConnectionString))
            {
                ShowMessageBox("Connection String is required", "Error");
                return;
            }
            this.DialogResult = DialogResult.OK;
        }

        private void cancelSimpleButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BackupFolderTextEdit_Click(object sender, EventArgs e)
        {
            string backupRoot = AppSettings.GetAppSetting("RootDirectoryDataStoreBackups");
            if (string.IsNullOrWhiteSpace(backupRoot))
            {
                ShowMessageBox("RootDirectoryDataStoreBackups is required in config file app settings.", "App.Config Error");
                return;
            }
            DirectoryInfo di = new DirectoryInfo(backupRoot);
            if (!di.Exists)
                ShowMessageBox($"{backupRoot} directory in the config file does not exist.", "Error"); ;
            xtraFolderBrowserDialog1.SelectedPath = di.FullName;
            if (xtraFolderBrowserDialog1.ShowDialog() == DialogResult.OK)
                this.BackupFolderTextEdit.EditValue = xtraFolderBrowserDialog1.SelectedPath;
            else
                this.BackupFolderTextEdit.EditValue = null;
        }

        private void RestoreSchema_Load(object sender, EventArgs e)
        {
            this.restoreSchemaPromptsXPBindingSource.DataSource = RestoreSchemaPrompts;
        }
    }


    public class RestoreSchemaPrompts : XPObject
    {
        public RestoreSchemaPrompts() { }

        private string fConnectionString;

        public string ConnectionString
        {
            get { return fConnectionString; }
            set
            {
                SetPropertyValue(nameof(ConnectionString), ref fConnectionString, PrimitiveUtils.Clean(value));
            }
        }

        private string fBackupFolder;
        public string BackupFolder
        {
            get { return fBackupFolder; }
            set
            {
                SetPropertyValue(nameof(BackupFolder), ref fBackupFolder, PrimitiveUtils.Clean(value));
            }
        }

    }
}