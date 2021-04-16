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
using NStateMachine;
using NBagOfTricks;


//https://devblogs.microsoft.com/dotnet/whats-new-in-windows-forms-runtime-in-net-5-0/
//In.NET 5.0 we’ve lifted the bar higher and optimised several painting paths. Historically Windows Forms relied on GDI+ (and some GDI)
//for rendering operations. Whilst GDI + is easier to use than GDI because it abstracts the device context(a structure with information
//about a particular display device, such as a monitor or a printer) via the Graphics object, it’s also slow due to the additional overhead.
//In a number of situations where we deal with solid colours and brushes, we have opted to use GDI.
//We have also extended a number rendering-related APIs(e.g.PaintEventArgs) with IDeviceContext interface, which whilst may not be available
//to Windows Forms developers directly, allow us to bypass the GDI+ Graphics object, and thus reduce allocations and gain speed. These optimisations
//have shown a significant reduction in memory consumptions in redraw paths, in some cases saving x10 in memory allocations).


namespace NDraw
{
    public partial class Canvas : UserControl
    {
        /// <summary>Current drawing.</summary>
        Page _page = new();

        /// <summary>The various shapes in _page converted to internal format.</summary>
        List<Shape> _shapes = new();

        /// <summary>Top left corner of the drawing in pixels.</summary>
        PointF _origin = new(0, 0);

        /// <summary>Cosmetics.</summary>
        public const float GRID_LINE_WIDTH = 1.0f;

        /// <summary>How close do you have to be to select a feature in pixels.</summary>
        public const int SELECT_RANGE = 5;

        #region Control states
        /// <summary>If control is pressed.</summary>
        bool _ctrlPressed = false;

        /// <summary>If shift is pressed.</summary>
        bool _shiftPressed = false;

        /// <summary>Saves state as to whether left button is down.</summary>
        bool _mouseDown = false;

        /// <summary>Boolean to determine if the user is dragging the mouse.</summary>
        bool _dragging = false;
        #endregion

        #region Memories
        /// <summary>Start mouse position when button pressed.</summary>
        Point _startMousePos = new();

        /// <summary>End mouse position when button unpressed.</summary>
        Point _endMousePos = new();

        /// <summary>Saves the previous mouse position for move operations.</summary>
        Point _lastMousePos = new();
        #endregion

        #region Zoom
        /// <summary>Current zoom.</summary>
        float _zoomFactor = 1.0F;

        /// <summary>Maximum zoom in limit.</summary>
        const float MAX_ZOOM_LIMIT = 10.0f;

        /// <summary>Minimum zoom out limit.</summary>
        const float MIN_ZOOM_LIMIT = 0.1f;
        #endregion

        #region Drawing resources
        Pen _penGrid = new(Color.Gray);

        // Temp pen.
        Pen _penShapeTemp = new(Color.Green);

        Pen _penSelect = new(Color.Gray);
        #endregion

        #region Lifecycle
        public Canvas()
        {
            InitializeComponent();
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        /// <summary>Perform initialization.</summary>
        public void Init(Page page)
        {
            _page = page;

            _zoomFactor = 1.0f;

            _shapes.Clear();
            _shapes.AddRange(_page.Rects);
            _shapes.AddRange(_page.Lines);

            _penGrid.Color = UserSettings.TheSettings.GridColor;
            _penGrid.Width = GRID_LINE_WIDTH;

            Invalidate();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);

            _penGrid?.Dispose();
            _penShapeTemp?.Dispose();
            _penSelect?.Dispose();
        }
        #endregion

        #region Painting
        /// <summary>OnPaint event handler.  Calls the appropriate method to paint the chart based upon the chosen chartType.</summary>
        /// <param name="sender">Object that sent triggered the event.</param>
        /// <param name="e">The particular PaintEventArgs.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(UserSettings.TheSettings.BackColor);

            // Draw the grid and axes.
            PaintGrid(e.Graphics);
                                 
            // Draw the shapes.
            RectangleF cr = ClientRectangle;

