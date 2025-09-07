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
using System.Drawing.Imaging;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;


namespace NDraw
{
    public partial class MainForm : Form
    {
        #region Fields
        /// <summary>Settings.</summary>
        readonly UserSettings _settings;

        /// <summary>Detect changed script files.</summary>
        readonly MultiFileWatcher _watcher = new();
        //readonly MultiFileWatcher _watcher = new();

        /// <summary>Current file name.</summary>
        string _fn = "";

        /// <summary>Cache layers.</summary>
        readonly List<ToolStripButton> _layerButtons = [];
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            Text = "NDraw - no file";

            // Receive key events before the event is passed to the control that has focus.
            KeyPreview = true;

            // Open settings.
            string appDir = MiscUtils.GetAppDataDir("NDraw", "Ephemera");
            _settings = (UserSettings)SettingsCore.Load(appDir, typeof(UserSettings));

            StartPosition = FormStartPosition.Manual;
            Location = new Point(_settings.FormGeometry.X, _settings.FormGeometry.Y);
            Size = new Size(_settings.FormGeometry.Width, _settings.FormGeometry.Height);

            PopulateRecentMenu();

            // Menu items.
            OpenMenuItem.Click += Open_Click;
            RecentMenuItem.Click += Recent_Click;
            RenderMenuItem.Click += Render_Click;
            SettingsMenuItem.Click += Settings_Click;
            AboutMenuItem.Click += About_Click;

            // Toolstrip buttons.
            BtnGrid.Checked = true;
            BtnRuler.Checked = true;
            BtnAllLayers.Checked = true;
            BtnGrid.Click += Btn_Click;
            BtnRuler.Click += Btn_Click;
            BtnAllLayers.Click += Btn_Click;

            // Other handlers.
            _watcher.FileChange += Watcher_Changed;

