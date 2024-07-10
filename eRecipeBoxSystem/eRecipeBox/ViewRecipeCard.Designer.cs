
namespace eRecipeBox
{
    partial class ViewRecipeCard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewRecipeCard));
            DevExpress.XtraGrid.GridLevelNode gridLevelNode1 = new DevExpress.XtraGrid.GridLevelNode();
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.closeVarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.editRecipeBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.deleteRecipeBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.editGroceryListBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.addIngToGLButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.scheduleBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonRefresh = new DevExpress.XtraBars.BarButtonItem();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageGroup2 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.titleHyperLinkEdit = new DevExpress.XtraEditors.HyperLinkEdit();
            this.Root = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.recipeXPBindingSource = new DevExpress.Xpo.XPBindingSource(this.components);
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colOid = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSortOrder = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colQty = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colUoM = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colItemDescription = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemMemoEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            this.instructionsMemoEdit = new DevExpress.XtraEditors.MemoEdit();
            this.itemsXPBindingSource = new DevExpress.Xpo.XPBindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.titleHyperLinkEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).BeginInit();
            this.splitContainerControl1.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).BeginInit();
            this.splitContainerControl1.Panel2.SuspendLayout();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.recipeXPBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.instructionsMemoEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemsXPBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.EmptyAreaImageOptions.ImagePadding = new System.Windows.Forms.Padding(40, 42, 40, 42);
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem,
            this.ribbon.SearchEditItem,
            this.closeVarButtonItem,
            this.editRecipeBarButtonItem,
            this.deleteRecipeBarButtonItem,
            this.editGroceryListBarButtonItem,
            this.addIngToGLButtonItem,
            this.scheduleBarButtonItem,
            this.barButtonRefresh});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ribbon.MaxItemId = 8;
            this.ribbon.Name = "ribbon";
            this.ribbon.OptionsMenuMinWidth = 440;
            this.ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1});
            this.ribbon.Size = new System.Drawing.Size(1253, 177);
            this.ribbon.StatusBar = this.ribbonStatusBar;
            // 
            // closeVarButtonItem
            // 
            this.closeVarButtonItem.Caption = "Close";
            this.closeVarButtonItem.Id = 1;
            this.closeVarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("closeVarButtonItem.ImageOptions.SvgImage")));
            this.closeVarButtonItem.Name = "closeVarButtonItem";
            this.closeVarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem1_ItemClick);
            // 
            // editRecipeBarButtonItem
            // 
            this.editRecipeBarButtonItem.Caption = "Edit Recipe";
            this.editRecipeBarButtonItem.Id = 2;
            this.editRecipeBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("editRecipeBarButtonItem.ImageOptions.SvgImage")));
            this.editRecipeBarButtonItem.Name = "editRecipeBarButtonItem";
            this.editRecipeBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem2_ItemClick);
            // 
            // deleteRecipeBarButtonItem
            // 
            this.deleteRecipeBarButtonItem.Caption = "Delete Recipe";
            this.deleteRecipeBarButtonItem.Id = 3;
            this.deleteRecipeBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("deleteRecipeBarButtonItem.ImageOptions.SvgImage")));
            this.deleteRecipeBarButtonItem.Name = "deleteRecipeBarButtonItem";
            this.deleteRecipeBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem3_ItemClick);
            // 
            // editGroceryListBarButtonItem
            // 
            this.editGroceryListBarButtonItem.Caption = "Open Grocery List";
            this.editGroceryListBarButtonItem.Id = 4;
            this.editGroceryListBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("editGroceryListBarButtonItem.ImageOptions.SvgImage")));
            this.editGroceryListBarButtonItem.Name = "editGroceryListBarButtonItem";
            this.editGroceryListBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem4_ItemClick);
            // 
            // addIngToGLButtonItem
            // 
            this.addIngToGLButtonItem.Caption = "Add Ingredients to Grocery List";
            this.addIngToGLButtonItem.Id = 5;
            this.addIngToGLButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("addIngToGLButtonItem.ImageOptions.SvgImage")));
            this.addIngToGLButtonItem.Name = "addIngToGLButtonItem";
            this.addIngToGLButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem5_ItemClick);
            // 
            // scheduleBarButtonItem
            // 
            this.scheduleBarButtonItem.Caption = "Schedule Recipe";
            this.scheduleBarButtonItem.Id = 6;
            this.scheduleBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("scheduleBarButtonItem.ImageOptions.SvgImage")));
            this.scheduleBarButtonItem.Name = "scheduleBarButtonItem";
            this.scheduleBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.scheduleBarButtonItem_ItemClick);
            // 
            // barButtonRefresh
            // 
            this.barButtonRefresh.Caption = "Refresh";
            this.barButtonRefresh.Id = 7;
            this.barButtonRefresh.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonRefresh.ImageOptions.SvgImage")));
            this.barButtonRefresh.Name = "barButtonRefresh";
            this.barButtonRefresh.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem1_ItemClick_1);
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup1,
            this.ribbonPageGroup2});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "Home";
            // 
            // ribbonPageGroup1
            // 
            this.ribbonPageGroup1.ItemLinks.Add(this.closeVarButtonItem);
            this.ribbonPageGroup1.ItemLinks.Add(this.editRecipeBarButtonItem);
            this.ribbonPageGroup1.ItemLinks.Add(this.scheduleBarButtonItem);
            this.ribbonPageGroup1.ItemLinks.Add(this.deleteRecipeBarButtonItem);
            this.ribbonPageGroup1.ItemLinks.Add(this.barButtonRefresh);
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "Recipe Card";
            // 
            // ribbonPageGroup2
            // 
            this.ribbonPageGroup2.ItemLinks.Add(this.editGroceryListBarButtonItem);
            this.ribbonPageGroup2.ItemLinks.Add(this.addIngToGLButtonItem);
            this.ribbonPageGroup2.Name = "ribbonPageGroup2";
            this.ribbonPageGroup2.Text = "Grocery List";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 980);
            this.ribbonStatusBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbon;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1253, 24);
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.layoutControl1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 177);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1253, 87);
            this.panelControl1.TabIndex = 2;
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.titleHyperLinkEdit);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(2, 2);
            this.layoutControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.Root;
            this.layoutControl1.Size = new System.Drawing.Size(1249, 83);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // titleHyperLinkEdit
            // 
            this.titleHyperLinkEdit.Location = new System.Drawing.Point(16, 17);
            this.titleHyperLinkEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.titleHyperLinkEdit.MenuManager = this.ribbon;
            this.titleHyperLinkEdit.Name = "titleHyperLinkEdit";
            this.titleHyperLinkEdit.Size = new System.Drawing.Size(1217, 24);
            this.titleHyperLinkEdit.StyleController = this.layoutControl1;
            this.titleHyperLinkEdit.TabIndex = 4;
            this.titleHyperLinkEdit.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(this.titleHyperLinkEdit_CustomDisplayText);
            // 
            // Root
            // 
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.emptySpaceItem1});
            this.Root.Name = "Root";
            this.Root.Size = new System.Drawing.Size(1249, 83);
            this.Root.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.titleHyperLinkEdit;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(1223, 30);
            this.layoutControlItem1.Text = "Title";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 30);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(1223, 25);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 264);
            this.splitContainerControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainerControl1.Name = "splitContainerControl1";
            // 
            // splitContainerControl1.Panel1
            // 
            this.splitContainerControl1.Panel1.Controls.Add(this.gridControl1);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            // 
            // splitContainerControl1.Panel2
            // 
            this.splitContainerControl1.Panel2.Controls.Add(this.instructionsMemoEdit);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(1253, 716);
            this.splitContainerControl1.SplitterPosition = 557;
            this.splitContainerControl1.TabIndex = 3;
            // 
            // gridControl1
            // 
            this.gridControl1.DataMember = "Ingredients";
            this.gridControl1.DataSource = this.recipeXPBindingSource;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            gridLevelNode1.RelationName = "Level1";
            this.gridControl1.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[] {
            gridLevelNode1});
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gridControl1.MenuManager = this.ribbon;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemMemoEdit1});
            this.gridControl1.Size = new System.Drawing.Size(557, 716);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // recipeXPBindingSource
            // 
            this.recipeXPBindingSource.ObjectType = typeof(RecipeBoxSolutionModel.RecipeCard);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colOid,
            this.gridColumn1,
            this.colSortOrder,
            this.colQty,
            this.colUoM,
            this.colItemDescription});
            this.gridView1.DetailHeight = 485;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.RowAutoHeight = true;
            this.gridView1.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(this.gridView1_RowStyle);
            // 
            // colOid
            // 
            this.colOid.FieldName = "Oid";
            this.colOid.MinWidth = 27;
            this.colOid.Name = "colOid";
            this.colOid.Width = 100;
            // 
            // gridColumn1
            // 
            this.gridColumn1.FieldName = "RecipeCard!Key";
            this.gridColumn1.MinWidth = 27;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Width = 100;
            // 
            // colSortOrder
            // 
            this.colSortOrder.FieldName = "SortOrder";
            this.colSortOrder.MinWidth = 27;
            this.colSortOrder.Name = "colSortOrder";
            this.colSortOrder.Width = 100;
            // 
            // colQty
            // 
            this.colQty.AppearanceCell.Options.UseTextOptions = true;
            this.colQty.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colQty.FieldName = "Qty";
            this.colQty.MinWidth = 27;
            this.colQty.Name = "colQty";
            this.colQty.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.colQty.Visible = true;
            this.colQty.VisibleIndex = 0;
            this.colQty.Width = 100;
            // 
            // colUoM
            // 
            this.colUoM.FieldName = "UoM";
            this.colUoM.MinWidth = 27;
            this.colUoM.Name = "colUoM";
            this.colUoM.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.colUoM.Visible = true;
            this.colUoM.VisibleIndex = 1;
            this.colUoM.Width = 100;
            // 
            // colItemDescription
            // 
            this.colItemDescription.ColumnEdit = this.repositoryItemMemoEdit1;
            this.colItemDescription.FieldName = "ItemDescription";
            this.colItemDescription.MinWidth = 27;
            this.colItemDescription.Name = "colItemDescription";
            this.colItemDescription.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.colItemDescription.Visible = true;
            this.colItemDescription.VisibleIndex = 2;
            this.colItemDescription.Width = 100;
            // 
            // repositoryItemMemoEdit1
            // 
            this.repositoryItemMemoEdit1.Appearance.Options.UseTextOptions = true;
            this.repositoryItemMemoEdit1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.repositoryItemMemoEdit1.Name = "repositoryItemMemoEdit1";
            // 
            // instructionsMemoEdit
            // 
            this.instructionsMemoEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.instructionsMemoEdit.Location = new System.Drawing.Point(0, 0);
            this.instructionsMemoEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.instructionsMemoEdit.MenuManager = this.ribbon;
            this.instructionsMemoEdit.Name = "instructionsMemoEdit";
            this.instructionsMemoEdit.Size = new System.Drawing.Size(686, 716);
            this.instructionsMemoEdit.TabIndex = 0;
            // 
            // itemsXPBindingSource
            // 
            this.itemsXPBindingSource.DisplayableProperties = "This;Oid;Name";
            this.itemsXPBindingSource.ObjectType = typeof(RecipeBoxSolutionModel.Item);
            // 
            // ViewRecipeCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1253, 1004);
            this.Controls.Add(this.splitContainerControl1);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbon);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ViewRecipeCard";
            this.Ribbon = this.ribbon;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "ViewRecipeCard";
            this.Load += new System.EventHandler(this.ViewRecipe_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.titleHyperLinkEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).EndInit();
            this.splitContainerControl1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).EndInit();
            this.splitContainerControl1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.recipeXPBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.instructionsMemoEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemsXPBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.MemoEdit instructionsMemoEdit;
        private DevExpress.XtraBars.BarButtonItem closeVarButtonItem;
        private DevExpress.XtraBars.BarButtonItem editRecipeBarButtonItem;
        private DevExpress.XtraBars.BarButtonItem deleteRecipeBarButtonItem;
        private DevExpress.XtraBars.BarButtonItem editGroceryListBarButtonItem;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup2;
        private DevExpress.XtraBars.BarButtonItem addIngToGLButtonItem;
        private DevExpress.XtraBars.BarButtonItem scheduleBarButtonItem;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraEditors.HyperLinkEdit titleHyperLinkEdit;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonRefresh;
        private DevExpress.Xpo.XPBindingSource recipeXPBindingSource;
        private DevExpress.XtraGrid.Columns.GridColumn colOid;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn colSortOrder;
        private DevExpress.XtraGrid.Columns.GridColumn colQty;
        private DevExpress.XtraGrid.Columns.GridColumn colUoM;
        private DevExpress.XtraGrid.Columns.GridColumn colItemDescription;
        private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit1;
        private DevExpress.Xpo.XPBindingSource itemsXPBindingSource;
    }
}