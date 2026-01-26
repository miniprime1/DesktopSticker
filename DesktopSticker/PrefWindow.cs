using System;
using System.Windows;
using System.Windows.Forms;

namespace DesktopSticker {
    public partial class PrefWindow : Form {
        private bool _isLoading = true;
        private bool _isUpdating = false;
        private bool _isShowingError = false;

        private MainWindow GetMainWindow() {
            return System.Windows.Application.Current.MainWindow as MainWindow;
        }

        public PrefWindow() {
            InitializeComponent();

            InitializeTextBoxEvents();

            UpdatePrefWindow();

            var main = GetMainWindow();
            if (main != null) {
                main.SizeChanged += Main_SizeChanged;
                main.LocationChanged += Main_LocationChanged;
            }

            _isLoading = false;
        }

        protected override void OnFormClosed(FormClosedEventArgs e) {
            var main = GetMainWindow();
            if (main != null) {
                main.SizeChanged -= Main_SizeChanged;
                main.LocationChanged -= Main_LocationChanged;
            }

            base.OnFormClosed(e);
        }

        private void InitializeTextBoxEvents() {
            textBoxWidth.Leave += (s, e) => ApplyWidth();
            textBoxHeight.Leave += (s, e) => ApplyHeight();
            textBoxLeft.Leave += (s, e) => ApplyLeft();
            textBoxBottom.Leave += (s, e) => ApplyBottom();

            textBoxWidth.KeyDown += TextBox_KeyDown;
            textBoxHeight.KeyDown += TextBox_KeyDown;
            textBoxLeft.KeyDown += TextBox_KeyDown;
            textBoxBottom.KeyDown += TextBox_KeyDown;

            textBoxImagePath.Leave += (s, e) => ApplyImage();
            textBoxImagePath.KeyDown += TextBox_KeyDown;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                this.ActiveControl = null;
                e.SuppressKeyPress = true;
            }
        }

        public void UpdatePrefWindow() {
            _isUpdating = true;

            checkBoxFixPosition.Checked = Properties.Settings.Default.SavedFixPosition;
            checkBoxClickThrough.Checked = Properties.Settings.Default.SavedClickThrough;
            checkBoxAlwaysOnTop.Checked = Properties.Settings.Default.SavedAlwaysOnTop;
            checkBoxShowInTaskbar.Checked = Properties.Settings.Default.SavedShowInTaskbar;
            checkBoxHideWindow.Checked = Properties.Settings.Default.SavedHideWindow;

            textBoxWidth.Text = Properties.Settings.Default.SavedWidth.ToString();
            textBoxHeight.Text = Properties.Settings.Default.SavedHeight.ToString();
            textBoxLeft.Text = Properties.Settings.Default.SavedLeft.ToString();

            double bottomGap = System.Windows.SystemParameters.PrimaryScreenHeight - Properties.Settings.Default.SavedTop - Properties.Settings.Default.SavedHeight;
            textBoxBottom.Text = bottomGap.ToString();

            textBoxImagePath.Text = Properties.Settings.Default.SavedImagePath;

            _isUpdating = false;
        }

        private void Main_SizeChanged(object sender, SizeChangedEventArgs e) {
            _isUpdating = true;

            var main = sender as MainWindow;
            if (main != null) {
                textBoxWidth.Text = main.Width.ToString();
                textBoxHeight.Text = main.Height.ToString();
                double bottomGap = System.Windows.SystemParameters.PrimaryScreenHeight - main.Top - main.Height;
                textBoxBottom.Text = bottomGap.ToString();
            }

            _isUpdating = false;
        }

        private void Main_LocationChanged(object sender, EventArgs e) {
            _isUpdating = true;

            var main = sender as MainWindow;
            if (main != null) {
                textBoxLeft.Text = main.Left.ToString();
                double bottomGap = System.Windows.SystemParameters.PrimaryScreenHeight - main.Top - main.Height;
                textBoxBottom.Text = bottomGap.ToString();
            }

            _isUpdating = false;
        }

        private void checkBoxFixPosition_CheckedChanged(object sender, EventArgs e) {
            if (_isLoading || _isUpdating) return;

            bool isChecked = checkBoxFixPosition.Checked;
            Properties.Settings.Default.SavedFixPosition = isChecked;
            Properties.Settings.Default.Save();

            var main = GetMainWindow();
            if (main != null) {
                main.SetFixPosition(isChecked);
                main.RefreshMenuItems();
            }
        }

        private void checkBoxClickThrough_CheckedChanged(object sender, EventArgs e) {
            if (_isLoading || _isUpdating) return;

            bool isChecked = checkBoxClickThrough.Checked;
            Properties.Settings.Default.SavedClickThrough = isChecked;
            Properties.Settings.Default.Save();

            var main = GetMainWindow();
            if (main != null) {
                main.SetPassthru(isChecked);
                main.RefreshMenuItems();
            }
        }

