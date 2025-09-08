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
        #region Properties
        /// <summary>Width in virtual units.</summary>
        public float Width { get; set; } = 100.0f;

        /// <summary>Height  virtual units.</summary>
        public float Height { get; set; } = 100.0f;

        /// <summary>Grid spacing in virtual units.</summary>
        public float Grid { get; set; } = 1.0f;

        /// <summary>Real world.</summary>
        public string UnitsName { get; set; } = "feet";

        /// <summary>All the shapes.</summary>
        public List<Shape> Shapes { get; set; } = [];
        #endregion

        #region Fields
        /// <summary>The file name.</summary>
        string _fn = "";
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
        public static Page? Load(string fn)
        {
            Page? page = null;
            if (File.Exists(fn))
            {
                string json = File.ReadAllText(fn);
                page = JsonSerializer.Deserialize<Page>(json);
                if(page is not null)
                {
                    page._fn = fn;
                }
            }
            return page;
        }
        #endregion

    }
}
