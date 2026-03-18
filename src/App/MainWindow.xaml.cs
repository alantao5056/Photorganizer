using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace Alan.Photorganizer.App
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
    }
}
