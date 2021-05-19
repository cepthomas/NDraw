
namespace NDraw
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.fileDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.OpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RecentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RenderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Btn_Layer1 = new System.Windows.Forms.ToolStripButton();
            this.Btn_Layer2 = new System.Windows.Forms.ToolStripButton();
            this.Btn_Layer3 = new System.Windows.Forms.ToolStripButton();
            this.Btn_Layer4 = new System.Windows.Forms.ToolStripButton();
            this.canvas = new NDraw.Canvas();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.Txt_Info = new System.Windows.Forms.ToolStripTextBox();
            this.ToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolStrip
            // 
            this.ToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileDropDownButton,
            this.Btn_Layer1,
            this.Btn_Layer2,
            this.Btn_Layer3,
            this.Btn_Layer4,
            this.Txt_Info});
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Size = new System.Drawing.Size(1293, 27);
            this.ToolStrip.TabIndex = 0;
            this.ToolStrip.Text = "ToolStrip";
            // 
            // fileDropDownButton
            // 
            this.fileDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fileDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenMenuItem,
            this.RecentMenuItem,
            this.RenderMenuItem,
            this.SettingsMenuItem});
            this.fileDropDownButton.Image = global::NDraw.Properties.Resources.glyphicons_37_file;
            this.fileDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileDropDownButton.Name = "fileDropDownButton";
            this.fileDropDownButton.Size = new System.Drawing.Size(34, 24);
            this.fileDropDownButton.Text = "File";
            // 
            // OpenMenuItem
            // 
            this.OpenMenuItem.Name = "OpenMenuItem";
            this.OpenMenuItem.Size = new System.Drawing.Size(145, 26);
            this.OpenMenuItem.Text = "Open";
            // 
            // RecentMenuItem
            // 
            this.RecentMenuItem.Name = "RecentMenuItem";
            this.RecentMenuItem.Size = new System.Drawing.Size(145, 26);
            this.RecentMenuItem.Text = "Recent";
            // 
            // RenderMenuItem
            // 
            this.RenderMenuItem.Name = "RenderMenuItem";
            this.RenderMenuItem.Size = new System.Drawing.Size(145, 26);
            this.RenderMenuItem.Text = "Render";
            // 
            // SettingsMenuItem
            // 
            this.SettingsMenuItem.Name = "SettingsMenuItem";
            this.SettingsMenuItem.Size = new System.Drawing.Size(145, 26);
            this.SettingsMenuItem.Text = "Settings";
            // 
            // Btn_Layer1
            // 
            this.Btn_Layer1.BackColor = System.Drawing.SystemColors.Control;
            this.Btn_Layer1.CheckOnClick = true;
            this.Btn_Layer1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Btn_Layer1.Image = ((System.Drawing.Image)(resources.GetObject("Btn_Layer1.Image")));
            this.Btn_Layer1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Btn_Layer1.Name = "Btn_Layer1";
            this.Btn_Layer1.Size = new System.Drawing.Size(29, 24);
            this.Btn_Layer1.Text = "1";
            this.Btn_Layer1.ToolTipText = "Layer 1";
            // 
            // Btn_Layer2
            // 
            this.Btn_Layer2.BackColor = System.Drawing.SystemColors.Control;
            this.Btn_Layer2.CheckOnClick = true;
            this.Btn_Layer2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Btn_Layer2.Image = ((System.Drawing.Image)(resources.GetObject("Btn_Layer2.Image")));
            this.Btn_Layer2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Btn_Layer2.Name = "Btn_Layer2";
            this.Btn_Layer2.Size = new System.Drawing.Size(29, 24);
            this.Btn_Layer2.Text = "2";
            this.Btn_Layer2.ToolTipText = "Layer 2";
            // 
            // Btn_Layer3
            // 
            this.Btn_Layer3.BackColor = System.Drawing.SystemColors.Control;
            this.Btn_Layer3.CheckOnClick = true;
            this.Btn_Layer3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Btn_Layer3.Image = ((System.Drawing.Image)(resources.GetObject("Btn_Layer3.Image")));
            this.Btn_Layer3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Btn_Layer3.Name = "Btn_Layer3";
            this.Btn_Layer3.Size = new System.Drawing.Size(29, 24);
            this.Btn_Layer3.Text = "3";
            this.Btn_Layer3.ToolTipText = "Layer 3";
            // 
            // Btn_Layer4
            // 
            this.Btn_Layer4.BackColor = System.Drawing.SystemColors.Control;
            this.Btn_Layer4.CheckOnClick = true;
            this.Btn_Layer4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Btn_Layer4.Image = ((System.Drawing.Image)(resources.GetObject("Btn_Layer4.Image")));
            this.Btn_Layer4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Btn_Layer4.Name = "Btn_Layer4";
            this.Btn_Layer4.Size = new System.Drawing.Size(29, 24);
            this.Btn_Layer4.Text = "4";
            this.Btn_Layer4.ToolTipText = "Layer 4";
            // 
            // canvas
            // 
            this.canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvas.Location = new System.Drawing.Point(0, 0);
            this.canvas.Name = "canvas";
            this.canvas.Size = new System.Drawing.Size(1018, 626);
            this.canvas.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.rtbLog);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.canvas);
            this.splitContainer1.Size = new System.Drawing.Size(1293, 626);
            this.splitContainer1.SplitterDistance = 271;
            this.splitContainer1.TabIndex = 2;
            // 
            // rtbLog
            // 
            this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Location = new System.Drawing.Point(0, 0);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.Size = new System.Drawing.Size(271, 626);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // Txt_Info
            // 
            this.Txt_Info.Name = "Txt_Info";
            this.Txt_Info.ReadOnly = true;
            this.Txt_Info.Size = new System.Drawing.Size(600, 27);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1293, 653);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.ToolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip ToolStrip;
        private Canvas canvas;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripDropDownButton fileDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem RecentMenuItem;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.ToolStripMenuItem OpenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SettingsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RenderMenuItem;
        private System.Windows.Forms.ToolStripButton Btn_Layer1;
        private System.Windows.Forms.ToolStripButton Btn_Layer2;
        private System.Windows.Forms.ToolStripButton Btn_Layer3;
        private System.Windows.Forms.ToolStripButton Btn_Layer4;
        private System.Windows.Forms.ToolStripTextBox Txt_Info;
    }
}

