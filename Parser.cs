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

// [](C:\Users\cepth\OneDrive\OneDriveDocuments\condo\physical\yard.nd)


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

        /// <summary>Identified globals. Probably not really necessary.</summary>
        readonly Dictionary<string, string> _globals = [];
        #endregion

        #region Enum mappings
        readonly Dictionary<string, PointStyle> _pointStyle = new()
        {
            { "a", PointStyle.Arrow }, { "t", PointStyle.Tee }
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
            Page = new();
            _row = 0;
            Errors.Clear();

            foreach (string sf in File.ReadAllLines(fn))
            {
                _row++;

                try
                {
                    ParseLine(sf);
                }
                catch (Exception ex)
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
                    LineShape line = new();
                    InitShapeCommon(line);
                    line.Start = ParsePoint("sx", "sy");
                    line.End = ParsePoint("ex", "ey");
                    line.StartStyle = ParsePointStyle("ss", "$ss");
                    line.EndStyle = ParsePointStyle("es", "$es");
                    Page.Lines.Add(line);
                    break;

                case "rect":
                    RectShape rect = new();
                    InitShapeCommon(rect);
                    rect.Location = ParsePoint("x", "y");
                    rect.Width = ParseNumeric("w");
                    rect.Height = ParseNumeric("h");
                    Page.Rects.Add(rect);
                    break;

                case "ellipse":
                    EllipseShape ellipse = new();
                    InitShapeCommon(ellipse);
                    ellipse.Center = ParsePoint("x", "y");
                    ellipse.Width = ParseNumeric("w");
                    ellipse.Height = ParseNumeric("h");
                    Page.Ellipses.Add(ellipse);
                    break;

                default: // Assume value assignment.
                    if (lhs.StartsWith('$'))
                    {
                        _globals[lhs] = rhs;
                    }
                    else
                    {
                        UserVals[lhs] = Evaluate(rhs);
                    }
                    break;
            }
        }

        /// <summary>
        /// Populates common elements of shapes.
        /// </summary>
        /// <param name="shape"></param>
        void InitShapeCommon(Shape shape)
        {
            shape.Layer = (int)ParseNumeric("lr", 1);
            shape.Text = ParseText("tx", "");
            shape.Hatch = ParseHatch("ht", "");
            shape.LineThickness = ParseNumericS("lt", "$lt");
            shape.LineColor = ParseColor("lc", "$lc");
            shape.FillColor = ParseColor("fc", "$fc");
            shape.TextAlignment = ParseAlignment("ta", "$ta");
        }

        /// <summary>
        /// Evaluate the scalar or simple expression.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>The value or throws if invalid.</returns>
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

        /// <summary>
        /// 
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

            // no good
            throw new SyntaxException($"Invalid numeric value: {paramName}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        float ParseNumericS(string paramName, string? def = null) // TODO a bit klunky.
        {
            string? valName = GetParamValueOrDefault(paramName, def);
            if (valName is not null)
            {
                return Evaluate(valName); // may throw
            }

            // no good
            throw new SyntaxException($"Invalid numeric value: {paramName}");
        }

        /// <summary>
        /// 
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
                return (string)def;
            }

            // no good
            throw new SyntaxException($"Invalid text: {paramName}");
        }

        /// <summary>
        /// 
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

            // no good
            throw new SyntaxException($"Invalid point: {paramNameX} {paramNameY}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        Color ParseColor(string paramName, string? def = null)
        {
            string? colorName = GetParamValueOrDefault(paramName, def);

            if (colorName is not null && Color.FromName(colorName).IsKnownColor)
            {
                return Color.FromName(colorName);
            }

            // no good
            throw new SyntaxException($"Invalid color: {paramName}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        ContentAlignment ParseAlignment(string paramName, string? def = null)
        {
            string? alignName = GetParamValueOrDefault(paramName, def);

            if (alignName is not null && _alignment.TryGetValue(alignName, out ContentAlignment val))
            {
                return val;
            }

            // no good
            throw new SyntaxException($"Invalid alignment: {paramName}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        HatchStyle ParseHatch(string paramName, string? def = null)
        {
            string? hatchName = GetParamValueOrDefault(paramName, def);

            if (hatchName == "")
            {
                return Shape.NO_HATCH;
            }

            if (hatchName is not null && _hatchStyle.TryGetValue(hatchName, out HatchStyle val))
            {
                return val;
            }

            // no good
            throw new SyntaxException($"Invalid hatch: {paramName}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        PointStyle ParsePointStyle(string paramName, string? def = null)
        {
            string? pointStyleName = GetParamValueOrDefault(paramName, def);

            if (pointStyleName is not null && _pointStyle.TryGetValue(pointStyleName, out PointStyle val))
            {
                return val;
            }

            // no good
            throw new SyntaxException($"Invalid point style: {paramName}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        string? GetParamValueOrDefault(string paramName, string? def)
        {
            _params.TryGetValue(paramName, out string? val); // valid name?

            if (val is null && def is not null) // try default - global or literal
            {
                if (!_globals.TryGetValue(def, out val))
                {
                    // Assume literal.
                    val = def;
                }
            }

            return val;
        }
    }
}
