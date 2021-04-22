using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NBagOfTricks;


//TODO Undo/redo: insert, cut/copy/delete, move, resize, edit properties, ...


namespace NDraw
{
    public partial class MainForm : Form
    {
        UserSettings _settings;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string appDir = NBagOfTricks.Utils.MiscUtils.GetAppDataDir("NDraw");
            DirectoryInfo di = new DirectoryInfo(appDir);
            di.Create();
            _settings = UserSettings.Load(appDir);

            //var pp = Page.Load("page.json");

            // Test stuff.
            _settings.AllStyles.Add(new() { Id = "ST_1", LineColor = Color.Green, FillColor = Color.Pink });
            _settings.AllStyles.Add(new() { Id = "ST_2", LineColor = Color.Purple, FillColor = Color.Pink });

            // What to draw.
            Page page = new()
            {
                Width = 20.0f,
                Height = 10.0f,
                Units = "feet",
                Grid = 0.5f,
                Snap = 0.1f
            };

            page.Rects.Add(new RectShape() { Id = "R_1", Text = "foo", TL = new(50, 50), BR = new(100, 100) });
            page.Rects.Add(new RectShape() { Id = "R_2", Text = "bar", TL = new(160, 170), BR = new(200, 300) });
            page.Lines.Add(new LineShape() { Id = "L_1", Text = "bar",  Start = new(250, 250), End = new(300, 300) });

            canvas.Init(page, _settings);

            // Edit away....

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up.
            canvas.SavePage("page.json");
            _settings.Save();
            _settings.Dispose();
        }
    }
}
