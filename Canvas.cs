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


// Deals exclusively in display units. Translation between display and virtual is done in GeometryMap.


namespace NDraw
{
    public partial class Canvas : UserControl // Should be Control but breaks in designer
    {
        #region Fields
        /// <summary>Current drawing.</summary>
        Page _page = new();

        /// <summary>The settings.</summary>
        UserSettings _settings = new();

        /// <summary>The various shapes in _page converted to internal format.</summary>
        readonly List<Shape> _shapes = new();

        /// <summary>Current horizontal offset in pixels.</summary>
        int _offsetX = 0;

        /// <summary>Current vertical offset in pixels.</summary>
        int _offsetY = 0;

        /// <summary>Current zoom level.</summary>
        float _zoom = 1.0F;
        #endregion

        #region Constants
        /// <summary>Cosmetics.</summary>
        const float GRID_LINE_WIDTH = 1.0f;

        /// <summary>How close do you have to be to select a shape in pixels.</summary>
        const float SELECT_RANGE = 10;

        /// <summary>How fast the mouse wheel goes.</summary>
        const int WHEEL_RESOLUTION = 4;

        /// <summary>Maximum zoom in limit.</summary>
        const float MAX_ZOOM = 10.0f;

        /// <summary>Minimum zoom out limit.</summary>
        const float MIN_ZOOM = 0.1f;

        /// <summary>Speed at which to zoom in/out.</summary>
        const float ZOOM_SPEED = 0.1F;

        /// <summary>Cosmetics.</summary>
        const int HIGHLIGHT_SIZE = 5;
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

            // Init geometry.
            Reset();

            Invalidate();
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

            // Draw the grid.
            DrawGrid(e.Graphics);

            // Draw the shapes.
            var tl = DisplayToVirtual(ClientRectangle.Location);
            var br = DisplayToVirtual(new Point(ClientRectangle.Right, ClientRectangle.Bottom));
            var virtVisible = new RectangleF(tl, new SizeF(br.X - tl.X, br.Y - tl.Y));

