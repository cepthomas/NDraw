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


// Deals exclusively in display units. Translation between display and virtual is done in XXXShape.


namespace NDraw
{
    public partial class Canvas : UserControl
    {
        /// <summary>DOC</summary>

        /// <summary>Current drawing.</summary>
        Page _page = new();

        /// <summary>The settings.</summary>
        UserSettings _settings = new();

        /// <summary>The various shapes in _page converted to internal format.</summary>
        readonly List<Shape> _shapes = new();

        /// <summary>Cosmetics.</summary>
        const float GRID_LINE_WIDTH = 1.0f;

        /// <summary>How close do you have to be to select a shape in pixels.</summary>
        const int SELECT_RANGE = 5;

        // Around drawing area.
        //const int MARGIN = 50;

        /// <summary>Maximum zoom in limit.</summary>
        const float MAX_ZOOM = 10.0f;

        /// <summary>Minimum zoom out limit.</summary>
        const float MIN_ZOOM = 0.1f;

        /// <summary>Speed at which to zoom in/out.</summary>
        const float ZOOM_SPEED = 1.25F;

        #region Control states
        /// <summary>If control is pressed.</summary>
        bool _ctrlPressed = false;

        /// <summary>If shift is pressed.</summary>
        bool _shiftPressed = false;

        /// <summary>Saves state as to whether left button is down.</summary>
        //bool _mouseDown = false;

        /// <summary>The user is dragging the mouse.</summary>
        bool _dragging = false;
        #endregion

        #region Memories
        /// <summary>Mouse position when button pressed.</summary>
        Point _startPos = new();

        /// <summary>Mouse position when button unpressed.</summary>
        //Point _endMousePos = new();

        /// <summary>Current mouse position.</summary>
        //Point _currentMousePos = new();
        #endregion

        #region Drawing resources
        Pen _penGrid = new(Color.Gray);

        // Temp pen. Will be from style.
        Pen _penShapeTemp = new(Color.Green, 2);
        Pen _penHighlightTemp = new(Color.Green, 4);

        Pen _penSelect = new(Color.Gray);
        #endregion






        void CalcGeometry() // stuff on resize
        {
            //_drawArea = new(ClientRectangle.X + MARGIN, ClientRectangle.Y + MARGIN, Width - MARGIN, Height - MARGIN);
            //_drawArea = ClientRectangle;

            //public float Grid { get; set; } = 2;
            //public float Snap { get; set; } = 2;
        }

        void Reset() // initial state
        {
            GeometryMap.Reset();
            //Invalidate();
        }

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

            GeometryMap.Zoom = 1.0f;

            _shapes.Clear();
            _shapes.AddRange(_page.Rects);
            _shapes.AddRange(_page.Lines);

            _penGrid.Color = _settings.GridColor;
            _penGrid.Width = GRID_LINE_WIDTH;

            // Init geometry.
            GeometryMap.Reset();
            GeometryMap.DrawArea = ClientRectangle;

            //CalcGeometry();

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
            e.Graphics.Clear(_settings.BackColor);

            ///// Draw the grid.
            float spacing = 42.0f; //TODO calculate along with others

            // Draw X-Axis
            for (float tickPos = spacing; tickPos < Width; tickPos += spacing) //XXX spacing
            {
                e.Graphics.DrawLine(_penGrid, tickPos, 0, tickPos, Width);
            }

            // Draw Y-Axis
            for (float tickPos = spacing; tickPos < Height; tickPos += spacing) //XXX spacing
            {
                e.Graphics.DrawLine(_penGrid, 0, tickPos, Right, tickPos);
            }




            ///// Draw the shapes.
            RectangleF cr = ClientRectangle;
            int marker = 3;

