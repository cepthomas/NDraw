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
        [JsonConverter(typeof(PointFConverter))]
        public PointF XXXPoint { get; set; } = new(123, 456);

        [JsonConverter(typeof(ColorConverter))]
        public Color XXXColor { get; set; } = Color.Red;
        [JsonConverter(typeof(FontConverter))]
        public Font XXXFont { get; set; } = new Font("Consolas", 20);


        #region Properties
        /// <summary>Display grid in Page units.</summary>
        public float Grid { get; set; } = 2;

        /// <summary>Display snap grid in Page units.</summary>
        public float Snap { get; set; } = 2;

        /// <summary>All the styles. The first one is considered the default.</summary>
        public List<Style> AllStyles { get; set; } = new();

        /// <summary>DOC</summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color BackColor { get; set; } = Color.White;

        /// <summary>DOC</summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color GridColor { get; set; } = Color.Black;

        /// <summary>DOC</summary>
        [Browsable(false)] // Persisted non-editable
        public List<string> RecentFiles { get; set; } = new List<string>();

        /// <summary>Form geometry</summary>
        [Browsable(false)]
        public int FormX { get; set; } = 50;

        /// <summary>Form geometry</summary>
        [Browsable(false)]
        public int FormY { get; set; } = 50;

        /// <summary>Form geometry</summary>
        [Browsable(false)]
        public int FormWidth { get; set; } = 1000;

        /// <summary>Form geometry</summary>
        [Browsable(false)]
        public int FormHeight { get; set; } = 700;
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

            // Make sure at least the default exists.
            if(set.AllStyles.Count == 0)
            {
                set.AllStyles.Add(new Style());
            }

            return set;
        }

        /// <summary>Save object to file.</summary>
        public void Save()
        {
            AllStyles.ForEach(s => s.Save());

            JsonSerializerOptions opts = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, opts);
            File.WriteAllText(_fn, json);
        }
        #endregion
    }
}