            foreach (var shape in _shapes)
            {
                // Is it visible?
                if (shape.ContainedIn(virtVisible, true))
                {
                    using Pen penLine = new(shape.LineColor, shape.LineThickness);

                    switch (shape)
                    {
                        case RectShape shapeRect:
                            var disptl = VirtualToDisplay(shapeRect.Location);
                            var dispbr = VirtualToDisplay(shapeRect.BR);
                            var dispRect = new RectangleF(disptl, new SizeF(dispbr.X - disptl.X, dispbr.Y - disptl.Y));

                            e.Graphics.DrawRectangle(penLine, dispRect.X, dispRect.Y, dispRect.Width, dispRect.Height);
                            e.Graphics.DrawString(shapeRect.Text, _settings.Font, Brushes.Black, dispRect.X + 2, dispRect.Y + 2);

                            if (shapeRect.State == ShapeState.Highlighted)
                            {
                                e.Graphics.FillEllipse(penLine.Brush, Squarify(dispRect.Left, dispRect.Top, HIGHLIGHT_SIZE));
                                e.Graphics.FillEllipse(penLine.Brush, Squarify(dispRect.Right, dispRect.Top, HIGHLIGHT_SIZE));
                                e.Graphics.FillEllipse(penLine.Brush, Squarify(dispRect.Left, dispRect.Bottom, HIGHLIGHT_SIZE));
                                e.Graphics.FillEllipse(penLine.Brush, Squarify(dispRect.Right, dispRect.Bottom, HIGHLIGHT_SIZE));
                            }
                            break;

                        case LineShape shapeLine:
                            var dispStart = VirtualToDisplay(shapeLine.Start);
                            var dispEnd = VirtualToDisplay(shapeLine.End);
                            e.Graphics.DrawLine(penLine, dispStart, dispEnd);
                            e.Graphics.DrawString(shapeLine.Text, _settings.Font, Brushes.Black, dispStart.X + 2, dispStart.Y + 2);

                            if (shapeLine.State == ShapeState.Highlighted)
                            {
                                e.Graphics.FillEllipse(penLine.Brush, Squarify(dispStart.X, dispStart.Y, HIGHLIGHT_SIZE));
                                e.Graphics.FillEllipse(penLine.Brush, Squarify(dispEnd.X, dispEnd.Y, HIGHLIGHT_SIZE));
                            }
                            else
                            {
                                // Draw line ends.
                                DrawPoint(shapeLine.Start, shapeLine.StartStyle);
                                DrawPoint(shapeLine.End, shapeLine.EndStyle);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="ps"></param>
        void DrawPoint(PointF pt, PointStyle ps) // TODO
        {
            switch (ps)
            {
                case PointStyle.ArrowFilled:
                    break;

                case PointStyle.None:
                    break;
            }

            //case Circle:
            //    g.DrawArc(pen, point.ClientPoint.X - x * 2, point.ClientPoint.Y - x * 2, x * 4, x * 4, 0, 360);
            //case Square:
            //    g.FillRectangle(pen.Brush, point.ClientPoint.X - x, point.ClientPoint.Y - x, series.PointWidth, series.PointWidth);
            //case Triangle:
            //    g.FillPolygon(pen.Brush, new PointF[]
            //        {
            //          new PointF(point.ClientPoint.X - x, point.ClientPoint.Y + x),   // Bottom-Left
            //          new PointF(point.ClientPoint.X + x, point.ClientPoint.Y + x),   // Bottom-Right
            //          new PointF(point.ClientPoint.X, point.ClientPoint.Y - x),       // Top-Middle
            //        });
            //case Dot:
            //    g.FillEllipse(pen.Brush, point.ClientPoint.X - x, point.ClientPoint.Y - x, series.PointWidth, series.PointWidth);
        }

        /// <summary>
        /// Draw the grid.
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        void DrawGrid(Graphics g)
        {
            using Pen penGrid = new(_settings.GridColor, GRID_LINE_WIDTH);

            float spacing = 50.0f; // TODO Need grid? calculate along with others

            // Draw X-Axis
            for (float tickPos = spacing; tickPos < Width; tickPos += spacing)
            {
                g.DrawLine(penGrid, tickPos, 0, tickPos, Width);
            }

            // Draw Y-Axis
            for (float tickPos = spacing; tickPos < Height; tickPos += spacing)
            {
                g.DrawLine(penGrid, 0, tickPos, Right, tickPos);
            }
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
            float range = SELECT_RANGE * _zoom / _page.Scale; // TODO this really should be done in display domain.

            foreach (Shape shape in _shapes)
            {
                if (shape.FeaturePoint(virtLoc, range) > 0)
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
            int delta = WHEEL_RESOLUTION * e.Delta / SystemInformation.MouseWheelScrollDelta;

            switch (e.Button, ControlPressed(), ShiftPressed())
            {
                case (MouseButtons.None, true, false): // Zoom in/out at mouse position
                    var zoomFactor = delta > 0 ? ZOOM_SPEED : -ZOOM_SPEED;
                    var newZoom = _zoom + zoomFactor;

                    if (newZoom > MIN_ZOOM && newZoom < MAX_ZOOM)
                    {
                        _zoom = newZoom;

                        // Adjust offsets to center zoom at mouse.
                        float offx = e.X * zoomFactor;// / 2;
                        float offy = e.Y * zoomFactor;// / 2;

                        _offsetX += (int)-offx;
                        _offsetY += (int)-offy;

                        //Trace($"offx:{offx} offy:{offy} Zoom:{Geometry.Zoom} GeoX:{Geometry.OffsetX} GeoY:{Geometry.OffsetY}");

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
        public void HandleKeyDown(KeyEventArgs e)
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
        #endregion

        /// <summary>
        /// Map a virtual point to the display.
        /// </summary>
        /// <param name="virt"></param>
        /// <returns></returns>
        PointF VirtualToDisplay(PointF virt)
        {
            var dispX = (virt.X * _zoom) * _page.Scale + _offsetX;
            var dispY = (virt.Y * _zoom) * _page.Scale + _offsetY;
            return new(dispX, dispY);
        }

        /// <summary>
        /// Obtain the virtual point for a display position.
        /// </summary>
        /// <param name="disp">The display point.</param>
        /// <returns>The virtual point.</returns>
        PointF DisplayToVirtual(Point disp)
        {
            var virtX = (disp.X - _offsetX) / _page.Scale / _zoom;
            var virtY = (disp.Y - _offsetY) / _page.Scale / _zoom;
            return new PointF(virtX, virtY);
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
    }
}
