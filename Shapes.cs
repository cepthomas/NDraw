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

// Later ellipse, poly, ...


namespace NDraw
{
    /// <summary>DOC</summary>
    public enum ShapeState { Default, Highlighted };

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

        /// <summary>Dynamic state.</summary>
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

        #region Fields
        protected int _lastFeature = 0;
        #endregion

        #region Common functions
        /// <summary>
        /// Make a square with the point at the center.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public RectangleF Expand(PointF pt, float range)
        {
            RectangleF r = new(pt.X - range, pt.Y - range, range * 2, range * 2);
            return r;
        }
        #endregion

        #region Abstract functions
        /// <summary>
        /// Determine if pt is within range of this shape's features.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="range"></param>
        /// <returns>Arbitrary 1->N or 0 if none.</returns>
        public abstract int FeaturePoint(PointF pt, float range);

        /// <summary>
        /// Determine if this is within rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="any">True if any part is contained otherwise must be the entire.</param>
        /// <returns></returns>
        public abstract bool ContainedIn(RectangleF rect, bool any);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract string GetTooltipText();
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

        /// <inheritdoc />
        public override int FeaturePoint(PointF pt, float range)
        {
            _lastFeature = 0;

            if (Expand(Start, range).Contains(pt))
            {
                _lastFeature = 1;
            }
            else if (Expand(End, range).Contains(pt))
            {
                _lastFeature = 2;
            }

            return _lastFeature;
        }

        /// <inheritdoc />
        public override bool ContainedIn(RectangleF rect, bool any)
        {
            return rect.Contains(Start) || rect.Contains(End);
        }

        /// <inheritdoc />
        public override string GetTooltipText()
        {
            return "";
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString()
        {
            return $"{Id} S:{Start} E:{End}";
        }
    }

    /// <summary>Drawing rectangle.</summary>
    [Serializable]
    public class RectShape : Shape
    {
        #region Properties
        /// <summary>DOC</summary>
        [JsonConverter(typeof(PointFConverter))]
        public PointF Location { get; set; } = new(0, 0);

        /// <summary>DOC</summary>
        public float Width { get; set; }

        /// <summary>DOC</summary>
        public float Height { get; set; }

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public PointF TL => new(Location.X, Location.Y);

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public PointF TR => new(Location.X + Width, Location.Y);

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public PointF BR => new(Location.X + Width, Location.Y + Height);

        /// <summary>DOC</summary>
        [JsonIgnore, Browsable(false)]
        public PointF BL => new(Location.X, Location.Y + Height);
        #endregion

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public PointF Center()
        {
            PointF center = new(Location.X + Width / 2, Location.Y + Height / 2);
            return center;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Contains(PointF pf)
        {
            bool ret = (pf.X <= BR.X) && (pf.X >= Location.X) && (pf.Y <= BR.Y) && (pf.Y >= Location.Y);
            return ret;
        }

        /// <inheritdoc />
        public override bool ContainedIn(RectangleF rect, bool any)
        {
            bool contained = false;

            if (any)
            {
                contained |= rect.Contains(Location);
                contained |= rect.Contains(BL);
                contained |= rect.Contains(TR);
                contained |= rect.Contains(BR);
            }
            else
            {
                contained = true;
                contained &= rect.Contains(Location);
                contained &= rect.Contains(BL);
                contained &= rect.Contains(TR);
                contained &= rect.Contains(BR);
            }

            return contained;
        }

        /// <inheritdoc />
        public override string GetTooltipText()
        {
            return "";
        }

        /// <summary>
        /// Gets list of lines defining rect edges. Clockwise from top left.
        /// </summary>
        /// <returns></returns>
        public List<(PointF start, PointF end)> GetEdges()
        {
            List<(PointF start, PointF end)> lines = new() { (Location, TR), (TR, BR), (BR, BL), (BL, Location) };
            return lines;
        }

        /// <inheritdoc />
        public override int FeaturePoint(PointF pt, float range)
        {
            _lastFeature = 0;

            if (Expand(Location, range).Contains(pt))
            {
                _lastFeature = 1;
            }
            else if (Expand(TR, range).Contains(pt))
            {
                _lastFeature = 2;
            }
            else if (Expand(BL, range).Contains(pt))
            {
                _lastFeature = 3;
            }
            else if (Expand(BR, range).Contains(pt))
            {
                _lastFeature = 4;
            }

            return _lastFeature;
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString()
        {
            return $"{Id} L:{Location} W:{Width} H:{Height}";
        }
    }
}