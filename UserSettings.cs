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
using NBagOfTricks;


namespace NDraw
{
    [Serializable]
    public class UserSettings : IDisposable
    {
        #region Properties - editable
        /// <summary>Display font.</summary>
        [JsonConverter(typeof(JsonFontConverter))]
        public Font Font { get; set; } = new Font("Consolas", 10);

        /// <summary>Form color.</summary>
        [JsonConverter(typeof(JsonColorConverter))]
        public Color BackColor { get; set; } = Color.LightGray;

        /// <summary>Form color.</summary>
        [JsonConverter(typeof(JsonColorConverter))]
        public Color GridColor { get; set; } = Color.Gray;

        /// <summary>How fast the mouse wheel goes.</summary>
        public int WheelResolution { get; set; } = 8;
        #endregion

        #region Properties - internal
        /// <summary>Recent file list.</summary>
        [Browsable(false)]
        public List<string> RecentFiles { get; set; } = new List<string>();

        [Browsable(false)]
        [JsonConverter(typeof(JsonRectangleConverter))]
        public Rectangle FormGeometry { get; set; } = new Rectangle(50, 50, 600, 400);
        #endregion

        #region Fields
        /// <summary>The file name.</summary>
        string _fn = "";
        #endregion

        #region Lifecycle
        /// <summary>Constructor</summary>
        public UserSettings()
        {
        }

        /// <summary>Clean up.</summary>
        public void Dispose() => Font?.Dispose();
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

            return set;
        }

        /// <summary>Save object to file.</summary>
        public void Save()
        {
            JsonSerializerOptions opts = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, opts);
            File.WriteAllText(_fn, json);
        }
        #endregion
    }
}
