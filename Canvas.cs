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


//If you are starting from scratch and you want to use Json then go for Utf8Json which is generic and very fast. Also the startup costs are
//lower than from JIL because Utf8Json and MessagePackSharp (same author) seem to hit a fine spot by having most code already put into the
//main library and generate only a small amount of code around that. DataContractSerializer generates only the de/serialize code depending
//on the code path you actually hit on the fly.

//System.Text.Json library (above this is labeled as “NetCoreJson”).


namespace NDraw
{
    public partial class Canvas : UserControl
    {
        /// <summary>Current drawing.</summary>
        Page _page = new();

        /// <summary>The various shapes in _page converted to internal format.</summary>
        List<Shape> _shapes = new();

        /// <summary>The center of the display in pixels.</summary>
        PointF _origin = new(0, 0);

        /// <summary>Cosmetics.</summary>
        const float GRID_LINE_WIDTH = 1.0f;

        /// <summary>How close do you have to be to select a feature in pixels.</summary>
        const int SELECT_RANGE = 5;

        #region Control states
        /// <summary>If control is pressed.</summary>
        bool _ctrlPressed = false;

        /// <summary>If shift is pressed.</summary>
        bool _shiftPressed = false;

        /// <summary>Saves state as to whether left button is down.</summary>
 //       bool _mouseDown = false;

        /// <summary>The user is dragging the mouse.</summary>
        bool _dragging = false;
        #endregion

        #region Memories
        /// <summary>Mouse position when button pressed.</summary>
        Point _startPos = new();

        /// <summary>Mouse position when button unpressed.</summary>
//        Point _endMousePos = new();

        /// <summary>Current mouse position.</summary>
 //       Point _currentMousePos = new();
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
        Pen _penShapeTemp = new(Color.Green, 2);
        Pen _penHighlightTemp = new(Color.Green, 4);

        Pen _penSelect = new(Color.Gray);
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
        /// Perform initialization. Convert page to internal format.
        /// </summary>
        /// <param name="page"></param>
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
        /// Convert current shapes to Page and save to file.
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Object that sent triggered the event.</param>
        /// <param name="e">The particular PaintEventArgs.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(UserSettings.TheSettings.BackColor);

            // Draw the grid and axes.
            DrawGrid(e.Graphics);
                                 
            // Draw the shapes.
            RectangleF cr = ClientRectangle;
            int marker = 3;

            foreach (var s in _shapes)
            {
                if (s.ContainedIn(cr, true))
                {
                    switch (s)
                    {
                        case RectShape r:
                            e.Graphics.DrawRectangle(r.State == ShapeState.Highlighted ? _penHighlightTemp : _penShapeTemp, r.L, r.T, r.Width, r.Height);

                            if (r.State == ShapeState.Selected)
                            {
                                e.Graphics.FillEllipse(_penShapeTemp.Brush, r.L, r.T, marker, marker);
                            }
                            break;

                        case LineShape l:
                            e.Graphics.DrawLine(l.State == ShapeState.Highlighted ? _penHighlightTemp : _penShapeTemp, l.X1, l.Y1, l.X2, l.Y2);

                            if (l.State == ShapeState.Selected)
                            {
                                e.Graphics.FillEllipse(_penShapeTemp.Brush, l.X1, l.Y1, marker, marker);
                            }
                            break;
                    }
                }
            }

            // Draw Selection Rectangle if dragging cursor.
            if (_dragging)
            {
                Point start = new();
                Point pos = PointToClient(Cursor.Position);

                float width = Math.Abs(pos.X - _startPos.X);
                float height = Math.Abs(pos.Y - _startPos.Y);
                start.X = pos.X > _startPos.X ? _startPos.X : pos.X;
                start.Y = pos.Y > _startPos.Y ? _startPos.Y : pos.Y;

                e.Graphics.DrawRectangle(_penSelect, start.X, start.Y, width, height);
            }

            DrawText(e.Graphics, 450, 150, "Hello!", UserSettings.TheSettings.DefaultFont, 45);
        }

