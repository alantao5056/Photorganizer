using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.UI;
using System.Threading.Tasks;
using Alan.Sortify.App.Models;
using Alan.Sortify.App.Services;

namespace Alan.Sortify.App
{
    public sealed partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

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
        private Dictionary<MediaFormat, Dictionary<DateTime, List<FileItem>>> _dateGroups = [];
        private Dictionary<MediaFormat, List<FileItem>> _noExifFiles = [];
        private string _currentFormat = "yyyy-MM-dd";
        private string _customFormat = "";
        private bool _isDone;
        private string _sourceFolder = "";

        public bool IsDarkTheme { get; set; } = Application.Current.RequestedTheme == ApplicationTheme.Dark;

        public MainWindow()
        {
            InitializeComponent();
            LoadPersistedFormat();
            SetupWindow();
            SetAllModeVisuals();

            CheckForMandatoryUpdate();
        }

        private void SetupWindow()
        {
            Title = "Sortify - Photo &amp; Video Organizer";
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            UpdateCaptionButtonColors(IsDarkTheme);
            AppWindow.SetIcon("Assets/app.ico");

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
            return files.Select(f =>
            {
                var fmt = MediaFormatExtensions.FromExtension(f.Extension)!.Value;

                return new FileItem
                {
                    Name = f.Name,
                    Format = fmt.ToString(),
                    FilePath = f.FullName,
                    CaptureTime = "\u2014",
                    DestFolder = "\u2014",
                    HasExif = false,
                    StatusText = "Pending",
                };
            }).ToList();
        }

        private async Task ExtractMetadataAsync(List<FileItem> files)
        {
            foreach (var file in files)
            {
                var dateTaken = await Task.Run(() => MetadataService.ExtractDateTaken(file.FilePath));

                file.DateTaken = dateTaken;

                if (dateTaken.HasValue)
                {
                    file.CaptureTime = dateTaken.Value.ToString("yyyy-MM-dd  HH:mm:ss");
                    file.DestFolder = FormatDestFolder(dateTaken.Value);
                    file.HasExif = true;
                    file.StatusText = "Ready";
                }
                else
                {
                    file.CaptureTime = "";
                    file.DestFolder = "\u2014";
                    file.HasExif = false;
                    file.StatusText = "No EXIF";
                }
            }

            BuildGrouping();
            UpdateGroupAndNoExifPills();
            UpdateOrganizeButton();
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

            // Disable format chips with 0 files
            foreach (var (chip, fmt) in GetFormatChips())
            {
                int count = counts.GetValueOrDefault(fmt.ToString());
                chip.IsEnabled = count > 0;
                chip.Opacity = count > 0 ? (_allMode ? 0.38 : (_activeFormats.Contains(fmt.ToString()) ? 1.0 : 0.38)) : 0.25;
            }
        }

        private void BuildGrouping()
        {
            _dateGroups = [];
            _noExifFiles = [];
            foreach (var fmt in Enum.GetValues<MediaFormat>())
            {
                var dateMap = new Dictionary<DateTime, List<FileItem>>();
                var noExif = new List<FileItem>();
                foreach (var file in _allFiles.Where(f => f.Format == fmt.ToString()))
                {
                    if (file.DateTaken is { } dt)
                    {
                        var key = dt.Date;
                        if (!dateMap.TryGetValue(key, out var list))
                        {
                            list = [];
                            dateMap[key] = list;
                        }
                        list.Add(file);
                    }
                    else
                    {
                        noExif.Add(file);
                    }
                }
                _dateGroups[fmt] = dateMap;
                _noExifFiles[fmt] = noExif;
            }
        }

