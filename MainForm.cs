using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using NBagOfTricks;
using NBagOfUis;


namespace NDraw
{
    public partial class MainForm : Form
    {
        #region Fields
        /// <summary>Settings.</summary>
        readonly UserSettings _settings;

        /// <summary>Detect changed script files.</summary>
        readonly MultiFileWatcher _watcher = new();

        /// <summary>Current file name.</summary>
        string _fn = "";
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            //this.OnKeyDown(KeyEventArgs)

            // Receive key events before the event is passed to the control that has focus.
            KeyPreview = true;

            // Open settings.
            string appDir = MiscUtils.GetAppDataDir("NDraw", "Ephemera");
            _settings = (UserSettings)SettingsCore.Load(appDir, typeof(UserSettings));

            Location = new Point(_settings.FormGeometry.X, _settings.FormGeometry.Y);
            Size = new Size(_settings.FormGeometry.Width, _settings.FormGeometry.Height);

            PopulateRecentMenu();

            // Hook events.
            Canvas.InfoEvent += (_, msg) => TxtInfo.Text = msg;

            OpenMenuItem.Click += Open_Click;
            RecentMenuItem.Click += Recent_Click;
            RenderMenuItem.Click += Render_Click;
            SettingsMenuItem.Click += Settings_Click;
            AboutMenuItem.Click += About_Click;

            foreach (var btn in new List<ToolStripButton>() { BtnLayer1, BtnLayer2, BtnLayer3, BtnLayer4, BtnRuler, BtnGrid })
            {
                btn.Click += Btn_Click;
                btn.Checked = true;
                Btn_Click(btn, EventArgs.Empty);
            }

            _watcher.FileChangeEvent += Watcher_Changed;

            var args = Environment.GetCommandLineArgs();
            if(args.Length >= 2)
            {
                OpenFile(args[1]);
                Parse();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _settings.FormGeometry = new Rectangle(Location.X, Location.Y, Size.Width, Size.Height);

            _settings.Save();
            _settings.Dispose();
            base.OnFormClosing(e);
        }
        #endregion

        #region File handling
        /// <summary>
        /// Allows the user to select a file from file system.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Open_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog openDlg = new()
            {
                Filter = "NDraw files (*.nd)|*.nd",
                Title = "Select a NDraw file"
            };

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                OpenFile(openDlg.FileName);
                Parse();
            }
        }

        /// <summary>
        /// The user has asked to open a recent file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Recent_Click(object? sender, EventArgs e)
        {
            if(sender is not null)
            {
                string fn = sender.ToString()!;
                if (fn != "Recent")
                {
                    OpenFile(fn);
                    Parse();
                }
            }
        }

        /// <summary>
        /// Open a file and process it.
        /// </summary>
        /// <param name="fn"></param>
        void OpenFile(string fn)
        {
            if(fn.EndsWith(".nd"))
            {
                try
                {
                    _fn = fn;

                    // Update file watcher.
                    _watcher.Clear();
                    _watcher.Add(fn);

                    AddToRecentDefs(fn);
                }
                catch (Exception ex)
                {
                    Tell($"Open Fail: {ex.Message}");
                }
            }
            else
            {
                Tell($"Invalid NDraw File: {fn}");
            }
        }

        /// <summary>
        /// Parse current file.
        /// </summary>
        void Parse()
        {
            RtbLog.Clear();

            try
            {
                Parser p = new();
                p.ParseFile(_fn);
                //p.Page.Save("xyz.json");

                if (p.Errors.Count == 0)
                {
                    Canvas.Init(p.Page, _settings);
                }
                else
                {
                    foreach (var err in p.Errors)
                    {
                        Tell($"Err: {err}");
                    }
                }
            }
            catch (Exception ex)
            {
                Tell($"Parse Fail: {ex}");
            }
        }
        #endregion

        #region Recent files
        /// <summary>
        /// Create the menu with the recently used files.
        /// </summary>
        void PopulateRecentMenu()
        {
            ToolStripItemCollection menuItems = RecentMenuItem.DropDownItems;
            menuItems.Clear();

            _settings.RecentFiles.ForEach(f =>
            {
                ToolStripMenuItem menuItem = new(f, null, new EventHandler(Recent_Click));
                menuItems.Add(menuItem);
            });
        }

        /// <summary>
        /// Update the mru with the user selection.
        /// </summary>
        /// <param name="fn">The selected file.</param>
        void AddToRecentDefs(string fn)
        {
            if (fn.EndsWith(".nd") && File.Exists(fn))
            {
                _settings.RecentFiles.UpdateMru(fn);
                PopulateRecentMenu();
            }
        }
        #endregion

        #region Settings
        /// <summary>
        /// Edit the common options in a property grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Settings_Click(object? sender, EventArgs e)
        {
            var changes = SettingsEditor.Edit(_settings, "User Settings", 400);

            if (changes.Count > 0)
            {
                MessageBox.Show("Restart required for device changes to take effect");
            }

            _settings.Save();
        }
        #endregion

        #region Misc
        /// <summary>
        /// One or more files have changed so recompile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Watcher_Changed(object? sender, MultiFileWatcher.FileChangeEventArgs e)
        {
            // Kick over to main UI thread.
            this.InvokeIfRequired(_ => { Parse(); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.H:
                    Canvas.Reset();
                    break;
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Btn_Click(object? sender, EventArgs e)
        {
            if (sender is not null)
            {
                var b = sender as ToolStripButton;

                switch (b!.Text)
                {
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                        int n = int.Parse(b.Text);
                        Canvas.SetLayer(n - 1, b.Checked);
                        break;

                    case "Ruler":
                    case "Grid":
                        Canvas.SetVisibility(BtnRuler.Checked, BtnGrid.Checked);
                        break;
                }
            }
        }

        /// <summary>
        /// Make a picture.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Render_Click(object? sender, EventArgs e)
        {
            if(_fn != "")
            {
                Bitmap bmp = new(Canvas.Width, Canvas.Height);
                Canvas.DrawToBitmap(bmp, new Rectangle(0, 0, Canvas.Width, Canvas.Height));
                bmp.Save(_fn.Replace(".nd", ".png"), System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        void Tell(string s)
        {
            RtbLog.AppendText(s);
            RtbLog.AppendText(Environment.NewLine);
            RtbLog.ScrollToCaret();
        }

        /// <summary>
        /// All about me.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void About_Click(object? sender, EventArgs e)
        {
            MiscUtils.ShowReadme("NDraw");
        }
        #endregion
    }
}
