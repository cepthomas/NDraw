using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace NDraw
{
    [Serializable]
    public class UserSettings : IDisposable
    {
        #region Persisted editable properties

        //public string BackColorName { get; set; } = "Pink";
        //public string GridColorName { get; set; } = "Red";

        /// <summary>Virtual - grid lines</summary>
        public float Grid { get; set; } = 2;

        /// <summary>Virtual - snap resolution</summary>
        public float Snap { get; set; } = 2;

        /// <summary>All the styles. The first one is considered the default.</summary>
        public List<Style> AllStyles { get; set; } = new();


        [Browsable(false)]
        public string BackColorName { get; set; } = "LightGray";

        [JsonIgnore]
        public Color BackColor { get; set; } = Color.White;

        [Browsable(false)]
        public string GridColorName { get; set; } = "Gray";

        [JsonIgnore]
        public Color GridColor { get; set; } = Color.Black;

        #endregion

        #region Persisted non-editable properties
        [Browsable(false)]
        public FormInfo MainFormInfo { get; set; } = new FormInfo();

        [Browsable(false)]
        public List<string> RecentFiles { get; set; } = new List<string>();
        #endregion

        #region Classes
        /// <summary>
        /// General purpose container for persistence.
        /// </summary>
        [Serializable]
        public class FormInfo
        {
            public int X { get; set; } = 50;
            public int Y { get; set; } = 50;
            public int Width { get; set; } = 1000;
            public int Height { get; set; } = 700;

            public void FromForm(Form f)
            {
                X = f.Location.X;
                Y = f.Location.Y;
                Width = f.Width;
                Height = f.Height;
            }
        }
        #endregion

        #region Fields
        /// <summary>The file name.</summary>
        string _fn = "";
        #endregion

        #region Persistence
        /// <summary>Create object from file.</summary>
        public static UserSettings Load(string appDir)
        {
            UserSettings set = null;
            string fn = Path.Combine(appDir, "settings.json");

            if (File.Exists(fn))
            {
                string json = File.ReadAllText(fn);
                set = JsonSerializer.Deserialize<UserSettings>(json);

                // Clean up any bad file names.
                set.RecentFiles.RemoveAll(f => !File.Exists(f));

                set._fn = fn;
            }
            else
            {
                // Doesn't exist, create a new one.
                set = new UserSettings
                {
                    _fn = fn
                };
            }

            // Sanity check.
            if(set.AllStyles.Count == 0)
            {
                set.AllStyles.Add(new Style());
            }

            // Fixups.
            set.BackColor = Color.FromName(set.BackColorName);
            set.GridColor = Color.FromName(set.GridColorName);

            return set;
        }

        /// <summary>Save object to file.</summary>
        public void Save()
        {
            // Fixups.
            BackColorName = BackColor.Name;
            GridColorName = GridColor.Name;

            AllStyles.ForEach(s => s.Save());

            JsonSerializerOptions opts = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, opts);
            File.WriteAllText(_fn, json);
        }

        /// <summary>Clean up.</summary>
        public void Dispose()
        {
            foreach(Style style in AllStyles)
            {
                style.Dispose();
            }
        }
        #endregion
    }
}
