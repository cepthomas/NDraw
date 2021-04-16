using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace NDraw
{
    // All dimensions are in the same units as defined in the page.

    // Replacement classes because the .NET serialize ugly.

    public enum ShapeState { Default, Highlighted, Selected };

    [Serializable]
    public class PointX
    {
        public float X { get; set; } = 0.0f;

        public float Y { get; set; } = 0.0f;

        /// <summary>Constructor.</summary>
        public PointX(float x, float y)
        {
            X = x;
            Y = y;
        }

        ///// <summary>Constructor.</summary>
        //public PointX(PointX p)
        //{
        //    X = p.X;
        //    Y = p.Y;
        //}

        /// <summary>For viewing pleasure.</summary>
        public override string ToString() => string.Format($"X:{X} Y:{Y}");
    }

    [Serializable]
    public class Shape
    {
        /// <summary>xxxx</summary>
        public string Id { get; set; } = "";

        /// <summary>xxxx</summary>
        public string StyleId { get; set; } = "";

        /// <summary>xxxx</summary>
        public int Layer { get; set; } = 0;

        /// <summary>xxxx</summary>
        public string Text { get; set; } = ""; // also Text position TODO

        public ShapeState State { get; set; } = ShapeState.Default;
    }

    [Serializable]
    public class RectShape : Shape
    {
        public float L { get; set; } = 0.0f;

        public float T { get; set; } = 0.0f;

        public float R { get; set; } = 0.0f;

        public float B { get; set; } = 0.0f;

        [JsonIgnore]
        [Browsable(false)]
        public float Width { get { return R - L; } }

        [JsonIgnore]
        [Browsable(false)]
        public float Height { get { return B - T; } }

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
        //public List<LineX> GetEdges()
        //{
        //    List<LineX> lines = new List<LineX>();
        //    lines.Add(new LineX(TopLeft, TopRight));
        //    lines.Add(new LineX(TopRight, BottomRight));
        //    lines.Add(new LineX(BottomRight, BottomLeft));
        //    lines.Add(new LineX(BottomLeft, TopLeft));
        //    return lines;
        //}

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public PointX Center()
        {
            PointX center = new(L + Width / 2, T + Height / 2);
            return center;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Contains(PointX pf)
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
        public bool ContainedIn(RectangleF rect, bool any)
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
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsClose(Point pt)
        {
            bool close = false;

            //Rectangle r = new();
            //r.Inflate(5, 5);

            return close;
        }

        public RectShape Translate(PointF pt, float z)
        {
            return new();
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString() => string.Format($"L:{L} T:{T} R:{R} B:{B} W:{Width} H:{Height}");
    }

    [Serializable]
    public class LineShape : Shape
    {
        public float X1 { get; set; } = 0.0f;
        public float Y1 { get; set; } = 0.0f;
        public float X2 { get; set; } = 0.0f;
        public float Y2 { get; set; } = 0.0f;

        //public PointX Start { get; set; }

        //public PointX End { get; set; }

        // TODO end arrows etc, multi-segment lines

        public LineShape Translate(PointX pt, float z) //or float?
        {
            return new();
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString() => string.Format($"X1:{X1} Y1:{Y1} X2:{X2} Y2:{Y2}");
    }
}