        private void checkBoxAlwaysOnTop_CheckedChanged(object sender, EventArgs e) {
            if (_isLoading || _isUpdating) return;

            bool isChecked = checkBoxAlwaysOnTop.Checked;
            Properties.Settings.Default.SavedAlwaysOnTop = isChecked;
            Properties.Settings.Default.Save();

            var main = GetMainWindow();
            if (main != null) {
                main.Topmost = isChecked;
                main.RefreshMenuItems();
            }
        }

        private void checkBoxShowInTaskbar_CheckedChanged(object sender, EventArgs e) {
            if (_isLoading || _isUpdating) return;

            bool isChecked = checkBoxShowInTaskbar.Checked;
            Properties.Settings.Default.SavedShowInTaskbar = isChecked;
            Properties.Settings.Default.Save();

            var main = GetMainWindow();
            if (main != null) {
                main.ShowInTaskbar = isChecked;
                main.RefreshMenuItems();
            }
        }

        private void checkBoxHideWindow_CheckedChanged(object sender, EventArgs e) {
            if (_isLoading || _isUpdating) return;

            bool isChecked = checkBoxHideWindow.Checked;
            Properties.Settings.Default.SavedHideWindow = isChecked;
            Properties.Settings.Default.Save();

            var main = GetMainWindow();
            if (main != null) {
                if (isChecked) main.Hide();
                else main.Show();
                main.RefreshMenuItems();
            }
        }

        private void ApplyWidth() {
            if (_isLoading || _isUpdating) return;

            if (double.TryParse(textBoxWidth.Text, out double val)) {
                var main = GetMainWindow();
                if (main != null) main.Width = val;
                Properties.Settings.Default.SavedWidth = val;
                Properties.Settings.Default.Save();
            }
            else {
                var main = GetMainWindow();
                if (main != null) textBoxWidth.Text = main.Width.ToString();
            }
        }

        private void ApplyHeight() {
            if (_isLoading || _isUpdating) return;

            if (double.TryParse(textBoxHeight.Text, out double newHeight)) {
                var main = GetMainWindow();
                if (main != null) {
                    main.Height = newHeight;
                    Properties.Settings.Default.SavedHeight = newHeight;
                    Properties.Settings.Default.Save();

                    _isUpdating = true;
                    double bottomGap = System.Windows.SystemParameters.PrimaryScreenHeight - main.Top - newHeight;
                    textBoxBottom.Text = bottomGap.ToString();
                    _isUpdating = false;
                }
            }
            else {
                var main = GetMainWindow();
                if (main != null) textBoxHeight.Text = main.Height.ToString();
            }
        }

        private void ApplyLeft() {
            if (_isLoading || _isUpdating) return;

            if (double.TryParse(textBoxLeft.Text, out double val)) {
                var main = GetMainWindow();
                if (main != null) main.Left = val;
                Properties.Settings.Default.SavedLeft = val;
                Properties.Settings.Default.Save();
            }
            else {
                var main = GetMainWindow();
                if (main != null) textBoxLeft.Text = main.Left.ToString();
            }
        }

        private void ApplyBottom() {
            if (_isLoading || _isUpdating) return;

            if (double.TryParse(textBoxBottom.Text, out double bottomGap)) {
                var main = GetMainWindow();
                if (main != null) {
                    double newTop = System.Windows.SystemParameters.PrimaryScreenHeight - main.Height - bottomGap;
                    main.Top = newTop;
                    Properties.Settings.Default.SavedTop = newTop;
                    Properties.Settings.Default.Save();
                }
            }
            else {
                var main = GetMainWindow();
                if (main != null) {
                    double currentBottom = System.Windows.SystemParameters.PrimaryScreenHeight - main.Top - main.Height;
                    textBoxBottom.Text = currentBottom.ToString();
                }
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e) {
            using (var openFileDialog = new OpenFileDialog()) {
                openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                openFileDialog.Title = "Select an Image";
                openFileDialog.Filter = "Image Files|*.gif;*.png;*.jpg;*.jpeg|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    string newPath = openFileDialog.FileName;
                    textBoxImagePath.Text = newPath;
                }
            }
        }

        private void textBoxImagePath_TextChanged(object sender, EventArgs e) {
            if (_isLoading || _isUpdating) return;

            string newPath = textBoxImagePath.Text;

            if (System.IO.File.Exists(newPath)) {
                var main = GetMainWindow();
                if (main != null) main.UpdateStickerImage(newPath);

                Properties.Settings.Default.SavedImagePath = newPath;
                Properties.Settings.Default.Save();
            }
        }

        private void ApplyImage() {
            if (_isShowingError) return;

            string newPath = textBoxImagePath.Text;

            if (System.IO.File.Exists(newPath)) {
                Properties.Settings.Default.SavedImagePath = newPath;
                Properties.Settings.Default.Save();
            }
            else {
                _isShowingError = true;

                System.Windows.Forms.MessageBox.Show(
                    "Failed to load image: The file path is invalid or does not exist.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                textBoxImagePath.Text = Properties.Settings.Default.SavedImagePath;

                _isShowingError = false;
            }
        }
    }
}
