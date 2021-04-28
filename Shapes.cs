﻿using System;
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


// Deals exclusively in virtual (page) units. Translation between display and virtual is done in GeometryMap.

//TODO don't serialize extra stuff in PointF. "TL": { "IsEmpty": false, "X": 50, "Y": 50 },  https://stackoverflow.com/q/62775694



namespace NDraw
{

    public enum ShapeState { Default, Highlighted, Selected };

    /// <summary>Base/abstract class for all shape types.</summary>
    [Serializable]
    public abstract class Shape
    {
        #region Properties
        /// <summary>TODO</summary>
        public string Id { get; set; } = "";

        /// <summary>TODO</summary>
        public string StyleId { get; set; } = "";

        /// <summary>TODO</summary>
        public int Layer { get; set; } = 0;

        /// <summary>TODO</summary>
        public string Text { get; set; } = ""; // also Text position TODO

        /// <summary>DOC</summary>
        public ShapeState State { get; set; } = ShapeState.Default;
        #endregion

        #region Abstract functions
        //        public abstract void RecalcDisplay(int shiftX, int shiftY, float zoom);

        /// <summary>
        /// Determine if pt is within range of this.
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

    /// <summary>Drawing rectangle.</summary>
    [Serializable]
    public class RectShape : Shape
    {
        #region Properties
        /// <summary>DOC</summary>
        public PointF TL { get; set; } = new(0, 0);

        /// <summary>DOC</summary>
        public PointF BR { get; set; } = new(0, 0);

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public PointF TR { get { return new(BR.X, TL.Y); } }

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public PointF BL { get { return new(TL.X, BR.Y); } }

        //public float L { get { return TL.X; } }
        //public float T { get { return TL.Y; } }
        //public float R { get { return BR.X; } }
        //public float B { get { return BR.Y; } }
        //public float L { get; set; } = 0.0f;
        //public float T { get; set; } = 0.0f;
        //public float R { get; set; } = 0.0f;
        //public float B { get; set; } = 0.0f;

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public float Width { get { return BR.X - TL.X; } }

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public float Height { get { return BR.Y - TL.Y; } }
        #endregion

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
        /// Gets list of lines defining rect edges. Clockwise from top left.
        /// </summary>
        /// <returns></returns>
        public List<(PointF start, PointF end)> GetEdges()
        {
            List<(PointF start, PointF end)> lines = new();
            lines.Add((TL, TR));
            lines.Add((TR, BR));
            lines.Add((BR, BL));
            lines.Add((BL, TL));
            //lines.Add((new PointF(L, T), new PointF(R, T)));
            //lines.Add((new PointF(R, T), new PointF(R, B)));
            //lines.Add((new PointF(R, B), new PointF(L, B)));
            //lines.Add((new PointF(L, B), new PointF(L, T)));
            return lines;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public PointF Center()
        {
           PointF center = new((BR.X - TL.X) / 2, (BR.Y - TL.Y) / 2);
           return center;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Contains(PointF pf)
        {
           bool ret = (pf.X <= BR.X) && (pf.X >= TL.X) && (pf.Y <= BR.Y) && (pf.Y >= TL.Y);
           return ret;
        }

        /// <inheritdoc />
        public override bool ContainedIn(RectangleF rect, bool any)
        {
            bool contained = false;

            if (any)
            {
                contained |= rect.Contains(TL);
                contained |= rect.Contains(BL);
                contained |= rect.Contains(TR);
                contained |= rect.Contains(BR);
            }
            else
            {
                contained = true;
                contained &= rect.Contains(TL);
                contained &= rect.Contains(BL);
                contained &= rect.Contains(TR);
                contained &= rect.Contains(BR);
            }

            return contained;
        }

        /// <inheritdoc />
        public override bool IsClose(PointF pt, int range)
        {
            bool close = false;

            var edges = GetEdges();

            foreach (var (start, end) in edges)
            {
                // Make rectangles out of each side and test for point contained.
                if (ShapeUtils.Expand(start, end, range).Contains(pt))
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
        public override string ToString() => string.Format($"TL:{TL} BR:{BR} W:{Width} H:{Height}");
        //public override string ToString() => string.Format($"L:{L} T:{T} R:{R} B:{B} W:{Width} H:{Height}");
    }

    /// <summary>Drawing line.</summary>
    [Serializable]
    public class LineShape : Shape
    {
        #region Properties
        //public float X1 { get; set; } = 0.0f;

        //public float Y1 { get; set; } = 0.0f;

        //public float X2 { get; set; } = 0.0f;

        //public float Y2 { get; set; } = 0.0f;

        /// <summary>DOC</summary>
        public PointF Start { get; set; } = new PointF(0, 0);

        /// <summary>DOC</summary>
        public PointF End { get; set; } = new PointF(0, 0);
        #endregion

        // TODO end arrows etc, multi-segment lines

        //public LineShape Translate(PointF pt, float z) //or float?
        //{
        //    return new();
        //}

        /// <inheritdoc />
        public override bool IsClose(PointF pt, int range)
        {
            var close = ShapeUtils.Expand(Start, End, range).Contains(pt);
            return close;
        }

        /// <inheritdoc />
        public override bool ContainedIn(RectangleF rect, bool any)
        {
            return rect.Contains(Start) || rect.Contains(End);
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString() => string.Format($"Start:{Start} End:{End}");
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
