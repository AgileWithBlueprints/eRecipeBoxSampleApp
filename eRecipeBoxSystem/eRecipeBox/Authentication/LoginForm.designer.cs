namespace eRecipeBox 
{
    partial class LoginForm
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
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.dataLayoutControl1 = new DevExpress.XtraDataLayout.DataLayoutControl();
            this.FirstNameTextEdit = new DevExpress.XtraEditors.TextEdit();
            this.userProfileXPBindingSource = new DevExpress.Xpo.XPBindingSource(this.components);
            this.LastNameTextEdit = new DevExpress.XtraEditors.TextEdit();
            this.PersonalEmailTextEdit = new DevExpress.XtraEditors.TextEdit();
            this.PersonalCellNumberTextEdit = new DevExpress.XtraEditors.TextEdit();
            this.HomeZipTextEdit = new DevExpress.XtraEditors.TextEdit();
            this.Root = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.ItemForLastName = new DevExpress.XtraLayout.LayoutControlItem();
            this.ItemForPersonalEmail = new DevExpress.XtraLayout.LayoutControlItem();
            this.ItemForPersonalCellNumber = new DevExpress.XtraLayout.LayoutControlItem();
            this.ItemForHomeZip = new DevExpress.XtraLayout.LayoutControlItem();
            this.simpleLabelItem1 = new DevExpress.XtraLayout.SimpleLabelItem();
            this.ItemForFirstName = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.cancelSimpleButton = new DevExpress.XtraEditors.SimpleButton();
            this.OKSimpleButton = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).BeginInit();
            this.splitContainerControl1.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).BeginInit();
            this.splitContainerControl1.Panel2.SuspendLayout();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataLayoutControl1)).BeginInit();
            this.dataLayoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FirstNameTextEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.userProfileXPBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LastNameTextEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PersonalEmailTextEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PersonalCellNumberTextEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HomeZipTextEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForLastName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForPersonalEmail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForPersonalCellNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForHomeZip)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleLabelItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForFirstName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Horizontal = false;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainerControl1.Name = "splitContainerControl1";
            // 
            // splitContainerControl1.Panel1
            // 
            this.splitContainerControl1.Panel1.Controls.Add(this.dataLayoutControl1);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            // 
            // splitContainerControl1.Panel2
            // 
            this.splitContainerControl1.Panel2.Controls.Add(this.cancelSimpleButton);
            this.splitContainerControl1.Panel2.Controls.Add(this.OKSimpleButton);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(700, 283);
            this.splitContainerControl1.SplitterPosition = 207;
            this.splitContainerControl1.TabIndex = 2;
            // 
            // dataLayoutControl1
            // 
            this.dataLayoutControl1.Controls.Add(this.FirstNameTextEdit);
            this.dataLayoutControl1.Controls.Add(this.LastNameTextEdit);
            this.dataLayoutControl1.Controls.Add(this.PersonalEmailTextEdit);
            this.dataLayoutControl1.Controls.Add(this.PersonalCellNumberTextEdit);
            this.dataLayoutControl1.Controls.Add(this.HomeZipTextEdit);
            this.dataLayoutControl1.DataSource = this.userProfileXPBindingSource;
            this.dataLayoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataLayoutControl1.Location = new System.Drawing.Point(0, 0);
            this.dataLayoutControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dataLayoutControl1.Name = "dataLayoutControl1";
            this.dataLayoutControl1.Root = this.Root;
            this.dataLayoutControl1.Size = new System.Drawing.Size(700, 207);
            this.dataLayoutControl1.TabIndex = 0;
            this.dataLayoutControl1.Text = "dataLayoutControl1";
            // 
            // FirstNameTextEdit
            // 
            this.FirstNameTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.userProfileXPBindingSource, "FirstName", true));
            this.FirstNameTextEdit.Location = new System.Drawing.Point(151, 67);
            this.FirstNameTextEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FirstNameTextEdit.Name = "FirstNameTextEdit";
            this.FirstNameTextEdit.Size = new System.Drawing.Size(535, 22);
            this.FirstNameTextEdit.StyleController = this.dataLayoutControl1;
            this.FirstNameTextEdit.TabIndex = 4;
            // 
            // userProfileXPBindingSource
            // 
            this.userProfileXPBindingSource.ObjectType = typeof(RecipeBoxSolutionModel.UserProfile);
            // 
            // LastNameTextEdit
            // 
            this.LastNameTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.userProfileXPBindingSource, "LastName", true));
            this.LastNameTextEdit.Location = new System.Drawing.Point(151, 93);
            this.LastNameTextEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.LastNameTextEdit.Name = "LastNameTextEdit";
            this.LastNameTextEdit.Size = new System.Drawing.Size(535, 22);
            this.LastNameTextEdit.StyleController = this.dataLayoutControl1;
            this.LastNameTextEdit.TabIndex = 5;
            // 
            // PersonalEmailTextEdit
            // 
            this.PersonalEmailTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.userProfileXPBindingSource, "PersonalEmail", true));
            this.PersonalEmailTextEdit.Location = new System.Drawing.Point(151, 119);
            this.PersonalEmailTextEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PersonalEmailTextEdit.Name = "PersonalEmailTextEdit";
            this.PersonalEmailTextEdit.Size = new System.Drawing.Size(535, 22);
            this.PersonalEmailTextEdit.StyleController = this.dataLayoutControl1;
            this.PersonalEmailTextEdit.TabIndex = 6;
            // 
            // PersonalCellNumberTextEdit
            // 
            this.PersonalCellNumberTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.userProfileXPBindingSource, "PersonalCellNumber", true));
            this.PersonalCellNumberTextEdit.Location = new System.Drawing.Point(151, 145);
            this.PersonalCellNumberTextEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PersonalCellNumberTextEdit.Name = "PersonalCellNumberTextEdit";
            this.PersonalCellNumberTextEdit.Size = new System.Drawing.Size(535, 22);
            this.PersonalCellNumberTextEdit.StyleController = this.dataLayoutControl1;
            this.PersonalCellNumberTextEdit.TabIndex = 7;
            // 
            // HomeZipTextEdit
            // 
            this.HomeZipTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.userProfileXPBindingSource, "HomeZipCode", true));
            this.HomeZipTextEdit.Location = new System.Drawing.Point(151, 171);
            this.HomeZipTextEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.HomeZipTextEdit.Name = "HomeZipTextEdit";
            this.HomeZipTextEdit.Size = new System.Drawing.Size(535, 22);
            this.HomeZipTextEdit.StyleController = this.dataLayoutControl1;
            this.HomeZipTextEdit.TabIndex = 8;
            // 
            // Root
            // 
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlGroup1});
            this.Root.Name = "Root";
            this.Root.Size = new System.Drawing.Size(700, 207);
            this.Root.TextVisible = false;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.AllowDrawBackground = false;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.ItemForLastName,
            this.ItemForPersonalEmail,
            this.ItemForPersonalCellNumber,
            this.ItemForHomeZip,
            this.simpleLabelItem1,
            this.ItemForFirstName,
            this.emptySpaceItem1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "autoGeneratedGroup0";
            this.layoutControlGroup1.Size = new System.Drawing.Size(676, 183);
            // 
            // ItemForLastName
            // 
            this.ItemForLastName.Control = this.LastNameTextEdit;
            this.ItemForLastName.Location = new System.Drawing.Point(0, 79);
            this.ItemForLastName.Name = "ItemForLastName";
            this.ItemForLastName.Size = new System.Drawing.Size(676, 26);
            this.ItemForLastName.Text = "Last Name";
            this.ItemForLastName.TextSize = new System.Drawing.Size(123, 16);
            // 
            // ItemForPersonalEmail
            // 
            this.ItemForPersonalEmail.Control = this.PersonalEmailTextEdit;
            this.ItemForPersonalEmail.Location = new System.Drawing.Point(0, 105);
            this.ItemForPersonalEmail.Name = "ItemForPersonalEmail";
            this.ItemForPersonalEmail.Size = new System.Drawing.Size(676, 26);
            this.ItemForPersonalEmail.Text = "Personal Email";
            this.ItemForPersonalEmail.TextSize = new System.Drawing.Size(123, 16);
            // 
            // ItemForPersonalCellNumber
            // 
            this.ItemForPersonalCellNumber.Control = this.PersonalCellNumberTextEdit;
            this.ItemForPersonalCellNumber.Location = new System.Drawing.Point(0, 131);
            this.ItemForPersonalCellNumber.Name = "ItemForPersonalCellNumber";
            this.ItemForPersonalCellNumber.Size = new System.Drawing.Size(676, 26);
            this.ItemForPersonalCellNumber.Text = "Personal Cell Number";
            this.ItemForPersonalCellNumber.TextSize = new System.Drawing.Size(123, 16);
            // 
            // ItemForHomeZip
            // 
            this.ItemForHomeZip.Control = this.HomeZipTextEdit;
            this.ItemForHomeZip.Location = new System.Drawing.Point(0, 157);
            this.ItemForHomeZip.Name = "ItemForHomeZip";
            this.ItemForHomeZip.Size = new System.Drawing.Size(676, 26);
            this.ItemForHomeZip.Text = "Home Zip";
            this.ItemForHomeZip.TextSize = new System.Drawing.Size(123, 16);
            // 
            // simpleLabelItem1
            // 
            this.simpleLabelItem1.AllowHotTrack = false;
            this.simpleLabelItem1.AppearanceItemCaption.Options.UseTextOptions = true;
            this.simpleLabelItem1.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.simpleLabelItem1.Location = new System.Drawing.Point(0, 0);
            this.simpleLabelItem1.Name = "simpleLabelItem1";
            this.simpleLabelItem1.Size = new System.Drawing.Size(676, 20);
            this.simpleLabelItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.SupportHorzAlignment;
            this.simpleLabelItem1.Text = "Login ";
            this.simpleLabelItem1.TextSize = new System.Drawing.Size(123, 16);
            // 
            // ItemForFirstName
            // 
            this.ItemForFirstName.Control = this.FirstNameTextEdit;
            this.ItemForFirstName.Location = new System.Drawing.Point(0, 53);
            this.ItemForFirstName.Name = "ItemForFirstName";
            this.ItemForFirstName.Size = new System.Drawing.Size(676, 26);
            this.ItemForFirstName.Text = "First Name";
            this.ItemForFirstName.TextSize = new System.Drawing.Size(123, 16);
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 20);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(676, 33);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // cancelSimpleButton
            // 
            this.cancelSimpleButton.Location = new System.Drawing.Point(374, 9);
            this.cancelSimpleButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cancelSimpleButton.Name = "cancelSimpleButton";
            this.cancelSimpleButton.Size = new System.Drawing.Size(88, 28);
            this.cancelSimpleButton.TabIndex = 1;
            this.cancelSimpleButton.Text = "Cancel";
            this.cancelSimpleButton.Click += new System.EventHandler(this.CancelSimpleButton_Click);
            // 
            // OKSimpleButton
            // 
            this.OKSimpleButton.Location = new System.Drawing.Point(178, 9);
            this.OKSimpleButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.OKSimpleButton.Name = "OKSimpleButton";
            this.OKSimpleButton.Size = new System.Drawing.Size(88, 28);
            this.OKSimpleButton.TabIndex = 0;
            this.OKSimpleButton.Text = "OK";
            this.OKSimpleButton.Click += new System.EventHandler(this.OKSimpleButton_Click);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 283);
            this.Controls.Add(this.splitContainerControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "LoginForm";
            this.Text = "Create User";
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).EndInit();
            this.splitContainerControl1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).EndInit();
            this.splitContainerControl1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataLayoutControl1)).EndInit();
            this.dataLayoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FirstNameTextEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.userProfileXPBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LastNameTextEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PersonalEmailTextEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PersonalCellNumberTextEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HomeZipTextEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForLastName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForPersonalEmail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForPersonalCellNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForHomeZip)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleLabelItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemForFirstName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraDataLayout.DataLayoutControl dataLayoutControl1;
        private DevExpress.Xpo.XPBindingSource userProfileXPBindingSource;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.XtraEditors.SimpleButton cancelSimpleButton;
        private DevExpress.XtraEditors.SimpleButton OKSimpleButton;
        private DevExpress.XtraEditors.TextEdit FirstNameTextEdit;
        private DevExpress.XtraEditors.TextEdit LastNameTextEdit;
        private DevExpress.XtraEditors.TextEdit PersonalEmailTextEdit;
        private DevExpress.XtraEditors.TextEdit PersonalCellNumberTextEdit;
        private DevExpress.XtraEditors.TextEdit HomeZipTextEdit;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem ItemForFirstName;
        private DevExpress.XtraLayout.LayoutControlItem ItemForLastName;
        private DevExpress.XtraLayout.LayoutControlItem ItemForPersonalEmail;
        private DevExpress.XtraLayout.LayoutControlItem ItemForPersonalCellNumber;
        private DevExpress.XtraLayout.LayoutControlItem ItemForHomeZip;
        private DevExpress.XtraLayout.SimpleLabelItem simpleLabelItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
    }
}