using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Alan.Photorganizer.App
{
    public sealed partial class StatusPill : UserControl
    {
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(string), typeof(StatusPill),
                new PropertyMetadata("Pending", OnStatusChanged));

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public StatusPill()
        {
            InitializeComponent();
            ActualThemeChanged += (_, _) => UpdateVisuals();
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StatusPill)d).UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            var status = Status;
            if (string.IsNullOrEmpty(status)) return;

            // Map status text to resource key prefix: "Pending"→"Pending", "Ready"→"Ready", "No EXIF"→"NoExif"
            var key = status switch
            {
                "No EXIF" => "NoExif",
                _ => status
            };

            PillText.Text = status;
            PillBorder.Background = Lookup($"{key}Background");
            PillBorder.BorderBrush = Lookup($"{key}Border");
            PillText.Foreground = Lookup($"{key}Foreground");
            PillDot.Fill = Lookup($"{key}Foreground");
        }

        private Brush Lookup(string key) =>
            Resources.TryGetValue(key, out var val) ? (Brush)val
            : (Brush)Application.Current.Resources[key];
    }
}
