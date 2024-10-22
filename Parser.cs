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
        /// Parse a single line. Really only used for testing.
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

            ///// Get the statement elements.
            var parts = s.SplitByToken(",");
            if (parts.Count <= 0)
            {
                throw new SyntaxException("No statement values");
            }

            // The first entry is statement type and name.
            var (stype, sname) = SplitParam(parts[0]);

            // The rest are the params, gather them.
            var parms = new Dictionary<string, string>();
            foreach (var elem in parts.GetRange(1, parts.Count - 1))
            {
                var (pname, pval) = SplitParam(elem);
                parms.Add(pname is null ? parms.Count.ToString() : pname, pval);
            }

            ///// Param parsers per statement type.
            switch (stype)
            {
                case "page":
                    Page.UnitsName = ParseString(parms, "un", "");// parms.ContainsKey("un") ? ParseText(parms["un"]) : "";
                    Page.Scale = ParseInt(parms, "sc");// int.Parse(parms["sc"]); // required
                    Page.Grid = ParseFloat(parms, "gr");// float.Parse(parms["gr"]); // required
                    break;

                case "line":
                    LineShape line = new();
                    InitShapeCommon(line, parms);
                    line.Start = ParsePoint(parms, "sx", "sy");// new PointF(ParseValue(parms["sx"]), ParseValue(parms["sy"])); // required
                    line.End = ParsePoint(parms, "ex", "ey");// new PointF(ParseValue(parms["ex"]), ParseValue(parms["ey"])); // required
                    line.StartStyle = ParsePointStyle(parms, "ss", PointStyle.None);// parms.ContainsKey("ss") ? _pointStyle[parms["ss"]] : _ss;
                    line.EndStyle = ParsePointStyle(parms, "es", PointStyle.None);//parms.ContainsKey("es") ? _pointStyle[parms["es"]] : _es;
                    Page.Lines.Add(line);
                    break;

                case "rect":
                    RectShape rect = new();
                    InitShapeCommon(rect, parms);
                    rect.Location = ParsePoint(parms, "x", "y");//new PointF(ParseValue(parms["x"]), ParseValue(parms["y"])); // required
                    rect.Width = ParseFloat(parms, "w");// ParseValue(parms["w"]); // required
                    rect.Height = ParseFloat(parms, "h");//ParseValue(parms["h"]); // required
                    Page.Rects.Add(rect);
                    break;

                case "ellipse":
                    EllipseShape ellipse = new();
                    InitShapeCommon(ellipse, parms);
                    ellipse.Center = ParsePoint(parms, "x", "y");//new PointF(ParseValue(parms["x"]), ParseValue(parms["y"])); // required
                    ellipse.Width = ParseFloat(parms, "w");//ParseValue(parms["w"]); // required
                    ellipse.Height = ParseFloat(parms, "h");//ParseValue(parms["h"]); // required
                    Page.Ellipses.Add(ellipse);
                    break;


                //$lt=3
                //$lc=salmon
                //ParseColor(parms, "lc", _lc);

                case "$fc": _fc = Color.FromName(sname); break;
                case "$lc": _lc = Color.FromName(sname); break;
                case "$lt": _lt = float.Parse(sname); break;
                case "$ta": _ta = _alignment[sname]; break;
                case "$ss": _ss = _pointStyle[sname]; break;
                case "$es": _es = _pointStyle[sname]; break;

                default: // variable or expression
                    //size_1_w=10
                    //size_2_w=size_1_w - 2.33
                    UserVals[stype] = ParseValue(sname);
                    break;

                //default: // assume a user value type
                //    switch (stype) // global  TODO broken for junk
                //    {
                //        case "$fc": _fc = Color.FromName(sname); break;
                //        case "$lc": _lc = Color.FromName(sname); break;
                //        case "$lt": _lt = float.Parse(sname); break;
                //        case "$ta": _ta = _alignment[sname]; break;
                //        case "$ss": _ss = _pointStyle[sname]; break;
                //        case "$es": _es = _pointStyle[sname]; break;
                //
                //        default: // user scalar or expression
                //            UserVals[stype] = ParseValue(sname);
                //            break;
                //    }
                //    break;
            }
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
        /// <param name="parms"></param>
        void InitShapeCommon(Shape shape, Dictionary<string, string> parms)
        {
            // Common.
            shape.Layer = ParseInt(parms, "lr", 1);// parms.ContainsKey("lr") ? int.Parse(parms["lr"]) : 1;
            shape.Text = ParseString(parms, "tx");// parms.ContainsKey("tx") ? ParseText(parms["tx"]) : "";
            shape.Hatch = ParseHatch(parms, "ht", Shape.NO_HATCH);// parms.ContainsKey("ht") ? _hatchStyle[parms["ht"]] : Shape.NO_HATCH;
            shape.LineThickness = ParseFloat(parms, "lt", _lt);// parms.ContainsKey("lt") ? float.Parse(parms["lt"]) : _lt;
            shape.LineColor = ParseColor(parms, "lc", _lc);// parms.ContainsKey("lc") ? Color.FromName(parms["lc"]) : _lc;
            shape.FillColor = ParseColor(parms, "fc", _fc);// parms.ContainsKey("fc") ? Color.FromName(parms["fc"]) : _fc;
            shape.TextAlignment = ParseAlignment(parms, "ta", _ta);// parms.ContainsKey("ta") ? _alignment[parms["ta"]] : _ta;
            //shape.Layer = parms.ContainsKey("lr") ? int.Parse(parms["lr"]) : 1;
            //shape.Text = parms.ContainsKey("tx") ? ParseText(parms["tx"]) : "";
            //shape.Hatch = parms.ContainsKey("ht") ? _hatchStyle[parms["ht"]] : Shape.NO_HATCH;
            //shape.LineThickness = parms.ContainsKey("lt") ? float.Parse(parms["lt"]) : _lt;
            //shape.LineColor = parms.ContainsKey("lc") ? Color.FromName(parms["lc"]) : _lc;
            //shape.FillColor = parms.ContainsKey("fc") ? Color.FromName(parms["fc"]) : _fc;
            //shape.TextAlignment = parms.ContainsKey("ta") ? _alignment[parms["ta"]] : _ta;
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
                        if (UserVals.ContainsKey(p))
                        {
                            f = UserVals[p];
                        }
                        else
                        {
                            throw new SyntaxException($"Unknown name for value {p}");
                        }
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






        //////// can be scalar or expression
        int ParseInt(Dictionary<string, string> parms, string name, int? def = null)
        {
            if (parms.TryGetValue(name, out var val)) // valid name?
            {
                var res = ParseValue(val);
                if (res != float.MaxValue) // valid value type?
                {
                    return (int)res;
                }
            }
            // use default?
            if (def is not null)
            {
                return (int)def;
            }
            // no good
            throw new SyntaxException($"Invalid value for {name}");
        }

        float ParseFloat(Dictionary<string, string> parms, string name, float? def = null)
        {
            if (parms.TryGetValue(name, out var val)) // valid name?
            {
                var res = ParseValue(val);
                if (res != float.MaxValue) // valid value type?
                {
                    return res;
                }
            }
            // use default?
            if (def is not null)
            {
                return (float)def;
            }
            // no good
            throw new SyntaxException($"Invalid value for {name}");
        }

        PointF ParsePoint(Dictionary<string, string> parms, string namex, string namey, PointF? def = null)
        {
            float x = 0;
            float y = 0;

            if (parms.TryGetValue(namex, out var valx)) // valid name?
            {
                var res = ParseValue(valx);
                if (res != float.MaxValue) // valid value type?
                {
                    x = res;
                }
                else
                {
                    throw new SyntaxException($"Invalid value for {namex}");
                }
            }

            if (parms.TryGetValue(namey, out var valy)) // valid name?
            {
                var res = ParseValue(valy);
                if (res != float.MaxValue) // valid value type?
                {
                    y = res;
                }
                else
                {
                    throw new SyntaxException($"Invalid value for {namey}");
                }
            }

            return new(x, y);
        }





        // public Color FillColor { get; set; } = ;
        Color ParseColor(Dictionary<string, string> parms, string name, Color? def = null)
        {
            var color = Color.FromName(parms["lc"]);

            if (color.IsKnownColor)
            {
                return color;
            }
            else if (def != null)
            {
                return (Color)def;
            }
            else
            {
                throw new SyntaxException($"Invalid value for {name}");
            }
        }

        ContentAlignment ParseAlignment(Dictionary<string, string> parms, string name, ContentAlignment? def = null)
        {
            if (parms.TryGetValue(name, out var val)) // valid name?
            {
                if (_alignment.TryGetValue(name, out ContentAlignment val))
                {
                    return val;
                }

                // use default?
                if (def is not null)
                {
                    return (float)def;
                }

                var res = ParseValue(val);
                if (res != float.MaxValue) // valid value type?
                {
                    return res;
                }
            }
            // use default?
            if (def is not null)
            {
                return (float)def;
            }
            // no good
            throw new SyntaxException($"Invalid value for {name}");




            if (_alignment.TryGetValue(name, out ContentAlignment val))
            {
                return val;
            }
            throw new SyntaxException($"Invalid value for {name}");
        }

        HatchStyle ParseHatch(Dictionary<string, string> parms, string name, HatchStyle? def = null)
        {
            if (_hatchStyle.TryGetValue(name, out HatchStyle val))
            {
                return val;
            }
            throw new SyntaxException($"Invalid value for {name}");
        }

        PointStyle ParsePointStyle(Dictionary<string, string> parms, string name, PointStyle? def = null)
        {
            if (_pointStyle.TryGetValue(name, out PointStyle val))
            {
                return val;
            }
            throw new SyntaxException($"Invalid value for {name}");
        }

        string ParseString(Dictionary<string, string> parms, string name, string? def = null)
        {
            //            s = s.Trim().Replace("\"", "").Trim();

            return "";
        }


        /// <summary>
        /// Parse a string in quotes or a scalar or simple expression.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        //string ParseText(string s)
        //{
        //    s = s.Trim().Replace("\"", "").Trim();


            //if (s.StartsWith("\"") && s.EndsWith("\""))
            //{
            //    s = s[1..^1];
            //}
            //else
            //{
            //    float f = ParseValue(s);
            //    if (!float.IsNaN(f))
            //    {
            //        s = f.ToString();
            //    }
            //    else
            //    {
            //        throw new Exception("Invalid string");
            //    }
            //}

        //    return s;
        //}






    }
}
