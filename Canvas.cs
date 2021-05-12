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

        /// <summary>The user is dragging the mouse.</summary>
        bool _dragging = false;

        /// <summary>Mouse position when button pressed.</summary>
        Point _startPos = new();

        /// <summary>Current mouse position.</summary>
        Point _currentPos = new();

        ///// <summary>If control is pressed.</summary>
        //bool _ctrlPressed = false;

        ///// <summary>If shift is pressed.</summary>
        //bool _shiftPressed = false;

        /// <summary>Saves state as to whether left button is down.</summary>
        //bool _mouseDown = false;

        /// <summary>Mouse position when button unpressed.</summary>
        //Point _endMousePos = new();
        #endregion

        #region Constants
        /// <summary>Cosmetics.</summary>
        const float GRID_LINE_WIDTH = 1.0f;

        /// <summary>How close do you have to be to select a shape in pixels.</summary>
        const float SELECT_RANGE = 5;

        /// <summary>How fast the mouse wheel goes.</summary>
        const int WHEEL_RESOLUTION = 4;

        /// <summary>Maximum zoom in limit.</summary>
        const float MAX_ZOOM = 10.0f;

        /// <summary>Minimum zoom out limit.</summary>
        const float MIN_ZOOM = 0.1f;

        /// <summary>Speed at which to zoom in/out.</summary>
        const float ZOOM_SPEED = 0.1F;

        /// <summary>Cosmetics.</summary>
        const int HIGHLIGHT_SIZE = 3;
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
            Geometry.Reset();
            //Geometry.DrawArea = ClientRectangle;

            Invalidate();
        }

        /// <summary>
        /// Cleaan up and save to file.
        /// </summary>
        /// <returns></returns>
        public void SavePage(string fn)
        {
            // Collect changes.
            _page.Lines.Clear();
            _page.Rects.Clear();

            foreach(var sh in _shapes)
            {
                switch(sh)
                {
                    case LineShape ls:
                        _page.Lines.Add(ls);
                        break;

                    case RectShape rs:
                        _page.Rects.Add(rs);
                        break;
                }
            }

            _page.Save(fn);
        }
        #endregion

        #region Misc window events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            //Geometry.DrawArea = ClientRectangle;
            lblInfo.Location = new(10, Bottom - lblInfo.Height - 10);
            Invalidate();
        }

        /// <summary>
        /// Resets key states when control loses focus.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        protected override void OnLostFocus(EventArgs e)
        {
            //_ctrlPressed = false;
            //_shiftPressed = false;
            _dragging = false;
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
            RectangleF virtVisible = Geometry.DisplayToVirtual(ClientRectangle);

            foreach (var shape in _shapes)
            {
                // Is it visible?
                if (shape.ContainedIn(virtVisible, true))
                {
                    using Pen penLine = new(shape.LineColor, shape.LineThickness);
                    using Pen penHighlight = new(shape.LineColor, shape.LineThickness + 2);

                    switch (shape)
                    {
                        case RectShape shapeRect:
                            var dispRect = Geometry.VirtualToDisplay(shapeRect.TL, shapeRect.BR);
                            e.Graphics.DrawRectangle(shapeRect.State == ShapeState.Highlighted ? penHighlight : penLine, dispRect.X, dispRect.Y, dispRect.Width, dispRect.Height);
                            e.Graphics.DrawString(shapeRect.Text, _settings.Font, Brushes.Black, dispRect.X + 2, dispRect.Y + 2);

                            //// test code......
                            if (shapeRect.State == ShapeState.Highlighted)
                            {
                                var edges = shapeRect.GetEdges();
                                int i = 1;
                                foreach (var (start, end) in edges)
                                {
                                    var edgeRect = Geometry.Expand(start, end, 5 / Geometry.Zoom);//TODO1 this doesn't work for angled lines

                                    var dispEdge = Geometry.VirtualToDisplay(edgeRect);

                                    e.Graphics.DrawRectangle(penLine, dispEdge.X, dispEdge.Y, dispEdge.Width, dispEdge.Height);
                                    e.Graphics.DrawString($"{i}", _settings.Font, Brushes.Black, dispEdge.X + 1, dispEdge.Y + 1);
                                    i++;
                                }
                            }

                            if (shapeRect.State == ShapeState.Selected)
                            {
                                e.Graphics.FillEllipse(penLine.Brush, dispRect.X, dispRect.Y, HIGHLIGHT_SIZE, HIGHLIGHT_SIZE);
                            }
                            break;

                        case LineShape shapeLine:
                            var dispStart = Geometry.VirtualToDisplay(shapeLine.Start);
                            var dispEnd = Geometry.VirtualToDisplay(shapeLine.End);
                            e.Graphics.DrawLine(shapeLine.State == ShapeState.Highlighted ? penHighlight : penLine, dispStart, dispEnd);
                            e.Graphics.DrawString(shapeLine.Text, _settings.Font, Brushes.Black, dispStart.X + 2, dispStart.Y + 2);

                            if (shapeLine.State == ShapeState.Selected)
                            {
                                e.Graphics.FillEllipse(penLine.Brush, dispStart.X, dispStart.Y, HIGHLIGHT_SIZE, HIGHLIGHT_SIZE);
                            }
                            break;
                    }
                }
            }

            // Draw Selection Rectangle if dragging cursor.
            if (_dragging)
            {
                using Pen penSelect = new(_settings.GridColor, GRID_LINE_WIDTH);

                Point start = new();
                Point pos = PointToClient(Cursor.Position);

                float width = Math.Abs(pos.X - _startPos.X);
                float height = Math.Abs(pos.Y - _startPos.Y);
                start.X = pos.X > _startPos.X ? _startPos.X : pos.X;
                start.Y = pos.Y > _startPos.Y ? _startPos.Y : pos.Y;

                e.Graphics.DrawRectangle(penSelect, start.X, start.Y, width, height);
            }
        }

        /// <summary>
        /// Draw the grid.
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        void DrawGrid(Graphics g)
        {
            using Pen penGrid = new(_settings.GridColor, GRID_LINE_WIDTH);

            float spacing = 42.0f; // TODO1 calculate along with others

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
        protected override void OnMouseDown(MouseEventArgs e)
        {
            _startPos = e.Location;
            _currentPos = e.Location;

            if(e.Button == MouseButtons.Left && ControlPressed()) // if near a shape toggle selected
            {
                Shape pt = GetCloseShape(e.Location);

                if (pt != null)
                {
                    pt.State = pt.State == ShapeState.Selected ? ShapeState.Default : ShapeState.Selected;
                    Invalidate();
                }
            }

            ShowInfo();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _currentPos = e.Location;

            if (e.Button == MouseButtons.Left && ControlPressed()) // Select shapes within drag rectangle TODOSEL adjust
            {
                //List<DataPoint> tempPoints = GetSelectedPoints();
                //foreach (DataPoint pt in tempPoints)
                //{
                //    if (!SelectedPoints.Contains(pt))
                //    {
                //        SelectedPoints.Add(pt);
                //        pt.Selected = true;
                //    }
                //}
                // }
            }

            ShowInfo();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            _currentPos = e.Location;

            bool redraw = false;

            switch (e.Button, ControlPressed())
            {
                case (MouseButtons.Left, false): // drawing selection rect
                    if(!_dragging)
                    {
                        _dragging = true;
                    }
                    else
                    {

                    }
                    
                    //_endMousePos = new Point(e.X, e.Y);
                    redraw = true;
                    break;

                case (MouseButtons.None, false): // highlight any close shapes
                    var virtLoc = Geometry.DisplayToVirtual(e.Location);
                    foreach (Shape shape in _shapes)
                    {
                        if(shape.IsClose(virtLoc, SELECT_RANGE / Geometry.Zoom))//TODO1 this should be done in display domain
                        {
                            if (shape.State == ShapeState.Default)
                            {
                                shape.State = ShapeState.Highlighted;
                                redraw = true;
                            }
                        }
                        else
                        {
                            // Unhighlight those away from.
                            if(shape.State == ShapeState.Highlighted)
                            {
                                shape.State = ShapeState.Default;
                                redraw = true;
                            }
                        }
                    }
                    break;

                default:
                    break;
            };

            if(redraw)
            {
                Invalidate();
            }

            ShowInfo();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //(e as HandledMouseEventArgs).Handled = true; // This prevents the mouse wheel event from getting back to the parent. TODO3?

            bool redraw = false;

            // Number of detents the mouse wheel has rotated, multiplied by the WHEEL_DELTA constant.
            int delta = WHEEL_RESOLUTION * e.Delta / SystemInformation.MouseWheelScrollDelta;

            switch (e.Button, ControlPressed(), ShiftPressed())
            {
                case (MouseButtons.None, true, false): // Zoom in/out at mouse position
                    var zoomFactor = delta > 0 ? ZOOM_SPEED : -ZOOM_SPEED;
                    var newZoom = Geometry.Zoom + zoomFactor;

                    if (newZoom > MIN_ZOOM && newZoom < MAX_ZOOM)
                    {
                        Geometry.Zoom = newZoom;

                        // Adjust offsets to center zoom at mouse.
                        float offx = e.X * zoomFactor;// / 2;
                        float offy = e.Y * zoomFactor;// / 2;

                        Geometry.OffsetX += (int)-offx;
                        Geometry.OffsetY += (int)-offy;

                        //Trace($"offx:{offx} offy:{offy} Zoom:{Geometry.Zoom} GeoX:{Geometry.OffsetX} GeoY:{Geometry.OffsetY}");

                        redraw = true;
                    }
                    break;

                case (MouseButtons.None, false, true): // Shift left/right
                    Geometry.OffsetX += delta;
                    redraw = true;
                    break;

                case (MouseButtons.None, false, false): // Shift up/down
                    Geometry.OffsetY += delta;
                    redraw = true;
                    break;

                default:
                    break;
            };

            if(redraw)
            {
                Invalidate();
            }

            ShowInfo();
        }
        #endregion

        #region Key events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void HandleKeyDown(KeyEventArgs e)
        {
            //bool recalc = false;
            bool redraw = false;

            switch (e.KeyCode)
            {
                //case Keys.ControlKey:
                //    if (!_ctrlPressed)
                //    {
                //        _startPos = Cursor.Position;
                //        _ctrlPressed = true;
                //    }
                //    break;

                //case Keys.ShiftKey:
                //    _shiftPressed = true;
                //    break;

                case Keys.H: // reset
                    //Trace("Got H");
                    Reset();
                    redraw = true;
                    break;

                case Keys.A: //TODOSEL select all
                    redraw = true;
                    break;

                case Keys.C: //TODOSEL copy/paste/cut
                    redraw = true;
                    break;

                case Keys.Escape: //TODOSEL reset all selections
                    redraw = true;
                    break;
            }

            if(redraw)
            {
                Invalidate();
            }

            ShowInfo();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void Handle_KeyUp(KeyEventArgs e)
        {
            bool redraw = false;

            //switch (e.KeyCode)
            //{
            //    case Keys.ControlKey:
            //        _ctrlPressed = false;

            //        if (_dragging)
            //        {
            //            GetSelectedShapes();
            //            _dragging = false;
            //            Cursor = Cursors.Default;

            //            //_dragCursor = null;
            //            redraw = true;
            //            //Refresh();
            //        }

            //        _startPos = new Point();
            //        //_endMousePos = new Point();
            //        break;

            //    case Keys.ShiftKey:
            //        _shiftPressed = false;
            //        break;
            //}

            if (redraw)
            {
                Invalidate();
            }

            ShowInfo();
        }
        #endregion

        #region Private helpers
        /// <summary>
        /// Debug helper.
        /// </summary>
        void ShowInfo()
        {
            lblInfo.Text = $"Mouse:{_currentPos} OffsetX:{Geometry.OffsetX} OffsetY:{Geometry.OffsetY} Zoom:{Geometry.Zoom}";
            //lblInfo.Text = $"Mouse:{_currentPos} Ctrl:{(_ctrlPressed ? "D" : "U")} Shift:{(_shiftPressed ? "D" : "U")} OffsetX:{GeometryMap.OffsetX} OffsetY:{GeometryMap.OffsetY} Zoom:{GeometryMap.Zoom}";
        }

        /// <summary>
        /// To initial state.
        /// </summary>
        void Reset()
        {
            Geometry.Reset();
            //Invalidate();
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

        /// <summary>Get shape that is within range of point.</summary>
        /// <param name="point">Mouse point</param>
        /// <returns>The closest DataPoint or null if none in range.</returns>
        Shape GetCloseShape(Point pt)
        {
            Shape close = null;

            var virtPoint = Geometry.DisplayToVirtual(pt);

            foreach (var shape in _shapes)
            {
                if(shape.IsClose(virtPoint, SELECT_RANGE / Geometry.Zoom))//TODO1 this should be done in display domain
                {
                    close = shape;
                    break;
                }
            }

            return close;
        }

        /// <summary>Select multiple shapes.</summary>
        /// <returns>The shapes.</returns>
        List<Shape> GetSelectedShapes() //TODOSEL
        {
            List<Shape> shapes = new();

            return shapes;
        }
        #endregion
    }
}

/*
    // Draw Point Shape
    switch (point.PointShape)
    {
        case DataPoint.PointShapeType.Circle:
            g.DrawArc(pen, point.ClientPoint.X - x * 2, point.ClientPoint.Y - x * 2, x * 4, x * 4, 0, 360);
            break;

        case DataPoint.PointShapeType.Square:
            g.FillRectangle(pen.Brush, point.ClientPoint.X - x, point.ClientPoint.Y - x, series.PointWidth, series.PointWidth);
            break;

        case DataPoint.PointShapeType.Triangle:
            g.FillPolygon(pen.Brush, new PointF[]
                {
                    new PointF(point.ClientPoint.X - x, point.ClientPoint.Y + x),   // Bottom-Left
                    new PointF(point.ClientPoint.X + x, point.ClientPoint.Y + x),   // Bottom-Right
                    new PointF(point.ClientPoint.X, point.ClientPoint.Y - x),       // Top-Middle
                });
            break;

        case DataPoint.PointShapeType.X:
            g.DrawLine(pen,
                new PointF(point.ClientPoint.X - x, point.ClientPoint.Y - x),       // Top-Left --> Bottom-Right
                new PointF(point.ClientPoint.X + x, point.ClientPoint.Y + x));
            g.DrawLine(pen,
                new PointF(point.ClientPoint.X - x, point.ClientPoint.Y + x),       // Bottom-Left --> Top-Right
                new PointF(point.ClientPoint.X + x, point.ClientPoint.Y - x));
            break;

        case DataPoint.PointShapeType.Asterix:
            g.DrawLine(pen,
                new PointF(point.ClientPoint.X - x, point.ClientPoint.Y - x),       // Top-Left --> Bottom-Right
                new PointF(point.ClientPoint.X + x, point.ClientPoint.Y + x));
            g.DrawLine(pen,
                new PointF(point.ClientPoint.X - x, point.ClientPoint.Y + x),       // Bottom-Left --> Top-Right
                new PointF(point.ClientPoint.X + x, point.ClientPoint.Y - x));
            g.DrawLine(pen,
                new PointF(point.ClientPoint.X, point.ClientPoint.Y - x),           // Top-Middle --> Bottom-Middle
                new PointF(point.ClientPoint.X, point.ClientPoint.Y + x));
            break;

        case DataPoint.PointShapeType.Minus:
            g.DrawLine(pen, point.ClientPoint.X - x, point.ClientPoint.Y, point.ClientPoint.X + x, point.ClientPoint.Y);
            break;

        case DataPoint.PointShapeType.Plus:
            g.DrawLine(pen, point.ClientPoint.X - x, point.ClientPoint.Y, point.ClientPoint.X + x, point.ClientPoint.Y);
            g.DrawLine(pen, point.ClientPoint.X, point.ClientPoint.Y + x, point.ClientPoint.X, point.ClientPoint.Y - x);
            break;

        case DataPoint.PointShapeType.Smiley:
            // 1/3 of the point width
            float x_1_3 = (series.PointWidth / 3F);
            // 1/6 of the point width
            float x_1_6 = (series.PointWidth / 6F);
            // Circle
            g.DrawEllipse(pen, point.ClientPoint.X - x, point.ClientPoint.Y - x, series.PointWidth, series.PointWidth);
            // Left Eye
            g.FillEllipse(pen.Brush, point.ClientPoint.X - x_1_6, point.ClientPoint.Y - x_1_6, series.PointWidth / 4F, series.PointWidth / 4F);
            // Right Eye
            g.FillEllipse(pen.Brush, point.ClientPoint.X + x_1_6, point.ClientPoint.Y - x_1_6, series.PointWidth / 4F, series.PointWidth / 4F);
            // Mouth
            g.DrawArc(pen, point.ClientPoint.X - x_1_6, point.ClientPoint.Y + x_1_6, x_1_3, x_1_6, 0, 180);
            break;

        case DataPoint.PointShapeType.Dot:
            g.FillEllipse(pen.Brush, point.ClientPoint.X - x, point.ClientPoint.Y - x, series.PointWidth, series.PointWidth);
            break;
    }
*/