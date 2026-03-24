using Microsoft.UI.Xaml.Controls;
using System;

namespace Alan.Photorganizer.App
{
    public sealed partial class CustomFormatPanel : UserControl
    {
        public string FormatString
        {
            get => FormatBox.Text.Trim();
            set => FormatBox.Text = value ?? "";
        }

        public CustomFormatPanel()
        {
            InitializeComponent();
        }

        private void FormatBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var fmt = FormatBox.Text.Trim();
            if (string.IsNullOrEmpty(fmt))
            {
                PreviewBlock.Text = "";
                return;
            }
            try
            {
                PreviewBlock.Text = "Preview: " + DateTime.Now.ToString(fmt);
            }
            catch
            {
                PreviewBlock.Text = "Invalid format";
            }
        }
    }
}
