using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NBagOfTricks;


// TODO main menu: file, settings, status info

// TODO context menu: insert, cut, copy, delete, move, resize, edit properties, ...


namespace NDraw
{
    public partial class MainForm : Form
    {
        UserSettings _settings;

        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Gets or sets a value indicating whether the form will receive key events before
            //the event is passed to the control that has focus.
            KeyPreview = true;

            //canvas.KeyDown += Canvas_KeyDown;
            this.KeyDown += MainForm_KeyDown;
            //this.KeyUp += MainForm_KeyUp;

            string appDir = NBagOfTricks.Utils.MiscUtils.GetAppDataDir("NDraw");
            DirectoryInfo di = new DirectoryInfo(appDir);
            di.Create();
            _settings = UserSettings.Load(appDir);

            //var pp = Page.Load("page.json");

            // Test stuff.
            _settings.AllStyles.Add(new() { Id = "ST_1", LineColor = Color.Green, FillColor = Color.Pink });
            _settings.AllStyles.Add(new() { Id = "ST_2", LineColor = Color.Purple, FillColor = Color.Salmon });

            // What to draw.
            Page page = new()
            {
                Width = 20.0f,
                Height = 10.0f,
                UnitsName = "feet",
                Grid = 0.5f,
                Snap = 0.1f
            };

            page.Rects.Add(new RectShape() { Id = "R_1", Text = "foo", TL = new(50, 50), BR = new(100, 100) });
            page.Rects.Add(new RectShape() { Id = "R_2", Text = "bar", TL = new(160, 170), BR = new(200, 300) });
            page.Lines.Add(new LineShape() { Id = "L_1", Text = "bar",  Start = new(250, 250), End = new(300, 300) });

            canvas.Init(page, _settings);

            // Edit away....
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if(canvas.DisplayRectangle.Contains(Control.MousePosition))
            {
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
            canvas.SavePage("page.json");
            _settings.Save();
            _settings.Dispose();
        }

        private void ToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
