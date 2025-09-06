
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
            ToolStrip = new System.Windows.Forms.ToolStrip();
            FileDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            OpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            RecentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            RenderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            SettingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            AboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            BtnGrid = new System.Windows.Forms.ToolStripButton();
            BtnRuler = new System.Windows.Forms.ToolStripButton();
            MyCanvas = new Canvas();
            RtbLog = new System.Windows.Forms.RichTextBox();
            ToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // ToolStrip
            // 
            ToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { FileDropDownButton, BtnGrid, BtnRuler });
            ToolStrip.Location = new System.Drawing.Point(0, 0);
            ToolStrip.Name = "ToolStrip";
            ToolStrip.Size = new System.Drawing.Size(858, 27);
            ToolStrip.TabIndex = 0;
            ToolStrip.Text = "ToolStrip";
            // 
            // FileDropDownButton
            // 
            FileDropDownButton.AutoToolTip = false;
            FileDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            FileDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { OpenMenuItem, RecentMenuItem, RenderMenuItem, SettingsMenuItem, AboutMenuItem });
            FileDropDownButton.Image = Properties.Resources.glyphicons_37_file;
            FileDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            FileDropDownButton.Name = "FileDropDownButton";
            FileDropDownButton.Size = new System.Drawing.Size(34, 24);
            FileDropDownButton.Text = "File";
            // 
            // OpenMenuItem
            // 
            OpenMenuItem.Name = "OpenMenuItem";
            OpenMenuItem.Size = new System.Drawing.Size(145, 26);
            OpenMenuItem.Text = "Open";
            // 
            // RecentMenuItem
            // 
            RecentMenuItem.Name = "RecentMenuItem";
            RecentMenuItem.Size = new System.Drawing.Size(145, 26);
            RecentMenuItem.Text = "Recent";
            // 
            // RenderMenuItem
            // 
            RenderMenuItem.Name = "RenderMenuItem";
            RenderMenuItem.Size = new System.Drawing.Size(145, 26);
            RenderMenuItem.Text = "Render";
            // 
            // SettingsMenuItem
            // 
            SettingsMenuItem.Name = "SettingsMenuItem";
            SettingsMenuItem.Size = new System.Drawing.Size(145, 26);
            SettingsMenuItem.Text = "Settings";
            // 
            // AboutMenuItem
            // 
            AboutMenuItem.Name = "AboutMenuItem";
            AboutMenuItem.Size = new System.Drawing.Size(145, 26);
            AboutMenuItem.Text = "About";
            // 
            // BtnGrid
            // 
            BtnGrid.CheckOnClick = true;
            BtnGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            BtnGrid.Name = "BtnGrid";
            BtnGrid.Size = new System.Drawing.Size(41, 24);
            BtnGrid.Text = "Grid";
            BtnGrid.ToolTipText = "Grid Visible";
            // 
            // BtnRuler
            // 
            BtnRuler.CheckOnClick = true;
            BtnRuler.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnRuler.ImageTransparentColor = System.Drawing.Color.Magenta;
            BtnRuler.Name = "BtnRuler";
            BtnRuler.Size = new System.Drawing.Size(47, 24);
            BtnRuler.Text = "Ruler";
            BtnRuler.ToolTipText = "Ruler Visible";
            // 
            // Canvas
            // 
            MyCanvas.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            MyCanvas.BackColor = System.Drawing.Color.WhiteSmoke;
            MyCanvas.Location = new System.Drawing.Point(12, 30);
            MyCanvas.Name = "Canvas";
            MyCanvas.Size = new System.Drawing.Size(832, 466);
            MyCanvas.TabIndex = 1;
            // 
            // RtbLog
            // 
            RtbLog.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            RtbLog.BackColor = System.Drawing.Color.Ivory;
            RtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            RtbLog.Location = new System.Drawing.Point(12, 502);
            RtbLog.Name = "RtbLog";
            RtbLog.ReadOnly = true;
            RtbLog.Size = new System.Drawing.Size(832, 139);
            RtbLog.TabIndex = 0;
            RtbLog.Text = "";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(858, 653);
            Controls.Add(MyCanvas);
            Controls.Add(RtbLog);
            Controls.Add(ToolStrip);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Form1";
            ToolStrip.ResumeLayout(false);
            ToolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip ToolStrip;
        private Canvas MyCanvas;
        private System.Windows.Forms.ToolStripDropDownButton FileDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem RecentMenuItem;
        private System.Windows.Forms.RichTextBox RtbLog;
        private System.Windows.Forms.ToolStripMenuItem OpenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SettingsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RenderMenuItem;
        private System.Windows.Forms.ToolStripButton BtnGrid;
        private System.Windows.Forms.ToolStripButton BtnRuler;
        private System.Windows.Forms.ToolStripMenuItem AboutMenuItem;
    }
}

