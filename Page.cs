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
    public class Page
    {
        ///// <summary>Physical - inches</summary>
        //public float Width { get; set; } = 8.5f;

        ///// <summary>Physical - inches</summary>
        //public float Height { get; set; } = 11.0f;

        ///// <summary>Physical - inches</summary>
        //public float Border { get; set; } = 0.75f;

        /// <summary>Drawing area in virtual units.</summary>
        public float Width { get; set; } = 100.0f;

        /// <summary>Drawing area in virtual units.</summary>
        public float Height { get; set; } = 50.0f;

        /// <summary>In virtual units.</summary>
        public float Grid { get; set; } = 5.0f;

        /// <summary>In virtual units.</summary>
        public float Snap { get; set; } = 1.0f;

        /// <summary>Real world.</summary>
        public string UnitsName { get; set; } = "feet";

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
        public void Save(string fn)
        {
            JsonSerializerOptions opts = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, opts);
            File.WriteAllText(fn, json);
        }

        /// <summary>Create object from file.</summary>
        public static Page Load(string fn)
        {
            Page page = null;
            if (File.Exists(fn))
            {
                string json = File.ReadAllText(fn);
                page = JsonSerializer.Deserialize<Page>(json);
                page._fn = fn;
            }
            return page;
        }
        #endregion
    }
}