        private void UpdateGroupAndNoExifPills()
        {
            if (_dateGroups.Count == 0)
            {
                ChipGroups.Visibility = Visibility.Collapsed;
                ChipNoExif.Visibility = Visibility.Collapsed;
                return;
            }

            IEnumerable<MediaFormat> selectedFormats;
            if (_allMode)
                selectedFormats = Enum.GetValues<MediaFormat>();
            else
                selectedFormats = _activeFormats
                    .Select(f => Enum.TryParse<MediaFormat>(f, out var mf) ? (MediaFormat?)mf : null)
                    .Where(mf => mf.HasValue)
                    .Select(mf => mf!.Value);

            // Collect unique date groups and total no-EXIF count across selected formats
            var uniqueDates = new HashSet<DateTime>();
            int noExifCount = 0;
            foreach (var fmt in selectedFormats)
            {
                if (_dateGroups.TryGetValue(fmt, out var dateMap))
                    foreach (var date in dateMap.Keys)
                        uniqueDates.Add(date);

                if (_noExifFiles.TryGetValue(fmt, out var noExif))
                    noExifCount += noExif.Count;
            }

            int groupCount = uniqueDates.Count;

            ChipGroupsVal.Text = groupCount.ToString();
            ChipGroups.Visibility = groupCount > 0 ? Visibility.Visible : Visibility.Collapsed;

            ChipNoExifVal.Text = noExifCount.ToString();
            ChipNoExif.Visibility = noExifCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ApplyFilter()
        {
            if (_allMode)
                FileList.ItemsSource = _allFiles;
            else
                FileList.ItemsSource = _allFiles.Where(f => _activeFormats.Contains(f.Format)).ToList();

            UpdateGroupAndNoExifPills();
            UpdateOrganizeButton();
        }

        private void UpdateOrganizeButton()
        {
            if (!_folderLoaded) return;
            bool hasReady = _allMode
                ? _allFiles.Any(f => f.StatusText == "Ready")
                : _allFiles.Any(f => f.StatusText == "Ready" && _activeFormats.Contains(f.Format));
            OrganizeBtn.IsEnabled = hasReady;
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

            // Reset done state from previous organize
            if (_isDone)
            {
                _isDone = false;
                ProgressStrip.Visibility = Visibility.Collapsed;
            }

            _sourceFolder = folder.Path;

            // Scan root-level supported files
            var dir = new DirectoryInfo(folder.Path);
            _scannedFiles = dir.EnumerateFiles()
                .Where(f => MediaFormatExtensions.IsSupportedExtension(f.Extension))
                .OrderBy(f => f.Name)
                .ToList();

            _allMode = true;
            _activeFormats.Clear();
            SetAllModeVisuals();

            if (_scannedFiles.Count == 0)
            {
                _folderLoaded = false;
                ShowNoFilesState();
            }
            else
            {
                _folderLoaded = true;
                NoFilesState.Visibility = Visibility.Collapsed;
                LoadFiles(_scannedFiles);
                await ExtractMetadataAsync(_allFiles);
            }
        }

        // ── Theme Toggle ──
        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isDark = ThemeToggle.IsChecked == true;
            RootGrid.RequestedTheme = isDark ? ElementTheme.Dark : ElementTheme.Light;

            UpdateChipVisuals();
            UpdateFormatDropdownVisuals(_currentFormat);

            if (_folderLoaded)
            {
                var accent = IsDark ? Color.FromArgb(255, 99, 179, 237) : Color.FromArgb(255, 0, 120, 212);
                FolderPath.Foreground = new SolidColorBrush(accent);
            }

            UpdateCaptionButtonColors(isDark);
            App.Settings.IsDarkTheme = isDark;
        }


        private void ShowNoFilesState()
        {
            EmptyState.Visibility = Visibility.Collapsed;
            NoFilesState.Visibility = Visibility.Visible;
            FileList.ItemsSource = null;

            // Hide chip count values
            ChipAllVal.Visibility = Visibility.Collapsed;
            CntJpg.Visibility = Visibility.Collapsed;
            CntHeic.Visibility = Visibility.Collapsed;
            CntPng.Visibility = Visibility.Collapsed;
            CntMov.Visibility = Visibility.Collapsed;

            // Hide Groups and No EXIF pills
            ChipGroups.Visibility = Visibility.Collapsed;
            ChipNoExif.Visibility = Visibility.Collapsed;

            OrganizeBtn.IsEnabled = false;
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

            // Disabled chips (0 count) stay dim; otherwise toggle active/inactive
            if (!chip.IsEnabled)
                chip.Opacity = 0.25;
            else
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
            if (sender is Button chip && chip.IsEnabled && !IsChipActive(chip))
                chip.Opacity = 1.0;
        }

