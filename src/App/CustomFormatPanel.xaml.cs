using Microsoft.UI.Xaml;
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
                PreviewBlock.Text = "Preview: ";
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
}