            foreach (var s in _shapes)
            {
                switch(s)
                {
                    case RectShape r:
                        if (r.ContainedIn(cr, true))
                        {
                            e.Graphics.DrawRectangle(_penShapeTemp, r.L, r.T, r.Width, r.Height);
                        }
                        break;

                    case LineShape l:
                        if (cr.Contains(l.X1, l.Y1) || cr.Contains(l.X2, l.Y2))
                        {
                            e.Graphics.DrawLine(_penShapeTemp, l.X1, l.Y1, l.X2, l.Y2);
                        }
                        break;
                }
            }

            // Draw Selection Rectangle if ctrl is down and not dragging cursor.
            if (_ctrlPressed && _dragging)
            {
                float width = 0.0F, height = 0.0F;

                Point startPoint = new();

                if (_endMousePos.X > _startMousePos.X)
                {
                    width = _endMousePos.X - _startMousePos.X;
                    startPoint.X = _startMousePos.X;
                }
                else
                {
                    width = _startMousePos.X - _endMousePos.X;
                    startPoint.X = _endMousePos.X;
                }

                if (_endMousePos.Y > _startMousePos.Y)
                {
                    height = _endMousePos.Y - _startMousePos.Y;
                    startPoint.Y = _startMousePos.Y;
                }
                else
                {
                    height = _startMousePos.Y - _endMousePos.Y;
                    startPoint.Y = _endMousePos.Y;
                }

                e.Graphics.DrawRectangle(_penSelect, startPoint.X, startPoint.Y, width, height);
            }

            // Draw the Border
            //e.Graphics.DrawRectangle(new Pen(Color.RoyalBlue, 1), new Rectangle(0, 0, Width - 1, Height - 1));

            //_firstPaint = false;
        }

        /// <summary>Draw the axes on the chart.</summary>
        /// <param name="g">The Graphics object to use.</param>
        /// <param name="xticks">List of X Tick values.</param>
        /// <param name="yticks">List of Y Tick values.</param>
        void PaintGrid(Graphics g)//, out List<StringFloatPair> xticks, out List<StringFloatPair> yticks)
        {
            float _gridWidth = 42.0f; //TODO calculate along with others

            // Draw X-Axis Increments
            for (float tickPos = _gridWidth; tickPos < Width; tickPos += _gridWidth)
            {
                g.DrawLine(_penGrid, tickPos, 0, tickPos, Width);
            }

            // Draw Y-Axis Increments
            for (float tickPos = _gridWidth; tickPos < Height; tickPos += _gridWidth)
            {
                //float x = 
                g.DrawLine(_penGrid, 0, tickPos, Right, tickPos);
            }
        }

        /// <summary>An axis label should be drawn using the specified transformations.</summary>
        /// <param name="g">The Graphics object to use.</param>
        /// <param name="transformX">The X transform</param>
        /// <param name="transformY">The Y transform</param>
        /// <param name="labelText">The text of the label.</param>
        /// <param name="rotationDegrees">The rotation of the axis.</param>
        void PaintLabel(Graphics g, float transformX, float transformY, string labelText, Font font, int rotationDegrees)
        {
            g.TranslateTransform(transformX, transformY);
            g.RotateTransform(rotationDegrees);
            g.DrawString(labelText, font, Brushes.Black, new PointF(0F, 0F));
            g.ResetTransform();
        }
        #endregion

        #region Misc window events
        protected override void OnResize(EventArgs e)
        {
            // Force repaint of chart.
            //_firstPaint = true; // Need to recalc the grid too.
           // Invalidate();
            //Refresh();
        }

        /// <summary>Resets key states when control loses focus.</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        protected override void OnLostFocus(EventArgs e)
        {
            _ctrlPressed = false;
            _shiftPressed = false;
        }
        #endregion

        #region Mouse events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    // Retain mouse state.
                    _mouseDown = true;
                    _startMousePos = new Point(e.X, e.Y);

                    if (!_shiftPressed)
                    {
                        //// Are we on a cursor?
                        //_dragCursor = GetClosestCursor(_startMousePos);

                        //// Unselect previously selected points.
                        //UnselectPoints();
                        //SelectedPoints = new List<DataPoint>();

                        // Force repaint of chart.
                        Invalidate();
                        //Refresh();
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_ctrlPressed)
                    {
                        // Get points within bounds of dragged rectangle
                        if (_shiftPressed)
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
                        }
                        else
                        {
                            //SelectedPoints = GetSelectedPoints();
                        }

