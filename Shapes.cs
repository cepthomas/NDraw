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
    public enum ShapeState { Default, Highlighted, Selected };

    /// <summary>Base/abstract class for all shape types.</summary>
    [Serializable]
    public abstract class Shape
    {
        #region Properties
        ///// <summary>TODO2</summary>
        //public string Id { get; set; } = "";

        /// <summary>TODO2</summary>
        public string StyleId { get; set; } = "";

        /// <summary>TODO2</summary>
        public int Layer { get; set; } = 0;

        /// <summary>TODO1</summary>
        public string Text { get; set; } = ""; // also Text position TODO1

        /// <summary>DOC</summary>
        public ShapeState State { get; set; } = ShapeState.Default;
        #endregion

        /// <summary>
        /// Make a rectangle from the line start/end.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        protected RectangleF Expand(PointF start, PointF end, int range)
        {
            RectangleF r = new(start.X - range, start.Y - range, Math.Abs(end.X - start.X) + range * 2, Math.Abs(end.Y - start.Y) + range * 2);
            return r;
        }

        #region Abstract functions
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
                if (Expand(start, end, range).Contains(pt))
                {
                    close = true;
                    break;
                }
            }

            return close;
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString() => string.Format($"TL:{TL} BR:{BR} W:{Width} H:{Height}");
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
        #endregion

        // TODO1 end arrows etc, multi-segment lines

        /// <inheritdoc />
        public override bool IsClose(PointF pt, int range)
        {
            var close = Expand(Start, End, range).Contains(pt);
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
}
