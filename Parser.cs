using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NBagOfTricks.Utils;


namespace NDraw
{
    //public class ParseException : Exception
    //{
    //    public int Row { get; set; }
    //    public int Column { get; set; }
    //    public ParseException(string msg) : base(msg) { }
    //}


    public class Parser
    {
        #region Global defaults
        Color _fc = Color.LightBlue;
        Color _lc = Color.Red;
        float _lt = 2.5f;
        ContentAlignment _tp = ContentAlignment.MiddleCenter;
        PointStyle _ss = PointStyle.None;
        PointStyle _es = PointStyle.None;
        #endregion

        #region Enum mappings
        readonly Dictionary<string, PointStyle> _pointStyle = new()
        {
            { "NO", PointStyle.None },
            { "CH", PointStyle.CircleHollow }, { "CF", PointStyle.CircleFilled }, { "SH", PointStyle.SquareHollow },
            { "SF", PointStyle.SquareFilled }, { "AH", PointStyle.ArrowHollow },  { "AF", PointStyle.ArrowFilled },
        };

        readonly Dictionary<string, ContentAlignment> _alignment = new()
        {
            { "TL", ContentAlignment.TopLeft },    { "TC", ContentAlignment.TopCenter },    { "TR", ContentAlignment.TopRight },
            { "CL", ContentAlignment.MiddleLeft }, { "CC", ContentAlignment.MiddleCenter }, { "CR", ContentAlignment.MiddleRight },
            { "BL", ContentAlignment.BottomLeft }, { "BC", ContentAlignment.BottomCenter }, { "BR", ContentAlignment.BottomRight },
        };
        #endregion

        /// <summary>User assigned values: <name, value></name></summary>
        Dictionary<string, float> _vals = new();

        /// <summary>Where we are.</summary>
        int _row = 0;

        /// <summary>Parsing errors.</summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        public Page ParseFile(string fn)
        {
            Page page = new();
            _row = 0;

            foreach (string sf in File.ReadAllLines(fn))
            {
                _row++;

                try
                {
                    ///// Remove any comments.
                    string s = "";
                    int pos = sf.Trim().IndexOf("//");

                    if (pos < 0 && sf.Length > 0) // no comment
                    {
                        s = sf;
                    }
                    else if (pos > 0) // part line comment
                    {
                        s = sf.Substring(0, pos); // strip
                    }
                    else
                    {
                        continue; // skip this
                    }

                    ///// Get the statement values.
                    var parts = s.SplitByToken(",");
                    var first = SplitParam(parts[0]);

                    var elemParams = new Dictionary<string, string>();
                    foreach (var elem in parts.GetRange(1, parts.Count - 1))
                    {
                        var (name, val) = SplitParam(elem);
                        elemParams.Add(name is null ? elemParams.Count.ToString() : name, val);
                    }

                    ///// Section parsers.
                    switch (first.rhs)
                    {
                        case "page":
                            // pg_1=page, un=feet, w=100, h=50, gr=2
                            page.UnitsName = elemParams.ContainsKey("un") ? elemParams["un"] : "";
                            page.Width = ParseValue(elemParams["w"]); // required
                            page.Height = ParseValue(elemParams["h"]); // required
                            page.Grid = ParseValue(elemParams["gr"]); // required
                            break;

                        case "line":
                            // my_line1=line, lr=2, sx=loc_1_x, sy=loc_1_y, ex=my_rect2_x, ey=my_rect3_y, lt=2, tx=Hoohaa, tp=TL, es=CH, ss=AF
                            LineShape line = new() { Id = first.lhs };
                            InitShapeCommon(line, elemParams);
                            line.Start = new PointF(ParseValue(elemParams["sx"]), ParseValue(elemParams["sy"])); // required
                            line.End = new PointF(ParseValue(elemParams["ex"]), ParseValue(elemParams["ey"])); // required
                            line.StartStyle = elemParams.ContainsKey("ss") ? _pointStyle[elemParams["ss"]] : _ss;
                            line.EndStyle = elemParams.ContainsKey("es") ? _pointStyle[elemParams["es"]] : _es;
                            // Do sanity check on start and end. TODO
                            page.Lines.Add(line);
                            break;

                        case "rect":
                            // my_rect1=rect, lr=1, x=loc_2_x, y=loc_2_y, w=size_1_w, h=size_1_h, lc=green, fc=lightgreen, tx=Nice day, tp=TL
                            RectShape rect = new() { Id = first.lhs };
                            InitShapeCommon(rect, elemParams);
                            rect.TL = new PointF(ParseValue(elemParams["x"]), ParseValue(elemParams["y"])); // required
                            rect.BR = new PointF(rect.TL.X + ParseValue(elemParams["w"]), rect.TL.Y + ParseValue(elemParams["h"])); // required
                            // TODO do sanity check.
                            page.Rects.Add(rect);
                            break;

                        default:
                            // global values: can be changed any time
                            //$lt=4
                            //$lc=salmon
                            switch (first.lhs)
                            {
                                case "$fc": _fc = Color.FromName(first.rhs); break;
                                case "$lc": _lc = Color.FromName(first.rhs); break;
                                case "$lt": _lt = float.Parse(first.rhs); break;
                                case "$tp": _tp = _alignment[first.rhs]; break;
                                case "$ss": _ss = _pointStyle[first.rhs]; break;
                                case "$es": _es = _pointStyle[first.rhs]; break;

                                default:
                                    // user scalar or expression
                                    _vals[first.lhs] = ParseValue(first.rhs);
                                    // _vals[first.lhs] = pval.Contains("\"") ? pval.Replace("\"", "") : ParseNumber(pval);
                                    break;
                            }
                            break;
                    }
                }
                //catch (ParseException ex)
                //{
                //    Errors.Add($"Parse error: {ex}");
                //}
                catch (Exception ex)
                {
                    Errors.Add($"Parse error at row {_row}: {ex.Message}"); // TODO more info?
                }
            }

            return page;
        }