        /// <summary>
        /// Draw the grid.
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        void DrawGrid(Graphics g)
        {
            float spacing = 42.0f; //TODO calculate along with others

            // Draw X-Axis Increments
            for (float tickPos = spacing; tickPos < Width; tickPos += spacing)
            {
                g.DrawLine(_penGrid, tickPos, 0, tickPos, Width);
            }

            // Draw Y-Axis Increments
            for (float tickPos = spacing; tickPos < Height; tickPos += spacing)
            {
                g.DrawLine(_penGrid, 0, tickPos, Right, tickPos);
            }
        }

        /// <summary>
        /// A label should be drawn using the specified transformations.
        /// </summary>
        /// <param name="g">The Graphics object to use.</param>
        /// <param name="transformX">The X transform</param>
        /// <param name="transformY">The Y transform</param>
        /// <param name="labelText">The text of the label.</param>
        /// <param name="rotationDegrees">The rotation of the axis.</param>
        void DrawText(Graphics g, float transformX, float transformY, string labelText, Font font, int rotationDegrees)
        {
            g.TranslateTransform(transformX, transformY);
            g.RotateTransform(rotationDegrees);
            g.DrawString(labelText, font, Brushes.Black, 0, 0);
            g.ResetTransform();
        }
        #endregion

        #region Misc window events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Resets key states when control loses focus.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        protected override void OnLostFocus(EventArgs e)
        {
            _ctrlPressed = false;
            _shiftPressed = false;
            _dragging = false;
            Invalidate();
        }
        #endregion

        #region Mouse events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool update = false;
            switch (e.Button, _ctrlPressed, _shiftPressed) 
            {
                case (MouseButtons.Left, true, false): // if near a shape toggle selected
                    Shape pt = GetCloseShape(new PointF(e.X, e.Y));

                    if (pt != null)
                    {
                        pt.State = pt.State == ShapeState.Selected ? ShapeState.Default : ShapeState.Selected;
                        update = true;
                    }
                    break;

                default:
                    break;
            };

            _startPos = new Point(e.X, e.Y);
//            _mouseDown = true;

