namespace eRecipeBox
{
    partial class ImportGPTRecipeCard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportGPTRecipeCard));
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.cancelBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.askGPTBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.tablePanel1 = new DevExpress.Utils.Layout.TablePanel();
            this.memoEdit1 = new SimpleCRUDFramework.MemoEditWithSpeech();
            this.instructionsLabelControl = new DevExpress.XtraEditors.LabelControl();
            this.statusMessageBarStaticItem = new DevExpress.XtraBars.BarStaticItem();
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).BeginInit();
            this.tablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem,
            this.cancelBarButtonItem,
            this.askGPTBarButtonItem,
            this.statusMessageBarStaticItem});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.MaxItemId = 4;
            this.ribbon.Name = "ribbon";
            this.ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1});
            this.ribbon.Size = new System.Drawing.Size(982, 169);
            this.ribbon.StatusBar = this.ribbonStatusBar;
            // 
            // cancelBarButtonItem
            // 
            this.cancelBarButtonItem.Caption = "Cancel";
            this.cancelBarButtonItem.Id = 1;
            this.cancelBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("cancelBarButtonItem.ImageOptions.SvgImage")));
            this.cancelBarButtonItem.Name = "cancelBarButtonItem";
            this.cancelBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.cancelBarButtonItem_ItemClick);
            // 
            // askGPTBarButtonItem
            // 
            this.askGPTBarButtonItem.Caption = "Ask GPT";
            this.askGPTBarButtonItem.Id = 2;
            this.askGPTBarButtonItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("askGPTBarButtonItem.ImageOptions.SvgImage")));
            this.askGPTBarButtonItem.Name = "askGPTBarButtonItem";
            this.askGPTBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.askGPTBarButtonItem_ItemClick);
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
            this.ribbonPageGroup1.ItemLinks.Add(this.askGPTBarButtonItem);
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "Import";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.ItemLinks.Add(this.statusMessageBarStaticItem);
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 598);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbon;
            this.ribbonStatusBar.Size = new System.Drawing.Size(982, 22);
            // 
            // tablePanel1
            // 
            this.tablePanel1.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] {
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 100F)});
            this.tablePanel1.Controls.Add(this.memoEdit1);
            this.tablePanel1.Controls.Add(this.instructionsLabelControl);
            this.tablePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel1.Location = new System.Drawing.Point(0, 169);
            this.tablePanel1.Name = "tablePanel1";
            this.tablePanel1.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] {
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F)});
            this.tablePanel1.Size = new System.Drawing.Size(982, 429);
            this.tablePanel1.TabIndex = 5;
            this.tablePanel1.UseSkinIndents = true;
            // 
            // memoEdit1
            // 
            this.tablePanel1.SetColumn(this.memoEdit1, 0);
            this.memoEdit1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.memoEdit1.Location = new System.Drawing.Point(13, 38);
            this.memoEdit1.MenuManager = this.ribbon;
            this.memoEdit1.Name = "memoEdit1";
            this.tablePanel1.SetRow(this.memoEdit1, 1);
            this.memoEdit1.Size = new System.Drawing.Size(956, 378);
            this.memoEdit1.TabIndex = 1;
            // 
            // instructionsLabelControl
            // 
            this.instructionsLabelControl.Appearance.Font = new System.Drawing.Font("Tahoma", 11F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.instructionsLabelControl.Appearance.Options.UseFont = true;
            this.instructionsLabelControl.Appearance.Options.UseTextOptions = true;
            this.instructionsLabelControl.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.tablePanel1.SetColumn(this.instructionsLabelControl, 0);
            this.instructionsLabelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.instructionsLabelControl.Location = new System.Drawing.Point(13, 12);
            this.instructionsLabelControl.Name = "instructionsLabelControl";
            this.tablePanel1.SetRow(this.instructionsLabelControl, 0);
            this.instructionsLabelControl.Size = new System.Drawing.Size(956, 22);
            this.instructionsLabelControl.TabIndex = 0;
            this.instructionsLabelControl.Text = "Type or Say your RecipeCard search,then Click \"Ask GPT\"";
            // 
            // statusMessageBarStaticItem
            // 
            this.statusMessageBarStaticItem.Id = 3;
            this.statusMessageBarStaticItem.Name = "statusMessageBarStaticItem";
            // 
            // AskGPTRecipeCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 620);
            this.Controls.Add(this.tablePanel1);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbon);
            this.Name = "AskGPTRecipeCard";
            this.Ribbon = this.ribbon;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "Ask GPT";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AskGPTRecipeCard_FormClosing);
            this.Load += new System.EventHandler(this.AskGPTRecipeCard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).EndInit();
            this.tablePanel1.ResumeLayout(false);
            this.tablePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit1.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraBars.BarButtonItem cancelBarButtonItem;
        private DevExpress.XtraBars.BarButtonItem askGPTBarButtonItem;
        private DevExpress.Utils.Layout.TablePanel tablePanel1;
        private DevExpress.XtraEditors.LabelControl instructionsLabelControl;
        private SimpleCRUDFramework.MemoEditWithSpeech memoEdit1;
        private DevExpress.XtraBars.BarStaticItem statusMessageBarStaticItem;
    }
}