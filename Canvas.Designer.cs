
namespace NDraw
{
    partial class Canvas
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
            // From designer.
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolTip = new System.Windows.Forms.ToolTip(components);
            SuspendLayout();
            // 
            // toolTip
            // 
            toolTip.AutomaticDelay = 100;
            toolTip.UseAnimation = false;
            toolTip.UseFading = false;
            // 
            // Canvas
            // 
            BackColor = System.Drawing.Color.WhiteSmoke;
            Name = "Canvas";
            Size = new System.Drawing.Size(731, 437);
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip;
    }
}
