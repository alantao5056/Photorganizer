using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
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

        private static readonly Dictionary<MediaFormat, (Color Light, Color Dark)> FormatFgColors = new()
        {
            [MediaFormat.JPG]  = (Color.FromArgb(255, 16, 124, 65),  Color.FromArgb(255, 104, 211, 145)),
            [MediaFormat.HEIC] = (Color.FromArgb(255, 0, 120, 212),   Color.FromArgb(255, 99, 179, 237)),
            [MediaFormat.PNG]  = (Color.FromArgb(255, 202, 80, 16),   Color.FromArgb(255, 246, 173, 85)),
            [MediaFormat.MOV]  = (Color.FromArgb(255, 164, 38, 44),   Color.FromArgb(255, 252, 129, 129)),
        };

        private static readonly Dictionary<MediaFormat, (Color Light, Color Dark)> FormatBgColors = new()
        {
            [MediaFormat.JPG]  = (Color.FromArgb(0x14, 16, 124, 65),  Color.FromArgb(0x1A, 104, 211, 145)),
            [MediaFormat.HEIC] = (Color.FromArgb(0x14, 0, 120, 212),   Color.FromArgb(0x1A, 99, 179, 237)),
            [MediaFormat.PNG]  = (Color.FromArgb(0x14, 202, 80, 16),   Color.FromArgb(0x1A, 246, 173, 85)),
            [MediaFormat.MOV]  = (Color.FromArgb(0x14, 164, 38, 44),   Color.FromArgb(0x1A, 252, 129, 129)),
        };

        private static readonly Dictionary<MediaFormat, (Color Light, Color Dark)> FormatBdColors = new()
        {
            [MediaFormat.JPG]  = (Color.FromArgb(0x33, 16, 124, 65),  Color.FromArgb(0x33, 104, 211, 145)),
            [MediaFormat.HEIC] = (Color.FromArgb(0x33, 0, 120, 212),   Color.FromArgb(0x33, 99, 179, 237)),
            [MediaFormat.PNG]  = (Color.FromArgb(0x33, 202, 80, 16),   Color.FromArgb(0x33, 246, 173, 85)),
            [MediaFormat.MOV]  = (Color.FromArgb(0x33, 164, 38, 44),   Color.FromArgb(0x33, 252, 129, 129)),
        };

        // Chip active colors (for the stat bar chips)
        private static readonly Dictionary<MediaFormat, (Color Light, Color Dark)> ChipFgColors = new()
        {
            [MediaFormat.JPG]  = (Color.FromArgb(255, 16, 124, 65),   Color.FromArgb(255, 104, 211, 145)),
            [MediaFormat.HEIC] = (Color.FromArgb(255, 0, 120, 212),    Color.FromArgb(255, 99, 179, 237)),
            [MediaFormat.PNG]  = (Color.FromArgb(255, 202, 80, 16),    Color.FromArgb(255, 246, 173, 85)),
            [MediaFormat.MOV]  = (Color.FromArgb(255, 164, 38, 44),    Color.FromArgb(255, 252, 129, 129)),
        };

        private static readonly Dictionary<MediaFormat, (Color Light, Color Dark)> ChipBgColors = new()
        {
            [MediaFormat.JPG]  = (Color.FromArgb(0x14, 16, 124, 65),  Color.FromArgb(0x14, 104, 211, 145)),
            [MediaFormat.HEIC] = (Color.FromArgb(0x14, 0, 120, 212),   Color.FromArgb(0x14, 99, 179, 237)),
            [MediaFormat.PNG]  = (Color.FromArgb(0x14, 202, 80, 16),   Color.FromArgb(0x14, 246, 173, 85)),
            [MediaFormat.MOV]  = (Color.FromArgb(0x14, 164, 38, 44),   Color.FromArgb(0x14, 252, 129, 129)),
        };

        private static readonly Dictionary<MediaFormat, (Color Light, Color Dark)> ChipBdColors = new()
        {
            [MediaFormat.JPG]  = (Color.FromArgb(0x59, 16, 124, 65),  Color.FromArgb(0x66, 104, 211, 145)),
            [MediaFormat.HEIC] = (Color.FromArgb(0x59, 0, 120, 212),   Color.FromArgb(0x66, 99, 179, 237)),
            [MediaFormat.PNG]  = (Color.FromArgb(0x59, 202, 80, 16),   Color.FromArgb(0x66, 246, 173, 85)),
            [MediaFormat.MOV]  = (Color.FromArgb(0x59, 164, 38, 44),   Color.FromArgb(0x66, 252, 129, 129)),
        };

        private List<FileInfo> _scannedFiles = [];
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

        private List<FileItem> BuildFileItems(IEnumerable<FileInfo> files)
        {
            bool dark = IsDark;

            return files.Select(f =>
            {
                var fmt = MediaFormatExtensions.FromExtension(f.Extension)!.Value;
                var fmtFg = FormatFgColors[fmt];
                var fmtBg = FormatBgColors[fmt];
                var fmtBd = FormatBdColors[fmt];

                return new FileItem
                {
                    Name = f.Name,
                    Format = fmt.ToString(),
                    CaptureTime = "\u2014",
                    DestFolder = "\u2014",
                    HasExif = false,
                    FormatFg = new SolidColorBrush(dark ? fmtFg.Dark : fmtFg.Light),
                    FormatBg = new SolidColorBrush(dark ? fmtBg.Dark : fmtBg.Light),
                    FormatBd = new SolidColorBrush(dark ? fmtBd.Dark : fmtBd.Light),
                    DestIcon = "\uE946",
                    DestFg = new SolidColorBrush(dark
                        ? Color.FromArgb(255, 246, 173, 85) : Color.FromArgb(255, 202, 80, 16)),
                    StatusText = "Pending",
                    StatusFg = new SolidColorBrush(dark
                        ? Color.FromArgb(255, 246, 173, 85) : Color.FromArgb(255, 202, 80, 16)),
                    StatusBg = new SolidColorBrush(dark
                        ? Color.FromArgb(0x14, 246, 173, 85) : Color.FromArgb(0x12, 202, 80, 16)),
                    StatusBd = new SolidColorBrush(dark
                        ? Color.FromArgb(0x33, 246, 173, 85) : Color.FromArgb(0x33, 202, 80, 16)),
                };
            }).ToList();
        }

        private void LoadFiles(IEnumerable<FileInfo> files)
        {
            _allFiles = BuildFileItems(files);
            ApplyFilter();
            EmptyState.Visibility = Visibility.Collapsed;

            // Update chip counts
            var counts = _allFiles.GroupBy(f => f.Format)
                .ToDictionary(g => g.Key, g => g.Count());
            ChipAllVal.Text = _allFiles.Count.ToString();
            CntJpg.Text = counts.GetValueOrDefault(nameof(MediaFormat.JPG)).ToString();
            CntHeic.Text = counts.GetValueOrDefault(nameof(MediaFormat.HEIC)).ToString();
            CntPng.Text = counts.GetValueOrDefault(nameof(MediaFormat.PNG)).ToString();
            CntMov.Text = counts.GetValueOrDefault(nameof(MediaFormat.MOV)).ToString();

            ChipAllVal.Visibility = Visibility.Visible;
            CntJpg.Visibility = Visibility.Visible;
            CntHeic.Visibility = Visibility.Visible;
            CntPng.Visibility = Visibility.Visible;
            CntMov.Visibility = Visibility.Visible;
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
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add("*");

            // Initialize the picker with the current window handle (required for WinUI 3)
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder == null) return;

            FolderPath.Text = folder.Path;
            FolderPath.Foreground = (Brush)Application.Current.Resources["AccentBrush"];

            // Scan root-level supported files
            var dir = new DirectoryInfo(folder.Path);
            _scannedFiles = dir.EnumerateFiles()
                .Where(f => MediaFormatExtensions.IsSupportedExtension(f.Extension))
                .OrderBy(f => f.Name)
                .ToList();

            _folderLoaded = true;
            _allMode = true;
            _activeFormats.Clear();
            SetAllModeVisuals();
            LoadFiles(_scannedFiles);
        }

        // ── Theme Toggle ──
        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isDark = ThemeToggle.IsChecked == true;
            RootGrid.RequestedTheme = isDark ? ElementTheme.Dark : ElementTheme.Light;

            if (_folderLoaded)
            {
                _allFiles = BuildFileItems(_scannedFiles);
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

        private void ApplyFormatChipColors(Button chip, MediaFormat fmt)
        {
            bool dark = IsDark;
            bool active = !_allMode && _activeFormats.Contains(fmt.ToString());

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

        private (Button Chip, MediaFormat Fmt)[] GetFormatChips() =>
        [
            (ChipJpg, MediaFormat.JPG),
            (ChipHeic, MediaFormat.HEIC),
            (ChipPng, MediaFormat.PNG),
            (ChipMov, MediaFormat.MOV),
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
