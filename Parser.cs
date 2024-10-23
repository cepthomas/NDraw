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
        public List<string> Errors { get; private set; } = new();

        /// <summary>User assigned values.</summary>
        public Dictionary<string, float> UserVals { get; private set; } = new();
        #endregion

        #region Fields
        /// <summary>Where we are currently.</summary>
        int _row = 0;
        #endregion

        // Parms for current line.
        Dictionary<string, string> _params = new();
        Dictionary<string, string> _globals = new();

        #region Global defaults
        Color _fc = Color.LightBlue;
        Color _lc = Color.Red;
        float _lt = 2.5f;
        ContentAlignment _ta = ContentAlignment.MiddleCenter;
        PointStyle _ss = PointStyle.None;
        PointStyle _es = PointStyle.None;
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
        /// 
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

            foreach (var elem in parts)
            {
                var pp = elem.SplitByToken("=");
                if (pp.Count == 2)
                {
                    _params.Add((pp[0], pp[1]));
                }
                throw new SyntaxException($"Badly formed param {p}");
            }

            // [](C:\Users\cepth\OneDrive\OneDriveDocuments\condo\physical\yard.nd)
            ///// Process line contents based on type. Some are required so have no default arg.
            // The first describes the line type. The rest are the actual params.
            var (lhs, rhs) = _params[0];

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
                    if (lhs.StartsWith("$"))
                    {
                        _globals[lhs] = rhs;
// Field | Type | Req | Description
// ----  | ---- | --- | ----------
// $fc   |  C   |  N  | Fill color
// $lc   |  C   |  N  | Line color
// $lt   |  F   |  N  | Line thickness
// $ta   |  A   |  N  | Text alignment
// $ss   |  P   |  N  | Start point style
// $es   |  P   |  N  | End point style
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
// Field | Type | Req | Description
// ----  | ---- | --- | ----------
// lr    |  I   |  N  | Layer 1 to 4 or 0 for all
// tx    |  T   |  N  | Display text
// fc    |  C   |  N  | Fill color
// ht    |  H   |  N  | Hatch type
// lc    |  C   |  N  | Line color
// lt    |  F   |  N  | Line thickness
// ta    |  A   |  N  | Text alignment

            // Common.
            shape.Layer = ParseNumeric("lr", 1);
            shape.Text = ParseText("tx");
            shape.Hatch = ParseHatch("ht", Shape.NO_HATCH);
            shape.LineThickness = ParseNumeric("lt", "$lt");
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
                var ops = "+-";
                string op = "";

                foreach (var p in s.SplitKeepDelims(ops))
                {
                    float f;// = float.NaN;
                    if (ops.Contains(p)) // op
                    {
                        op = p;
                    }
                    else if (float.TryParse(p, out f)) // literal
                    {
                        //op = "";
                    }
                    else // named
                    {
                        if (UserVals.ContainsKey(p)) // TODO check type?
                        {
                            f = UserVals[p];
                        }
                        else
                        {
                            throw new SyntaxException($"Unknown name for value {p}");
                        }
                    }

                    // Do the math.
                    if (op == "-") v -= f;
                    else if (op == "+") v += f;
                    else v = f;
                    op = "";
                }
            }

            return v;
        }



///////=================================================================


        float ParseNumeric(string name, float? def = null)
        {
            if (_params.TryGetValue(name, out var val)) // valid name?
            {
                return Evaluate(val); // may throw
            }
            if (def is not null) // use default?
            {
                return (float)def;
            }

            // no good
            throw new SyntaxException($"Invalid numeric value: {name}");
        }

        string ParseText(string name, string? def = null)
        {
            if (_params.TryGetValue(name, out var val)) // valid name?
            {
                return val.Trim().Replace("\"", "").Trim();
            }
            if (def is not null) // use default?
            {
                return (string)def;
            }

            // no good
            throw new SyntaxException($"Invalid text: {name}");
        }


        PointF ParsePoint(string xname, string yname)
        {
            float x = float.NaN;
            float y = float.NaN;

            if (_params.TryGetValue(xname, out var xval)) // valid name?
            {
                x = Evaluate(xval); // may throw
            }

            if (_params.TryGetValue(yname, out var yval)) // valid name?
            {
                y = Evaluate(yval); // may throw
            }

            if (x != float.NaN && y != float.NaN)
            {
                return new(x, y);
            }

            // no good
            throw new SyntaxException($"Invalid point: {name}");
        }

        Color ParseColor(string name, string? def = null)
        {
            // shape.LineColor = ParseColor("lc", "$lc");
            string? colorName = null;

            _params.TryGetValue(name, out colorName); // valid name?

            if (colorName is null && def is not null) // try default - global or literal
            {
                if (!_globals.TryGetValue(name, out colorName))
                {
                    // Assume literal.
                    colorName = def;
                }
            }

            if (colorName is not null)
            {
                if (Color.FromName(name).IsKnownColor)
                {
                    return color;
                }
            }

            // no good
            throw new SyntaxException($"Invalid color: {name}");
        }




        ContentAlignment ParseAlignment(string name, string? def = null)
        {
            string? alignName = null;


            _params.TryGetValue(name, out alignName); // valid name?

            if (alignName is null && def is not null) // try default - global or literal
            {
                if (_globals.TryGetValue(name, out alignName))
                {
                    // Assume literal.
                    alignName = def;
                }
            }

            if (alignName is not null)
            {
                if (_alignment.TryGetValue(alignName, out ContentAlignment val))
                {
                    return val;
                }
            }

            // no good
            throw new SyntaxException($"Invalid alignment: {name}");
        }


///////=================================================================


        HatchStyle ParseHatch(string name, string? def = null)
        {
            if (_hatchStyle.TryGetValue(name, out HatchStyle val))
            {
                return val;
            }
            throw new SyntaxException($"Invalid hatch: {name}");
        }

        PointStyle ParsePointStyle(string name, string? def = null)
        {
            if (_pointStyle.TryGetValue(name, out PointStyle val))
            {
                return val;
            }
            throw new SyntaxException($"Invalid point style: {name}");
        }
    }
}
