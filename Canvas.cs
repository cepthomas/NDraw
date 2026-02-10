using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;
using Ephemera.NBagOfTricks;
using System.Reflection;


namespace NDraw
{
    public partial class Canvas : UserControl
    {
        #region Fields
        /// <summary>Current drawing.</summary>
        Page _page = new();

        /// <summary>The settings.</summary>
        UserSettings _settings = new();

        /// <summary>Display font.</summary>
        readonly Font _font = new("Cascadia Code", 12);

        /// <summary>Show/hide layers.</summary>
        readonly Dictionary<string, bool> _layers = [];

        /// <summary>Ruler visibility.</summary>
        bool _showRuler = true;

        /// <summary>Grid visibility.</summary>
        bool _showGrid = true;

        /// <summary>Based on range.</summary>
        string _numFormat = "";

        /// <summary>Top left of visible (pixels).</summary>
        int _originX = 0;

        /// <summary>Top left of visible (pixels).</summary>
        int _originY = 0;

        /// <summary>H/V scroll increment (pixels).</summary>
        int _planarScroll = 2;

        /// <summary>Current zoom level aka scale.</summary>
        float _zoom = 1.0f;
        #endregion

        #region Dev
        /// <summary>Dev.</summary>
        readonly bool _debug = false;

        /// <summary>Dev.</summary>
        public void DumpBitmap()
        {
            Bitmap bmp = GenBitmap(1600);
            bmp.Save(@"out.bmp", ImageFormat.Bmp);
        }
        #endregion

        #region Constants
        /// <summary>Left and top borders (pixels).</summary>
        const int BORDER_SIZE = 80;

        /// <summary>Axis tick size (pixels).</summary>
        const int TICK_SIZE = 10;

        /// <summary>Grid lines (pixels).</summary>
        const int GRID_LINE_WIDTH = 1;

        /// <summary>Selected shape size (pixels).</summary>
        const int HIGHLIGHT_SIZE = 8;

        /// <summary>How close do you have to be to select a shape (pixels).</summary>
        const int SELECT_RANGE = 10;
        #endregion

        #region Enum mappings
        readonly Dictionary<ContentAlignment, (StringAlignment vert, StringAlignment hor)> _alignment = new()
        {
            { ContentAlignment.TopLeft,      (StringAlignment.Near,   StringAlignment.Near) },
            { ContentAlignment.TopCenter,    (StringAlignment.Near,   StringAlignment.Center) },
            { ContentAlignment.TopRight,     (StringAlignment.Near,   StringAlignment.Far) },
            { ContentAlignment.MiddleLeft,   (StringAlignment.Center, StringAlignment.Near) },
            { ContentAlignment.MiddleCenter, (StringAlignment.Center, StringAlignment.Center) },
            { ContentAlignment.MiddleRight,  (StringAlignment.Center, StringAlignment.Far) },
            { ContentAlignment.BottomLeft,   (StringAlignment.Far,    StringAlignment.Near) },
            { ContentAlignment.BottomCenter, (StringAlignment.Far,    StringAlignment.Center) },
            { ContentAlignment.BottomRight,  (StringAlignment.Far,    StringAlignment.Far) },
        };
        #endregion

