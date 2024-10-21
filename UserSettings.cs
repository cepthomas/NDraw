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
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;


namespace NDraw
{
    [Serializable]
    public sealed class UserSettings : SettingsCore
    {
        #region Properties - editable
        /// <summary>Form color.</summary>
        [JsonConverter(typeof(JsonColorConverter))]
        public Color BackColor { get; set; } = Color.LightGray;

        /// <summary>Form color.</summary>
        [JsonConverter(typeof(JsonColorConverter))]
        public Color GridColor { get; set; } = Color.Gray;

        /// <summary>How fast the mouse wheel goes.</summary>
        public int WheelResolution { get; set; } = 8;

        /// <summary>Open the last used file when starting.</summary>
        public bool OpenLastFile { get; set; } = true;
        #endregion
    }
}
