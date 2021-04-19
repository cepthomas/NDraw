using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace NDraw
{
    // All dimensions are in the same units as defined in the page.

    // Replacement classes because the .NET serialize ugly.

    public enum ShapeState { Default, Highlighted, Selected };

    /// <summary>
    /// 
    /// </summary>
    //[Serializable]
    //public class PointX
    //{
    //    #region Serialized properties
    //    public float X { get; set; } = 0.0f;

    //    public float Y { get; set; } = 0.0f;
    //    #endregion

    //    /// <summary>Constructor.</summary>
    //    public PointX(float x, float y)
    //    {
    //        X = x;
    //        Y = y;
    //    }

    //    ///// <summary>Copy onstructor.</summary>
    //    //public PointX(PointX p)
    //    //{
    //    //    X = p.X;
    //    //    Y = p.Y;
    //    //}

    //    /// <summary>For viewing pleasure.</summary>
    //    public override string ToString() => string.Format($"X:{X} Y:{Y}");
    //}


    //[Serializable]
    //public class LineX
    //{
    //    public float X1 { get; set; } = 0.0f;
    //    public float Y1 { get; set; } = 0.0f;
    //    public float X2 { get; set; } = 0.0f;
    //    public float Y2 { get; set; } = 0.0f;

    //    public LineX()
    //    {
    //    }

    //    public LineX(float x1, float y1, float x2, float y2)
    //    {
    //        X1 = x1;
    //        Y1 = y1;
    //        X2 = x2;
    //        Y2 = y2;
    //    }

    //    public RectangleF Expand(int range)
    //    {
    //        RectangleF r = new RectangleF(X1 - range, Y1 - range, Math.Abs(X2 - X1) + range * 2, Math.Abs(Y2 - Y1) + range * 2);
    //        return r;
    //    }
    //}


    [Serializable]
    public abstract class Shape
    {
        #region Properties
        /// <summary>xxxx</summary>
        public string Id { get; set; } = "";

        /// <summary>xxxx</summary>
        public string StyleId { get; set; } = "";

        /// <summary>xxxx</summary>
        public int Layer { get; set; } = 0;

        /// <summary>xxxx</summary>
        public string Text { get; set; } = ""; // also Text position TODO

        /// <summary>xxxx</summary>
        public ShapeState State { get; set; } = ShapeState.Default;
        #endregion

        #region Abstract functions
        /// <summary>
        /// Determine if pt is within range of me.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public abstract bool IsClose(PointF pt, int range);

        /// <summary>
        /// Determine if this is within rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="any">True if any part is contained otherwise must be the entire.</param>
        /// <returns></returns>
        public abstract bool ContainedIn(RectangleF rect, bool any);
        #endregion
    }

    [Serializable]
    public class RectShape : Shape
    {
        #region Serialized properties
        public float L { get; set; } = 0.0f;

        public float T { get; set; } = 0.0f;

        public float R { get; set; } = 0.0f;

        public float B { get; set; } = 0.0f;
        #endregion

        [JsonIgnore]
        [Browsable(false)]
        public float Width { get { return R - L; } }

        [JsonIgnore]
        [Browsable(false)]
        public float Height { get { return B - T; } }

        //public RectShape()
        //{
        //}

        //public RectShape(float left, float top, float right, float bottom)
        //{
        //    L = left;
        //    T = top;
        //    R = right;
        //    B = bottom;
        //}

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public List<(PointF start, PointF end)> GetEdges()
        {
            List<(PointF start, PointF end)> lines = new();
            lines.Add((new PointF(L, T), new PointF(R, T)));
            lines.Add((new PointF(R, T), new PointF(R, B)));
            lines.Add((new PointF(R, B), new PointF(L, B)));
            lines.Add((new PointF(L, B), new PointF(L, T)));
            return lines;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public PointF Center()
        {
            PointF center = new(L + Width / 2, T + Height / 2);
            return center;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Contains(PointF pf)
        {
            bool ret = (pf.X <= R) && (pf.X >= L) && (pf.Y <= B) && (pf.Y >= T);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="any">True if any part is contained, otherwise all must be.</param>
        /// <returns></returns>
        public override bool ContainedIn(RectangleF rect, bool any)
        {
            bool contained = false;

            if (any)
            {
                contained |= rect.Contains(L, T);
                contained |= rect.Contains(L, B);
                contained |= rect.Contains(R, T);
                contained |= rect.Contains(R, B);
            }
            else
            {
                contained = true;
                contained &= rect.Contains(L, T);
                contained &= rect.Contains(L, B);
                contained &= rect.Contains(R, T);
                contained &= rect.Contains(R, B);
            }

            return contained;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt">Display/mouse coordinates.</param>
        /// <param name="range"></param>
        /// <returns></returns>
        public override bool IsClose(PointF pt, int range)
        {
            bool close = false;

            var edges = GetEdges();

            foreach (var edge in edges)
            {
                // Make rectangles out of each side and test for point contained.
                if (ShapeUtils.Expand(edge.start, edge.end, range).Contains(pt))
                {
                    close = true;
                    break;
                }
            }

            return close;
        }

        //public RectShape Translate(PointF pt, float z)
        //{
        //    return new();
        //}

        /// <summary>For viewing pleasure.</summary>
        public override string ToString() => string.Format($"L:{L} T:{T} R:{R} B:{B} W:{Width} H:{Height}");
    }

    [Serializable]
    public class LineShape : Shape
    {
        #region Serialized properties
        public float X1 { get; set; } = 0.0f;

        public float Y1 { get; set; } = 0.0f;

        public float X2 { get; set; } = 0.0f;
        
        public float Y2 { get; set; } = 0.0f;
        #endregion

        //public PointF Start { get; set; } = new PointF(0, 0);
        //public PointF End { get; set; } = new PointF(0, 0);



        // TODO end arrows etc, multi-segment lines

        //public LineShape Translate(PointF pt, float z) //or float?
        //{
        //    return new();
        //}

        public override bool IsClose(PointF pt, int range)
        {
            var close = ShapeUtils.Expand(new PointF(X1, Y1), new PointF(X2, Y2), range).Contains(pt);
            return close;
        }

        public override bool ContainedIn(RectangleF rect, bool any)
        {
            return rect.Contains(X1, Y1) || rect.Contains(X2, Y2);
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString() => string.Format($"X1:{X1} Y1:{Y1} X2:{X2} Y2:{Y2}");
    }

    public class ShapeUtils //TODO new home
    {
        /// <summary>
        /// Make a rectangle from the line start/end.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static RectangleF Expand(PointF start, PointF end, int range)
        {
            RectangleF r = new RectangleF(start.X - range, start.Y - range, Math.Abs(end.X - start.X) + range * 2, Math.Abs(end.Y - start.Y) + range * 2);
            return r;
        }
    }
}
