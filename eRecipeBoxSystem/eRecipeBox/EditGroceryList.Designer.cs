
namespace eRecipeBox
{
    partial class EditGroceryList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditGroceryList));
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.barButtonItemSaveAndClose = new DevExpress.XtraBars.BarButtonItem();
            this.deleteItemBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.clearBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.emailBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.statusBarStaticItem = new DevExpress.XtraBars.BarStaticItem();
            this.setFocusToInputButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemSave = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemCancel = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemRefresh = new DevExpress.XtraBars.BarButtonItem();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.addSimpleButton = new DevExpress.XtraEditors.SimpleButton();
            this.itemLookUpEdit = new DevExpress.XtraEditors.LookUpEdit();
            this.groceryListGridControl = new DevExpress.XtraGrid.GridControl();
            this.groceryListXPBindingSource = new DevExpress.Xpo.XPBindingSource(this.components);
            this.groceryListGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colOid = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colItem = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colQty = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colUoM = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colItemDescription = new DevExpress.XtraGrid.Columns.GridColumn();
            this.itemsXPBindingSource = new DevExpress.Xpo.XPBindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.itemLookUpEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groceryListGridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groceryListXPBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groceryListGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemsXPBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.EmptyAreaImageOptions.ImagePadding = new System.Windows.Forms.Padding(35, 37, 35, 37);
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem,
            this.barButtonItemSaveAndClose,
            this.deleteItemBarButtonItem,
            this.clearBarButtonItem,
            this.emailBarButtonItem,
            this.statusBarStaticItem,
            this.setFocusToInputButtonItem,
            this.barButtonItemSave,
            this.barButtonItemCancel,
            this.barButtonItemRefresh});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ribbon.MaxItemId = 10;
            this.ribbon.Name = "ribbon";
            this.ribbon.OptionsMenuMinWidth = 385;
            this.ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1});
            this.ribbon.Size = new System.Drawing.Size(887, 169);
            this.ribbon.StatusBar = this.ribbonStatusBar;
            // 
            // barButtonItemSaveAndClose
            // 
            this.barButtonItemSaveAndClose.Caption = "Save And Close";
            this.barButtonItemSaveAndClose.Id = 1;
            this.barButtonItemSaveAndClose.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemSaveAndClose.ImageOptions.SvgImage")));
            this.barButtonItemSaveAndClose.Name = "barButtonItemSaveAndClose";
            this.barButtonItemSaveAndClose.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemSaveAndClose_ItemClick);
            // 
            // deleteItemBarButtonItem
            // 
            this.deleteItemBarButtonItem.Caption = "Delete Item(s)";
            this.deleteItemBarButtonItem.Id = 2;
            this.deleteItemBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("deleteItemBarButtonItem.ImageOptions.SvgImage")));
            this.deleteItemBarButtonItem.Name = "deleteItemBarButtonItem";
            this.deleteItemBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.deleteItemBarButtonItem_ItemClick);
            // 
            // clearBarButtonItem
            // 
            this.clearBarButtonItem.Caption = "Clear All";
            this.clearBarButtonItem.Id = 3;
            this.clearBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("clearBarButtonItem.ImageOptions.SvgImage")));
            this.clearBarButtonItem.Name = "clearBarButtonItem";
            this.clearBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.clearBarButtonItem_ItemClick);
            // 
            // emailBarButtonItem
            // 
            this.emailBarButtonItem.Caption = "Email";
            this.emailBarButtonItem.Id = 4;
            this.emailBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("emailBarButtonItem.ImageOptions.SvgImage")));
            this.emailBarButtonItem.Name = "emailBarButtonItem";
            this.emailBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.emailBarButtonItem_ItemClick);
            // 
            // statusBarStaticItem
            // 
            this.statusBarStaticItem.Id = 5;
            this.statusBarStaticItem.Name = "statusBarStaticItem";
            // 
            // setFocusToInputButtonItem
            // 
            this.setFocusToInputButtonItem.Caption = "SetFocusToInput";
            this.setFocusToInputButtonItem.Id = 6;
            this.setFocusToInputButtonItem.Name = "setFocusToInputButtonItem";
            this.setFocusToInputButtonItem.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            this.setFocusToInputButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.setFocusToInputButtonItem_ItemClick);
            // 
            // barButtonItemSave
            // 
            this.barButtonItemSave.Caption = "Save";
            this.barButtonItemSave.Id = 7;
            this.barButtonItemSave.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemSave.ImageOptions.SvgImage")));
            this.barButtonItemSave.Name = "barButtonItemSave";
            this.barButtonItemSave.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemSave_ItemClick);
            // 
            // barButtonItemCancel
            // 
            this.barButtonItemCancel.Caption = "Cancel";
            this.barButtonItemCancel.Id = 8;
            this.barButtonItemCancel.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemCancel.ImageOptions.SvgImage")));
            this.barButtonItemCancel.Name = "barButtonItemCancel";
            this.barButtonItemCancel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemCancel_ItemClick);
            // 
            // barButtonItemRefresh
            // 
            this.barButtonItemRefresh.Caption = "Refresh";
            this.barButtonItemRefresh.Id = 9;
            this.barButtonItemRefresh.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemRefresh.ImageOptions.SvgImage")));
            this.barButtonItemRefresh.Name = "barButtonItemRefresh";
            this.barButtonItemRefresh.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemRefresh_ItemClick);
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
            this.ribbonPageGroup1.ItemLinks.Add(this.barButtonItemSaveAndClose);
            this.ribbonPageGroup1.ItemLinks.Add(this.barButtonItemSave);
            this.ribbonPageGroup1.ItemLinks.Add(this.barButtonItemCancel);
            this.ribbonPageGroup1.ItemLinks.Add(this.deleteItemBarButtonItem);
            this.ribbonPageGroup1.ItemLinks.Add(this.clearBarButtonItem);
            this.ribbonPageGroup1.ItemLinks.Add(this.emailBarButtonItem);
            this.ribbonPageGroup1.ItemLinks.Add(this.barButtonItemRefresh);
            this.ribbonPageGroup1.ItemLinks.Add(this.setFocusToInputButtonItem);
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "Grocery List";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.ItemLinks.Add(this.statusBarStaticItem);
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 511);
            this.ribbonStatusBar.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbon;
            this.ribbonStatusBar.Size = new System.Drawing.Size(887, 22);
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.addSimpleButton);
            this.panelControl1.Controls.Add(this.itemLookUpEdit);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 169);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(887, 77);
            this.panelControl1.TabIndex = 2;
            // 
            // addSimpleButton
            // 
            this.addSimpleButton.Location = new System.Drawing.Point(345, 24);
            this.addSimpleButton.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.addSimpleButton.Name = "addSimpleButton";
            this.addSimpleButton.Size = new System.Drawing.Size(89, 24);
            this.addSimpleButton.TabIndex = 1;
            this.addSimpleButton.Text = "Add";
            this.addSimpleButton.Click += new System.EventHandler(this.addSimpleButton_Click);
            // 
            // itemLookUpEdit
            // 
            this.itemLookUpEdit.Location = new System.Drawing.Point(6, 24);
            this.itemLookUpEdit.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.itemLookUpEdit.MenuManager = this.ribbon;
            this.itemLookUpEdit.Name = "itemLookUpEdit";
            this.itemLookUpEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.itemLookUpEdit.Size = new System.Drawing.Size(316, 22);
            this.itemLookUpEdit.TabIndex = 0;
            this.itemLookUpEdit.ProcessNewValue += new DevExpress.XtraEditors.Controls.ProcessNewValueEventHandler(this.itemLookUpEdit_ProcessNewValue);
            this.itemLookUpEdit.Closed += new DevExpress.XtraEditors.Controls.ClosedEventHandler(this.itemLookUpEdit_Closed);
            this.itemLookUpEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.itemLookUpEdit_KeyDown);
            // 
            // groceryListGridControl
            // 
            this.groceryListGridControl.DataMember = "GroceryListItems";
            this.groceryListGridControl.DataSource = this.groceryListXPBindingSource;
            this.groceryListGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groceryListGridControl.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.groceryListGridControl.Location = new System.Drawing.Point(0, 246);
            this.groceryListGridControl.MainView = this.groceryListGridView;
            this.groceryListGridControl.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.groceryListGridControl.MenuManager = this.ribbon;
            this.groceryListGridControl.Name = "groceryListGridControl";
            this.groceryListGridControl.Size = new System.Drawing.Size(887, 265);
            this.groceryListGridControl.TabIndex = 6;
            this.groceryListGridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.groceryListGridView});
            // 
            // groceryListXPBindingSource
            // 
            this.groceryListXPBindingSource.ObjectType = typeof(RecipeBoxSolutionModel.GroceryList);
            // 
            // groceryListGridView
            // 
            this.groceryListGridView.ColumnPanelRowHeight = 0;
            this.groceryListGridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colOid,
            this.colItem,
            this.colQty,
            this.colUoM,
            this.colItemDescription});
            this.groceryListGridView.DetailHeight = 431;
            this.groceryListGridView.FooterPanelHeight = 0;
            this.groceryListGridView.GridControl = this.groceryListGridControl;
            this.groceryListGridView.GroupRowHeight = 0;
            this.groceryListGridView.LevelIndent = 0;
            this.groceryListGridView.Name = "groceryListGridView";
            this.groceryListGridView.PreviewIndent = 0;
            this.groceryListGridView.RowHeight = 0;
            this.groceryListGridView.ViewCaptionHeight = 0;
            this.groceryListGridView.SelectionChanged += new DevExpress.Data.SelectionChangedEventHandler(this.gridView1_SelectionChanged);
            this.groceryListGridView.DataSourceChanged += new System.EventHandler(this.gridView1_DataSourceChanged);
            this.groceryListGridView.RowCountChanged += new System.EventHandler(this.gridView1_RowCountChanged);
            // 
            // colOid
            // 
            this.colOid.FieldName = "Oid";
            this.colOid.MinWidth = 19;
            this.colOid.Name = "colOid";
            this.colOid.Width = 74;
            // 
            // colItem
            // 
            this.colItem.FieldName = "Item";
            this.colItem.MinWidth = 19;
            this.colItem.Name = "colItem";
            this.colItem.Visible = true;
            this.colItem.VisibleIndex = 0;
            this.colItem.Width = 74;
            // 
            // colQty
            // 
            this.colQty.FieldName = "Qty";
            this.colQty.MinWidth = 19;
            this.colQty.Name = "colQty";
            this.colQty.Visible = true;
            this.colQty.VisibleIndex = 1;
            this.colQty.Width = 74;
            // 
            // colUoM
            // 
            this.colUoM.FieldName = "UoM";
            this.colUoM.MinWidth = 19;
            this.colUoM.Name = "colUoM";
            this.colUoM.Visible = true;
            this.colUoM.VisibleIndex = 2;
            this.colUoM.Width = 74;
            // 
            // colItemDescription
            // 
            this.colItemDescription.FieldName = "ItemDescription";
            this.colItemDescription.MinWidth = 19;
            this.colItemDescription.Name = "colItemDescription";
            this.colItemDescription.Visible = true;
            this.colItemDescription.VisibleIndex = 3;
            this.colItemDescription.Width = 74;
            // 
            // itemsXPBindingSource
            // 
            this.itemsXPBindingSource.DisplayableProperties = "This;Oid;Name";
            this.itemsXPBindingSource.ObjectType = typeof(RecipeBoxSolutionModel.Item);
            // 
            // EditGroceryList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 533);
            this.Controls.Add(this.groceryListGridControl);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbon);
            this.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.Name = "EditGroceryList";
            this.Ribbon = this.ribbon;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "EditGroceryList";
            this.Load += new System.EventHandler(this.EditGroceryList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.itemLookUpEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groceryListGridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groceryListXPBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groceryListGridView)).EndInit();
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
        private DevExpress.XtraEditors.SimpleButton addSimpleButton;
        private DevExpress.XtraEditors.LookUpEdit itemLookUpEdit;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSaveAndClose;
        private DevExpress.XtraBars.BarButtonItem deleteItemBarButtonItem;
        private DevExpress.XtraBars.BarButtonItem clearBarButtonItem;
        private DevExpress.XtraBars.BarButtonItem emailBarButtonItem;
        private DevExpress.XtraBars.BarStaticItem statusBarStaticItem;
        private DevExpress.XtraBars.BarButtonItem setFocusToInputButtonItem;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSave;
        private DevExpress.XtraBars.BarButtonItem barButtonItemCancel;
        private DevExpress.XtraBars.BarButtonItem barButtonItemRefresh;
        private DevExpress.XtraGrid.GridControl groceryListGridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView groceryListGridView;
        private DevExpress.Xpo.XPBindingSource groceryListXPBindingSource;
        private DevExpress.XtraGrid.Columns.GridColumn colOid;
        private DevExpress.XtraGrid.Columns.GridColumn colQty;
        private DevExpress.XtraGrid.Columns.GridColumn colUoM;
        private DevExpress.XtraGrid.Columns.GridColumn colItemDescription;
        private DevExpress.XtraGrid.Columns.GridColumn colItem;
        private DevExpress.Xpo.XPBindingSource itemsXPBindingSource;
    }
}