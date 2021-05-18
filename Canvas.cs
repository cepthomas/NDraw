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
using NBagOfTricks;
using NBagOfTricks.Utils;

namespace NDraw
{
    public partial class Canvas : UserControl // Should be Control but breaks in designer.
    {
        #region Fields
        /// <summary>Current drawing.</summary>
        Page _page = new();

        /// <summary>The settings.</summary>
        UserSettings _settings = new();

        /// <summary>All the shapes.</summary>
        readonly List<Shape> _shapes = new();

        /// <summary>Show/hide layers.</summary>
        bool[] _layers = new bool[NUM_LAYERS];

        /// <summary>Current horizontal offset in pixels.</summary>
        int _offsetX = 0;

        /// <summary>Current vertical offset in pixels.</summary>
        int _offsetY = 0;

        /// <summary>Current zoom level.</summary>
        float _zoom = 1.0F;

        /// <summary>Range in virtual units.</summary>
        float _xMin = float.MaxValue;

        /// <summary>Range in virtual units.</summary>
        float _yMin = float.MaxValue;

        /// <summary>Range in virtual units.</summary>
        float _xMax = float.MinValue;

        /// <summary>Range in virtual units.</summary>
        float _yMax = float.MinValue;

        /// <summary>Based on range.</summary>
        string _numericalFormat = "";
        #endregion

        #region Constants
        /// <summary>How many layers.</summary>
        public const int NUM_LAYERS = 4;

        /// <summary>Cosmetics in pixels.</summary>
        const float GRID_LINE_WIDTH = 0.5f;

        /// <summary>Cosmetics in pixels.</summary>
        const int HIGHLIGHT_SIZE = 5;

        /// <summary>Cosmetics in pixels.</summary>
        const int BORDER_SIZE = 40;

        /// <summary>Cosmetics in pixels.</summary>
        const int TICK_SIZE = 10;

        /// <summary>How close do you have to be to select a shape in pixels.</summary>
        const float SELECT_RANGE = 10;

        /// <summary>How fast the mouse wheel goes.</summary>
        const int WHEEL_RESOLUTION = 4;

        /// <summary>Maximum zoom in limit.</summary>
        const float ZOOM_MAX = 10.0f;

        /// <summary>Minimum zoom out limit.</summary>
        const float ZOOM_MIN = 0.1f;

        /// <summary>Speed at which to zoom in/out.</summary>
        const float ZOOM_SPEED = 0.1F;
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
        /// 
        /// </summary>
        public Canvas()
        {
            InitializeComponent();
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Perform initialization.
        /// </summary>
        /// <param name="page"></param>
        public void Init(Page page, UserSettings settings)
        {
            _page = page;
            _settings = settings;

            _shapes.Clear();
            _shapes.AddRange(_page.Rects);
            _shapes.AddRange(_page.Lines);
            _shapes.AddRange(_page.Ellipses);

            // Get ranges.
            foreach(var shape in _shapes)
            {
                var rect = shape.ToRect();
                _xMin = Math.Min(_xMin, rect.Left);
                _xMax = Math.Max(_xMax, rect.Right);
                _yMin = Math.Min(_yMin, rect.Top);
                _yMax = Math.Max(_yMax, rect.Bottom);
            }

            // Fit to grid intervals.
            _xMin = Box(_xMin).low;
            _xMax = Box(_xMax).high;
            _yMin = Box(_yMin).low;
            _yMax = Box(_yMax).high;

            _numericalFormat = FormatSpecifier(MathF.Max(_xMax, _yMax));

            // Init geometry.
            Reset();

            Invalidate();
        }
        #endregion

        #region Public functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="value"></param>
        public void SetLayer(int layer, bool value)
        {
            if(layer >= 0 && layer < NUM_LAYERS)
            {
                _layers[layer] = value;
                Invalidate();
            }
        }
        #endregion

        #region Misc window events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            lblInfo.Location = new(10, Bottom - lblInfo.Height - 10);
            Invalidate();
        }
        #endregion

