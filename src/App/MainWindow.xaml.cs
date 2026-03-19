using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.Storage;
using Windows.UI;
using Alan.Photorganizer.App.Models;

namespace Alan.Photorganizer.App
{
    public sealed partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        private static readonly (string Name, string Fmt, string Time, string Dest, string Status)[] SampleFiles =
        [
            ("IMG_4821.jpg",      "JPG",  "2024-07-15  09:14:32", "2024-07-15", "ready"),
            ("IMG_4822.jpg",      "JPG",  "2024-07-15  09:21:08", "2024-07-15", "ready"),
            ("IMG_4823.heic",     "HEIC", "2024-07-15  11:03:55", "2024-07-15", "ready"),
            ("VID_0088.mov",      "MOV",  "2024-07-15  14:47:20", "2024-07-15", "ready"),
            ("IMG_4900.jpg",      "JPG",  "2024-07-16  08:05:11", "2024-07-16", "ready"),
            ("IMG_4901.heic",     "HEIC", "2024-07-16  10:33:44", "2024-07-16", "ready"),
            ("VID_0091.mov",      "MOV",  "2024-07-16  15:22:09", "2024-07-16", "ready"),
            ("screenshot_01.png", "PNG",  "2024-07-17  18:00:03", "2024-07-17", "ready"),
            ("IMG_4950.jpg",      "JPG",  "2024-07-17  09:41:28", "2024-07-17", "ready"),
            ("IMG_4951.heic",     "HEIC", "2024-07-17  12:14:50", "2024-07-17", "ready"),
            ("IMG_5010.jpg",      "JPG",  "2024-07-18  07:58:22", "2024-07-18", "ready"),
            ("IMG_5011.heic",     "HEIC", "2024-07-18  08:30:00", "2024-07-18", "ready"),
            ("IMG_5012.jpg",      "JPG",  "2024-07-18  09:11:37", "2024-07-18", "ready"),
            ("VID_0100.mov",      "MOV",  "2024-07-18  17:45:55", "2024-07-18", "ready"),
            ("IMG_5088.heic",     "HEIC", "2024-07-19  08:02:14", "2024-07-19", "ready"),
            ("VID_0102.mov",      "MOV",  "2024-07-19  19:30:42", "2024-07-19", "ready"),
            ("IMG_5120.jpg",      "JPG",  "2024-07-20  10:09:01", "2024-07-20", "ready"),
            ("wallpaper.png",     "PNG",  "\u2014",               "(no EXIF)",  "noexif"),
            ("download.jpg",      "JPG",  "\u2014",               "(no EXIF)",  "noexif"),
        ];

        private static readonly Dictionary<string, (Color Light, Color Dark)> FormatFgColors = new()
        {
            ["JPG"]  = (Color.FromArgb(255, 16, 124, 65),  Color.FromArgb(255, 104, 211, 145)),
            ["HEIC"] = (Color.FromArgb(255, 0, 120, 212),   Color.FromArgb(255, 99, 179, 237)),
            ["PNG"]  = (Color.FromArgb(255, 202, 80, 16),   Color.FromArgb(255, 246, 173, 85)),
            ["MOV"]  = (Color.FromArgb(255, 164, 38, 44),   Color.FromArgb(255, 252, 129, 129)),
        };

        private static readonly Dictionary<string, (Color Light, Color Dark)> FormatBgColors = new()
        {
            ["JPG"]  = (Color.FromArgb(0x14, 16, 124, 65),  Color.FromArgb(0x1A, 104, 211, 145)),
            ["HEIC"] = (Color.FromArgb(0x14, 0, 120, 212),   Color.FromArgb(0x1A, 99, 179, 237)),
            ["PNG"]  = (Color.FromArgb(0x14, 202, 80, 16),   Color.FromArgb(0x1A, 246, 173, 85)),
            ["MOV"]  = (Color.FromArgb(0x14, 164, 38, 44),   Color.FromArgb(0x1A, 252, 129, 129)),
        };

