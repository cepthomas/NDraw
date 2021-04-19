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
        [DisplayName("Default Font"), Description("The font to use if not specified in a style."), Browsable(true)]
        public Font DefaultFont { get; set; } = new Font("Consolas", 10);

        //public string BackColorName { get; set; } = "Pink";
        //public string GridColorName { get; set; } = "Red";

        /// <summary>Virtual - grid lines</summary>
        public float Grid { get; set; } = 2;

        /// <summary>Virtual - snap resolution</summary>
        public float Snap { get; set; } = 2;

        /// <summary>All the styles</summary>
        public List<Style> AllStyles { get; set; } = new();

        //[JsonIgnore]
        public Color BackColor { get; set; } = Color.LightGray;

        //[JsonIgnore]
        public Color GridColor { get; set; } = Color.Gray;

        //[DisplayName("Background Color"), Description("The color used for overall background."), Browsable(true)]
        //public Color BackColor { get; set; } = Color.AliceBlue;

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

        /// <summary>Current global user settings.</summary>
        public static UserSettings TheSettings { get; set; } = new UserSettings();

        #region Persistence
        /// <summary>Save object to file.</summary>
        public void Save()
        {
            JsonSerializerOptions opts = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, opts);
            File.WriteAllText(_fn, json);
        }

        /// <summary>Create object from file.</summary>
        public static void Load(string appDir)
        {
            TheSettings = null;
            string fn = Path.Combine(appDir, "settings.json");

            if(File.Exists(fn))
            {
                string json = File.ReadAllText(fn);
                TheSettings = JsonSerializer.Deserialize<UserSettings>(json);

                // Clean up any bad file names.
                TheSettings.RecentFiles.RemoveAll(f => !File.Exists(f));

                TheSettings._fn = fn;
            }
            else
            {
                // Doesn't exist, create a new one.
                TheSettings = new UserSettings
                {
                    _fn = fn
                };
            }
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
