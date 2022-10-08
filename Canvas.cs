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
        Font _font = new("Consolas", 10);

        /// <summary>All the shapes.</summary>
        readonly List<Shape> _shapes = new();

        /// <summary>Show/hide layers.</summary>
        readonly bool[] _layers = new bool[NUM_LAYERS];

        /// <summary>Ruler visibility.</summary>
        bool _ruler = true;

        /// <summary>Grid visibility.</summary>
        bool _grid = true;

        /// <summary>Current horizontal offset in pixels.</summary>
        int _offsetX = SCROLL_NEGATIVE;

        /// <summary>Current vertical offset in pixels.</summary>
        int _offsetY = SCROLL_NEGATIVE;

        /// <summary>Current zoom level.</summary>
        float _zoom = 1.0F;

        /// <summary>Based on range.</summary>
        string _numericalFormat = "";

        /// <summary>Range in virtual units.</summary>
        float _xMin = float.MaxValue;

        /// <summary>Range in virtual units.</summary>
        float _yMin = float.MaxValue;

        /// <summary>Range in virtual units.</summary>
        float _xMax = float.MinValue;

        /// <summary>Range in virtual units.</summary>
        float _yMax = float.MinValue;
        #endregion

        #region Constants
        /// <summary>How many layers.</summary>
        const int NUM_LAYERS = 4;

        /// <summary>Cosmetics in pixels.</summary>
        const float GRID_LINE_WIDTH = 0.5f;

        /// <summary>Cosmetics in pixels.</summary>
        const int HIGHLIGHT_SIZE = 8;

        /// <summary>Cosmetics in pixels.</summary>
        const int BORDER_SIZE = 50;

        /// <summary>Cosmetics in pixels.</summary>
        const int TICK_SIZE = 10;

        /// <summary>How close do you have to be to select a shape in pixels.</summary>
        const float SELECT_RANGE = 10;

        /// <summary>Allow negative scrolling by this much. Looks better.</summary>
        const int SCROLL_NEGATIVE = 20;

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

        #region Events
        /// <summary>Display stuff.</summary>
        public event EventHandler<string>? InfoEvent;
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
        /// <param name="settings"></param>
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

            _numericalFormat = StringUtils.FormatSpecifier(MathF.Max(_xMax, _yMax));

            // Init geometry.
            Reset();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruler"></param>
        /// <param name="grid"></param>
        public void SetVisibility(bool ruler, bool grid)
        {
            _ruler = ruler;
            _grid = grid;
            Invalidate();
        }

        /// <summary>
        /// Defaults.
        /// </summary>
        public void Reset()
        {
            _zoom = 1.0f;
            _offsetX = SCROLL_NEGATIVE;
            _offsetY = SCROLL_NEGATIVE;
            Invalidate();
        }
        #endregion

        #region Misc window handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(EventArgs e)
        {
            // Clear any highlights.
            foreach (Shape shape in _shapes)
            {
                shape.State = ShapeState.Default;
            }

            Invalidate();
            base.OnLostFocus(e);
        }
        #endregion

        #region Painting
        /// <summary>
        /// 
        /// </summary>
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
                    using Brush brush = shape.Hatch == Shape.NO_HATCH ? new SolidBrush(shape.FillColor) : new HatchBrush(shape.Hatch, shape.LineColor, shape.FillColor);

                    // Map to display coordinates.
                    var bounds = shape.ToRect();
                    var disptl = VirtualToDisplay(bounds.Location);
                    var dispbr = VirtualToDisplay(new(bounds.Right, bounds.Bottom));
                    var dispRect = new RectangleF(disptl, new SizeF(dispbr.X - disptl.X, dispbr.Y - disptl.Y));

                    //e.Graphics.DrawRectangle(Pens.Red, dispRect.X, dispRect.Y, dispRect.Width, dispRect.Height);

                    switch (shape)
                    {
                        case RectShape shapeRect:
                            e.Graphics.FillRectangle(brush, dispRect.X, dispRect.Y, dispRect.Width, dispRect.Height);
                            e.Graphics.DrawRectangle(penLine, dispRect.X, dispRect.Y, dispRect.Width, dispRect.Height);
                            break;

                        case EllipseShape shapeEllipse:
                            e.Graphics.FillEllipse(brush, dispRect);
                            e.Graphics.DrawEllipse(penLine, dispRect);
                            break;

                        case LineShape shapeLine:
                            e.Graphics.DrawLine(penLine, VirtualToDisplay(shapeLine.Start), VirtualToDisplay(shapeLine.End));

                            if (shapeLine.State != ShapeState.Highlighted)
                            {
                                // Draw line ends.
                                float angle = (float)MathUtils.RadiansToDegrees(shapeLine.Angle);
                                DrawPoint(e.Graphics, penLine, VirtualToDisplay(shapeLine.Start), shapeLine.StartStyle, angle - 180);
                                DrawPoint(e.Graphics, penLine, VirtualToDisplay(shapeLine.End), shapeLine.EndStyle, angle);
                            }
                            break;
                    }

                    // Text.
                    if(shape.Text != "")
                    {
                        var (vert, hor) = _alignment[shape.TextAlignment];
                        using StringFormat fmt = new() { Alignment = hor, LineAlignment = vert };
                        e.Graphics.DrawString(shape.Text, _font, Brushes.Black, dispRect, fmt);
                    }

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
        /// <param name="pt">Origin for rotate.</param>
        /// <param name="pen">The tool.</param>
        /// <param name="ps">End type.</param>
        /// <param name="degrees">The rotation of the axis.</param>
        void DrawPoint(Graphics g, Pen pen, PointF pt, PointStyle ps, float degrees)
        {
            g.TranslateTransform(pt.X, pt.Y);
            g.RotateTransform(degrees);

            switch (ps)
            {
                case PointStyle.Arrow:
                    var pts = new PointF[] { new PointF(0, 0), new PointF(-20, -10), new PointF(-20, 10) };
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

        /// <summary>
        /// Draw the grid.
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        void DrawGrid(Graphics g)
        {
            using Pen penGrid = new(_settings.GridColor, GRID_LINE_WIDTH);

            // Draw main axes.
            //g.DrawLine(penGrid, BORDER_SIZE, BORDER_SIZE, BORDER_SIZE, Height);
            //g.DrawLine(penGrid, BORDER_SIZE, BORDER_SIZE, Width, BORDER_SIZE);

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
                    if(_grid)
                    {
                        g.DrawLine(penGrid, xd, BORDER_SIZE - TICK_SIZE, xd, Width);
                    }

                    if (_ruler)
                    {
                        g.DrawString(x.ToString(_numericalFormat), _font, Brushes.Black, xd - TICK_SIZE, 0);
                    }
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
                    if (_grid)
                    {
                        g.DrawLine(penGrid, BORDER_SIZE - TICK_SIZE, yd, Width, yd);
                    }

                    if (_ruler)
                    {
                        g.DrawString(y.ToString(_numericalFormat), _font, Brushes.Black, 0, yd - TICK_SIZE);
                    }
                }
            }
        }

        ///// <summary>
        ///// Draw text using the specified transformations.
        ///// </summary>
        ///// <param name="g">The Graphics object to use.</param>
        ///// <param name="pt">Origin for rotate.</param>
        ///// <param name="text">The text of the label.</param>
        ///// <param name="font">The tool.</param>
        ///// <param name="degrees">The rotation of the axis.</param>
        //void DrawText(Graphics g, PointF pt, string text, Font font, int degrees)
        //{
        //    g.TranslateTransform(pt.X, pt.Y);
        //    g.RotateTransform(degrees);
        //    g.DrawString(text, font, Brushes.Black, 0, 0);
        //    g.ResetTransform();
        //}
        #endregion

        #region Mouse events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool redraw = false;
            Shape? toHighlight = null;

            var virtLoc = DisplayToVirtual(e.Location);
            float range = SELECT_RANGE / _zoom / _page.Scale; // This really should be done in display domain.

            foreach (Shape shape in _shapes)
            {
                int fp = shape.IsFeaturePoint(virtLoc, range);

                if (fp > 0 && _layers[shape.Layer - 1])
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

            InfoEvent?.Invoke(this, $"Disp:{e.Location}  Virt:{DisplayToVirtual(e.Location)}  OffsetX:{_offsetX}  OffsetY:{_offsetY}  Zoom:{_zoom}");
            base.OnMouseMove(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            bool redraw = false;

            // Number of detents the mouse wheel has rotated, multiplied by the WHEEL_DELTA constant.
            int delta = _settings.WheelResolution * e.Delta / SystemInformation.MouseWheelScrollDelta;

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
            _offsetX = Math.Min(_offsetX, SCROLL_NEGATIVE);
            _offsetY = Math.Min(_offsetY, SCROLL_NEGATIVE);

            if (redraw)
            {
                Invalidate();
            }
            base.OnMouseWheel(e);
        }
        #endregion

        #region Key events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.H: // reset
                    Reset();
                    break;
            }

            // ShowInfo();
            base.OnKeyDown(e);
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
            var virtX = ((float)disp.X - _offsetX - BORDER_SIZE) / _page.Scale / _zoom;
            var virtY = ((float)disp.Y - _offsetY - BORDER_SIZE) / _page.Scale / _zoom;
            return new PointF(virtX, virtY);
        }
        #endregion

        #region Private helpers
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
        #endregion
    }
}
