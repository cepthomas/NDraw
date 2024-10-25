using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing.Drawing2D;
using Ephemera.NBagOfTricks;


namespace NDraw
{
    #region Enums
    /// <summary>Presentation state.</summary>
    public enum ShapeState { Default, Highlighted };

    /// <summary>Cosmetics.</summary>
    public enum PointStyle { None, Arrow, Tee };
    #endregion

    /// <summary>Base/abstract class for all shape types. Deals exclusively in virtual (page) units.</summary>
    [Serializable]
    public abstract class Shape
    {
        #region Constants
        /// <summary>Since there is no None value, use this one.</summary>
        public const HatchStyle NO_HATCH = HatchStyle.Percent90; // (HatchStyle)255; TODO get rid of this
        #endregion

        #region Properties
        /// <summary>Arbitrary id.</summary>
        public int Id { get { return _id; } }

        /// <summary>Layer 1-4.</summary>
        public int Layer { get; set; } = 1;

        /// <summary>Text to display.</summary>
        public string Text { get; set; } = "";

        /// <summary>Text alignment. Technically ContentAlignment is misapplied here but it makes for easier handling.</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContentAlignment TextAlignment { get; set; } = ContentAlignment.TopLeft;

        /// <summary>Line Thickness</summary>
        public float LineThickness { get; set; } = 1.0f;

        /// <summary>Line Color</summary>
        [JsonConverter(typeof(JsonColorConverter))]
        public Color LineColor { get; set; } = Color.Green;

        /// <summary>Fill Color</summary>
        [JsonConverter(typeof(JsonColorConverter))]
        public Color FillColor { get; set; } = Color.Black;

        /// <summary>Shape fill hatch.</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HatchStyle Hatch { get; set; } = NO_HATCH;

        /// <summary>Dynamic state.</summary>
        [JsonIgnore]
        public ShapeState State { get; set; } = ShapeState.Default;
        #endregion

        #region Fields
        protected int _lastFeature = 0;
        // Hand out unique id.
        protected int _id;
        protected static int _nextId = 1;
        #endregion

        protected Shape()
        {
            _id = _nextId++;
        }

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

        /// <summary>
        /// Is the pt near a feature point.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="range"></param>
        /// <returns>The feature point or 0 if none.</returns>
        public int IsFeaturePoint(PointF pt, float range)
        {
            _lastFeature = 0;
            var features = AllFeaturePoints();

            for (int i = 0; i < features.Count; i++)
            {
                if (Expand(features[i], range).Contains(pt))
                {
                    _lastFeature = i + 1;
                }
            }

            return _lastFeature;
        }
        #endregion

        #region Abstract functions
        /// <summary>
        /// Bounds of shape.
        /// </summary>
        /// <returns></returns>
        public abstract RectangleF ToRect();

        /// <summary>
        /// Determine if this is within rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="any">True if any part is contained otherwise must be the entire.</param>
        /// <returns></returns>
        public abstract bool ContainedIn(RectangleF rect, bool any);

        /// <summary>
        /// Derived classes supply this.
        /// </summary>
        /// <returns></returns>
        public abstract List<PointF> AllFeaturePoints();
        #endregion
    }

    /// <summary>Drawing line.</summary>
    [Serializable]
    public class LineShape : Shape
    {
        #region Properties
        /// <summary>Line start.</summary>
        [JsonConverter(typeof(JsonPointFConverter))]
        public PointF Start { get; set; } = new(0, 0);

        /// <summary>Line end.</summary>
        [JsonConverter(typeof(JsonPointFConverter))]
        public PointF End { get; set; } = new(0, 0);

        /// <summary>Start type.</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PointStyle StartStyle { get; set; } = PointStyle.None;

        /// <summary>End type.</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PointStyle EndStyle { get; set; } = PointStyle.None;

        /// <summary>Get the line angle in radians.</summary>
        [JsonIgnore]
        public float Angle { get { return MathF.Atan((End.Y - Start.Y) / (End.X - Start.X)); } }
        #endregion

        /// <inheritdoc />
        public override List<PointF> AllFeaturePoints()
        {
            return new List<PointF>() { Start, End };
        }

        /// <inheritdoc />
        public override bool ContainedIn(RectangleF rect, bool any)
        {
            return rect.Contains(Start) || rect.Contains(End);
        }

