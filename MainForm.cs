using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NBagOfTricks.Utils;


namespace NDraw
{
    public partial class MainForm : Form
    {
        /// <summary>Settings.</summary>
        UserSettings _settings;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Go!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Receive key events before the event is passed to the control that has focus.
            KeyPreview = true;

            KeyDown += MainForm_KeyDown;
            //KeyUp += MainForm_KeyUp;

            string appDir = MiscUtils.GetAppDataDir("NDraw");
            DirectoryInfo di = new(appDir);
            di.Create();
            _settings = UserSettings.Load(appDir);

            try
            {
                Parser p = new();
                p.ParseFile(@"C:\Dev\repos\NDraw\Test\drawing1.nd");

                if(p.Errors.Count > 0) // TO-DO
                {
                    foreach(var err in p.Errors)
                    {
                        Trace(err);
                    }
                }
                else
                {
                    canvas.Init(p.Page, _settings);
                }
            }
            catch (Exception ex)
            {
                Trace($"Fail: {ex}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            Point pt = canvas.PointToClient(MousePosition);
            if (canvas.Bounds.Contains(pt))
            {
                //canvas.Log($"Hit!!!");
                canvas.HandleKeyDown(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up.
            //canvas.SavePage("page.json");
            _settings.Save();
            _settings.Dispose();
        }

        /// <summary>
        /// General toolbar handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        void Trace(string s)
        {
            rtbInfo.AppendText(s);
            rtbInfo.AppendText(Environment.NewLine);
            rtbInfo.ScrollToCaret();
        }
    }
}