        private static readonly Dictionary<string, (Color Light, Color Dark)> FormatBdColors = new()
        {
            ["JPG"]  = (Color.FromArgb(0x33, 16, 124, 65),  Color.FromArgb(0x33, 104, 211, 145)),
            ["HEIC"] = (Color.FromArgb(0x33, 0, 120, 212),   Color.FromArgb(0x33, 99, 179, 237)),
            ["PNG"]  = (Color.FromArgb(0x33, 202, 80, 16),   Color.FromArgb(0x33, 246, 173, 85)),
            ["MOV"]  = (Color.FromArgb(0x33, 164, 38, 44),   Color.FromArgb(0x33, 252, 129, 129)),
        };

        // Chip active colors (for the stat bar chips)
        private static readonly Dictionary<string, (Color Light, Color Dark)> ChipFgColors = new()
        {
            ["JPG"]  = (Color.FromArgb(255, 16, 124, 65),   Color.FromArgb(255, 104, 211, 145)),
            ["HEIC"] = (Color.FromArgb(255, 0, 120, 212),    Color.FromArgb(255, 99, 179, 237)),
            ["PNG"]  = (Color.FromArgb(255, 202, 80, 16),    Color.FromArgb(255, 246, 173, 85)),
            ["MOV"]  = (Color.FromArgb(255, 164, 38, 44),    Color.FromArgb(255, 252, 129, 129)),
        };

        private static readonly Dictionary<string, (Color Light, Color Dark)> ChipBgColors = new()
        {
            ["JPG"]  = (Color.FromArgb(0x14, 16, 124, 65),  Color.FromArgb(0x14, 104, 211, 145)),
            ["HEIC"] = (Color.FromArgb(0x14, 0, 120, 212),   Color.FromArgb(0x14, 99, 179, 237)),
            ["PNG"]  = (Color.FromArgb(0x14, 202, 80, 16),   Color.FromArgb(0x14, 246, 173, 85)),
            ["MOV"]  = (Color.FromArgb(0x14, 164, 38, 44),   Color.FromArgb(0x14, 252, 129, 129)),
        };

        private static readonly Dictionary<string, (Color Light, Color Dark)> ChipBdColors = new()
        {
            ["JPG"]  = (Color.FromArgb(0x59, 16, 124, 65),  Color.FromArgb(0x66, 104, 211, 145)),
            ["HEIC"] = (Color.FromArgb(0x59, 0, 120, 212),   Color.FromArgb(0x66, 99, 179, 237)),
            ["PNG"]  = (Color.FromArgb(0x59, 202, 80, 16),   Color.FromArgb(0x66, 246, 173, 85)),
            ["MOV"]  = (Color.FromArgb(0x59, 164, 38, 44),   Color.FromArgb(0x66, 252, 129, 129)),
        };

        private List<FileItem> _allFiles = [];
        private bool _allMode = true;
        private readonly HashSet<string> _activeFormats = [];
        private bool _folderLoaded;

        public bool IsDarkTheme { get; set; } = Application.Current.RequestedTheme == ApplicationTheme.Dark;

        public MainWindow()
        {
            InitializeComponent();
            SetupWindow();
            SetAllModeVisuals();
        }

        private void SetupWindow()
        {
            Title = "Photorganizer";
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            UpdateCaptionButtonColors(IsDarkTheme);

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var dpi = GetDpiForWindow(hwnd);
            var scale = dpi / 96.0;
            AppWindow.Resize(new SizeInt32((int)(860 * scale), (int)(620 * scale)));

            CenterOnScreen();
        }

        private void CenterOnScreen()
        {
            var appWindow = AppWindow;
            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            var workArea = displayArea.WorkArea;
            var size = appWindow.Size;
            int x = (workArea.Width - size.Width) / 2 + workArea.X;
            int y = (workArea.Height - size.Height) / 2 + workArea.Y;
            appWindow.Move(new PointInt32(x, y));
        }

        private bool IsDark => RootGrid.ActualTheme == ElementTheme.Dark;

