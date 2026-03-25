using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;

namespace Alan.Photorganizer.App
{
    public sealed partial class CustomFormatPanel : UserControl
    {
        private static readonly char[] InvalidFolderChars = { '\\', '/', ':', '*', '"', '<', '>', '|' };
        private static readonly string[] FormatTokens =
            { "yyyy", "yy", "MMMM", "MMM", "MM", "M", "dddd", "ddd", "dd", "d" };

        public event EventHandler<FormatValidationEventArgs>? ValidationChanged;

        public string FormatString
        {
            get => FormatBox.Text.Trim();
            set => FormatBox.Text = value ?? "";
        }

        public CustomFormatPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Re-run validation and raise ValidationChanged. Call after the panel becomes visible.
        /// </summary>
        public void Revalidate() => Validate(FormatBox.Text.Trim());

        private void FormatBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var fmt = FormatBox.Text.Trim();
            Validate(fmt);
        }

        private void Validate(string fmt)
        {
            var errorBrush = Application.Current.Resources.TryGetValue("ErrorBrush", out var eb)
                ? (SolidColorBrush)eb : null;
            var normalBrush = Application.Current.Resources.TryGetValue(
                "SystemControlForegroundBaseMediumLowBrush", out var nb)
                ? (SolidColorBrush)nb : null;

            // Empty input
            if (string.IsNullOrEmpty(fmt))
            {
                PreviewBlock.Text = "Preview: ";
                InputBorder.BorderBrush = normalBrush;
                ValidationChanged?.Invoke(this,
                    new FormatValidationEventArgs(false, null));
                return;
            }

            // Must contain at least one recognized format token
            bool hasToken = false;
            foreach (var token in FormatTokens)
            {
                if (fmt.Contains(token, StringComparison.Ordinal))
                {
                    hasToken = true;
                    break;
                }
            }
            if (!hasToken)
            {
                PreviewBlock.Text = "Preview: ";
                InputBorder.BorderBrush = errorBrush;
                ValidationChanged?.Invoke(this,
                    new FormatValidationEventArgs(false, "Must include at least one format token."));
                return;
            }

            // Try to expand the format string
            string expanded;
            try
            {
                expanded = DateTime.Now.ToString(fmt);
            }
            catch
            {
                PreviewBlock.Text = "Invalid format";
                InputBorder.BorderBrush = errorBrush;
                ValidationChanged?.Invoke(this,
                    new FormatValidationEventArgs(false, "Invalid format pattern."));
                return;
            }

            // Check for invalid folder characters in expanded result
            int idx = expanded.IndexOfAny(InvalidFolderChars);
            if (idx >= 0)
            {
                PreviewBlock.Text = "Preview: " + expanded;
                InputBorder.BorderBrush = errorBrush;
                ValidationChanged?.Invoke(this,
                    new FormatValidationEventArgs(false,
                        $"Result contains invalid folder character '{expanded[idx]}'."));
                return;
            }

            // Valid
            PreviewBlock.Text = "Preview: " + expanded;
            InputBorder.BorderBrush = normalBrush;
            ValidationChanged?.Invoke(this, new FormatValidationEventArgs(true, null));
        }

        private void Token_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton btn && btn.Content is string token)
            {
                var pos = FormatBox.SelectionStart;
                var text = FormatBox.Text;
                FormatBox.Text = text.Insert(pos, token);
                FormatBox.SelectionStart = pos + token.Length;
                FormatBox.Focus(FocusState.Programmatic);
            }
        }
    }

    public class FormatValidationEventArgs : EventArgs
    {
        public bool IsValid { get; }
        public string? ErrorMessage { get; }

        public FormatValidationEventArgs(bool isValid, string? errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
    }
}
