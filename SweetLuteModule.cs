using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SweetLute.Controls;
using SweetLute.Notation.Persistance;
using SweetLute.Player;
using SweetLute.Controls.Instrument;
using static Blish_HUD.GameService;
using System.Diagnostics;

namespace SweetLute
{
    [Export(typeof(Module))]
    public class SweetLuteModule : Module
    {
        internal static SweetLuteModule ModuleInstance;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        #region Textures

        private Texture2D CORNER_ICON;
        private Texture2D WINDOW_ICON;

        #endregion

        #region Constants

        private const int TOP_MARGIN = 25;
        private const int RIGHT_MARGIN = 5;
        private const int BOTTOM_MARGIN = 15;
        private const int LEFT_MARGIN = 8;
        private const string DD_TITLE = "Title";
        private const string DD_HARP = "Harp";
        private const string DD_FLUTE = "Flute";
        private const string DD_LUTE = "Lute";
        private const string DD_HORN = "Horn";
        private const string DD_BASS = "Bass";
        private const string DD_BELL = "Bell";
        private const string DD_BELL2 = "Bell2";

        #endregion

        #region Controls

        private HealthPoolButton _stopButton;
        private List<SheetButton> _displayedSheets;
        private CornerIcon _cornerIcon;
        private StandardWindow _window;
        private Texture2D _windowBackgroundTexture;
        private StandardButton _openSongsFolder;

        #endregion

        private XmlMusicSheetReader _xmlParser;

        private List<RawMusicSheet> _rawMusicSheets;

        internal MusicPlayer MusicPlayer { get; private set; }

        [ImportingConstructor]
        public SweetLuteModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { ModuleInstance = this; }

        #region Settings

        private SettingEntry<bool> settingBackgroundPlayback;

        protected override void DefineSettings(SettingCollection settingsManager)
        {
            settingBackgroundPlayback = settingsManager.DefineSetting("backgroundPlayback", false, "No background playback", "Stop key emulation when GW2 is in the background");
        }

        #endregion

        protected override void Initialize()
        {
            CORNER_ICON = CORNER_ICON ?? ContentsManager.GetTexture("musician_icon.png");
            WINDOW_ICON = WINDOW_ICON ?? ContentsManager.GetTexture("sweet_icon.png");
            _xmlParser = new XmlMusicSheetReader();
            _displayedSheets = new List<SheetButton>();
            _stopButton = new HealthPoolButton()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Text = "Stop Playback",
                ZIndex = -1,
                Visible = false
            };
                
            _stopButton.Click += StopPlayback;

            GameIntegration.Gw2LostFocus += OnGw2LostFocus;
        }

        protected override async Task LoadAsync()
        {
            _windowBackgroundTexture = ContentsManager.GetTexture("155985.png");

            _window = new StandardWindow(
                _windowBackgroundTexture,
                new Rectangle(40, 26, 413, 691),
                new Rectangle(70, 41, 370, 640))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Sweet Lute",
                Emblem = WINDOW_ICON,
                Subtitle = "Yeeeehaw",
                Location = new Point(300, 300),
                SavesPosition = true,
                Id = $"{nameof(SweetLuteModule)}_My_Unique_ID_123"
            };

            await Task.Run(() => _rawMusicSheets = _xmlParser.LoadDirectory(DirectoriesManager.GetFullDirectoryPath("songs")));
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            _cornerIcon = new CornerIcon()
            {
                Icon = CORNER_ICON,
                BasicTooltipText = $"{Name}",
                Parent = GameService.Graphics.SpriteScreen
                
            };
            _window.AddChild(BuildLibraryPanel(_window));

            _cornerIcon.RightMouseButtonPressed += delegate
            {
                ShowContextMenu();
            };
            _cornerIcon.Click += delegate
            {
                _window.Show();    
            };

