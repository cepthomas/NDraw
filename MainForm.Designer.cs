
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
            this.FileDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.OpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RecentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RenderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BtnGrid = new System.Windows.Forms.ToolStripButton();
            this.BtnRuler = new System.Windows.Forms.ToolStripButton();
            this.BtnLayer1 = new System.Windows.Forms.ToolStripButton();
            this.BtnLayer2 = new System.Windows.Forms.ToolStripButton();
            this.BtnLayer3 = new System.Windows.Forms.ToolStripButton();
            this.BtnLayer4 = new System.Windows.Forms.ToolStripButton();
            this.TxtInfo = new System.Windows.Forms.ToolStripTextBox();
            this.Canvas = new NDraw.Canvas();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.RtbLog = new System.Windows.Forms.RichTextBox();
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
            this.FileDropDownButton,
            this.BtnGrid,
            this.BtnRuler,
            this.BtnLayer1,
            this.BtnLayer2,
            this.BtnLayer3,
            this.BtnLayer4,
            this.TxtInfo});
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Size = new System.Drawing.Size(1293, 27);
            this.ToolStrip.TabIndex = 0;
            this.ToolStrip.Text = "ToolStrip";
            // 
            // FileDropDownButton
            // 
            this.FileDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FileDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenMenuItem,
            this.RecentMenuItem,
            this.RenderMenuItem,
            this.SettingsMenuItem,
            this.AboutMenuItem});
            this.FileDropDownButton.Image = global::NDraw.Properties.Resources.glyphicons_37_file;
            this.FileDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FileDropDownButton.Name = "FileDropDownButton";
            this.FileDropDownButton.Size = new System.Drawing.Size(34, 24);
            this.FileDropDownButton.Text = "File";
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
            // AboutMenuItem
            // 
            this.AboutMenuItem.Name = "AboutMenuItem";
            this.AboutMenuItem.Size = new System.Drawing.Size(145, 26);
            this.AboutMenuItem.Text = "About";
            // 
            // BtnGrid
            // 
            this.BtnGrid.CheckOnClick = true;
            this.BtnGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnGrid.Name = "BtnGrid";
            this.BtnGrid.Size = new System.Drawing.Size(41, 24);
            this.BtnGrid.Text = "Grid";
            this.BtnGrid.ToolTipText = "Grid Visible";
            // 
            // BtnRuler
            // 
            this.BtnRuler.CheckOnClick = true;
            this.BtnRuler.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnRuler.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnRuler.Name = "BtnRuler";
            this.BtnRuler.Size = new System.Drawing.Size(47, 24);
            this.BtnRuler.Text = "Ruler";
            this.BtnRuler.ToolTipText = "Ruler Visible";
            // 
            // BtnLayer1
            // 
            this.BtnLayer1.BackColor = System.Drawing.SystemColors.Control;
            this.BtnLayer1.CheckOnClick = true;
            this.BtnLayer1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnLayer1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnLayer1.Name = "BtnLayer1";
            this.BtnLayer1.Size = new System.Drawing.Size(29, 24);
            this.BtnLayer1.Text = "1";
            this.BtnLayer1.ToolTipText = "Layer 1";
            // 
            // BtnLayer2
            // 
            this.BtnLayer2.BackColor = System.Drawing.SystemColors.Control;
            this.BtnLayer2.CheckOnClick = true;
            this.BtnLayer2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnLayer2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnLayer2.Name = "BtnLayer2";
            this.BtnLayer2.Size = new System.Drawing.Size(29, 24);
            this.BtnLayer2.Text = "2";
            this.BtnLayer2.ToolTipText = "Layer 2";
            // 
            // BtnLayer3
            // 
            this.BtnLayer3.BackColor = System.Drawing.SystemColors.Control;
            this.BtnLayer3.CheckOnClick = true;
            this.BtnLayer3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnLayer3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnLayer3.Name = "BtnLayer3";
            this.BtnLayer3.Size = new System.Drawing.Size(29, 24);
            this.BtnLayer3.Text = "3";
            this.BtnLayer3.ToolTipText = "Layer 3";
            // 
            // BtnLayer4
            // 
            this.BtnLayer4.BackColor = System.Drawing.SystemColors.Control;
            this.BtnLayer4.CheckOnClick = true;
            this.BtnLayer4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnLayer4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnLayer4.Name = "BtnLayer4";
            this.BtnLayer4.Size = new System.Drawing.Size(29, 24);
            this.BtnLayer4.Text = "4";
            this.BtnLayer4.ToolTipText = "Layer 4";
            // 
            // TxtInfo
            // 
            this.TxtInfo.Name = "TxtInfo";
            this.TxtInfo.ReadOnly = true;
            this.TxtInfo.Size = new System.Drawing.Size(600, 27);
            // 
            // Canvas
            // 
            this.Canvas.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Canvas.Location = new System.Drawing.Point(0, 0);
            this.Canvas.Name = "Canvas";
            this.Canvas.Size = new System.Drawing.Size(1018, 626);
            this.Canvas.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.RtbLog);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Canvas);
            this.splitContainer1.Size = new System.Drawing.Size(1293, 626);
            this.splitContainer1.SplitterDistance = 271;
            this.splitContainer1.TabIndex = 2;
            // 
            // RtbLog
            // 
            this.RtbLog.BackColor = System.Drawing.Color.Ivory;
            this.RtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RtbLog.Location = new System.Drawing.Point(0, 0);
            this.RtbLog.Name = "RtbLog";
            this.RtbLog.ReadOnly = true;
            this.RtbLog.Size = new System.Drawing.Size(271, 626);
            this.RtbLog.TabIndex = 0;
            this.RtbLog.Text = "";
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
        private Canvas Canvas;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripDropDownButton FileDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem RecentMenuItem;
        private System.Windows.Forms.RichTextBox RtbLog;
        private System.Windows.Forms.ToolStripMenuItem OpenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SettingsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RenderMenuItem;
        private System.Windows.Forms.ToolStripButton BtnLayer1;
        private System.Windows.Forms.ToolStripButton BtnLayer2;
        private System.Windows.Forms.ToolStripButton BtnLayer3;
        private System.Windows.Forms.ToolStripButton BtnLayer4;
        private System.Windows.Forms.ToolStripTextBox TxtInfo;
        private System.Windows.Forms.ToolStripButton BtnGrid;
        private System.Windows.Forms.ToolStripButton BtnRuler;
        private System.Windows.Forms.ToolStripMenuItem AboutMenuItem;
    }
}

