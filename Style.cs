using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace NDraw
{
    [Serializable]
    public class Style : IDisposable
    {
        /// <summary>DOC</summary>
        [Browsable(false)]
        public string Id { get; set; } = "";

        /// <summary>DOC</summary>
        public float LineThickness { get; set; } = 1.0f;

        /// <summary>DOC</summary>
        [Browsable(false)]
        public string LineColorName { get; set; } = "Green";

        /// <summary>DOC</summary>
        [Browsable(false)]
        public string FillColorName { get; set; } = "Salmon";

        /// <summary>DOC</summary>
        public string FillStyle { get; set; } = "None";


        /// <summary>DOC</summary>
        [Browsable(false)]
        public string FontName { get; set; } = "Calibri";

        /// <summary>DOC</summary>
        [Browsable(false)]
        public float EmSize { get; set; } = 10;

        // TODO style  enum FontStyle { Regular = 0, Bold = 1, Italic = 2, Underline = 4, Strikeout = 8 }

        /// <summary>DOC</summary>
        [JsonIgnore]
        public Color LineColor { get; set; } = Color.Black;

        /// <summary>DOC</summary>
        [JsonIgnore]
        public Color FillColor { get; set; } = Color.Black;

        /// <summary>DOC</summary>
        [JsonIgnore]
        public Font Font { get; set; } = null;// new Font("Consolas", 10);

        /// <summary>DOC</summary>
        [JsonIgnore]
        public Pen LinePen { get; set; } = new Pen(Color.Black);

        /// <summary>DOC</summary>
        [JsonIgnore]
        public SolidBrush FillBrush { get; set; } = new SolidBrush(Color.Black);

        public Style()
        {
            Font = new Font(FontName, EmSize);

            LineColor = Color.FromName(LineColorName);
            FillColor = Color.FromName(FillColorName);

            LinePen.Color = LineColor;
            FillBrush.Color = FillColor;
        }

        public void Save()
        {
            // Fixups.
            LineColorName = LineColor.Name;
            FillColorName = FillColor.Name;

            FontName = Font.Name;
            EmSize = Font.SizeInPoints;
        }

        public void Dispose()
        {
            Font?.Dispose();
            LinePen?.Dispose();
            FillBrush?.Dispose();
        }
    }
}