            foreach (var s in _shapes)
            {
                if (s.ContainedIn(cr, true))
                {
                    switch (s)
                    {
                        case RectShape r: //XXX adjust
                            e.Graphics.DrawRectangle(r.State == ShapeState.Highlighted ? _penHighlightTemp : _penShapeTemp, r.TL.X, r.TL.Y, r.Width, r.Height);

                            if (r.State == ShapeState.Selected)
                            {
                                e.Graphics.FillEllipse(_penShapeTemp.Brush, r.TL.X, r.TL.Y, marker, marker);
                            }
                            break;

                        case LineShape l: //XXX adjust
                            e.Graphics.DrawLine(l.State == ShapeState.Highlighted ? _penHighlightTemp : _penShapeTemp, l.Start, l.End);

                            if (l.State == ShapeState.Selected)
                            {
                                e.Graphics.FillEllipse(_penShapeTemp.Brush, l.Start.X, l.Start.Y, marker, marker);
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

            DrawText(e.Graphics, 450, 150, "Hello!", _settings.AllStyles[0].Font, 45);
        }

        ///// <summary>
        ///// Draw the grid.
        ///// </summary>
        ///// <param name="g">The Graphics object to use.</param>
        //void DrawGrid(Graphics g)
        //{
        //    float spacing = 42.0f; //TODO calculate along with others

        //    // Draw X-Axis
        //    for (float tickPos = spacing; tickPos < Width; tickPos += spacing)
        //    {
        //        g.DrawLine(_penGrid, tickPos, 0, tickPos, Width);
        //    }

        //    // Draw Y-Axis
        //    for (float tickPos = spacing; tickPos < Height; tickPos += spacing)
        //    {
        //        g.DrawLine(_penGrid, 0, tickPos, Right, tickPos);
        //    }
        //}

        /// <summary>
        /// A label should be drawn using the specified transformations.
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

        #region Misc window events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            GeometryMap.DrawArea = ClientRectangle;
            //CalcGeometry();
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
            bool redraw = false;
            switch (e.Button, _ctrlPressed, _shiftPressed) 
            {
                case (MouseButtons.Left, true, false): // if near a shape toggle selected
                    Shape pt = GetCloseShape(new PointF(e.X, e.Y));

                    if (pt != null)
                    {
                        pt.State = pt.State == ShapeState.Selected ? ShapeState.Default : ShapeState.Selected;
                        redraw = true;
                    }
                    break;

                default:
                    break;
            };

            _startPos = new Point(e.X, e.Y);
            //_mouseDown = true;

            if (redraw)
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
            bool redraw = false;
            switch (e.Button, _ctrlPressed, _shiftPressed) 
            {
                case (MouseButtons.Left, false, false): // Select shapes within drag rectangle TODOX  //XXX adjust

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

                    redraw = true;
                    break;

                default:
                    break;
            };

            //_mouseDown = false;

            if (redraw)
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


            //// If there is a change in x or y...
            //if ((xChange + yChange) != 0)
            //{
            //    if (_dragCursor != null)
            //    {
            //        // Update the cursor position. Get mouse x and convert to x axis scaled value.
            //        PointF pt = GetChartPoint(new PointF(newPos.X, newPos.Y));
            //        _dragCursor.Position = pt.X;
            //    }
            //    else
            //    {
            //        // Adjust the axes
            //        _origin.Y += yChange;
            //        _origin.X += xChange;

            //        FireScaleChange();
            //    }

            //    // Repaint
            //    Invalidate();
            //    Refresh();
            //}

            //bool recalc = false;
            bool redraw = false;

            switch (e.Button, _ctrlPressed, _shiftPressed) 
            {
                //case (MouseButtons.Left, true, false): // toggle selecting shapes
                //    redraw = true;
                //    break;

                case (MouseButtons.Left, false, false): // drawing selection rect
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

                case (MouseButtons.None, false, false): // highlight any close shapes
                    foreach(Shape shape in _shapes)
                    {
                        if(shape.IsClose(e.Location, SELECT_RANGE))
                        {
                            if (shape.State == ShapeState.Default)
                            {
                                shape.State = ShapeState.Highlighted;
                                redraw = true;
                            }
                        }
                        else
                        {
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var hme = e as HandledMouseEventArgs;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent. TODO???

            bool recalc = false;
            bool redraw = false;

            switch (e.Button, _ctrlPressed, _shiftPressed) 
            {
                case (MouseButtons.Left, true, false): // Zoom in/out
                    GeometryMap.Zoom *= e.Delta > 0 ? 1.1f : 0.9f;
                    //>>>> check limits TODOX
                    //Gets a signed count of the number of detents the mouse wheel has rotated, multiplied
                    //     by the WHEEL_DELTA constant. A detent is one notch of the mouse wheel.
                    recalc = true;
                    redraw = true;
                    break;

                case (MouseButtons.Left, false, true): // Shift left/right
                    GeometryMap.ShiftX += e.Delta;
                    recalc = true;
                    redraw = true;
                    break;

                case (MouseButtons.Left, false, false): // Shift up/down
                    GeometryMap.ShiftY += e.Delta;
                    recalc = true;
                    redraw = true;
                    break;

                default:
                    break;
            };

            if(recalc)
            {
//                RecalcDisplayShapes();
            }

            if(redraw)
            {
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
            //bool recalc = false;
            bool redraw = false;

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
                    redraw = true;
                    break;

                case Keys.A: //TODO select all
                    redraw = true;
                    break;

                case Keys.C: //TODO copy/paste/cut
                    redraw = true;
                    break;

                case Keys.Escape: //TODO reset all selections
                    redraw = true;
                    break;
            }

            if(redraw)
            {
                Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            bool redraw = false;

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
                        redraw = true;
                        //Refresh();
                    }

                    _startPos = new Point();
 //                   _endMousePos = new Point();
                    break;

                case Keys.ShiftKey:
                    _shiftPressed = false;
                    break;
            }

            if(redraw)
            {
                Invalidate();
            }
        }
        #endregion


        /// <summary>Get shape that is within range of point.</summary>
        /// <param name="point">Mouse point</param>
        /// <returns>The closest DataPoint or null if none in range.</returns>
        Shape GetCloseShape(PointF pt) //XXX adjust
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
        List<Shape> GetSelectedShapes() //TODO
        {
            List<Shape> shapes = new();

            return shapes;
        }

        ///// <summary>Recenter the chart after zooming in or out.</summary>
        ///// <param name="xRatio">The change ratio for the x axis</param>
        ///// <param name="yRatio">The change ratio for the y axis</param>
        //private void Recenter(float xRatio, float yRatio)
        //{
        //    // Get the axes positions relative to the center of the control.
        //    float xAxisPosFromCenter = _shiftY - Height / 2;
        //    float yAxisPosFromCenter = _shiftX - Width / 2;

        //    // Calculate the change in positions.
        //    float dY = ((xAxisPosFromCenter * yRatio) - xAxisPosFromCenter);
        //    float dX = ((yAxisPosFromCenter * xRatio) - yAxisPosFromCenter);

        //    // Set the new x and y origin positions.
        //    _shiftY = xAxisPosFromCenter + dY + Height / 2;
        //    _shiftX = yAxisPosFromCenter + dX + Width / 2;

        //    Invalidate();
        //    Refresh();
        //}
    }
}
