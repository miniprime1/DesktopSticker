using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace DesktopSticker {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private System.Windows.Forms.MenuItem _menuAlwaysOnTop;
        private System.Windows.Forms.MenuItem _menuShowInTaskbar;
        private System.Windows.Forms.MenuItem _menuHideWindow;
        private System.Windows.Forms.MenuItem _menuFixPosition;
        private System.Windows.Forms.MenuItem _menuClickThrough;

        private bool _isFixed = false;
        private bool _isPassthru = false;

        private DispatcherTimer _topmostTimer;

        public MainWindow() {
            InitializeComponent();

            this.MouseDown += MainWindow_MouseDown;
            this.SizeChanged += MainWindow_SizeChanged;
            this.LocationChanged += MainWindow_LocationChanged;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            this.ShowInTaskbar = Properties.Settings.Default.SavedShowInTaskbar;
            this.Topmost = Properties.Settings.Default.SavedAlwaysOnTop;
            if (Properties.Settings.Default.SavedHideWindow == true) this.Hide();
            if (Properties.Settings.Default.SavedWidth > 0) this.Width = Properties.Settings.Default.SavedWidth;
            if (Properties.Settings.Default.SavedHeight > 0) this.Height = Properties.Settings.Default.SavedHeight;
            if (Properties.Settings.Default.SavedTop > 0) this.Top = Properties.Settings.Default.SavedTop;
            if (Properties.Settings.Default.SavedLeft > 0) this.Left = Properties.Settings.Default.SavedLeft;
            _isFixed = Properties.Settings.Default.SavedFixPosition;
            _isPassthru = Properties.Settings.Default.SavedClickThrough;

            string savedPath = Properties.Settings.Default.SavedImagePath;
            if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath)) {
                UpdateStickerImage(savedPath);
            }
            else {
                Properties.Settings.Default.SavedImagePath = "./maomao.png";
                Properties.Settings.Default.Save();
            }

            EnsureWindowOnScreen();

            _topmostTimer = new DispatcherTimer();
            _topmostTimer.Interval = TimeSpan.FromSeconds(1);
            _topmostTimer.Tick += (s, e) => {
                if (Properties.Settings.Default.SavedAlwaysOnTop) {
                    this.Topmost = true;
                }
            };
            _topmostTimer.Start();

            CreateMenu();
        }

        private void CreateMenu() {
            var menu = new System.Windows.Forms.ContextMenu();

            var iconUri = new Uri("pack://application:,,,/DesktopSticker;component/icon.ico");
            System.IO.Stream iconStream = System.Windows.Application.GetResourceStream(iconUri).Stream;

            var noti = new NotifyIcon
            {
                Icon = new Icon(iconStream),
                Visible = true,
                Text = "Desktop Sticker",
                ContextMenu = menu
            };

            var menuChangeImage = new System.Windows.Forms.MenuItem { Text = "Change Image..." };
            menuChangeImage.Click += (o, e) => {
                using (var openFileDialog = new OpenFileDialog()) {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    openFileDialog.Title = "Select an Image";
                    openFileDialog.Filter = "Image Files|*.gif;*.png;*.jpg;*.jpeg|All Files|*.*";

                    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                        string newPath = openFileDialog.FileName;
                        UpdateStickerImage(newPath);
                        Properties.Settings.Default.SavedImagePath = newPath;
                        Properties.Settings.Default.Save();
                    }
                }
            };

            _menuAlwaysOnTop = new System.Windows.Forms.MenuItem { Text = "Always on Top", Checked = Properties.Settings.Default.SavedAlwaysOnTop };
            _menuAlwaysOnTop.Click += (o, e) => ToggleSetting(ref _menuAlwaysOnTop, "SavedAlwaysOnTop", val => this.Topmost = val);

            _menuShowInTaskbar = new System.Windows.Forms.MenuItem { Text = "Show in Taskbar", Checked = Properties.Settings.Default.SavedShowInTaskbar };
            _menuShowInTaskbar.Click += (o, e) => ToggleSetting(ref _menuShowInTaskbar, "SavedShowInTaskbar", val => this.ShowInTaskbar = val);

            _menuHideWindow = new System.Windows.Forms.MenuItem { Text = "Hide Window", Checked = Properties.Settings.Default.SavedHideWindow };
            _menuHideWindow.Click += (o, e) => ToggleSetting(ref _menuHideWindow, "SavedHideWindow", val => { if (val) this.Hide(); else this.Show(); });

            _menuFixPosition = new System.Windows.Forms.MenuItem { Text = "Fix Position" };
            _menuFixPosition.Checked = _isFixed;
            _menuFixPosition.Click += (o, e) => {
                bool newState = !_menuFixPosition.Checked;
                _menuFixPosition.Checked = newState;
                _isFixed = newState;

                Properties.Settings.Default.SavedFixPosition = newState;
                Properties.Settings.Default.Save();
            };

            _menuClickThrough = new System.Windows.Forms.MenuItem { Text = "Click-Through" };
            _menuClickThrough.Checked = _isPassthru;
            _menuClickThrough.Click += (o, e) => {
                bool newState = !_menuClickThrough.Checked;
                _menuClickThrough.Checked = newState;

                SetPassthru(newState);

                Properties.Settings.Default.SavedClickThrough = newState;
                Properties.Settings.Default.Save();
            };

            var menuPreferences = new System.Windows.Forms.MenuItem { Text = "Preferences" };
            menuPreferences.Click += (o, e) => { new PrefWindow().Show(); };

            var menuAbout = new System.Windows.Forms.MenuItem { Text = "About" };
            menuAbout.Click += (o, e) => { new AboutWindow().Show(); };

            var menuExit = new System.Windows.Forms.MenuItem { Text = "Exit" };
            menuExit.Click += (o, e) => { System.Windows.Application.Current.Shutdown(); };

            menu.MenuItems.Add(menuChangeImage);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(_menuFixPosition);
            menu.MenuItems.Add(_menuClickThrough);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(_menuAlwaysOnTop);
            menu.MenuItems.Add(_menuShowInTaskbar);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(_menuHideWindow);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(menuPreferences);
            menu.MenuItems.Add(menuAbout);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(menuExit);
        }

        private void EnsureWindowOnScreen() {
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;

            bool isOffScreen = (this.Left + this.Width < screenLeft) || (this.Top + this.Height < screenTop) || (this.Left > screenLeft + screenWidth) || (this.Top > screenTop + screenHeight);

            if (isOffScreen) {
                this.Left = 0;
                this.Top = 0;

                Properties.Settings.Default.SavedLeft = 0;
                Properties.Settings.Default.SavedTop = 0;
                Properties.Settings.Default.Save();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            if (_isPassthru) SetPassthru(true);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e) {
            if (_isFixed) return;
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            Properties.Settings.Default.SavedWidth = this.Width;
            Properties.Settings.Default.SavedHeight = this.Height;
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e) {
            Properties.Settings.Default.SavedTop = this.Top;
            Properties.Settings.Default.SavedLeft = this.Left;
        }

        private void ToggleSetting(ref System.Windows.Forms.MenuItem menu, string settingName, Action<bool> action) {
            bool newState = !menu.Checked;
            menu.Checked = newState;
            Properties.Settings.Default[settingName] = newState;
            Properties.Settings.Default.Save();
            action(newState);
        }

        public void SetFixPosition(bool isFixed) {
            _isFixed = isFixed;
        }

        public void SetPassthru(bool enable) {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            if (enable) {
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            }
            else {
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
            }

            _isPassthru = enable;
        }

        public void UpdateStickerImage(string filePath) {
            StickerImage.Dispatcher.Invoke(() => {
                try {
                    var imageSource = new BitmapImage();
                    imageSource.BeginInit();
                    imageSource.UriSource = new Uri(filePath, UriKind.RelativeOrAbsolute);
                    imageSource.CacheOption = BitmapCacheOption.OnLoad;
                    imageSource.EndInit();

                    ImageBehavior.SetAnimatedSource(StickerImage, imageSource);
                }
                catch (Exception ex) {
                    System.Windows.Forms.MessageBox.Show(
                        "Failed to load image: " + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            });
        }

        public void RefreshMenuItems() {
            if (_menuFixPosition != null) _menuFixPosition.Checked = Properties.Settings.Default.SavedFixPosition;
            if (_menuClickThrough != null) _menuClickThrough.Checked = Properties.Settings.Default.SavedClickThrough;
            if (_menuAlwaysOnTop != null) _menuAlwaysOnTop.Checked = Properties.Settings.Default.SavedAlwaysOnTop;
            if (_menuShowInTaskbar != null) _menuShowInTaskbar.Checked = Properties.Settings.Default.SavedShowInTaskbar;
            if (_menuHideWindow != null) _menuHideWindow.Checked = Properties.Settings.Default.SavedHideWindow;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Properties.Settings.Default.Save();
        }
    }
}
