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
using NBagOfTricks.Utils;


// TODO2 main menu: file, settings, status info
// TODO2 Undo, redo
// TODO2 context menu: insert, cut, copy, delete, move, resize, edit properties, ...


namespace NDraw
{
    public partial class MainForm : Form
    {
        UserSettings _settings;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

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

            //// Test stuff.
            Page page = new();

            int which = 0;

            try
            {
                switch(which)
                {
                    case 0:
                        Parser p = new();
                        page = p.ParseFile(@"C:\Dev\repos\NDraw\drawing1.nd");
                        break;

                    case 1:
                        page = new() { Width = 20.0f, Height = 10.0f, UnitsName = "feet", Grid = 0.5f, Snap = 0.1f };
                        break;

                    case 2:
                        page.Rects.Add(new RectShape() { Id = "R_1", Text = "foo", TL = new(50, 50),   BR = new(100, 100) });
                        page.Rects.Add(new RectShape() { Id = "R_2", Text = "bar", TL = new(160, 170), BR = new(200, 300) });
                        page.Rects.Add(new RectShape() { Id = "R_3", Text = "abc", TL = new(300, 250), BR = new(330, 300) });
                        page.Rects.Add(new RectShape() { Id = "R_4", Text = "def", TL = new(400, 300), BR = new(550, 350) });
                        page.Rects.Add(new RectShape() { Id = "R_5", Text = "ggg", TL = new(450, 250), BR = new(460, 550) });
                        page.Lines.Add(new LineShape() { Id = "L_1", Text = "bar",  Start = new(250, 250), End = new(275, 455) });
                        break;

                    case 3:
                        int RECT_SIZE = 20;
                        int RECT_SPACE = 100;
                        for (int x = -70; x < 1500; x += RECT_SPACE)
                        {
                            for (int y = -50; y < 1000; y += RECT_SPACE)
                            {
                                page.Rects.Add(new RectShape() { Text = $"R_{x}_{y}", TL = new(x, y), BR = new(x + RECT_SIZE, y + RECT_SIZE) });
                            }
                        }
                        break;

                    case 4:
                        page = Page.Load("page.json");
                        break;
                }

                canvas.Init(page, _settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fail: {ex}");
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
            canvas.SavePage("page.json");
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
    }
}