        private void UpdateCaptionButtonColors(bool dark)
        {
            var titleBar = AppWindow.TitleBar;
            if (dark)
            {
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 255, 255, 255);
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x22, 255, 255, 255);
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(0x99, 255, 255, 255);
            }
            else
            {
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x1A, 0, 0, 0);
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x12, 0, 0, 0);
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(0x99, 0, 0, 0);
            }
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private List<FileItem> BuildFileItems()
        {
            bool dark = IsDark;
            var successColor = dark ? Color.FromArgb(255, 104, 211, 145) : Color.FromArgb(255, 16, 124, 65);
            var warningColor = dark ? Color.FromArgb(255, 246, 173, 85) : Color.FromArgb(255, 202, 80, 16);

            var readyBg = dark ? Color.FromArgb(0x14, 104, 211, 145) : Color.FromArgb(0x12, 16, 124, 65);
            var readyBd = dark ? Color.FromArgb(0x2E, 104, 211, 145) : Color.FromArgb(0x33, 16, 124, 65);
            var noexifBg = dark ? Color.FromArgb(0x14, 246, 173, 85) : Color.FromArgb(0x12, 202, 80, 16);
            var noexifBd = dark ? Color.FromArgb(0x33, 246, 173, 85) : Color.FromArgb(0x33, 202, 80, 16);

            return SampleFiles.Select(f =>
            {
                bool hasExif = f.Status == "ready";
                var fmtFg = FormatFgColors[f.Fmt];
                var fmtBg = FormatBgColors[f.Fmt];
                var fmtBd = FormatBdColors[f.Fmt];

                return new FileItem
                {
                    Name = f.Name,
                    Format = f.Fmt,
                    CaptureTime = f.Time,
                    DestFolder = f.Dest,
                    HasExif = hasExif,
                    FormatFg = new SolidColorBrush(dark ? fmtFg.Dark : fmtFg.Light),
                    FormatBg = new SolidColorBrush(dark ? fmtBg.Dark : fmtBg.Light),
                    FormatBd = new SolidColorBrush(dark ? fmtBd.Dark : fmtBd.Light),
                    DestIcon = hasExif ? "\uE8B7" : "\uE946",
                    DestFg = new SolidColorBrush(hasExif ? successColor : warningColor),
                    StatusText = hasExif ? "Ready" : "No EXIF",
                    StatusFg = new SolidColorBrush(hasExif ? successColor : warningColor),
                    StatusBg = new SolidColorBrush(hasExif ? readyBg : noexifBg),
                    StatusBd = new SolidColorBrush(hasExif ? readyBd : noexifBd),
                };
            }).ToList();
        }

        private void LoadFiles()
        {
            _allFiles = BuildFileItems();
            ApplyFilter();
            EmptyState.Visibility = Visibility.Collapsed;

            ChipAllVal.Visibility = Visibility.Visible;
            CntJpg.Visibility = Visibility.Visible;
            CntHeic.Visibility = Visibility.Visible;
            CntPng.Visibility = Visibility.Visible;
            CntMov.Visibility = Visibility.Visible;
            ChipGroups.Visibility = Visibility.Visible;
            ChipNoExif.Visibility = Visibility.Visible;
        }

        private void ApplyFilter()
        {
            if (_allMode)
                FileList.ItemsSource = _allFiles;
            else
                FileList.ItemsSource = _allFiles.Where(f => _activeFormats.Contains(f.Format)).ToList();
        }

        // ── Folder Pick ──
        private async void FolderPick_Click(object sender, RoutedEventArgs e)
        {
            if (_folderLoaded) return;

            FolderPath.Text = @"D:\Photos\2024_vacation";
            FolderPath.Foreground = (Brush)Application.Current.Resources["AccentBrush"];
            OrganizeBtn.IsEnabled = false;

            ProgressStrip.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressLabel.Text = "Scanning folder...";
            ProgressDetail.Text = "";
            ProgressPct.Text = "";

            await System.Threading.Tasks.Task.Delay(2000);

            ProgressStrip.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;

            _folderLoaded = true;
            OrganizeBtn.IsEnabled = true;
            _allMode = true;
            _activeFormats.Clear();
            SetAllModeVisuals();
            LoadFiles();
        }

        // ── Theme Toggle ──
        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isDark = ThemeToggle.IsChecked == true;
            RootGrid.RequestedTheme = isDark ? ElementTheme.Dark : ElementTheme.Light;

            if (_folderLoaded)
            {
                _allFiles = BuildFileItems();
                ApplyFilter();
            }

            UpdateChipVisuals();

            // Update folder path accent color
            if (_folderLoaded)
            {
                var accent = IsDark ? Color.FromArgb(255, 99, 179, 237) : Color.FromArgb(255, 0, 120, 212);
                FolderPath.Foreground = new SolidColorBrush(accent);
            }

            UpdateCaptionButtonColors(isDark);

            ApplicationData.Current.LocalSettings.Values["IsDarkTheme"] = isDark;
        }

        // ── Filter Chips ──
        private void ChipAll_Click(object sender, RoutedEventArgs e)
        {
            _allMode = true;
            _activeFormats.Clear();
            SetAllModeVisuals();
            if (_folderLoaded) ApplyFilter();
        }

        private void ChipFmt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string fmt) return;

            _allMode = false;

            if (_activeFormats.Contains(fmt))
            {
                _activeFormats.Remove(fmt);
                if (_activeFormats.Count == 0)
                {
                    _allMode = true;
                    SetAllModeVisuals();
                    if (_folderLoaded) ApplyFilter();
                    return;
                }
            }
            else
            {
                _activeFormats.Add(fmt);
            }

            UpdateChipVisuals();
            if (_folderLoaded) ApplyFilter();
        }

        private void SetAllModeVisuals()
        {
            _allMode = true;
            ApplyAllChipColors();
            foreach (var (chip, fmt) in GetFormatChips())
                ApplyFormatChipColors(chip, fmt);
        }

        private void UpdateChipVisuals()
        {
            ApplyAllChipColors();
            foreach (var (chip, fmt) in GetFormatChips())
                ApplyFormatChipColors(chip, fmt);
        }

        private void ApplyAllChipColors()
        {
            bool dark = IsDark;
            // Always use accent colors for the All chip
            ChipAll.Background = new SolidColorBrush(dark
                ? Color.FromArgb(0x14, 99, 179, 237) : Color.FromArgb(0x14, 0, 120, 212));
            ChipAll.BorderBrush = new SolidColorBrush(dark
                ? Color.FromArgb(0x73, 99, 179, 237) : Color.FromArgb(0x66, 0, 120, 212));
            // Only opacity changes between active/inactive
            ChipAll.Opacity = _allMode ? 1.0 : 0.38;
        }

        private void ApplyFormatChipColors(Button chip, string fmt)
        {
            bool dark = IsDark;
            bool active = !_allMode && _activeFormats.Contains(fmt);

            // Always keep format-specific colors
            chip.Background = new SolidColorBrush(dark ? ChipBgColors[fmt].Dark : ChipBgColors[fmt].Light);
            chip.BorderBrush = new SolidColorBrush(dark ? ChipBdColors[fmt].Dark : ChipBdColors[fmt].Light);
            SetChipTextColor(chip, dark ? ChipFgColors[fmt].Dark : ChipFgColors[fmt].Light);

            // Only opacity changes between active/inactive
            chip.Opacity = active ? 1.0 : 0.38;
        }

        private static void SetChipTextColor(Button chip, Color color)
        {
            if (chip.Content is StackPanel sp)
            {
                foreach (var child in sp.Children)
                {
                    if (child is TextBlock tb)
                        tb.Foreground = new SolidColorBrush(color);
                }
            }
        }

        private (Button Chip, string Fmt)[] GetFormatChips() =>
        [
            (ChipJpg, "JPG"),
            (ChipHeic, "HEIC"),
            (ChipPng, "PNG"),
            (ChipMov, "MOV"),
        ];

        // ── Chip Hover ──
        private bool IsChipActive(Button chip)
        {
            if (chip == ChipAll) return _allMode;
            return !_allMode && chip.Tag is string fmt && _activeFormats.Contains(fmt);
        }

        private void Chip_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button chip && !IsChipActive(chip))
                chip.Opacity = 1.0;
        }

        private void Chip_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button chip && !IsChipActive(chip))
                chip.Opacity = 0.38;
        }

        // ── Organize ──
        private void Organize_Click(object sender, RoutedEventArgs e)
        {
            // Will be implemented later
        }

        // ── Format Dropdown ──
        private void FormatItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioMenuFlyoutItem item)
            {
                FormatLabel.Text = item.Text;
            }
        }

    }
}
