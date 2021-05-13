using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NBagOfTricks.Utils;


namespace NDraw
{
    public class Parser
    {
        #region Properties
        /// <summary>The product.</summary>
        public Page Page { get; private set; } = new();

        /// <summary>Parsing errors.</summary>
        public List<string> Errors { get; private set; } = new();

        /// <summary>User assigned values: <name, value></name></summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn"></param>
        public void ParseFile(string fn)
        {
            Page = new();
            _row = 0;

            foreach (string sf in File.ReadAllLines(fn))
            {
                _row++;

                try
                {
                    ParseLine(sf);
                }
                //catch (ParseException ex)
                //{
                //    Errors.Add($"Parse error: {ex}");
                //}
                catch (Exception ex)
                {
                    Errors.Add($"Parse error at row {_row}: {ex.Message}");
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
            var (lhs, rhs) = SplitParam(parts[0]);

            var elemParams = new Dictionary<string, string>();
            foreach (var elem in parts.GetRange(1, parts.Count - 1))
            {
                var (name, val) = SplitParam(elem);
                elemParams.Add(name is null ? elemParams.Count.ToString() : name, val);
            }

            ///// Section parsers.
            switch (rhs)
            {
                case "page":
                    // pg_1=page, un=feet, w=100, h=50, gr=2
                    // pg_1=page, un=feet, gr=2
                    Page.UnitsName = elemParams.ContainsKey("un") ? elemParams["un"] : "";
                    //Page.Width = ParseValue(elemParams["w"]); // required
                    //Page.Height = ParseValue(elemParams["h"]); // required
                    Page.Grid = float.Parse(elemParams["gr"]); // required
                    Page.Scale = int.Parse(elemParams["sc"]); // required
                    break;

                case "line":
                    // my_line1=line, lr=2, sx=loc_1_x, sy=loc_1_y, ex=my_rect2_x, ey=my_rect3_y, lt=2, tx=Hoohaa, tp=TL, es=CH, ss=AF
                    LineShape line = new() { Id = lhs };
                    InitShapeCommon(line, elemParams);
                    line.Start = new PointF(ParseValue(elemParams["sx"]), ParseValue(elemParams["sy"])); // required
                    line.End = new PointF(ParseValue(elemParams["ex"]), ParseValue(elemParams["ey"])); // required
                    line.StartStyle = elemParams.ContainsKey("ss") ? _pointStyle[elemParams["ss"]] : _ss;
                    line.EndStyle = elemParams.ContainsKey("es") ? _pointStyle[elemParams["es"]] : _es;
                    // Do sanity check on start and end. TODO
                    Page.Lines.Add(line);
                    break;

                case "rect":
                    // my_rect1=rect, lr=1, x=loc_2_x, y=loc_2_y, w=size_1_w, h=size_1_h, lc=green, fc=lightgreen, tx=Nice day, tp=TL
                    RectShape rect = new() { Id = lhs };
                    InitShapeCommon(rect, elemParams);
                    rect.TL = new PointF(ParseValue(elemParams["x"]), ParseValue(elemParams["y"])); // required
                    rect.BR = new PointF(rect.TL.X + ParseValue(elemParams["w"]), rect.TL.Y + ParseValue(elemParams["h"])); // required
                    // TODO do sanity check.
                    Page.Rects.Add(rect);
                    break;

                default:
                    // global values: can be changed any time
                    //$lt=4
                    //$lc=salmon
                    switch (lhs)
                    {
                        case "$fc": _fc = Color.FromName(rhs); break;
                        case "$lc": _lc = Color.FromName(rhs); break;
                        case "$lt": _lt = float.Parse(rhs); break;
                        case "$tp": _tp = _alignment[rhs]; break;
                        case "$ss": _ss = _pointStyle[rhs]; break;
                        case "$es": _es = _pointStyle[rhs]; break;

                        default:
                            // user scalar or expression
                            UserVals[lhs] = ParseValue(rhs);
                            break;
                    }
                    break;
            }

        }

        /// <summary>
        /// Parse a single value - scalar or simple expression.
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
                var parts = Split(s, ops);

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

            Debug.WriteLine($"{s} >> {v}");

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
        public List<string> Split(string s, string delims) // TODO optimize
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
