using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NBagOfTricks;

namespace NDraw
{


    // page: w, h, border, scale, units

    // point: id, x, y

    // style: id, line_thickness, line_color, fill_style, fill_color

    // rect: id, layer, style_id, top, left, width, height

    // line: id, layer, style_id, start_pt, end_pt, 

    // //?
    // // ellipse, image, text, polyline/polygon
    // // dimensions

    // page [ 8.5, 11, 0.75, 50, ft]
    // style [ STY_DEF, 3, blue, hatch, green ]
    // scalar [ BOX_WIDTH, 15 ]
    // scalar [ BOX_HEIGHT, 10 ]
    // point [ ORIG, 2, 2 ]
    // point [ ANCHOR, 5.5, 7.5 ]
    // rect [ A_BOX, 1, STY_DEF, ORIG.y, ORIG.x, BOX_WIDTH, BOX_HEIGHT ]
    // rect [ B_BOX, 1, STY_DEF, ANCHOR.y, ANCHOR.x, BOX_WIDTH, BOX_HEIGHT ]


    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string dir = NBagOfTricks.Utils.MiscUtils.GetAppDataDir("NDraw");
            UserSettings.Load(dir);

            //var pp = Page.Load("page.json");

            // Common stuff.
            UserSettings.TheSettings.AllStyles.Add(new() { Id = "ST_1", LineColor = Color.Green, FillColor = Color.Pink });
            UserSettings.TheSettings.AllStyles.Add(new() { Id = "ST_2", LineColor = Color.Purple, FillColor = Color.Pink });

            // What to draw.
            Page page = new();

            page.Rects.Add(new RectShape() { Id = "R_1", Text = "foo", Extent = new RectX(50, 50, 100, 100) });
            page.Rects.Add(new RectShape() { Id = "R_2", Text = "bar", Extent = new RectX(160, 170, 200, 300) });
            page.Lines.Add(new LineShape() { Id = "L_1", Text = "bar", Start = new PointX(250, 250), End = new PointX(300, 300) });

            canvas.Init(page);

            // Edit away....
            // Collect changes
            page.Save("page.json");

        }
    }
}
