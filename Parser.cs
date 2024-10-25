using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ephemera.NBagOfTricks;


namespace NDraw
{
    class SyntaxException(string msg) : Exception(msg) { }

    public class Parser
    {
        #region Properties
        /// <summary>The parsing product.</summary>
        public Page Page { get; private set; } = new();
        
        /// <summary>Parsing errors.</summary>
        public List<string> Errors { get; private set; } = [];

        /// <summary>User assigned values.</summary>
        public Dictionary<string, float> UserVals { get; private set; } = [];
        #endregion

        #region Fields
        /// <summary>Where we are currently.</summary>
        int _row = 0;

        /// <summary>Parms for current line.</summary>
        readonly Dictionary<string, string> _params = [];

        #region Globals and defaults
        Color _fc;
        Color _lc;
        float _lt;
        ContentAlignment _ta;
        PointStyle _ss;
        PointStyle _es;
        #endregion
        #endregion

        #region Enum mappings
        readonly Dictionary<string, PointStyle> _pointStyle = new()
        {
            { "n", PointStyle.None }, { "a", PointStyle.Arrow }, { "t", PointStyle.Tee }
        };
        
        readonly Dictionary<string, HatchStyle> _hatchStyle = new()
        {
            { "ho", HatchStyle.Horizontal },       { "ve", HatchStyle.Vertical },    { "fd", HatchStyle.ForwardDiagonal },
            { "bd", HatchStyle.BackwardDiagonal }, { "lg", HatchStyle.LargeGrid },   { "dc", HatchStyle.DiagonalCross },
        };

        readonly Dictionary<string, ContentAlignment> _alignment = new()
        {
            { "tl", ContentAlignment.TopLeft },    { "tc", ContentAlignment.TopCenter },    { "tr", ContentAlignment.TopRight },
            { "ml", ContentAlignment.MiddleLeft }, { "mc", ContentAlignment.MiddleCenter }, { "mr", ContentAlignment.MiddleRight },
            { "bl", ContentAlignment.BottomLeft }, { "bc", ContentAlignment.BottomCenter }, { "br", ContentAlignment.BottomRight },
        };
        #endregion

