
namespace BidTrainer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelSouth = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenPBN = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSavePBN = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSelectDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.panelNorth = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSouth
            // 
            this.panelSouth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelSouth.Location = new System.Drawing.Point(201, 441);
            this.panelSouth.Name = "panelSouth";
            this.panelSouth.Size = new System.Drawing.Size(313, 97);
            this.panelSouth.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(730, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItemFile
            // 
            this.toolStripMenuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemOpenPBN,
            this.toolStripMenuItemSavePBN,
            this.toolStripMenuItemSelectDatabase});
            this.toolStripMenuItemFile.Name = "toolStripMenuItemFile";
            this.toolStripMenuItemFile.Size = new System.Drawing.Size(37, 20);
            this.toolStripMenuItemFile.Text = "File";
            // 
            // toolStripMenuItemOpenPBN
            // 
            this.toolStripMenuItemOpenPBN.Name = "toolStripMenuItemOpenPBN";
            this.toolStripMenuItemOpenPBN.Size = new System.Drawing.Size(183, 22);
            this.toolStripMenuItemOpenPBN.Text = "Open PBN";
            this.toolStripMenuItemOpenPBN.Click += new System.EventHandler(this.ToolStripMenuItemOpenPBNClick);
            // 
            // toolStripMenuItemSavePBN
            // 
            this.toolStripMenuItemSavePBN.Name = "toolStripMenuItemSavePBN";
            this.toolStripMenuItemSavePBN.Size = new System.Drawing.Size(183, 22);
            this.toolStripMenuItemSavePBN.Text = "Save PBN";
            this.toolStripMenuItemSavePBN.Click += new System.EventHandler(this.ToolStripMenuItemSavePBNClick);
            // 
            // toolStripMenuItemSelectDatabase
            // 
            this.toolStripMenuItemSelectDatabase.Name = "toolStripMenuItemSelectDatabase";
            this.toolStripMenuItemSelectDatabase.Size = new System.Drawing.Size(183, 22);
            this.toolStripMenuItemSelectDatabase.Text = "Select rules database";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // panelNorth
            // 
            this.panelNorth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelNorth.Location = new System.Drawing.Point(201, 40);
            this.panelNorth.Name = "panelNorth";
            this.panelNorth.Size = new System.Drawing.Size(313, 97);
            this.panelNorth.TabIndex = 2;
            this.panelNorth.Visible = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 540);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(730, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Green;
            this.ClientSize = new System.Drawing.Size(730, 562);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panelNorth);
            this.Controls.Add(this.panelSouth);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Bid Trainer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelSouth;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFile;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenPBN;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSavePBN;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectDatabase;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Panel panelNorth;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}

