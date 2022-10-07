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
using NBagOfUis;


namespace NDraw
{
    [Serializable]
    public sealed class UserSettings : SettingsCore, IDisposable
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

        #region Lifecycle
        /// <summary>Clean up.</summary>
        public void Dispose()
        {
            Font.Dispose();
        }
        #endregion
    }
}
