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


// Deals exclusively in virtual (page) units. Translation between display and virtual is done in GeometryMap.


namespace NDraw
{
    /// <summary>DOC</summary>
    public enum ShapeState { Default, Highlighted };//, Selected };

    /// <summary>DOC</summary>
    public enum PointStyle { None, CircleHollow, CircleFilled, SquareHollow, SquareFilled, ArrowHollow, ArrowFilled };


    /// <summary>Base/abstract class for all shape types.</summary>
    [Serializable]
    public abstract class Shape
    {
        #region Properties
        /// <summary></summary>
        public string Id { get; set; } = "???";

        /// <summary>Layer - 0 means all.</summary>
        public int Layer { get; set; } = 0;

        /// <summary>DOC</summary>
        [JsonIgnore]
        public ShapeState State { get; set; } = ShapeState.Default;

        /// <summary>Text to display.</summary>
        public string Text { get; set; } = "";

        /// <summary>Text to display.</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContentAlignment TextAlignment { get; set; } = ContentAlignment.TopLeft;

        /// <summary>DOC</summary>
        public float LineThickness { get; set; } = 1.0f;

        /// <summary>DOC</summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color LineColor { get; set; } = Color.Green;

        /// <summary>DOC</summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color FillColor { get; set; } = Color.Black;
        #endregion

        #region Common functions
        #endregion

        #region Abstract functions
        /// <summary>
        /// Determine if pt is within range of this.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="range"></param>
        /// <returns></returns>
       // public abstract bool IsClose(PointF pt, float range);

        /// <summary>
        /// Determine if pt is within range of this.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public abstract int KeyPoint(PointF pt, float range);

        /// <summary>
        /// Determine if this is within rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="any">True if any part is contained otherwise must be the entire.</param>
        /// <returns></returns>
        public abstract bool ContainedIn(RectangleF rect, bool any);
        #endregion
    }

    /// <summary>Drawing line.</summary>
    [Serializable]
    public class LineShape : Shape
    {
        #region Properties
        /// <summary>DOC</summary>
        [JsonConverter(typeof(PointFConverter))]
        public PointF Start { get; set; } = new(0, 0);

        /// <summary>DOC</summary>
        [JsonConverter(typeof(PointFConverter))]
        public PointF End { get; set; } = new(0, 0);

        /// <summary>DOC</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PointStyle StartStyle { get; set; } = PointStyle.None;

        /// <summary>DOC</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PointStyle EndStyle { get; set; } = PointStyle.None;
        #endregion

        ///// <inheritdoc />
        //public override bool IsClose(PointF pt, float range)
        //{
        //    var close = Geometry.Expand(Start, End, range).Contains(pt);
        //    return close;
        //}

        /// <inheritdoc />
        public override int KeyPoint(PointF pt, float range)
        {
            int which = 0;

            if (Geometry.Expand(Start, range).Contains(pt))
            {
                which = 1;
            }
            else if (Geometry.Expand(End, range).Contains(pt))
            {
                which = 2;
            }

            return which;
        }

        /// <inheritdoc />
        public override bool ContainedIn(RectangleF rect, bool any)
        {
            return rect.Contains(Start) || rect.Contains(End);
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString() => string.Format($"Start:{Start} End:{End}");
    }

    /// <summary>Drawing rectangle.</summary>
    [Serializable]
    public class RectShape : Shape
    {
        #region Properties
        /// <summary>DOC</summary>
        [JsonConverter(typeof(PointFConverter))]
        public PointF TL { get; set; } = new(0, 0);

        /// <summary>DOC</summary>
        [JsonConverter(typeof(PointFConverter))]
        public PointF BR { get; set; } = new(0, 0);

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public PointF TR => new(BR.X, TL.Y);

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public PointF BL => new(TL.X, BR.Y);

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public float Width => BR.X - TL.X;

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public float Height => BR.Y - TL.Y;
        #endregion

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

        /// <summary>
        /// Gets list of lines defining rect edges. Clockwise from top left.
        /// </summary>
        /// <returns></returns>
        public List<(PointF start, PointF end)> GetEdges()
        {
            List<(PointF start, PointF end)> lines = new() { (TL, TR), (TR, BR), (BR, BL), (BL, TL) };
            return lines;
        }

        ///// <inheritdoc />
        //public override bool IsClose(PointF pt, float range)
        //{
        //    bool close = false;

        //    foreach(var (start, end) in GetEdges())
        //    {
        //        // Make rectangles out of each side and test for point contained.
        //        if (Geometry.Expand(start, end, range).Contains(pt))
        //        {
        //            close = true;
        //            break;
        //        }
        //    }

        //    return close;
        //}

        /// <inheritdoc />
        public override int KeyPoint(PointF pt, float range)
        {
            int which = 0;

            if (Geometry.Expand(TL, range).Contains(pt))
            {
                which = 1;
            }
            else if (Geometry.Expand(TR, range).Contains(pt))
            {
                which = 2;
            }
            else if (Geometry.Expand(BL, range).Contains(pt))
            {
                which = 3;
            }
            else if (Geometry.Expand(BR, range).Contains(pt))
            {
                which = 4;
            }

            return which;
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString() => string.Format($"TL:{TL} BR:{BR} W:{Width} H:{Height}");
    }
}