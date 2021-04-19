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
        /// <summary>xxxx</summary>
        [Browsable(false)]
        public string Id { get; set; } = "";

        /// <summary>xxxx</summary>
        public float LineThickness { get; set; } = 1.0f;

        /// <summary>xxxx</summary>
        [Browsable(false)]
        public string LineColorName { get; set; } = "Green";

        /// <summary>xxxx</summary>
        [Browsable(false)]
        public string FillColorName { get; set; } = "Salmon";

        /// <summary>xxxx</summary>
        public string FillStyle { get; set; } = "None";


        [Browsable(false)]
        public string FontName { get; set; } = "Calibri";

        [Browsable(false)]
        public float EmSize { get; set; } = 10;

        // TODO style  enum FontStyle { Regular = 0, Bold = 1, Italic = 2, Underline = 4, Strikeout = 8 }

        /// <summary>xxxx</summary>
        [JsonIgnore]
        public Color LineColor { get; set; } = Color.Black;

        /// <summary>xxxx</summary>
        [JsonIgnore]
        public Color FillColor { get; set; } = Color.Black;


        [JsonIgnore]
        public Font Font { get; set; } = null;// new Font("Consolas", 10);

        [JsonIgnore]
        public Pen LinePen { get; set; } = new Pen(Color.Black);

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

        public void Save() //TODO make persistable
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
