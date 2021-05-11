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
        //// Key: sticky special names Value: current value as string.
        //Dictionary<string, object> _defs = new()
        //{
        //    //{ "id", "ID???" }, // identifier
        //    //{ "ly", 0 }, // layer
        //    //{ "sz", new SizeF(100, 100) }, // size
        //    //{ "lo", new PointF(10, 10) }, // location
        //    { "ht", 100 }, // height
        //    { "wd", 100 }, // width
        //    //{ "st", new PointF(20, 20) }, // start
        //    //{ "en", new PointF(30, 30) }, // end
        //    { "fc", Color.LightBlue }, // fill color
        //    { "lc", Color.Red }, // line color
        //    { "lt", 2.5 }, // line thickness
        //    //{ "tx", "NO TEXT" }, // text
        //    { "tp", ContentAlignment.MiddleCenter }, // text position
        //    { "ss", PointStyle.None }, // start style
        //    { "es", PointStyle.None }, // end style
        //};



        // Defaults this way.


        //SizeF _sz = new(100, 100);//, // size
        //PointF _lo = new(10, 10);// }, // location
        //float _ht = 100;// }, // height
        //float _wd = 100;// }, // width
        //PointF _st = new (20, 20);// }, // start
        //PointF _en = new (30, 30);// }, // end
        Color _fc = Color.LightBlue;// }, // fill color
        Color _lc = Color.Red;// }, // line color
        float _lt = 2.5f;// }, // line thickness
        ContentAlignment _tp = ContentAlignment.MiddleCenter;// }, // text position TL, TC, TR, CL, ...
        PointStyle _ss = PointStyle.None;// }, // line start style
        PointStyle _es = PointStyle.None;// }, // line end style



        // Key: User assigned value name Value: string, float, PointF
        Dictionary<string, object> _vals = new();


        PointStyle ParsePointStyle(string s)
        {
            //    public enum PointStyle { None, CircleHollow, CircleFilled, SquareHollow, SquareFilled, ArrowHollow, ArrowFilled };
            var ps = s switch
            {
                "NO" => PointStyle.None,
                "CH" => PointStyle.CircleHollow,
                "CF" => PointStyle.CircleFilled,
                "SH" => PointStyle.SquareHollow,
                "SF" => PointStyle.SquareFilled,
                "AH" => PointStyle.ArrowHollow,
                "AF" => PointStyle.ArrowFilled,
                _ => throw new ParseException($"Invalid point style: {s}"),
            };
            return ps;
        }


        ContentAlignment ParseAlignment(string s)
        {
            var cal = s switch
            {
                "TL" => ContentAlignment.TopLeft,
                "TC" => ContentAlignment.TopCenter,
                "TR" => ContentAlignment.TopRight,
                "CL" => ContentAlignment.MiddleLeft,
                "CC" => ContentAlignment.MiddleCenter,
                "CR" => ContentAlignment.MiddleRight,
                "BL" => ContentAlignment.BottomLeft,
                "BC" => ContentAlignment.BottomCenter,
                "BR" => ContentAlignment.BottomRight,
                _ => throw new ParseException($"Invalid alignment: {s}"),
            };
            return cal;
        }

        // Single value. TODO
        float ParseNumber(string s)
        {
            float v = float.NaN;

            // Try parse float.

            // Try parse expression.

            return v;
        }

        // x/y location. TODO
        PointF ParsePoint(string sx, string sy)
        {
            float x = float.NaN;
            float y = float.NaN;

            // Try parse float.

            // Try parse expression.

            return new PointF(x, y);
        }


        (string lhs, string rhs) SplitParam(string p)
        {
            var pp = p.SplitByToken("=");
            //TODO check for valid name?
            return pp.Count > 1 ? (pp[0], pp[1]) : (null, pp[0]);
        }

        void InitShapeCommon(Shape shape, Dictionary<string, string> elemParams)
        {
            // Common.
            shape.Layer = elemParams.ContainsKey("ly") ? int.Parse(elemParams["ly"]) : 0;
            shape.Text = elemParams.ContainsKey("tx") ? elemParams["tx"] : null;
            shape.LineThickness = elemParams.ContainsKey("lt") ? float.Parse(elemParams["lt"]) : _lt;
            shape.LineColor = elemParams.ContainsKey("lc") ? Color.FromName(elemParams["lc"]) : _lc;
            shape.FillColor = elemParams.ContainsKey("fc") ? Color.FromName(elemParams["fc"]) : _fc;
            shape.TextAlignment = elemParams.ContainsKey("tp") ? ParseAlignment(elemParams["tp"]) : _tp;
        }

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
                    switch(first.lhs)
                    {
                        case "page":
                            // page=pg_1, un=feet, wd=100, ht=50, gr=2
                            page.UnitsName = elemParams.ContainsKey("un") ? elemParams["un"] : "";
                            page.Width = elemParams.ContainsKey("wd") ? float.Parse(elemParams["wd"]) : 20.0f;
                            page.Height = elemParams.ContainsKey("ht") ? float.Parse(elemParams["ht"]) : 20.0f;
                            page.Grid = elemParams.ContainsKey("gr") ? float.Parse(elemParams["gr"]) : 1.0f;
                            break;

                        case "line":
                            // line=my_line1, ly=2, st=loc_1, en=my_rect1.2, lt=2, lc=red, tx=Hoohaa, tp=TL, es=CH, ss=AF
                            LineShape line = new() { Id = first.rhs };
                            InitShapeCommon(line, elemParams);
                            line.Start = ParsePoint(elemParams["st"]); // required
                            line.End = ParsePoint(elemParams["en"]); // required
                            line.StartStyle = elemParams.ContainsKey("ss") ? ParsePointStyle("ss") : _ss;
                            line.EndStyle = elemParams.ContainsKey("es") ? ParsePointStyle("es") : _es;

                            // Do sanity check on start and end. TODO

                            page.Lines.Add(line);
                            break;

                        case "rect":
                            // rect=my_rect1, ly=1, lo=loc_2, sz=size_1, lc=green, fc=lightgreen, tx=Nice day, tp=TL

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

                        case "val":
                            // one of:
                            //val=pabc, 23.5 // something
                            //val=sdog, "a big brown dog" // string
                            //val=loc_1, pabc-5, 84.12 // x,y
                            var pval = elemParams["1"];

                            switch (elemParams.Count)
                            {
                                case 1: // scalar: string or number
                                    _vals[first.rhs] = pval.Contains("\"") ? pval.Replace("\"", "") : ParseNumber(pval);
                                    break;

                                case 2: // x/y point
                                    _vals[first.rhs] = ParsePoint(pval, elemParams["2"]);
                                    break;

                                default:
                                    throw new ParseException($"Invalid param: {first.lhs}") { Row = row };
                            }

                            break;

                        default:
                            if(first.lhs.StartsWith("$"))
                            {
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
                                throw new ParseException($"Invalid param: {first.lhs}") { Row = row };

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
                    MessageBox.Show($"Fail: {ex}");
                }
            }

            return page;
        }
    }
}