        /// <summary>
        /// Parse a single value - scalar or simple expression. TODO
        /// </summary>
        /// <param name="s"></param>
        /// <returns>The value or NaN if invalid.</returns>
        float ParseValue(string s)
        {
            float v = 0;// float.NaN;

            // Try simple float.
            if(!float.TryParse(s, out v))
            {
                // Try parse expression. v1 + 4.4 - v2 + 123 + 290 ...
                var ops = "+-";
                var parts = Split(s, ops);

                string op = "";
                float f = float.NaN;

                foreach(string p in parts)
                {
                    if(ops.Contains(p)) // op
                    {
                        op = p;
                    }
                    else if(float.TryParse(p, out f)) // literal
                    {
                        op = "";
                    }
                    else // named
                    {
                        f = _vals[p];
                    }

                    if (f != float.NaN)
                    {
                        if (op == "-") v -= f;
                        else if (op == "+") v += f;
                        else v = f;
                    }
                }
            }

            return v;
        }

        /// <summary>
        /// Utility to chop up named params.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        (string lhs, string rhs) SplitParam(string p)
        {
            var pp = p.SplitByToken("=");
            return pp.Count > 1 ? (pp[0], pp[1]) : (null, pp[0]);
        }

        /// <summary>
        /// Populates column elements of shapes.
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="elemParams"></param>
        void InitShapeCommon(Shape shape, Dictionary<string, string> elemParams)
        {
            // Common.
            shape.Layer = elemParams.ContainsKey("lr") ? int.Parse(elemParams["lr"]) : 0;
            shape.Text = elemParams.ContainsKey("tx") ? elemParams["tx"] : null;
            shape.LineThickness = elemParams.ContainsKey("lt") ? float.Parse(elemParams["lt"]) : _lt;
            shape.LineColor = elemParams.ContainsKey("lc") ? Color.FromName(elemParams["lc"]) : _lc;
            shape.FillColor = elemParams.ContainsKey("fc") ? Color.FromName(elemParams["fc"]) : _fc;
            shape.TextAlignment = elemParams.ContainsKey("tp") ? _alignment[elemParams["tp"]] : _tp;
        }

        /// <summary>
        /// Split by one of the delims but keep the delim.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="delims"></param>
        /// <returns></returns>
        List<string> Split(string s, string delims) // TODO optimize
        {
            var parts = new List<string>();
            string acc = "";

            for (int i = 0; i < s.Length; i++)
            {
                if(!char.IsWhiteSpace(s[i]))
                {
                    if (delims.Contains(s[i]))
                    {
                        if(acc.Length > 0)
                        {
                            parts.Add(acc);
                            acc = "";
                        }
                        parts.Add(s[i].ToString());
                    }
                    else
                    {
                        acc += s[i];
                    }
                }
            }

            if (acc.Length > 0)
            {
                parts.Add(acc);
            }

            return parts;
        }
    }
}