        #region Painting
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Object that sent triggered the event.</param>
        /// <param name="e">The particular PaintEventArgs.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(_settings.BackColor);

            var tl = DisplayToVirtual(ClientRectangle.Location);
            var br = DisplayToVirtual(new Point(ClientRectangle.Right, ClientRectangle.Bottom));
            var virtVisible = new RectangleF(tl, new SizeF(br.X - tl.X, br.Y - tl.Y));

            // Draw the grid.
            DrawGrid(e.Graphics);

            e.Graphics.SetClip(new Rectangle(BORDER_SIZE, BORDER_SIZE, ClientRectangle.Width - BORDER_SIZE, ClientRectangle.Height - BORDER_SIZE));

            // Draw the shapes.
            foreach (var shape in _shapes)
            {
                // Is it visible?
                if (shape.ContainedIn(virtVisible, true) && _layers[shape.Layer - 1])
                {
                    using Pen penLine = new(shape.LineColor, shape.LineThickness);
                    // Map to display coordinates.
                    var bounds = shape.ToRect();
                    var disptl = VirtualToDisplay(bounds.Location);
                    var dispbr = VirtualToDisplay(new(bounds.Right, bounds.Bottom));
                    var dispRect = new RectangleF(disptl, new SizeF(dispbr.X - disptl.X, dispbr.Y - disptl.Y));
                    
                    e.Graphics.DrawRectangle(Pens.Red, dispRect.X, dispRect.Y, dispRect.Width, dispRect.Height);

                    switch (shape)
                    {
                        case RectShape shapeRect:
                            e.Graphics.DrawRectangle(penLine, dispRect.X, dispRect.Y, dispRect.Width, dispRect.Height);
                            break;

                        case EllipseShape shapeEllipse:
                            e.Graphics.DrawEllipse(penLine, dispRect);
                            break;

                        case LineShape shapeLine:
                            //e.Graphics.DrawLine(penLine, disptl, dispbr);
                            e.Graphics.DrawLine(penLine, VirtualToDisplay(shapeLine.Start), VirtualToDisplay(shapeLine.End));

                            if (shapeLine.State != ShapeState.Highlighted)
                            {
                                // Draw line ends.
                                float angle = shapeLine.Angle;
                                angle = (float)MathUtils.RadiansToDegrees(angle);
                                //angle = 45;
                                DrawPoint(e.Graphics, penLine, VirtualToDisplay(shapeLine.Start), shapeLine.StartStyle, angle);
                                DrawPoint(e.Graphics, penLine, VirtualToDisplay(shapeLine.End), shapeLine.EndStyle, angle);
                            }
                            break;
                    }

                    // Text.
                    var align = _alignment[shape.TextAlignment];
                    using StringFormat fmt = new() { Alignment = align.hor, LineAlignment = align.vert };
                    e.Graphics.DrawString(shape.Text, _settings.Font, Brushes.Black, dispRect, fmt);

                    // Highlight features.
                    if (shape.State == ShapeState.Highlighted)
                    {
                        foreach (var pt in shape.AllFeaturePoints())
                        {
                            PointF disppt = VirtualToDisplay(pt);
                            e.Graphics.FillEllipse(penLine.Brush, Squarify(disppt.X, disppt.Y, HIGHLIGHT_SIZE));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        /// <param name="pt"></param>
        /// <param name="ps"></param>
        /// <param name="degrees">The rotation of the axis.</param>
        void DrawPoint(Graphics g, Pen pen, PointF pt, PointStyle ps, float degrees)
        {
            g.TranslateTransform(pt.X, pt.Y);
            g.RotateTransform(degrees);

            switch (ps)
            {
                case PointStyle.Arrow:
                    g.FillPolygon(pen.Brush, new PointF[]
                    {
                        new PointF(- 15, 50),   // Bottom-Left
                        new PointF(15, 50),   // Bottom-Right
                        new PointF(0, 0),       // Top-Middle
                        //new PointF(pt.X - 15, pt.Y + 50),   // Bottom-Left
                        //new PointF(pt.X + 15, pt.Y + 50),   // Bottom-Right
                        //new PointF(pt.X, pt.Y),       // Top-Middle
                    });
                    break;

                case PointStyle.Tee:
                    //g.FillRectangle(pen.Brush, new RectangleF(pt.X, pt.Y, 50, 50));
                    g.FillRectangle(pen.Brush, new RectangleF(0, 0, 50, 50));
                    break;

                case PointStyle.None:
                    break;
            }

            g.ResetTransform();
        }

        /// <summary>
        /// Draw the grid.
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        void DrawGrid(Graphics g)
        {
            using Pen penGrid = new(_settings.GridColor, GRID_LINE_WIDTH * 6.0f);

            // Draw main axes.
            g.DrawLine(penGrid, BORDER_SIZE, BORDER_SIZE, BORDER_SIZE, Height);
            g.DrawLine(penGrid, BORDER_SIZE, BORDER_SIZE, Width, BORDER_SIZE);

            penGrid.Width = GRID_LINE_WIDTH;
            bool done = false;

            // Draw X-Axis ticks.
            for (float x = _xMin; !done; x += _page.Grid)
            {
                var xd = VirtualToDisplay(new(x, 0)).X;
                if(xd > Width)
                {
                    done = true;
                }
                else if(xd > BORDER_SIZE) // clip
                {
                    g.DrawLine(penGrid, xd, BORDER_SIZE - TICK_SIZE, xd, Width);
                    g.DrawString(x.ToString(_numericalFormat), _settings.Font, Brushes.Black, xd - TICK_SIZE, 0);
                }
            }

            // Draw Y-Axis ticks.
            done = false;
            for (float y = _yMin; !done; y += _page.Grid)
            {
                var yd = VirtualToDisplay(new(0, y)).Y;
                if (yd > Height)
                {
                    done = true;
                }
                else if (yd > BORDER_SIZE) // clip
                {
                    g.DrawLine(penGrid, BORDER_SIZE - TICK_SIZE, yd, Width, yd);
                    g.DrawString(y.ToString(_numericalFormat), _settings.Font, Brushes.Black, 0, yd - TICK_SIZE);
                }
            }
        }

        /// <summary>
        /// Draw text using the specified transformations.
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        /// <param name="x">The X transform</param>
        /// <param name="y">The Y transform</param>
        /// <param name="text">The text of the label.</param>
        /// <param name="degrees">The rotation of the axis.</param>
        void DrawText(Graphics g, float x, float y, string text, Font font, int degrees)
        {
            g.TranslateTransform(x, y);
            g.RotateTransform(degrees);
            g.DrawString(text, font, Brushes.Black, 0, 0);
            g.ResetTransform();
        }
        #endregion

        #region Mouse events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool redraw = false;
            Shape toHighlight = null;

            var virtLoc = DisplayToVirtual(e.Location);
            float range = SELECT_RANGE * _zoom / _page.Scale; // FUTURE this really should be done in display domain.

            foreach (Shape shape in _shapes)
            {
                if (shape.FeaturePoint(virtLoc, range) > 0 && _layers[shape.Layer - 1])
                {
                    shape.State = ShapeState.Highlighted;
                    toHighlight = shape;
                    redraw = true;
                }
                else if (shape.State == ShapeState.Highlighted) // Unhighlight those away from.
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

            ShowInfo(e.Location);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //(e as HandledMouseEventArgs).Handled = true; // This prevents the mouse wheel event from getting back to the parent. ???

            bool redraw = false;

            // Number of detents the mouse wheel has rotated, multiplied by the WHEEL_DELTA constant.
            int delta = 2 * WHEEL_RESOLUTION * e.Delta / SystemInformation.MouseWheelScrollDelta;

            switch (e.Button, ControlPressed(), ShiftPressed())
            {
                case (MouseButtons.None, true, false): // Zoom in/out at mouse position
                    var zoomFactor = delta > 0 ? ZOOM_SPEED : -ZOOM_SPEED;
                    var newZoom = _zoom + zoomFactor;

                    if (newZoom > ZOOM_MIN && newZoom < ZOOM_MAX)
                    {
                        _zoom = newZoom;

                        // Adjust offsets to center zoom at mouse.
                        float offx = e.X * zoomFactor;
                        float offy = e.Y * zoomFactor;

                        _offsetX -= (int)offx;
                        _offsetY -= (int)offy;

                        redraw = true;
                    }
                    break;

                case (MouseButtons.None, false, true): // Shift left/right
                    _offsetX += delta;
                    redraw = true;
                    break;

                case (MouseButtons.None, false, false): // Shift up/down
                    _offsetY += delta;
                    redraw = true;
                    break;

                default:
                    break;
            };

            // Clamp offsets.
            _offsetX = Math.Min(_offsetX, 0);
            _offsetY = Math.Min(_offsetY, 0);

            if (redraw)
            {
                Invalidate();
            }

            ShowInfo(e.Location);
        }
        #endregion

        #region Key events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool redraw = false;

            switch (e.KeyCode)
            {
                case Keys.H: // reset
                    Reset();
                    redraw = true;
                    break;
            }

            if (redraw)
            {
                Invalidate();
            }

           // ShowInfo();
        }
        #endregion

        #region Mapping functions
        /// <summary>
        /// Map a virtual point to the display.
        /// </summary>
        /// <param name="virt"></param>
        /// <returns></returns>
        PointF VirtualToDisplay(PointF virt)
        {
            var dispX = (virt.X * _zoom) * _page.Scale + _offsetX + BORDER_SIZE;
            var dispY = (virt.Y * _zoom) * _page.Scale + _offsetY + BORDER_SIZE;
            return new(dispX, dispY);
        }

        /// <summary>
        /// Obtain the virtual point for a display position.
        /// </summary>
        /// <param name="disp">The display point.</param>
        /// <returns>The virtual point.</returns>
        PointF DisplayToVirtual(Point disp)
        {
            var virtX = (disp.X - _offsetX - BORDER_SIZE) / _page.Scale / _zoom;
            var virtY = (disp.Y - _offsetY - BORDER_SIZE) / _page.Scale / _zoom;
            return new PointF(virtX, virtY);
        }
        #endregion

        #region Private helpers
        /// <summary>
        /// Debug helper.
        /// </summary>
        void ShowInfo(Point pt)
        {
            lblInfo.Text = $"Mouse:{pt} TX:{DisplayToVirtual(pt)} OffsetX:{_offsetX} OffsetY:{_offsetY} Zoom:{_zoom}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool ControlPressed()
        {
            return (ModifierKeys & Keys.Control) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool ShiftPressed()
        {
            return (ModifierKeys & Keys.Shift) > 0;
        }

        /// <summary>
        /// Defaults.
        /// </summary>
        void Reset()
        {
            _zoom = 1.0f;
            _offsetX = 0;
            _offsetY = 0;
        }

        /// <summary>
        /// Make a square with the point at the center.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        RectangleF Squarify(float x, float y, float range)
        {
            RectangleF r = new(x - range, y - range, range * 2, range * 2);
            return r;
        }

        /// <summary>
        /// Return the integral grid neighbors.
        /// </summary>
        /// <param name="val">(low,high) neighbors</param>
        /// <returns></returns>
        (float low, float high) Box(float val)
        {
            var v = val - (val % _page.Grid);
            return (v, v + _page.Grid);
        }

        /// <summary>Sets the format specifier based upon the range of data.</summary>
        /// <param name="range">Tick range</param>
        /// <returns>Format specifier</returns>
        string FormatSpecifier(float range) // FUTURE put in NBOT?
        {
            string format = "";

            if (range >= 100)
            {
                format = "0;-0;0";
            }
            else if (range < 100 && range >= 10)
            {
                format = "0.0;-0.0;0";
            }
            else if (range < 10 && range >= 1)
            {
                format = "0.00;-0.00;0";
            }
            else if (range < 1)
            {
                format = "0.000;-0.000;0";
            }

            return format;
        }
        #endregion
    }
}
