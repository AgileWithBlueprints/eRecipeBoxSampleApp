namespace eRecipeBox
{
    partial class SelectIngredientsForGroceryList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectIngredientsForGroceryList));
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.addIngrToGLbarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.cancelBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.selectIngrGridControl = new DevExpress.XtraGrid.GridControl();
            this.selectIngrGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectIngrGridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectIngrGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.EmptyAreaImageOptions.ImagePadding = new System.Windows.Forms.Padding(35, 37, 35, 37);
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem,
            this.ribbon.SearchEditItem,
            this.addIngrToGLbarButtonItem,
            this.cancelBarButtonItem});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ribbon.MaxItemId = 3;
            this.ribbon.Name = "ribbon";
            this.ribbon.OptionsMenuMinWidth = 385;
            this.ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1});
            this.ribbon.Size = new System.Drawing.Size(1141, 169);
            this.ribbon.StatusBar = this.ribbonStatusBar;
            // 
            // addIngrToGLbarButtonItem
            // 
            this.addIngrToGLbarButtonItem.Caption = "Add Items to Grocery List";
            this.addIngrToGLbarButtonItem.Id = 1;
            this.addIngrToGLbarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("addIngrToGLbarButtonItem.ImageOptions.SvgImage")));
            this.addIngrToGLbarButtonItem.Name = "addIngrToGLbarButtonItem";
            this.addIngrToGLbarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem1_ItemClick);
            // 
            // cancelBarButtonItem
            // 
            this.cancelBarButtonItem.Caption = "Cancel";
            this.cancelBarButtonItem.Id = 2;
            this.cancelBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("cancelBarButtonItem.ImageOptions.SvgImage")));
            this.cancelBarButtonItem.Name = "cancelBarButtonItem";
            this.cancelBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem2_ItemClick);
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup1});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "Home";
            // 
            // ribbonPageGroup1
            // 
            this.ribbonPageGroup1.ItemLinks.Add(this.cancelBarButtonItem);
            this.ribbonPageGroup1.ItemLinks.Add(this.addIngrToGLbarButtonItem);
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "Ingredients";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 827);
            this.ribbonStatusBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbon;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1141, 22);
            // 
            // selectIngrGridControl
            // 
            this.selectIngrGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectIngrGridControl.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.selectIngrGridControl.Location = new System.Drawing.Point(0, 169);
            this.selectIngrGridControl.MainView = this.selectIngrGridView;
            this.selectIngrGridControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.selectIngrGridControl.MenuManager = this.ribbon;
            this.selectIngrGridControl.Name = "selectIngrGridControl";
            this.selectIngrGridControl.Size = new System.Drawing.Size(1141, 658);
            this.selectIngrGridControl.TabIndex = 2;
            this.selectIngrGridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.selectIngrGridView});
            this.selectIngrGridControl.ProcessGridKey += new System.Windows.Forms.KeyEventHandler(this.gridControl1_ProcessGridKey);
            // 
            // selectIngrGridView
            // 
            this.selectIngrGridView.DetailHeight = 431;
            this.selectIngrGridView.GridControl = this.selectIngrGridControl;
            this.selectIngrGridView.Name = "selectIngrGridView";
            // 
            // SelectIngredientsForGroceryList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1141, 849);
            this.Controls.Add(this.selectIngrGridControl);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbon);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SelectIngredientsForGroceryList";
            this.Ribbon = this.ribbon;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "Select Ingredients";
            this.Load += new System.EventHandler(this.SelectRecipeIngForGL_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectIngrGridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectIngrGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraBars.BarButtonItem addIngrToGLbarButtonItem;
        private DevExpress.XtraGrid.GridControl selectIngrGridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView selectIngrGridView;
        private DevExpress.XtraBars.BarButtonItem cancelBarButtonItem;
    }
}