        #region Lifecycle
        /// <summary>
        /// Greetings earthling.
        /// </summary>
        public Canvas()
        {
            InitializeComponent();
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Process page to graphics.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="settings"></param>
        public void Init(Page page, UserSettings settings)
        {
            _page = page;
            _settings = settings;
            _numFormat = StringUtils.FormatSpecifier(_page.Width);
            _layers.Clear();
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Show/hide layer.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="value"></param>
        public void SetLayer(string layer, bool value)
        {
            _layers[layer] = value;
            Invalidate();
        }

        /// <summary>
        /// Show or hide features.
        /// </summary>
        /// <param name="ruler"></param>
        /// <param name="grid"></param>
        public void SetVisibility(bool ruler, bool grid)
        {
            _showRuler = ruler;
            _showGrid = grid;
            Invalidate();
        }

        /// <summary>
        /// Set defaults.
        /// </summary>
        public void Reset()
        {
            _zoom = DrawArea().Width / _page.Width; // pixels per unit
            _originX = 0;
            _originY = 0;
        }
        #endregion

        #region Misc window handlers
        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(EventArgs e)
        {
            // Clear any highlights.
            foreach (Shape shape in _page.Shapes)
            {
                shape.State = ShapeState.Default;
            }

            Invalidate();
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Render whole image to bitmap.
        /// </summary>
        /// <param name="width">Final width in pixels.</param>
        /// <returns></returns>
        public Bitmap GenBitmap(int width)
        {
            // Scale per request.
            var ratio = _page.Height / _page.Width;
            var height = (int)(width * ratio);

            // Cache current UI state.
            var zoom = _zoom;
            var originX = _originX;
            var originY = _originY;

            // Reset specifically for render.
            _zoom = (width - BORDER_SIZE) / _page.Width; // pixels per unit
            _originX = 0;
            _originY = 0;

            // Do the render.
            Bitmap bmp = new(width, height);
            var g = Graphics.FromImage(bmp);
            DrawIt(g, new(0, 0, width, height));

            // Restore state.
            _zoom = zoom;
            _originX = originX;
            _originY = originY;

            return bmp;
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Draw the UI.
        /// </summary>
        /// <param name="e">The particular PaintEventArgs.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            DrawIt(e.Graphics, ClientRectangle);
        }

        /// <summary>
        /// Does the actual drawing.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="client"></param>
        void DrawIt(Graphics g, Rectangle client)
        {
            g.Clear(BackColor);

            if (_page.Shapes.Count == 0)
            {
                g.DrawString($"No Data", _font, Brushes.Black, 5, 15);
                return;
            }

            // Draw the grid first.
            DrawGrid(g, client);

            if (_debug)
            {
                g.DrawString($"z:{_zoom.ToString(_numFormat)}", _font, Brushes.Black, 5, 15);
                g.DrawString($"x:{_originX.ToString(_numFormat)}", _font, Brushes.Black, 5, 30);
                g.DrawString($"y:{_originY.ToString(_numFormat)}", _font, Brushes.Black, 5, 45);
            }

            // Draw the shapes, clipped.
            //g.SetClip(DrawArea());
            var drawRect = new Rectangle(BORDER_SIZE, BORDER_SIZE, client.Width - BORDER_SIZE, client.Height - BORDER_SIZE);
            g.SetClip(drawRect);

            foreach (var shape in _page.Shapes)
            {
                // Is it visible?
                if (_layers.TryGetValue(shape.Layer, out bool value) && value)  // TODO && shape.ContainedIn(virtVisible, true)
                {
                    using Pen penLine = new(shape.LineColor, shape.LineThickness);
                    if (shape.LineDash is not null)
                    {
                        penLine.DashStyle = (DashStyle)shape.LineDash;
                    }
                    using Brush brush = shape.Hatch is null ? new SolidBrush(shape.FillColor) : new HatchBrush((HatchStyle)shape.Hatch, shape.LineColor, shape.FillColor);

                    // Map to display coordinates.
                    var bounds = shape.ToRect();
                    var disptl = VirtualToDisplay(bounds.Location);
                    var dispbr = VirtualToDisplay(new(bounds.Right, bounds.Bottom));
                    var dispRect = new RectangleF(disptl, new SizeF(dispbr.X - disptl.X, dispbr.Y - disptl.Y));

                    switch (shape)
                    {
                        case RectShape shapeRect:
                            g.FillRectangle(brush, dispRect.X, dispRect.Y, dispRect.Width, dispRect.Height);
                            g.DrawRectangle(penLine, dispRect.X, dispRect.Y, dispRect.Width, dispRect.Height);
                            break;

                        case EllipseShape shapeEllipse:
                            g.FillEllipse(brush, dispRect);
                            g.DrawEllipse(penLine, dispRect);
                            break;

                        case LineShape shapeLine:
                            g.DrawLine(penLine, VirtualToDisplay(shapeLine.Start), VirtualToDisplay(shapeLine.End));

                            if (shapeLine.State != ShapeState.Highlighted)
                            {
                                // Draw line ends.
                                float angle = (float)MathUtils.RadiansToDegrees(shapeLine.Angle);
                                DrawPoint(g, penLine, VirtualToDisplay(shapeLine.Start), shapeLine.StartStyle, angle - 180);
                                DrawPoint(g, penLine, VirtualToDisplay(shapeLine.End), shapeLine.EndStyle, angle);
                            }
                            break;
                    }

                    // Shape text.
                    if (shape.Text != "")
                    {
                        var (vert, hor) = _alignment[shape.TextAlignment];
                        using StringFormat fmt = new() { Alignment = hor, LineAlignment = vert };
                        g.DrawString(shape.Text, _font, Brushes.Black, dispRect, fmt);
                    }

                    // Highlight features.
                    if (shape.State == ShapeState.Highlighted)
                    {
                        foreach (var pt in shape.AllFeaturePoints())
                        {
                            PointF disppt = VirtualToDisplay(pt);
                            g.FillEllipse(penLine.Brush, Squarify(disppt.X, disppt.Y, HIGHLIGHT_SIZE));
                        }
                    }
                }
            }

            if (_debug)
            {
                var sz = 50;
                var da = DrawArea();
                g.FillRectangle(Brushes.Violet, da.Left,     da.Top,     sz, sz);
                g.FillRectangle(Brushes.Violet, da.Left+500, da.Top,     sz, sz);
                g.FillRectangle(Brushes.Violet, da.Left,     da.Top+500, sz, sz);
            }
        }

        /// <summary>
        /// Draw the grid.
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        void DrawGrid(Graphics g, Rectangle client)
        {
            using Pen penGrid = new(_settings.GridColor, GRID_LINE_WIDTH + 3);

            // Draw main axes.
            g.DrawLine(penGrid, BORDER_SIZE, BORDER_SIZE, BORDER_SIZE, client.Height);
            g.DrawLine(penGrid, BORDER_SIZE, BORDER_SIZE, client.Width, BORDER_SIZE);

            penGrid.Width = GRID_LINE_WIDTH;
            var fontPixels = g.MeasureString("X", _font);

            // Draw X-Axis ticks and vertical lines.
            for (float x = 0; x <= _page.Width; x += _page.Grid)
            {
                var xd = VtoDX(x);// (new(x, 0)).X;
                if (xd > client.Width)
                {
                    break;
                }
                else if (xd > BORDER_SIZE) // clip
                {
                    if (_showGrid)
                    {
                        g.DrawLine(penGrid, xd, BORDER_SIZE - TICK_SIZE, xd, client.Height);
                    }

                    if (_showRuler)
                    {
                        //g.DrawString(x.ToString(_numFormat), _font, Brushes.Black, xd - TICK_SIZE, 0);
                        g.TranslateTransform(xd + (int)(fontPixels.Width / 2), 5);
                        g.RotateTransform(90);
                        g.DrawString(x.ToString(_numFormat), _font, Brushes.Black, 0, 0);
                        g.ResetTransform();
                    }
                }
            }

            // Draw Y-Axis ticks and horizontal lines.
            for (float y = 0; y <= _page.Height ; y += _page.Grid)
            {
                var yd = VtoDY(y);// new(0, y)).Y;
                if (yd > client.Height)
                {
                    break;
                }
                else if (yd > BORDER_SIZE) // clip
                {
                    if (_showGrid)
                    {
                        g.DrawLine(penGrid, BORDER_SIZE - TICK_SIZE, yd, client.Width, yd);
                    }

                    if (_showRuler)
                    {
                        g.DrawString(y.ToString(_numFormat), _font, Brushes.Black, 0, yd - TICK_SIZE);
                    }
                }
            }
        }

        /// <summary>
        /// Draw a point.
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        /// <param name="pen">The tool.</param>
        /// <param name="pt">Origin for rotate.</param>
        /// <param name="ps">End type.</param>
        /// <param name="degrees">The rotation of the axis.</param>
        void DrawPoint(Graphics g, Pen pen, PointF pt, PointStyle ps, float degrees)
        {
            g.TranslateTransform(pt.X, pt.Y);
            g.RotateTransform(degrees);

            switch (ps)
            {
                case PointStyle.Arrow:
                    var pts = new PointF[] { new(0, 0), new(-20, -10), new(-20, 10) };
                    g.FillPolygon(pen.Brush, pts);
                    break;

                case PointStyle.Tee:
                    g.DrawLine(pen, 0, +10, 0, -10);
                    break;

                case PointStyle.None:
                    break;
            }

            g.ResetTransform();
        }
        #endregion

        #region Mouse events
        /// <summary>
        /// Mouse move.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool redraw = false;
            Shape? toHighlight = null;

            var virtLoc = DisplayToVirtualX(e.Location);

            if (_debug)
            {
                List<string> list =
                [
                    $"x:{virtLoc.X.ToString(_numFormat)}",
                    $"y:{virtLoc.Y.ToString(_numFormat)}",
                    $"[{e.X}:{e.Y}]"
                ];

                toolTip.Show(string.Join("  ", list), this, e.X + 15, e.Y);
                return;
            }

            foreach (Shape shape in _page.Shapes) // TODO if adjacent shapes this gets both.
            {
                int fp = shape.IsFeaturePoint(virtLoc, SELECT_RANGE / _zoom);

                if (fp > 0 && _layers.TryGetValue(shape.Layer, out bool value) && value)
                {
                    shape.State = ShapeState.Highlighted;
                    toHighlight = shape;
                    redraw = true;
                }
                else if (shape.State == ShapeState.Highlighted) // Unhighlight those not near.
                {
                    shape.State = ShapeState.Default;
                    redraw = true;
                }
            }

            if (toHighlight is not null)
            {
                toolTip.Show(toHighlight.ToString(), this, e.X + 15, e.Y);
            }
            else
            {
                toolTip.Hide(this);
            }

            if (redraw)
            {
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Mouse wheel.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            bool redraw = false;

            var ctrl = (ModifierKeys & Keys.Control) > 0;
            var shift = (ModifierKeys & Keys.Shift) > 0;

            // Number of detents the mouse wheel has rotated, multiplied by the WHEEL_DELTA constant.
            var delta = _settings.WheelResolution * e.Delta / SystemInformation.MouseWheelScrollDelta;
            var zoomIncr = _zoom / 20;

            switch (e.Button, ctrl, shift)
            {
                case (MouseButtons.None, true, false):
                    var zoomFactor = delta > 0 ? zoomIncr : -zoomIncr; // delta + => scroll up
                    var newZoom = _zoom + zoomFactor;
                    _zoom = newZoom;
                    redraw = true;
                    break;

                case (MouseButtons.None, false, true): // Shift left/right
                    _originX -= delta * _planarScroll;
                    _originX = Math.Max(0, _originX);
                    redraw = true;
                    break;

                case (MouseButtons.None, false, false): // Shift up/down
                    _originY -= delta * _planarScroll;
                    _originY = Math.Max(0, _originY);
                    redraw = true;
                    break;

                default:
                    break;
            };

            if (redraw)
            {
                Invalidate();
            }
            base.OnMouseWheel(e);
        }
        #endregion

        #region Key events
        /// <summary>
        /// Key down.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.H: // reset
                    Reset();
                    Invalidate();
                    break;
            }

            // ShowInfo();
            base.OnKeyDown(e);
        }
        #endregion

        #region Mapping functions
        /// <summary>Helper to get main drawing area.</summary>
        /// <returns></returns>
        Rectangle DrawArea() { return new Rectangle(BORDER_SIZE, BORDER_SIZE, ClientRectangle.Width - BORDER_SIZE, ClientRectangle.Height - BORDER_SIZE); }

        /// <summary>Map a virtual point to the display.</summary>
        Point VirtualToDisplay(PointF virt) { return new(VtoDX(virt.X), VtoDY(virt.Y)); }

        int VtoDX(float x) { return (int)((x * _zoom) - _originX + BORDER_SIZE); }

        int VtoDY(float y) { return (int)((y * _zoom) - _originY + BORDER_SIZE); }

        /// <summary>Map a display point to virtual.</summary>
        PointF DisplayToVirtualX(Point disp) { return new PointF(DtoVX(disp.X), DtoVY(disp.Y)); }

        float DtoVX(int x) { return ((float)x + _originX - BORDER_SIZE) / _zoom; }

        float DtoVY(int y) { return ((float)y + _originY - BORDER_SIZE) / _zoom; }

        /// <summary>Make a square with the point at the center.</summary>
        RectangleF Squarify(float x, float y, float range)
        {
            RectangleF r = new(x - range, y - range, range * 2, range * 2);
            return r;
        }

        /// <summary>Return the integral grid neighbors.</summary>
        (float low, float high) Box(float val)
        {
            var v = val - (val % _page.Grid);
            return (v, v + _page.Grid);
        }
        #endregion
    }
}
