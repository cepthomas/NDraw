using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;



namespace NDraw
{
    ////////////////////// replacement class for serialization //////////////////////////////
    //[Serializable]
    //public class PointX
    //{
    //    public float X { get; set; } = 0.0f;

    //    public float Y { get; set; } = 0.0f;

    //    /// <summary>Constructor.</summary>
    //    public PointX(float x, float y)
    //    {
    //        X = x;
    //        Y = y;
    //    }

    //    ///// <summary>Constructor.</summary>
    //    //public PointX(PointX p)
    //    //{
    //    //    X = p.X;
    //    //    Y = p.Y;
    //    //}

    //    /// <summary>For viewing pleasure.</summary>
    //    public override string ToString() => string.Format($"X:{X} Y:{Y}");
    //}

    //[Serializable]
    //public class RectX
    //{
    //    //FromLTRB(float left, float top, float right, float bottom)

    //    public float L { get; set; } = 0.0f;
    //    public float T { get; set; } = 0.0f;
    //    public float R { get; set; } = 0.0f;
    //    public float B { get; set; } = 0.0f;

    //    //public float Left { get; set; }
    //    //public float Top { get; set; }
    //    //public float Right { get; set; }
    //    //public float Bottom { get; set; }
    //    //public PointX TopLeft { get { return new PointX(Left, Top); } }
    //    //public PointX TopRight { get { return new PointX(Right, Top); } }
    //    //public PointX BottomLeft { get { return new PointX(Left, Bottom); } }
    //    //public PointX BottomRight { get { return new PointX(Right, Bottom); } }

    //    [JsonIgnore]
    //    [Browsable(false)]
    //    public float Width { get { return R - L; } }

    //    [JsonIgnore]
    //    [Browsable(false)]
    //    public float Height { get { return B - T; } }


    //    public RectX(float left, float top, float right, float bottom)
    //    {
    //        L = left;
    //        T = top;
    //        R = right;
    //        B = bottom;
    //    }

    //    /// <summary>
    //    /// </summary>
    //    /// <returns></returns>
    //    //public List<LineX> GetEdges()
    //    //{
    //    //    List<LineX> lines = new List<LineX>();
    //    //    lines.Add(new LineX(TopLeft, TopRight));
    //    //    lines.Add(new LineX(TopRight, BottomRight));
    //    //    lines.Add(new LineX(BottomRight, BottomLeft));
    //    //    lines.Add(new LineX(BottomLeft, TopLeft));
    //    //    return lines;
    //    //}

    //    /// <summary>
    //    /// </summary>
    //    /// <returns></returns>
    //    public PointX Center()
    //    {
    //        PointX center = new(L + Width  / 2, T + Height / 2);
    //        return center;
    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool Contains(PointX pf)
    //    {
    //        bool ret = (pf.X <= R) && (pf.X >= L) && (pf.Y <= B) && (pf.Y >= T);
    //        return ret;
    //    }

    //    /// <summary>For viewing pleasure.</summary>
    //    public override string ToString() => string.Format($"L:{L} T:{T} R:{R} B:{B} W:{Width} H:{Height}");
    //}



    public static class GeometryUtils
    {
        /// <summary>
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double DegreesToRadians(double angle)
        {
            return (Math.PI * angle / 180.0);
        }

        /// <summary>
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double RadiansToDegrees(double angle)
        {
            return (angle * 180.0 / Math.PI);
        }

        ///// <summary>
        ///// Helper
        ///// </summary>
        ///// <param name="rect"></param>
        ///// <param name="test"></param>
        ///// <param name="any">True if any part is contained, otherwise all must be.</param>
        ///// <returns></returns>
        //public static bool Contains(this RectangleF rect, RectX test, bool any)
        //{
        //    bool contained = false;

        //    if (any)
        //    {
        //        contained |= rect.Contains(test.L, test.T);
        //        contained |= rect.Contains(test.L, test.B);
        //        contained |= rect.Contains(test.R, test.T);
        //        contained |= rect.Contains(test.R, test.B);
        //    }
        //    else
        //    {
        //        contained = true;
        //        contained &= rect.Contains(test.L, test.T);
        //        contained &= rect.Contains(test.L, test.B);
        //        contained &= rect.Contains(test.R, test.T);
        //        contained &= rect.Contains(test.R, test.B);
        //    }

        //    return contained;
        //}

        ///// <summary>
        ///// Helper
        ///// </summary>
        ///// <param name="rect"></param>
        ///// <param name="test"></param>
        ///// <returns></returns>
        //public static bool Contains(this RectangleF rect, PointX test)
        //{
        //    bool contained = rect.Contains(test.X, test.Y);
        //    return contained;
        //}
    }
}