            if(update)
            {
                // TODO
                Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            bool update = false;
            switch (e.Button, _ctrlPressed, _shiftPressed) 
            {
                case (MouseButtons.Left, false, false): // Select shapes within drag rectangle TODOX
                    
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

                    update = true;
                    break;

                default:
                    break;
            };

//            _mouseDown = false;

            if(update)
            {
                // TODO
                Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //// Save position.
            //_lastMousePos.X = e.X;
            //_lastMousePos.Y = e.Y;

            bool update = false;
            switch (e.Button, _ctrlPressed, _shiftPressed) 
            {
                //case (MouseButtons.Left, true, false): // toggle selecting shapes
                //    update = true;
                //    break;

                case (MouseButtons.Left, false, false): // drawing selection rect
                    if(!_dragging)
                    {
                        _dragging = true;
                    }
                    else
                    {

                    }
//                    _endMousePos = new Point(e.X, e.Y);
                    update = true;
                    break;

                case (MouseButtons.None, false, false): // highlight any close shapes
                    foreach(Shape shape in _shapes)
                    {
                        if(shape.IsClose(e.Location, SELECT_RANGE))
                        {
                            if (shape.State == ShapeState.Default)
                            {
                                shape.State = ShapeState.Highlighted;
                                update = true;
                            }
                        }
                        else
                        {
                            if(shape.State == ShapeState.Highlighted)
                            {
                                shape.State = ShapeState.Default;
                                update = true;
                            }
                        }
                    }
                    break;

                default:
                    break;
            };

            if(update)
            {
                // TODO
                Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var hme = e as HandledMouseEventArgs;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent. TODO???

            bool update = false;
            switch (e.Button, _ctrlPressed, _shiftPressed) 
            {
                case (MouseButtons.Left, true, false): // Zoom in/out
                    _zoomFactor *= e.Delta > 0 ? 1.1f : 0.9f;
                    update = true;
                    break;

                case (MouseButtons.Left, false, true): // Shift left/right
                    _origin.X += e.Delta;
                    update = true;
                    break;

                case (MouseButtons.Left, false, false): // Shift up/down
                    _origin.Y += e.Delta;
                    update = true;
                    break;

                default:
                    break;
            };

            if(update)
            {
                // Check limits. TODOX
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
                        _startPos = Cursor.Position;
                        _ctrlPressed = true;
                    }
                    break;

                case Keys.ShiftKey:
                    _shiftPressed = true;
                    break;

                case Keys.H: // reset
                    Reset();
                    break;

                case Keys.A: //TODO select all
                    break;

                case Keys.C: //TODO copy/paste/cut
                    break;

                case Keys.Escape: //TODO reset all selections
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

                    _startPos = new Point();
 //                   _endMousePos = new Point();
                    break;

                case Keys.ShiftKey:
                    _shiftPressed = false;
                    break;
            }
        }
        #endregion

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



        /////////////////////// TODO mapping ////////////////////////////////
        ///////////////////////      mapping ////////////////////////////////
        ///////////////////////      mapping ////////////////////////////////


        /// <summary>Obtain the point on the page based on a client/display position.</summary>
        /// <param name="pdisp">The client/display point.</param>
        /// <returns>The page point.</returns>
        PointF DisplayToPage(PointF pdisp)
        {
            var ppageX = (pdisp.X - _origin.X) / _zoomFactor;
            var ppageY = (_origin.Y - pdisp.Y) / _zoomFactor;
            return new PointF(ppageX, ppageY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ppage"></param>
        /// <returns></returns>
        PointF PageToDisplay(PointF ppage)
        {
            var pdispX = ppage.X * _zoomFactor + _origin.X;
            var pdispY = - (ppage.Y * _zoomFactor - _origin.Y);

            return new(pdispX, pdispY);


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



            ///// <summary>Obtain a client point based on a point on the chart.</summary>
            ///// <param name="p">The PointF to correct</param>
            ///// <returns>A PointF corresponding to the proper raw client position of the given PointF.</returns>
            //private PointF GetClientPoint(PointF p)
            //{
            //    bool xPos = (p.X > 0);
            //    bool yPos = (p.Y > 0);
            //    float x = p.X * _zoomFactor;
            //    float y = p.Y * _zoomFactor;
            //    PointF retPoint = new PointF(0F, 0F);

            //    if (xPos && yPos) // Both Positive
            //    {
            //        retPoint = new PointF(x + _origin.X, _origin.Y - y);
            //    }
            //    else if (xPos && !yPos) // Y is negative
            //    {
            //        retPoint = new PointF(x + _origin.X, _origin.Y + Math.Abs(y));
            //    }
            //    else if (!xPos && yPos) // X is negative
            //    {
            //        retPoint = new PointF(_origin.X - Math.Abs(x), _origin.Y - y);
            //    }
            //    else // Both Negative
            //    {
            //        retPoint = new PointF(_origin.X - Math.Abs(x), _origin.Y + Math.Abs(y));
            //    }

            //    return retPoint;
            //}
        }



        /// <summary>Get shape that is within range of point.</summary>
        /// <param name="point">Mouse point</param>
        /// <returns>The closest DataPoint or null if none in range.</returns>
        Shape GetCloseShape(PointF pt)
        {
            Shape close = null;

            foreach(var shape in _shapes)
            {
                if(shape.IsClose(pt, SELECT_RANGE))
                {
                    close = shape;
                    break;
                }
            }

            return close;
        }

        /// <summary>Select multiple shapes.</summary>
        /// <returns>The shapes.</returns>
        List<Shape> GetSelectedShapes()
        {
            List<Shape> shapes = new();

            return shapes;
        }

        /// <summary>Recenter the chart after zooming in or out.</summary>
        /// <param name="xRatio">The change ratio for the x axis</param>
        /// <param name="yRatio">The change ratio for the y axis</param>
        private void Recenter(float xRatio, float yRatio)
        {
            // Get the axes positions relative to the center of the control.
            float xAxisPosFromCenter = _origin.Y - Height / 2;
            float yAxisPosFromCenter = _origin.X - Width / 2;

            // Calculate the change in positions.
            float dY = ((xAxisPosFromCenter * yRatio) - xAxisPosFromCenter);
            float dX = ((yAxisPosFromCenter * xRatio) - yAxisPosFromCenter);

            // Set the new x and y origin positions.
            _origin.Y = xAxisPosFromCenter + dY + Height / 2;
            _origin.X = yAxisPosFromCenter + dX + Width / 2;

            Invalidate();
            Refresh();
        }
    }
}