        /// <inheritdoc />
        public override RectangleF ToRect()
        {
            return new RectangleF(Start.X > End.X ? End.X : Start.X, Start.Y > End.Y ? End.Y : Start.Y, MathF.Abs(End.X - Start.X), MathF.Abs(End.Y - Start.Y));
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString()
        {
            string fp = _lastFeature > 0 ? $"  Feature:{_lastFeature}" : "";
            return $"Line:{_id}{fp}  Start:{Start.X}, {Start.Y}  End:{End.X}, {End.Y}";
        }
    }

    /// <summary>Drawing rectangle.</summary>
    [Serializable]
    public class RectShape : Shape
    {
        #region Properties
        /// <summary>Rectangle top left.</summary>
        [JsonConverter(typeof(JsonPointFConverter))]
        public PointF Location { get; set; } = new(0, 0);

        /// <summary>Width</summary>
        public float Width { get; set; }

        /// <summary>Height</summary>
        public float Height { get; set; }

        /// <summary>Top left.</summary>
        [JsonIgnore, Browsable(false)]
        public PointF TL => new(Location.X, Location.Y);

        /// <summary>Top right.</summary>
        [JsonIgnore, Browsable(false)]
        public PointF TR => new(Location.X + Width, Location.Y);

        /// <summary>Bottom right.</summary>
        [JsonIgnore, Browsable(false)]
        public PointF BR => new(Location.X + Width, Location.Y + Height);

        /// <summary>Bottom left.</summary>
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
        public override List<PointF> AllFeaturePoints()
        {
            return new List<PointF>() { Location, TR, BR, BL };
        }

        /// <inheritdoc />
        public override RectangleF ToRect()
        {
            return new RectangleF(Location.X, Location.Y, Width, Height);
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString()
        {
            string fp = _lastFeature > 0 ? $"  Feature:{_lastFeature}" : "";
            return $"Rect:{_id}{fp}  Location:{Location.X}, {Location.Y}  Width:{Width}  Height:{Height}";
        }
    }

    /// <summary>Drawing ellipse.</summary>
    [Serializable]
    public class EllipseShape : Shape
    {
        #region Properties
        /// <summary>Center</summary>
        [JsonConverter(typeof(JsonPointFConverter))]
        public PointF Center { get; set; } = new(0, 0);

        /// <summary>Width</summary>
        public float Width { get; set; }

        /// <summary>Height</summary>
        public float Height { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Contains(PointF pf)
        {
            var rect = ToRect();
            bool ret = (pf.X <= rect.Right) && (pf.X >= rect.Left) && (pf.Y <= rect.Bottom) && (pf.Y >= rect.Top);
            return ret;
        }

        /// <inheritdoc />
        public override bool ContainedIn(RectangleF rect, bool any)
        {
            bool contained = false;
            var elrect = ToRect();

            if (any)
            {
                contained |= rect.Contains(elrect.Left, elrect.Top);
                contained |= rect.Contains(elrect.Right, elrect.Top);
                contained |= rect.Contains(elrect.Left, elrect.Bottom);
                contained |= rect.Contains(elrect.Right, elrect.Bottom);
            }
            else
            {
                contained = true;
                contained &= rect.Contains(elrect.Left, elrect.Top);
                contained &= rect.Contains(elrect.Right, elrect.Top);
                contained &= rect.Contains(elrect.Left, elrect.Bottom);
                contained &= rect.Contains(elrect.Right, elrect.Bottom);
            }

            return contained;
        }

        /// <inheritdoc />
        public override List<PointF> AllFeaturePoints()
        {
            return new List<PointF>()
            {
                new PointF(Center.X, Center.Y - Height / 2),
                new PointF(Center.X + Width / 2, Center.Y),
                new PointF(Center.X, Center.Y + Height / 2),
                new PointF(Center.X - Width / 2, Center.Y),
            };
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString()
        {
            string fp = _lastFeature > 0 ? $"  Feature:{_lastFeature}" : "";
            return $"Ellipse:{_id}{fp}  Center:{Center.X}, {Center.Y}  Width:{Width}  Height:{Height}";
        }

        /// <summary>Helper.</summary>
        public override RectangleF ToRect()
        {
            return new(Center.X - Width / 2, Center.Y - Height / 2, Width, Height);
        }
    }
}