            base.OnModuleLoaded(e);
        }

        private void OnGw2LostFocus(object sender, EventArgs e)
        {
            if (!settingBackgroundPlayback.Value) return;
            StopPlayback(null, null);
        }

        protected override void Unload()
        {
            MusicPlayerFactory.Dispose();
            _stopButton?.Dispose();
            StopPlayback(null, null);
            ModuleInstance = null;
        }

        private void StopPlayback(object o, MouseEventArgs e)
        {
            if (_stopButton != null)
                _stopButton.Visible = false;
            MusicPlayer?.Dispose();
        }
        private void ShowContextMenu()
        {
            var pathingContextMenuStrip = new ContextMenuStrip();
            var reloadButton = new ContextMenuStripItem()
            {
                Text = "Reload songs"
            };

            reloadButton.Click += delegate
            {
                _rawMusicSheets.Clear();
                _displayedSheets.Clear();
                _rawMusicSheets = _xmlParser.LoadDirectory(DirectoriesManager.GetFullDirectoryPath("songs"));
                _window.ClearChildren();
                _window.AddChild(BuildLibraryPanel(_window));
            };

            pathingContextMenuStrip.AddMenuItem(reloadButton);
            pathingContextMenuStrip.Show(_cornerIcon);
        }

        public void TurnStopButtonInvisible()
        {
            if (_stopButton != null)
                _stopButton.Visible = false;
        }

        private Panel BuildLibraryPanel(WindowBase2 wndw)
        {
            var lPanel = new Panel()
            {
                CanScroll = false,
                Size = wndw.ContentRegion.Size,
                Parent = wndw
            };
            _openSongsFolder = new StandardButton()
            {
                Parent = lPanel,
                Text = "Open Songs Folder",
                Size = new Point(150, 25)
            };
            _openSongsFolder.Click += delegate
            {
                Process.Start("explorer.exe", DirectoriesManager.GetFullDirectoryPath("songs"));
            };

            var melodyPanel = new Panel()
            {
                Location = new Point(0, TOP_MARGIN),
                Size = new Point(lPanel.Width - RIGHT_MARGIN, lPanel.Size.Y - BOTTOM_MARGIN),
                Parent = lPanel,
                ShowTint = true,
                ShowBorder = true,
                CanScroll = true
            };

            foreach (RawMusicSheet sheet in _rawMusicSheets)
            {
                var melody = new SheetButton
                {
                    Parent = melodyPanel,
                    Icon = ContentsManager.GetTexture(@"instruments\" + sheet.Instrument.ToLowerInvariant() + ".png"),
                    Artist = sheet.Artist,
                    Title = sheet.Title,
                    User = sheet.User,
                    MusicSheet = sheet
                };
                _displayedSheets.Add(melody);
                melody.LeftMouseButtonPressed += delegate
                {
                    if (melody.MouseOverPlay)
                    {
                        StopPlayback(null, null);

                        Overlay.BlishHudWindow.Hide();

                        ScreenNotification.ShowNotification("Now Playing " + melody.Title);

                        MusicPlayer = MusicPlayerFactory.Create(
                            melody.MusicSheet,
                            InstrumentMode.Emulate
                        );

                        MusicPlayer.Worker.Start();
                        _stopButton.Visible = true;
                    }
                };
            }
            var ddSortMethod = new Dropdown()
            {
                Parent = lPanel,
                Visible = melodyPanel.Visible,
                Location = new Point(lPanel.Right - 150 - 3 - RIGHT_MARGIN, 0),
                Width = 150
            };
            ddSortMethod.Items.Add(DD_TITLE);
            ddSortMethod.Items.Add("------------------");
            ddSortMethod.Items.Add(DD_HARP);
            ddSortMethod.Items.Add(DD_FLUTE);
            ddSortMethod.Items.Add(DD_LUTE);
            ddSortMethod.Items.Add(DD_HORN);
            ddSortMethod.Items.Add(DD_BASS);
            ddSortMethod.Items.Add(DD_BELL);
            ddSortMethod.Items.Add(DD_BELL2);
            ddSortMethod.ValueChanged += UpdateSort;
            ddSortMethod.SelectedItem = DD_TITLE;

            UpdateSort(ddSortMethod, EventArgs.Empty);
            return lPanel;
        }

        private void UpdateSort(object sender, EventArgs e)
        {
            switch (((Dropdown)sender).SelectedItem)
            {
                case DD_TITLE:
                    _displayedSheets.Sort((e1, e2) => e1.Title.CompareTo(e2.Title));
                    foreach (SheetButton e1 in _displayedSheets)
                        e1.Visible = true;
                    break;
                case DD_HARP:
                    _displayedSheets.Sort((e1, e2) => e1.MusicSheet.Instrument.CompareTo(e2.MusicSheet.Instrument));
                    foreach (SheetButton e1 in _displayedSheets)
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_HARP, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case DD_FLUTE:
                    foreach (SheetButton e1 in _displayedSheets)
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_FLUTE, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case DD_LUTE:
                    foreach (SheetButton e1 in _displayedSheets)
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_LUTE, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case DD_HORN:
                    foreach (SheetButton e1 in _displayedSheets)
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_HORN, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case DD_BASS:
                    foreach (SheetButton e1 in _displayedSheets)
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_BASS, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case DD_BELL:
                    foreach (SheetButton e1 in _displayedSheets)

                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_BELL, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case DD_BELL2:
                    foreach (SheetButton e1 in _displayedSheets)
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_BELL2, StringComparison.InvariantCultureIgnoreCase);
                    break;
            }

            RepositionMel();
        }

        private void RepositionMel()
        {
            int pos = 0;
            foreach (var mel in _displayedSheets)
            {
                int y = pos;
                mel.Location = new Point(0, y * 52);

                ((Panel)mel.Parent).VerticalScrollOffset = 0;
                mel.Parent.Invalidate();
                if (mel.Visible) pos++;
            }
        }
    }
}
