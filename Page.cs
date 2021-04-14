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
    public class Page
    {
        ///// <summary>Physical - inches</summary>
        //public float Width { get; set; } = 8.5f;

        ///// <summary>Physical - inches</summary>
        //public float Height { get; set; } = 11.0f;

        ///// <summary>Physical - inches</summary>
        //public float Border { get; set; } = 0.75f;

        /// <summary>In Units.</summary>
        public float Width { get; set; } = 100.0f;

        /// <summary>In Units.</summary>
        public float Height { get; set; } = 50.0f;

        /// <summary>In Units.</summary>
        public float Grid { get; set; } = 5.0f;

        /// <summary>In Units.</summary>
        public float Snap { get; set; } = 1.0f;

        /// <summary>Real world.</summary>
        public string Units { get; set; } = "feet";

        /// <summary>All the shapes</summary>
        public List<RectShape> Rects { get; set; } = new();

        /// <summary>All the shapes</summary>
        public List<LineShape> Lines { get; set; } = new();

        #region Fields
        /// <summary>The file name.</summary>
        string _fn = null;
        #endregion

        #region Persistence
        /// <summary>Save object to file.</summary>
        public void Save(string fn)// = "")
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(fn, json);
        }

        /// <summary>Create object from file.</summary>
        public static Page Load(string fn)
        {
            Page page = null;
            if (File.Exists(fn))
            {
                string json = File.ReadAllText(fn);
                page = JsonConvert.DeserializeObject<Page>(json);
                page._fn = fn;
            }
            return page;
        }
        #endregion
    }


    [Serializable]
    public class Style
    {
        /// <summary>xxxx</summary>
        public string Id { get; set; } = "";

        /// <summary>xxxx</summary>
        public float LineThickness { get; set; } = 1.0f;

        /// <summary>xxxx</summary>
        //public string LineColorName { get; set; } = "Blue";

        /// <summary>xxxx</summary>
        public string FillStyle { get; set; } = "None";

        /// <summary>xxxx</summary>
        public Font Font { get; set; } = null;// new Font("Consolas", 10);

        /// <summary>xxxx</summary>
        public Color LineColor = Color.Green;

        /// <summary>xxxx</summary>
        public Color FillColor = Color.Salmon;
    }
}
