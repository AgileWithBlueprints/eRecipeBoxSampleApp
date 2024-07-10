namespace eRecipeBox.HelperForms
{
    partial class ImportRecipeCardPrompt
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
            DevExpress.XtraEditors.TileItemElement tileItemElement1 = new DevExpress.XtraEditors.TileItemElement();
            DevExpress.XtraEditors.TileItemElement tileItemElement2 = new DevExpress.XtraEditors.TileItemElement();
            DevExpress.XtraEditors.TileItemElement tileItemElement3 = new DevExpress.XtraEditors.TileItemElement();
            this.tileControl1 = new DevExpress.XtraEditors.TileControl();
            this.tileGroup1 = new DevExpress.XtraEditors.TileGroup();
            this.importTextItem = new DevExpress.XtraEditors.TileItem();
            this.importAllRecipesItem = new DevExpress.XtraEditors.TileItem();
            this.importGPTItem = new DevExpress.XtraEditors.TileItem();
            this.SuspendLayout();
            // 
            // tileControl1
            // 
            this.tileControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tileControl1.Groups.Add(this.tileGroup1);
            this.tileControl1.Location = new System.Drawing.Point(0, 0);
            this.tileControl1.MaxId = 3;
            this.tileControl1.Name = "tileControl1";
            this.tileControl1.Size = new System.Drawing.Size(578, 268);
            this.tileControl1.TabIndex = 0;
            this.tileControl1.Text = "tileControl1";
            // 
            // tileGroup1
            // 
            this.tileGroup1.Items.Add(this.importTextItem);
            this.tileGroup1.Items.Add(this.importAllRecipesItem);
            this.tileGroup1.Items.Add(this.importGPTItem);
            this.tileGroup1.Name = "tileGroup1";
            this.tileGroup1.Text = "tileGroup1";
            // 
            // importTextItem
            // 
            tileItemElement1.Text = "<size=16>Import Text</size>";
            tileItemElement1.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
            this.importTextItem.Elements.Add(tileItemElement1);
            this.importTextItem.Id = 0;
            this.importTextItem.Name = "importTextItem";
            this.importTextItem.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.ImportTextItem_ItemClick);
            // 
            // importAllRecipesItem
            // 
            tileItemElement2.Text = "<size=16>Import From AllRecipes</size>";
            tileItemElement2.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
            this.importAllRecipesItem.Elements.Add(tileItemElement2);
            this.importAllRecipesItem.Id = 1;
            this.importAllRecipesItem.Name = "importAllRecipesItem";
            this.importAllRecipesItem.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.ImportAllRecipesItem_ItemClick);
            // 
            // importGPTItem
            // 
            tileItemElement3.Text = "<size=16>Ask GPT</size>";
            tileItemElement3.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
            this.importGPTItem.Elements.Add(tileItemElement3);
            this.importGPTItem.Id = 2;
            this.importGPTItem.ItemSize = DevExpress.XtraEditors.TileItemSize.Medium;
            this.importGPTItem.Name = "importGPTItem";
            this.importGPTItem.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.ImportGPTItem_ItemClick);
            // 
            // ImportRecipeCardPrompt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 268);
            this.Controls.Add(this.tileControl1);
            this.Name = "ImportRecipeCardPrompt";
            this.Text = "Select Import Source";
            this.Load += new System.EventHandler(this.ImportRecipeCardPrompt_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.TileControl tileControl1;
        private DevExpress.XtraEditors.TileGroup tileGroup1;
        private DevExpress.XtraEditors.TileItem importTextItem;
        private DevExpress.XtraEditors.TileItem importAllRecipesItem;
        private DevExpress.XtraEditors.TileItem importGPTItem;
    }
}