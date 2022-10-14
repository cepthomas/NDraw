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


namespace Ephemera.NDraw
{
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
                    if(Errors.Count >= 5)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Parse a single line. Really only used for testing.
        /// </summary>
        /// <param name="sf"></param>
        public void ParseLine(string sf)
        {
            ///// Remove any comments.
            string s = sf.Trim();
            int pos = s.IndexOf("//");

            if (pos < 0 && s.Length > 0) // no comment
            {
                //s = s;
            }
            else if (pos > 0) // part line comment
            {
                s = s.Substring(0, pos); // strip
            }
            else
            {
                return; // continue; // skip this
            }

            ///// Get the statement values.
            var parts = s.SplitByToken(",");

            // The first one describes the type.
            var (elid, elval) = SplitParam(parts[0]);

            // The rest are the params.
            var elemParams = new Dictionary<string, string>();
            foreach (var elem in parts.GetRange(1, parts.Count - 1))
            {
                var (pname, pval) = SplitParam(elem);
                elemParams.Add(pname is null ? elemParams.Count.ToString() : pname, pval);
            }

            ///// Section parsers.
            switch (elval)
            {
                case "page":
                    Page.UnitsName = elemParams.ContainsKey("un") ? ParseText(elemParams["un"]) : "";
                    Page.Scale = int.Parse(elemParams["sc"]); // required
                    Page.Grid = float.Parse(elemParams["gr"]); // required
                    break;

                case "line":
                    LineShape line = new() { Id = elid };
                    InitShapeCommon(line, elemParams);
                    line.Start = new PointF(ParseValue(elemParams["sx"]), ParseValue(elemParams["sy"])); // required
                    line.End = new PointF(ParseValue(elemParams["ex"]), ParseValue(elemParams["ey"])); // required
                    line.StartStyle = elemParams.ContainsKey("ss") ? _pointStyle[elemParams["ss"]] : _ss;
                    line.EndStyle = elemParams.ContainsKey("es") ? _pointStyle[elemParams["es"]] : _es;
                    Page.Lines.Add(line);
                    break;

                case "rect":
                    RectShape rect = new() { Id = elid };
                    InitShapeCommon(rect, elemParams);
                    rect.Location = new PointF(ParseValue(elemParams["x"]), ParseValue(elemParams["y"])); // required
                    rect.Width = ParseValue(elemParams["w"]); // required
                    rect.Height = ParseValue(elemParams["h"]); // required
                    if (rect.Width < 1 || rect.Height < 1)
                    {
                        throw new Exception("Invalid rectangle");
                    }
                    Page.Rects.Add(rect);
                    break;

                case "ellipse":
                    EllipseShape ellipse = new() { Id = elid };
                    InitShapeCommon(ellipse, elemParams);
                    ellipse.Center = new PointF(ParseValue(elemParams["x"]), ParseValue(elemParams["y"])); // required
                    ellipse.Width = ParseValue(elemParams["w"]); // required
                    ellipse.Height = ParseValue(elemParams["h"]); // required
                    if (ellipse.Width < 1 || ellipse.Height < 1)
                    {
                        throw new Exception("Invalid ellipse");
                    }
                    Page.Ellipses.Add(ellipse);
                    break;

                default: // a value type
                    switch (elid) // global
                    {
                        case "$fc": _fc = Color.FromName(elval); break;
                        case "$lc": _lc = Color.FromName(elval); break;
                        case "$lt": _lt = float.Parse(elval); break;
                        case "$ta": _ta = _alignment[elval]; break;
                        case "$ss": _ss = _pointStyle[elval]; break;
                        case "$es": _es = _pointStyle[elval]; break;

                        default: // user scalar or expression
                            UserVals[elid] = ParseValue(elval);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Parse a numerical value - scalar or simple expression.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>The value or NaN if invalid.</returns>
        float ParseValue(string s)
        {
            // Try simple float.
            if (!float.TryParse(s, out float v))
            {
                // Try parse expression.
                var ops = "+-";
                var parts = s.SplitKeepDelims(ops);

                string op = "";

                foreach (string p in parts)
                {
                    float f = float.NaN;
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
                        f = UserVals[p];
                    }

                    if (!float.IsNaN(f))
                    {
                        if (op == "-") v -= f;
                        else if (op == "+") v += f;
                        else v = f;
                        op = "";
                    }
                }
            }

            return v;
        }

        /// <summary>
        /// Parse a string in quotes or a scalar or simple expression.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        string ParseText(string s)
        {
            s = s.Trim();

            if(s.StartsWith("\"") && s.EndsWith("\""))
            {
                s = s[1..^1];
            }
            else
            {
                float f = ParseValue(s);
                if(!float.IsNaN(f))
                {
                    s = f.ToString();
                }
                else
                {
                    throw new Exception("Invalid string");
                }
            }

            return s;
        }

        /// <summary>
        /// Utility to chop up named params.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        (string lhs, string rhs) SplitParam(string p)
        {
            var pp = p.SplitByToken("=");
            return pp.Count > 1 ? (pp[0], pp[1]) : (pp[0], "");
        }

        /// <summary>
        /// Populates column elements of shapes.
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="elemParams"></param>
        void InitShapeCommon(Shape shape, Dictionary<string, string> elemParams)
        {
            // Common.
            shape.Layer = elemParams.ContainsKey("lr") ? int.Parse(elemParams["lr"]) : 1;
            shape.Text = elemParams.ContainsKey("tx") ? ParseText(elemParams["tx"]) : "";
            shape.Hatch = elemParams.ContainsKey("ht") ? _hatchStyle[elemParams["ht"]] : Shape.NO_HATCH;
            shape.LineThickness = elemParams.ContainsKey("lt") ? float.Parse(elemParams["lt"]) : _lt;
            shape.LineColor = elemParams.ContainsKey("lc") ? Color.FromName(elemParams["lc"]) : _lc;
            shape.FillColor = elemParams.ContainsKey("fc") ? Color.FromName(elemParams["fc"]) : _fc;
            shape.TextAlignment = elemParams.ContainsKey("ta") ? _alignment[elemParams["ta"]] : _ta;
        }
    }
}
