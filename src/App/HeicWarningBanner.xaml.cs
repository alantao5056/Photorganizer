using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Alan.Photorganizer.App
{
    public sealed partial class HeicWarningBanner : UserControl
    {
        public HeicWarningBanner()
        {
            this.InitializeComponent();
        }

        private void BannerDismiss_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
