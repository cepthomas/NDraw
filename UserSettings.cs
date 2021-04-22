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
        #region Properties
        /// <summary>Display grid in Page units.</summary>
        public float Grid { get; set; } = 2;

        /// <summary>Display snap grid in Page units.</summary>
        public float Snap { get; set; } = 2;

        /// <summary>All the styles. The first one is considered the default.</summary>
        public List<Style> AllStyles { get; set; } = new();

        /// <summary>DOC</summary>
        [Browsable(false)] // Persisted non-editable
        public string BackColorName { get; set; } = "LightGray";

        /// <summary>DOC</summary>
        [JsonIgnore] // Editable non-persisted
        public Color BackColor { get; set; } = Color.White;

        /// <summary>DOC</summary>
        [Browsable(false)] // Persisted non-editable
        public string GridColorName { get; set; } = "Gray";

        /// <summary>DOC</summary>
        [JsonIgnore] // Editable non-persisted
        public Color GridColor { get; set; } = Color.Black;

        /// <summary>DOC</summary>
        [Browsable(false)] // Persisted non-editable
        public FormInfo MainFormInfo { get; set; } = new FormInfo();

        /// <summary>DOC</summary>
        [Browsable(false)] // Persisted non-editable
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

        #region Lifecycle
        /// <summary>Constructor</summary>
        public UserSettings()
        {
            if (AllStyles.Count == 0)
            {
                AllStyles.Add(new Style());
            }
        }

        /// <summary>Clean up.</summary>
        public void Dispose() => AllStyles.ForEach(s => s.Dispose());
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

            // Make sure at least the default.
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
        #endregion
    }
}