        private void Chip_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button chip && chip.IsEnabled && !IsChipActive(chip))
                chip.Opacity = 0.38;
        }

        // ── Organize ──
        private void Organize_Click(object sender, RoutedEventArgs e)
        {
            if (_allMode || _activeFormats.Count == Enum.GetValues<MediaFormat>().Length)
            {
                StartOrganize();
                return;
            }
            ShowPartialOrganizeDialog();
        }

        private void ShowPartialOrganizeDialog()
        {
            PartialOrganizePanel.PopulateBadges(_activeFormats, _allFiles);
            PartialOrganizeOverlay.Visibility = Visibility.Visible;
        }

        private void PartialOrganizeCancel_Click(object sender, RoutedEventArgs e)
        {
            PartialOrganizeOverlay.Visibility = Visibility.Collapsed;
        }

        private void PartialOrganizeContinue_Click(object sender, RoutedEventArgs e)
        {
            PartialOrganizeOverlay.Visibility = Visibility.Collapsed;
            StartOrganize();
        }

        private async void StartOrganize()
        {
            // Disable all toolbar buttons
            FolderPickBtn.IsEnabled = false;
            OrganizeBtn.IsEnabled = false;
            FormatDropdownBtn.IsEnabled = false;
            FormatFlyout.Hide();

            // Disable filter chips and theme toggle
            ChipAll.IsEnabled = false;
            foreach (var (chip, _) in GetFormatChips())
                chip.IsEnabled = false;
            ThemeToggle.IsEnabled = false;

            // Determine which files to organize (only Ready files)
            var filesToMove = _allFiles
                .Where(f => f.StatusText == "Ready")
                .Where(f => _allMode || _activeFormats.Contains(f.Format))
                .ToList();

            // Show progress strip in determinate mode
            ProgressSpinner.Visibility = Visibility.Visible;
            ProgressDoneIcon.Visibility = Visibility.Collapsed;
            ProgressStrip.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = filesToMove.Count;
            ProgressBar.Value = 0;
            ProgressLabel.Text = "Organizing files\u2026";
            ProgressDetail.Text = $"0 / {filesToMove.Count} files";
            ProgressPct.Text = "0%";

            int completed = 0;
            int errors = 0;

            foreach (var file in filesToMove)
            {
                try
                {
                    var destSubFolder = Path.Combine(_sourceFolder, file.DestFolder);
                    await Task.Run(() =>
                    {
                        Directory.CreateDirectory(destSubFolder);
                        var destPath = Path.Combine(destSubFolder, file.Name);

                        // Handle name collision by appending suffix
                        if (File.Exists(destPath))
                        {
                            var nameWithoutExt = Path.GetFileNameWithoutExtension(file.Name);
                            var ext = Path.GetExtension(file.Name);
                            int suffix = 1;
                            do
                            {
                                destPath = Path.Combine(destSubFolder, $"{nameWithoutExt} ({suffix}){ext}");
                                suffix++;
                            } while (File.Exists(destPath));
                        }

                        File.Move(file.FilePath, destPath);
                    });

                    file.StatusText = "Moved";
                }
                catch
                {
                    file.StatusText = "Error";
                    errors++;
                }

                completed++;
                ProgressBar.Value = completed;
                int pct = (int)((double)completed / filesToMove.Count * 100);
                ProgressDetail.Text = $"{completed} / {filesToMove.Count} files";
                ProgressPct.Text = $"{pct}%";
            }

            // Done state
            _isDone = true;
            ProgressSpinner.Visibility = Visibility.Collapsed;
            ProgressDoneIcon.Visibility = Visibility.Visible;
            int movedCount = completed - errors;
            int folderCount = filesToMove
                .Where(f => f.StatusText == "Moved")
                .Select(f => f.DestFolder)
                .Distinct()
                .Count();
            ProgressLabel.Text = errors > 0
                ? $"Done \u2014 {movedCount} files organized, {errors} errors"
                : "Done \u2014 all files organized";
            ProgressDetail.Text = $"{movedCount} files moved into {folderCount} folders";
            ProgressPct.Text = "";

            // Re-enable everything except Organize button
            FolderPickBtn.IsEnabled = true;
            FormatDropdownBtn.IsEnabled = true;
            ThemeToggle.IsEnabled = true;
            ChipAll.IsEnabled = true;
            foreach (var (chip, fmt) in GetFormatChips())
            {
                int count = _allFiles.Count(f => f.Format == fmt.ToString());
                chip.IsEnabled = count > 0;
            }
            UpdateOrganizeButton();
        }

        // ── Format Dropdown ──
        private void FormatItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string fmt)
            {
                _currentFormat = fmt;
                FormatLabel.Text = fmt;
                UpdateFormatDropdownVisuals(fmt);
                UpdateAllDestFolders();
                SaveFormatToSettings();
                FormatFlyout.Hide();
            }
        }

        private void CustomFormatItem_Click(object sender, RoutedEventArgs e)
        {
            CustomFmtPanel.FormatString = _customFormat;
            CustomFmtOverlay.Visibility = Visibility.Visible;
            CustomFmtPanel.Revalidate();
            FormatFlyout.Hide();
        }

        private void UpdateFormatDropdownVisuals(string selectedFormat)
        {
            bool dark = IsDark;
            var accentBrush = new SolidColorBrush(dark
                ? Color.FromArgb(255, 99, 179, 237) : Color.FromArgb(255, 0, 120, 212));
            var dimBrush = new SolidColorBrush(dark
                ? Color.FromArgb(255, 156, 163, 175) : Color.FromArgb(255, 121, 119, 117));
            var selectedBg = new SolidColorBrush(dark
                ? Color.FromArgb(0x12, 99, 179, 237) : Color.FromArgb(0x14, 0, 120, 212));
            var transparentBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            foreach (var child in FormatItemsPanel.Children)
            {
                if (child is not Button btn) continue;

                bool isCustom = btn == CustomFormatItem;
                bool isSelected = btn.Tag is string tag && tag == selectedFormat;
                bool isCustomSelected = isCustom && !FormatItemsPanel.Children
                    .OfType<Button>()
                    .Any(b => b != CustomFormatItem && b.Tag is string t && t == selectedFormat);

                bool selected = isSelected || isCustomSelected;

                btn.Background = selected ? selectedBg : transparentBrush;

                if (btn.Content is Grid grid)
                {
                    foreach (var element in grid.Children)
                    {
                        if (element is TextBlock tb && !isCustom)
                        {
                            tb.Foreground = selected ? accentBrush : dimBrush;
                        }
                        else if (element is FontIcon fi && fi.Glyph == "\uE73E")
                        {
                            fi.Visibility = selected ? Visibility.Visible : Visibility.Collapsed;
                            fi.Foreground = accentBrush;
                        }
                        else if (element is StackPanel sp)
                        {
                            // Custom item text/icon — refresh accent color for theme
                            foreach (var spChild in sp.Children)
                            {
                                if (spChild is TextBlock stb) stb.Foreground = accentBrush;
                                else if (spChild is FontIcon sfi) sfi.Foreground = accentBrush;
                            }
                        }
                    }
                }
            }
        }

        private void CustomFmtPanel_ValidationChanged(object sender, FormatValidationEventArgs e)
        {
            CustomFmtShell.IsPrimaryButtonEnabled = e.IsValid;
            if (e.IsValid || e.ErrorMessage is null)
            {
                CustomFmtError.Visibility = Visibility.Collapsed;
            }
            else
            {
                CustomFmtError.Text = e.ErrorMessage;
                CustomFmtError.Visibility = Visibility.Visible;
            }
        }

        private void CustomFmtApply_Click(object sender, RoutedEventArgs e)
        {
            var fmt = CustomFmtPanel.FormatString;
            _customFormat = fmt;
            _currentFormat = fmt;
            FormatLabel.Text = fmt;
            UpdateFormatDropdownVisuals(fmt);
            UpdateAllDestFolders();
            SaveFormatToSettings();
            CustomFmtOverlay.Visibility = Visibility.Collapsed;
        }

        private void CustomFmtCancel_Click(object sender, RoutedEventArgs e)
        {
            CustomFmtOverlay.Visibility = Visibility.Collapsed;
            // Only revert if custom format wasn't already the active selection
            if (_currentFormat != _customFormat || string.IsNullOrEmpty(_customFormat))
            {
                RevertFormatSelection();
            }
        }

        private void LoadPersistedFormat()
        {
            _currentFormat = App.Settings.CurrentFormat;
            FormatLabel.Text = _currentFormat;
            _customFormat = App.Settings.CustomFormat;
            UpdateFormatDropdownVisuals(_currentFormat);
        }

        private void SaveFormatToSettings()
        {
            App.Settings.CurrentFormat = _currentFormat;
            App.Settings.CustomFormat = _customFormat;
        }

        private void RevertFormatSelection()
        {
            UpdateFormatDropdownVisuals(_currentFormat);
        }

        private string FormatDestFolder(DateTime dateTaken)
        {
            var fmt = FormatLabel.Text;
            return dateTaken.ToString(fmt);
        }

        private void UpdateAllDestFolders()
        {
            foreach (var file in _allFiles)
            {
                if (file.DateTaken.HasValue)
                    file.DestFolder = FormatDestFolder(file.DateTaken.Value);
            }
        }

        // ── Mandatory Update Check ──
        private async void CheckForMandatoryUpdate()
        {
            if (await AppUpdateService.HasMandatoryUpdateAsync())
            {
                MandatoryUpdateOverlay.Visibility = Visibility.Visible;
            }
        }

        private async void MandatoryUpdateOpen_Click(object sender, RoutedEventArgs e)
        {
            var pfn = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            await Windows.System.Launcher.LaunchUriAsync(
                new Uri($"ms-windows-store://pdp/?PFN={pfn}"));
        }

    }
}