                        // Reset status.
                        _dragging = false;
                        Cursor = Cursors.Default;

                        // Force repaint of chart
                        Invalidate();
                        //Refresh();

                        _startMousePos = new Point();
                        _endMousePos = new Point();
                    }
                    else if (_shiftPressed)
                    {
                        Shape pt = GetClosestShape(new Point(e.X, e.Y));

                        if (pt != null)
                        {
                            //if (SelectedPoints.Contains(pt))
                            //{
                            //    SelectedPoints.Remove(pt);
                            //    pt.Selected = false;
                            //}
                            //else
                            //{
                            //    SelectedPoints.Add(pt);
                            //    pt.Selected = true;
                            //}

                            Invalidate();
                            //Refresh();
                        }
                    }
                    //else if (_dragCursor != null)
                    //{
                    //    // Notify clients.
                    //    ChartCursorMove(this, new ChartCursorMoveEventArgs() { CursorId = _dragCursor.Id, Position = _dragCursor.Position });
                    //}

                    // Reset status.
                    _mouseDown = false;
//                    _dragCursor = null;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Focus();
            if (!_ctrlPressed)
            {
                Point newPos = new(e.X, e.Y);

                // If the _mouseDown state is true then move the chart with the mouse.
                if (_mouseDown && (_lastMousePos != new Point(int.MaxValue, int.MaxValue)))
                {
                    int xChange = (newPos.X - _lastMousePos.X);
                    int yChange = (newPos.Y - _lastMousePos.Y);

                    // If there is a change in x or y...
                    if (xChange != 0 || yChange != 0)
                    {
                        if (_dragging)
                        {
                            // Update the cursor position. Get mouse x and convert to x axis scaled value.
                            PointF pt = GetChartPoint(new PointF(newPos.X, newPos.Y));
                            //_dragCursor.Position = pt.X;
                        }
                        else
                        {
                            // Adjust the axes
                            _origin.Y += yChange;
                            _origin.X += xChange;

                            //FireScaleChange();
                        }

                        // Repaint
                        Invalidate();
                        //Refresh();
                    }
                }

                _lastMousePos = newPos;
            }
            else
            {
                if (_mouseDown)
                {
                    // Do some special stuff to show a rectangle selection box.
                    _endMousePos = new Point(e.X, e.Y);
                    _dragging = true;
                    Cursor = Cursors.Hand;

                    // Force repaint of chart
                    Invalidate();
                    //Refresh();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var hme = e as HandledMouseEventArgs;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent. ???

            // mouse wheel:
            //    if ctrl: zoom
            //    else if shift: left/right
            //    else: up/down
            bool update = false;
            switch (_ctrlPressed, _shiftPressed) 
            {
                case (true, false):
                    _zoomFactor *= e.Delta > 0 ? 1.1f : 0.9f;
                    update = true;
                    break;

                case (false, true):
                    _origin.X += e.Delta;
                    update = true;
                    break;

                case (false, false):
                    _origin.Y += e.Delta;
                    update = true;
                    break;

                default:
                    break;
            };

            if(update)
            {
                // Check limits. TODO
                //Gets a signed count of the number of detents the mouse wheel has rotated, multiplied
                //     by the WHEEL_DELTA constant. A detent is one notch of the mouse wheel.

                Invalidate();
            }
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
                case Keys.ControlKey:
                    if (!_ctrlPressed)
                    {
                        _startMousePos = new(_lastMousePos.X, _lastMousePos.Y);
                    }
                    _ctrlPressed = true;
                    break;

                case Keys.ShiftKey:
                    _shiftPressed = true;
                    break;

                case Keys.H: // reset
                    //_firstPaint = true;
                    _zoomFactor = 1.0f;
                    Invalidate();
                    //Refresh();
                    break;

                case Keys.A: //TODO
                    break;

                case Keys.C: //TODO
                    break;

                case Keys.Escape: //TODO
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ControlKey:
                    _ctrlPressed = false;

                    if (_dragging)
                    {
                        GetSelectedShapes();
                        _dragging = false;
                        Cursor = Cursors.Default;

                        //                       _dragCursor = null;
                        Invalidate();
                        //Refresh();
                    }

                    _startMousePos = new Point();
                    _endMousePos = new Point();
                    break;

                case Keys.ShiftKey:
                    _shiftPressed = false;
                    break;
            }
        }
        #endregion

        #region Private helper functions
        /// <summary>
        /// 
        /// </summary>
        void Reset()
        {
            _zoomFactor = 1.0f;
            _origin.X = 0;
            _origin.Y = 0;
            Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pvirt"></param>
        void ConvertVirtualToScreen(PointX pvirt)
        {
            // C:\Dev\repos\NBagOfTricks\Source\Utils\MathUtils.cs:
            //   109: public static double Map(double val, double start1, double stop1, double start2, double stop2)
            //   123: public static int Map(int val, int start1, int stop1, int start2, int stop2)
            // 
            // for (int i = 0; i < _cpuBuff.Length; i++)
            // {
            //     int index = _buffIndex - i;
            //     index = index < 0 ? index + _cpuBuff.Length : index;
            // 
            //     double val = _cpuBuff[index];
            // 
            //     // Draw data point.
            //     double y = MathUtils.Map(val, _min, _max, Height, 0);
            //     pe.Graphics.DrawLine(_pen, (float)i, (float)y, (float)i, Height);
            // }

            // NBagOfTricks.Utils.MathUtils.Map()
        }

        /// <summary>Find the closest to the given point.</summary>
        /// <param name="point">Mouse point</param>
        /// <returns>The closest DataPoint</returns>
        Shape GetClosestShape(Point point)
        {
            Shape closest = null;

            //foreach (DataPoint p in series.DataPoints)
            //{
            //    if (Math.Abs(point.X - p.ClientPoint.X) < MOUSE_SELECT_RANGE && Math.Abs(point.Y - p.ClientPoint.Y) < MOUSE_SELECT_RANGE)
            //    {
            //        closestSeries = series;
            //        closestPoint = p;
            //    }
            //}

            return closest;
        }

        /// <summary>Select multiple shapes.</summary>
        /// <returns>The shapes.</returns>
        List<Shape> GetSelectedShapes()
        {
            List<Shape> shapes = new();

            //    if (!_startMousePos.IsEmpty && !_endMousePos.IsEmpty)
            //    {
            //        float xmin, xmax, ymin, ymax;
            //        if (_startMousePos.X > _endMousePos.X)
            //        {
            //            xmin = _endMousePos.X;
            //            xmax = _startMousePos.X;
            //        }
            //        else
            //        {
            //            xmin = _startMousePos.X;
            //            xmax = _endMousePos.X;
            //        }

            //        if (_startMousePos.Y > _endMousePos.Y)
            //        {
            //            ymin = _endMousePos.Y;
            //            ymax = _startMousePos.Y;
            //        }
            //        else
            //        {
            //            ymin = _startMousePos.Y;
            //            ymax = _endMousePos.Y;
            //        }

            //        foreach (DataSeries series in SeriesCollection.Items)
            //        {
            //            foreach (DataPoint point in series.DataPoints)
            //            {
            //                if (point.ClientPoint.X >= xmin && point.ClientPoint.X <= xmax &&
            //                    point.ClientPoint.Y >= ymin && point.ClientPoint.Y <= ymax)
            //                {
            //                    points.Add(point);
            //                    point.Selected = true;
            //                }
            //            }
            //        }
            //    }

            return shapes;
        }

        /// <summary>Obtain the point on the chart based on a client position.</summary>
        /// <param name="p">The PointF to restore</param>
        /// <returns>A PointF corresponding to the true coordinates (non-client) of the given PointF.</returns>
        PointF GetChartPoint(PointF p)
        {
            return new PointF();
            //return new PointF((float)((p.X - _origin.X) / _xZoomedScale), (float)((_origin.Y - p.Y) / _yZoomedScale));
        }
        #endregion
    }
}