            // CL args?
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                DoFile(args[1]);
            }
            else if (_settings.OpenLastFile && _settings.RecentFiles.Count > 0)
            {
                DoFile(_settings.RecentFiles[0]);
            }
        }

        /// <summary>
        /// Shutting down.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _settings.FormGeometry = new Rectangle(Location.X, Location.Y, Size.Width, Size.Height);

            _settings.Save();
            base.OnFormClosing(e);
        }




        //protected override void OnResize(EventArgs e)
        //{
        //    MyCanvas.Invalidate();
        //    base.OnResize(e);
        //}
        #endregion

        #region Main work
        /// <summary>
        /// Open a file and process it.
        /// </summary>
        /// <param name="fn">File to open. Could be current.</param>
        void DoFile(string fn)
        {
            bool newFile = fn != _fn;

            if (newFile)
            {
                if (fn.EndsWith(".nd"))
                {
                    _fn = fn;
                    _watcher.Clear();
                    _watcher.Add(fn);
                    AddToRecentDefs(fn);
                    Text = $"NDraw - {fn}";
                }
                else
                {
                    Tell($"Invalid NDraw File: {fn}");
                    Text = "NDraw - no file";
                    return; // => early return
                }
            }

            try
            {
                Tell($"Parsing {_fn}");
                Parser p = new(fn);
                //dev p.Page.Save("xyz.json");

                if (p.Errors.Count == 0)
                {
                    DestroyLayerButtons();

                    MyCanvas.Init(p.Page, _settings);
                    if (newFile)
                    {
                        MyCanvas.Reset();
                    }

                    CreateLayerButtons(p);

                    MyCanvas.Invalidate();
                }
                else
                {
                    p.Errors.ForEach(e => Tell($"Error: {e}"));
                }
            }
            catch (Exception ex)
            {
                Tell($"Parse Fail: {ex}");
                _fn = null;
                Text = "NDraw - no file";
            }
        }

        /// <summary>
        /// Make buttons from user layers.
        /// </summary>
        /// <param name="p"></param>
        void CreateLayerButtons(Parser p)
        {
            foreach (var layer in p.Layers)
            {
                ToolStripButton btn = new()
                {
                    Text = layer,
                    Size = new Size(29, 24),
                    CheckOnClick = true,
                    DisplayStyle = ToolStripItemDisplayStyle.Text,
                    ImageTransparentColor = Color.Magenta,
                    Name = $"LAYER_{layer}",
                };

                // Init it. Turn all layers on.
                btn.Click += Btn_Click;
                btn.Checked = true;
                Btn_Click(btn, EventArgs.Empty);
                ToolStrip.Items.Add(btn);
                _layerButtons.Add(btn);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DestroyLayerButtons()
        {
            _layerButtons.ForEach(b => { ToolStrip.Items.Remove(b); b.Dispose(); });
            _layerButtons.Clear();
        }

        /// <summary>
        /// Toolstrip button click handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Btn_Click(object? sender, EventArgs e)
        {
            var btn = (ToolStripButton)sender!;

            switch (btn.Text)
            {
                case "Ruler":
                case "Grid":
                    MyCanvas.SetVisibility(BtnRuler.Checked, BtnGrid.Checked);
                    break;

                case "All Layers":
                    _layerButtons.ForEach(b => { MyCanvas.SetLayer(b.Text, BtnAllLayers.Checked); });
                    break;

                default:
                    MyCanvas.SetLayer(btn.Text, btn.Checked);
                    break;
            }
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
                DoFile(openDlg.FileName);
                //Parse(true);
            }
        }

        /// <summary>
        /// The user has asked to open a recent file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Recent_Click(object? sender, EventArgs e)
        {
            if (sender is not null)
            {
                string fn = sender.ToString()!;
                if (fn != "Recent")
                {
                    DoFile(fn);
                    //Parse(true);
                }
            }
        }

        /// <summary>
        /// File has changed so recompile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Watcher_Changed(object? sender, MultiFileWatcher.FileChangeEventArgs e)
        {
            // Kick over to main UI thread.
            this.InvokeIfRequired(_ =>
            {
                Tell($"File changed {_fn}");
                DoFile(_fn);
            });
        }

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
                _settings.UpdateMru(fn);
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
        /// Key handler.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.H:
                    MyCanvas.Reset();
                    MyCanvas.Invalidate();
                    e.Handled = true;
                    break;

                case Keys.C:
                    RtbLog.Clear();
                    e.Handled = true;
                    break;
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Make a picture.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Render_Click(object? sender, EventArgs e)
        {
            //var bmp = MyCanvas.Render(1600);
            //ShowBitmap(bmp);

            var fn = _fn == "" ? Path.GetTempFileName().Replace(".tmp", ".bmp") : _fn.Replace(".nd", ".bmp");
            //Filter = $"Audio Files|*.wav;*.mp3;*.m4a;*.flac",

            using SaveFileDialog saveDlg = new()
            {
                Title = "Export to image file",
                Filter = "(*.bmp)|*.bmp|(*.png)|*.png|(*.jpg)|*.jpg",
                FileName = fn
            };

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = MyCanvas.GenBitmap(1600);

                ImageFormat? fmt = saveDlg.FilterIndex switch
                {
                    1 => ImageFormat.Bmp,
                    2 => ImageFormat.Png,
                    3 => ImageFormat.Jpeg,
                    _ => null

                };

                if (fmt is not null)
                {
                    bmp.Save(saveDlg.FileName, fmt);
                }

                bmp.Dispose();
            }
        }

        /// <summary>
        /// Debug utility.
        /// </summary>
        /// <param name="bmp"></param>
        void ShowBitmap(Bitmap bmp)
        {
            Size sz = new(bmp.Width / 2, bmp.Height / 2);

            using var bmpr = bmp.Resize(sz.Width, sz.Height);

            PictureBox pic = new()
            {
                Dock = DockStyle.Fill,
                Image = bmpr,
            };

            using Form f = new()
            {
                Location = Cursor.Position,
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                ShowIcon = false,
                ShowInTaskbar = false
            };
            f.ClientSize = sz; // do after construction
            f.Text = $"Original:{bmp.Width}X{bmp.Height} Window:{Width}X{Height}";

            f.Controls.Add(pic);

            f.ShowDialog();
        }

        /// <summary>
        /// Tell the user something.
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
            Tools.ShowReadme("NDraw");
        }
        #endregion
    }
}
