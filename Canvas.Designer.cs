
namespace NDraw
{
    partial class Canvas
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblInfo = new System.Windows.Forms.Label();
            this.rtb = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(0, 411);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(50, 20);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "label1";
            // 
            // rtb
            // 
            this.rtb.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtb.Location = new System.Drawing.Point(174, 274);
            this.rtb.Name = "rtb";
            this.rtb.Size = new System.Drawing.Size(543, 157);
            this.rtb.TabIndex = 1;
            this.rtb.Text = "";
            // 
            // Canvas
            // 
            this.Controls.Add(this.rtb);
            this.Controls.Add(this.lblInfo);
            this.Name = "Canvas";
            this.Size = new System.Drawing.Size(731, 437);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.RichTextBox rtb;
    }
}
