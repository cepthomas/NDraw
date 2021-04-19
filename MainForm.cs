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
            Page page = new();

            page.Rects.Add(new RectShape() { Id = "R_1", Text = "foo", L = 50, T = 50, R = 100, B = 100 });
            page.Rects.Add(new RectShape() { Id = "R_2", Text = "bar", L = 160, T = 170, R = 200, B = 300 });
            page.Lines.Add(new LineShape() { Id = "L_1", Text = "bar",  Start = new PointF(250, 250), End = new PointF(300, 300) });

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
