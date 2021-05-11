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
    public class ParseException : Exception
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public ParseException(string msg) : base(msg) { }
    }


    public class Parser
    {
        #region Defaults
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

        /// <summary>Key: User assigned value name Value: float</summary>
        Dictionary<string, float> _vals = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        public Page ParseFile(string fn)
        {
            Page page = new();

            int row = 0;

            foreach (string sf in File.ReadAllLines(fn))
            {
                row++;

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
                            page.Width = elemParams.ContainsKey("w") ? float.Parse(elemParams["w"]) : 20.0f;
                            page.Height = elemParams.ContainsKey("h") ? float.Parse(elemParams["h"]) : 20.0f;
                            page.Grid = elemParams.ContainsKey("gr") ? float.Parse(elemParams["gr"]) : 1.0f;
                            break;

                        case "line":
                            // my_line1=line, lr=2, sx=loc_1_x, sy=loc_1_y, ex=my_rect2_x, ey=my_rect3_y, lt=2, tx=Hoohaa, tp=TL, es=CH, ss=AF
                            LineShape line = new() { Id = first.lhs };
                            InitShapeCommon(line, elemParams);
                            line.Start = new PointF(ParseNumber(elemParams["sx"]), ParseNumber(elemParams["sy"])); // required
                            line.End = new PointF(ParseNumber(elemParams["sx"]), ParseNumber(elemParams["sy"])); // required
                            line.StartStyle = elemParams.ContainsKey("ss") ? _pointStyle[elemParams["ss"]] : _ss;
                            line.EndStyle = elemParams.ContainsKey("es") ? _pointStyle[elemParams["es"]] : _es;

                            // Do sanity check on start and end. TODO

                            page.Lines.Add(line);
                            break;

                        case "rect":
                            // my_rect1=rect, lr=1, x=loc_2_x, y=loc_2_y, w=size_1_w, h=size_1_h, lc=green, fc=lightgreen, tx=Nice day, tp=TL

                            // Init some defaults from _defs.

                            // public ContentAlignment Alignment { get; set; } = ContentAlignment.TopLeft;
                            // public float LineThickness { get; set; } = 2.0f;
                            // public Color LineColor { get; set; } = Color.Green;
                            // public Color FillColor { get; set; } = Color.Black;

                            // switch the values

                            // sanity check:
                            // public PointF TL { get; set; } = new(0, 0);
                            // public PointF BR { get; set; } = new(0, 0);

                            break;

                        default:
                            if(first.lhs.StartsWith("$"))
                            {
                                // special
                                //$lt=4
                                //$lc=salmon

                                //Color _fc = Color.LightBlue;// }, // fill color
                                //Color _lc = Color.Red;// }, // line color
                                //float _lt = 2.5f;// }, // line thickness
                                //ContentAlignment _tp = ContentAlignment.MiddleCenter;// }, // text position TL, TC, TR, CL, ...
                                //PointStyle _ss = PointStyle.None;// }, // line start style
                                //PointStyle _es = PointStyle.None;// }, // line end style

                            }
                            else
                            {
                                // user scalar
                                _vals[first.lhs] = ParseNumber(first.rhs);
                                // _vals[first.lhs] = pval.Contains("\"") ? pval.Replace("\"", "") : ParseNumber(pval);
                            }
                            break;
                    }
                }
                catch (ParseException ex)
                {
                    // May be transient. Do something though?
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Some other exception: {ex}");
                }
            }

            return page;
        }

        /// <summary>
        /// Parse a single number - scalar or simple expression. TODO
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        float ParseNumber(string s)
        {
            float v = float.NaN;

            // Try parse float.

            // Try parse expression.

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
            //TODO check for valid name?
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
    }
}
