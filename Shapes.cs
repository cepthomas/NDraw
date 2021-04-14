using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;



namespace NDraw
{
    [Serializable]
    public class Shape
    {
        /// <summary>xxxx</summary>
        public string Id { get; set; } = "";

        /// <summary>xxxx</summary>
        public string StyleId { get; set; } = "";

        /// <summary>xxxx</summary>
        public int Layer { get; set; } = 0;

        /// <summary>xxxx</summary>
        public string Text { get; set; } = ""; // Text position TODO

        public bool Selected { get; set; } = false;
    }

    [Serializable]
    public class RectShape : Shape
    {
        /// <summary>xxxx</summary>
        public RectX Extent { get; set; }

        public RectShape Translate(PointF pt, float z)
        {
            return new();
        }
    }

    [Serializable]
    public class LineShape : Shape
    {
        public PointX Start { get; set; }

        public PointX End { get; set; }

        //TODO end arrows etc, multi-segment lines

        public LineShape Translate(PointX pt, float z) //or float?
        {
            return new();
        }
    }
}