        /// <summary>
        /// Do a file.
        /// </summary>
        /// <param name="fn"></param>
        public void ParseFile(string fn)
        {
            // Reset.
            _fc = Color.GhostWhite;
            _lc = Color.DimGray;
            _lt = 2.0f;
            _ta = ContentAlignment.MiddleCenter;
            _ss = PointStyle.None;
            _es = PointStyle.None;
            _row = 0;
            Errors.Clear();
            Page = new();

            foreach (string sf in File.ReadAllLines(fn))
            {
                _row++;

                try
                {
                    ParseLine(sf);
                }
                catch (Exception ex) // SyntaxException KeyNotFoundException FormatException
                {
                    Errors.Add($"Parse error at row {_row}: {ex.Message}");

                    // Don't torture the author.
                    if (Errors.Count >= 5)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Parse a single line.
        /// </summary>
        /// <param name="sf"></param>
        public void ParseLine(string sf)
        {
            ///// Remove any comments.
            string s = sf.Trim();
            int pos = s.IndexOf("//");

            if (pos < 0 && s.Length > 0) // not a comment
            {
                //s = s;
            }
            else if (pos > 0) // part line comment
            {
                s = s[..pos]; // strip
            }
            else // full line comment, skip
            {
                return;
            }

            ///// Collect the line contents.
            _params.Clear();
            var parts = s.SplitByToken(",");
            if (parts.Count <= 0)
            {
                throw new SyntaxException("No values");
            }

            // Get the first element which describes the line type. The rest are the actual params.
            var hs = parts[0].SplitByToken("=");
            if (hs.Count != 2)
            {
                throw new SyntaxException($"Bad value: {parts[0]}");
            }

            var lhs = hs[0];
            var rhs = hs[1];

            foreach (var elem in parts)
            {
                var pp = elem.SplitByToken("=");
                if (pp.Count == 2)
                {
                    _params[pp[0]] = pp[1];
                }
                else
                {
                    throw new SyntaxException($"Badly formed param: {elem}");
                }
            }

            ///// Process line contents based on type. Some are required so have no default arg.
            switch (lhs)
            {
                case "page":
                    Page.UnitsName = ParseText("un", "");
                    Page.Scale = ParseNumeric("sc", 1.0f);
                    Page.Grid = ParseNumeric("gr", 1.0f);
                    break;

                case "line":
                case "rect":
                case "ellipse":
                    Page.Shapes.Add(CreateShape(lhs));
                    break;

                // Global/default defs.
                case "$fc":
                    _fc = KnownColor(rhs);
                    break;

                case "$lc":
                    _lc = KnownColor(rhs);
                    break;

                case "$lt":
                    _lt = float.Parse(rhs);
                    break;

                case "$ta":
                    _ta = _alignment[rhs];
                    break;

                case "$ss":
                    _ss = _pointStyle[rhs];
                    break;

                case "$es":
                    _es = _pointStyle[rhs];
                    break;

                default: // Assume user value assignment.
                    UserVals[lhs] = Evaluate(rhs);
                    break;
            }
        }

        /// <summary>
        /// Shape factory.
        /// </summary>
        /// <param name="stype"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        Shape CreateShape(string stype)
        {
            Shape shape;

            switch (stype)
            {
                case "line":
                    var line = new LineShape
                    {
                        Start = ParsePoint("sx", "sy"),
                        End = ParsePoint("ex", "ey"),
                        StartStyle = ParsePointStyle("ss", _ss),
                        EndStyle = ParsePointStyle("es", _es)
                    };
                    shape = line;
                    break;

                case "rect":
                    var rect = new RectShape
                    {
                        Location = ParsePoint("x", "y"),
                        Width = ParseNumeric("w"),
                        Height = ParseNumeric("h")
                    };
                    shape = rect;
                    break;

                case "ellipse":
                    var ellipse = new EllipseShape
                    {
                        Center = ParsePoint("x", "y"),
                        Width = ParseNumeric("w"),
                        Height = ParseNumeric("h")
                    };
                    shape = ellipse;
                    break;

                default:
                    throw new SyntaxException($"Invalid shape: {stype}");

            }

            // Common stuff.
            shape.Layer = (int)ParseNumeric("lr", 1);
            shape.Text = ParseText("tx", "");
            shape.Hatch = ParseHatch("ht");
            shape.LineThickness = ParseNumeric("lt", _lt);
            shape.LineColor = ParseColor("lc", _lc);
            shape.FillColor = ParseColor("fc", _fc);
            shape.TextAlignment = ParseAlignment("ta", _ta);

            return shape;
        }

        /// <summary>
        /// Evaluate the float scalar or simple expression.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>The value or throws if invalid.</returns>
        /// <exception cref="SyntaxException"></exception>
        float Evaluate(string s)
        {
            // Try simple float.
            if (!float.TryParse(s, out float v))
            {
                // Try parse expression.
                var operators = "+-";
                string oper = "";

                foreach (var p in s.SplitKeepDelims(operators))
                {
                    float f = float.NaN;
                    if (operators.Contains(p)) // op
                    {
                        oper = p;
                    }
                    else if (float.TryParse(p, out f)) // literal
                    {
                        //
                    }
                    else // named
                    {
                        f = UserVals.TryGetValue(p, out float value) ? value : throw new SyntaxException($"Unknown name for value {p}");
                    }

                    // Do the math.
                    if (!float.IsNaN(f))
                    {
                        if (oper == "-") v -= f;
                        else if (oper == "+") v += f;
                        else v = f;
                        oper = "";
                    }
                }
            }

            return v;
        }

        #region Typed parsers
        /// <summary>
        /// Parse a scalar or expression.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        float ParseNumeric(string paramName, float? def = null)
        {
            if (_params.TryGetValue(paramName, out var val)) // valid name?
            {
                return Evaluate(val); // may throw
            }

            if (def is not null) // use default?
            {
                return (float)def;
            }

            // >>> no good
            throw new SyntaxException($"Invalid numeric value: {paramName}");
        }

        /// <summary>
        /// Parse a text value.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        string ParseText(string paramName, string? def = null)
        {
            if (_params.TryGetValue(paramName, out var val)) // valid name?
            {
                return val.Trim().Replace("\"", "").Trim();
            }

            if (def is not null) // use default?
            {
                return def;
            }

            // >>> no good
            throw new SyntaxException($"Invalid text: {paramName}");
        }

        /// <summary>
        /// Parse a point definition. No default.
        /// </summary>
        /// <param name="paramNameX"></param>
        /// <param name="paramNameY"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        PointF ParsePoint(string paramNameX, string paramNameY)
        {
            float x = float.NaN;
            float y = float.NaN;

            if (_params.TryGetValue(paramNameX, out var xval)) // valid name?
            {
                x = Evaluate(xval); // may throw
            }

            if (_params.TryGetValue(paramNameY, out var yval)) // valid name?
            {
                y = Evaluate(yval); // may throw
            }

            if (!float.IsNaN(x) && !float.IsNaN(y))
            {
                return new(x, y);
            }

            // >>> no good
            throw new SyntaxException($"Invalid point: {paramNameX} {paramNameY}");
        }

        /// <summary>
        /// Parse a color definition.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        Color ParseColor(string paramName, Color? def = null)
        {
            if (_params.TryGetValue(paramName, out string? colorName))
            {
                return KnownColor(colorName);
            }

            if (def is not null)
            {
                return (Color)def;
            }

            // >>> no good
            throw new SyntaxException($"Invalid color: {paramName}");
        }

        /// <summary>
        /// Parse an alignment definition.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        ContentAlignment ParseAlignment(string paramName, ContentAlignment? def = null)
        {
            if (_params.TryGetValue(paramName, out string? alignName) && _alignment.TryGetValue(alignName, out ContentAlignment val))
            {
                return val;
            }

            if (def is not null)
            {
                return (ContentAlignment)def;
            }

            // >>> no good
            throw new SyntaxException($"Invalid alignment: {paramName}");
        }

        /// <summary>
        /// Parse a hatch definition. No default.
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        HatchStyle? ParseHatch(string paramName)
        {
            if (_params.TryGetValue(paramName, out string? hatchName) && _hatchStyle.TryGetValue(hatchName, out HatchStyle val))
            {
                return val;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Parse a point style definition.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        PointStyle ParsePointStyle(string paramName, PointStyle? def = null)
        {
            if (_params.TryGetValue(paramName, out string? pointStyleName) && _pointStyle.TryGetValue(pointStyleName, out PointStyle val))
            {
                return val;
            }

            if (def is not null)
            {
                return (PointStyle)def;
            }

            // >>> no good
            throw new SyntaxException($"Invalid point style: {paramName}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorName"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        Color KnownColor(string colorName)
        {
            if (Color.FromName(colorName).IsKnownColor)
            {
                return Color.FromName(colorName);
            }

            // >>> no good
            throw new SyntaxException($"Invalid color: {colorName}");
        }

        #endregion
    }
}
