using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Alan.Photorganizer.App
{
    public sealed partial class FormatBadge : UserControl
    {
        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register(nameof(Format), typeof(string), typeof(FormatBadge),
                new PropertyMetadata("", OnFormatChanged));

        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }

        public FormatBadge()
        {
            InitializeComponent();
            ActualThemeChanged += (_, _) => UpdateVisuals();
        }

        private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FormatBadge)d).UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            var fmt = Format;
            if (string.IsNullOrEmpty(fmt)) return;

            // Resource keys are PascalCase (e.g. "JpgBackground"), Format is uppercase (e.g. "JPG")
            var key = fmt[0] + fmt[1..].ToLowerInvariant();

            BadgeText.Text = fmt;
            BadgeBorder.Background = Lookup($"{key}Background");
            BadgeBorder.BorderBrush = Lookup($"{key}Border");
            BadgeText.Foreground = Lookup($"{key}Foreground");
        }

        private Brush Lookup(string key) =>
            Resources.TryGetValue(key, out var val) ? (Brush)val
            : (Brush)Application.Current.Resources[key];
    }
}
