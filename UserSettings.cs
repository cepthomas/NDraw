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
        [DisplayName("Grid Color")]
        [Description("Grid Color")]
        [Browsable(true)]
        [JsonConverter(typeof(JsonColorConverter))]
        public Color GridColor { get; set; } = Color.Gray;

        [DisplayName("Wheel Resolution")]
        [Description("How fast the mouse wheel goes.")]
        [Browsable(true)]
        public int WheelResolution { get; set; } = 8;

        [DisplayName("Open Last File")]
        [Description("Open the last used file when starting.")]
        [Browsable(true)]
        public bool OpenLastFile { get; set; } = true;
        #endregion
    }
}
