namespace eRecipeBox
{
    partial class RecipeBoxMDIParent
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
            this.mdiFormRibbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.parentFormRibbonPageGroup = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.documentManager1 = new DevExpress.XtraBars.Docking2010.DocumentManager(this.components);
            this.nativeMdiView1 = new DevExpress.XtraBars.Docking2010.Views.NativeMdi.NativeMdiView(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.mdiFormRibbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.documentManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nativeMdiView1)).BeginInit();
            this.SuspendLayout();
            // 
            // mdiFormRibbon
            // 
            this.mdiFormRibbon.EmptyAreaImageOptions.ImagePadding = new System.Windows.Forms.Padding(35, 37, 35, 37);
            this.mdiFormRibbon.ExpandCollapseItem.Id = 0;
            this.mdiFormRibbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.mdiFormRibbon.ExpandCollapseItem});
            this.mdiFormRibbon.Location = new System.Drawing.Point(0, 0);
            this.mdiFormRibbon.Margin = new System.Windows.Forms.Padding(4);
            this.mdiFormRibbon.MaxItemId = 1;
            this.mdiFormRibbon.Name = "mdiFormRibbon";
            this.mdiFormRibbon.OptionsMenuMinWidth = 385;
            this.mdiFormRibbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1});
            // 
            // 
            // 
            this.mdiFormRibbon.SearchEditItem.AccessibleName = "Search Item";
            this.mdiFormRibbon.SearchEditItem.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Left;
            this.mdiFormRibbon.SearchEditItem.EditWidth = 150;
            this.mdiFormRibbon.SearchEditItem.Id = -5000;
            this.mdiFormRibbon.SearchEditItem.ImageOptions.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.True;
            this.mdiFormRibbon.SearchEditItem.UseEditorPadding = false;
            this.mdiFormRibbon.Size = new System.Drawing.Size(1177, 169);
            this.mdiFormRibbon.StatusBar = this.ribbonStatusBar;
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.parentFormRibbonPageGroup});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "Home";
            // 
            // parentFormRibbonPageGroup
            // 
            this.parentFormRibbonPageGroup.Name = "parentFormRibbonPageGroup";
            this.parentFormRibbonPageGroup.Text = "ribbonPageGroup1";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 838);
            this.ribbonStatusBar.Margin = new System.Windows.Forms.Padding(4);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.mdiFormRibbon;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1177, 22);
            // 
            // documentManager1
            // 
            this.documentManager1.ContainerControl = this;
            this.documentManager1.MenuManager = this.mdiFormRibbon;
            this.documentManager1.View = this.nativeMdiView1;
            this.documentManager1.ViewCollection.AddRange(new DevExpress.XtraBars.Docking2010.Views.BaseView[] {
            this.nativeMdiView1});
            // 
            // RecipeBoxMDIParent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1177, 860);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.mdiFormRibbon);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RecipeBoxMDIParent";
            this.Ribbon = this.mdiFormRibbon;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "RecipeMDIParent";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RecipeBoxMDIParent_FormClosing);
            this.Load += new System.EventHandler(this.RecipeMDIParent_Load);
            ((System.ComponentModel.ISupportInitialize)(this.mdiFormRibbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.documentManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nativeMdiView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl mdiFormRibbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup parentFormRibbonPageGroup;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraBars.Docking2010.DocumentManager documentManager1;
        private DevExpress.XtraBars.Docking2010.Views.NativeMdi.NativeMdiView nativeMdiView1;
    }
}