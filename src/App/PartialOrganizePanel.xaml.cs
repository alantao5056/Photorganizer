using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;
using Alan.Sortify.App.Models;

namespace Alan.Sortify.App;

public sealed partial class PartialOrganizePanel : UserControl
{
    public PartialOrganizePanel()
    {
        InitializeComponent();
    }

    public void PopulateBadges(IEnumerable<string> activeFormats, List<FileItem> allFiles)
    {
        var badges = new List<Border>();
        foreach (var fmt in activeFormats.Order())
        {
            int count = allFiles.Count(f => f.Format == fmt);
            var label = $"{fmt}  \u00B7  {count} {(count == 1 ? "file" : "files")}";

            // Resource keys are PascalCase (e.g. "Jpg", "Heic"), format strings are uppercase
            var key = char.ToUpper(fmt[0]) + fmt[1..].ToLower();
            var badge = new Border
            {
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 3, 10, 3),
                Background = (Brush)Application.Current.Resources[$"{key}Background"],
                BorderBrush = (Brush)Application.Current.Resources[$"{key}Border"],
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = label,
                    FontSize = 11,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = (Brush)Application.Current.Resources[$"{key}Foreground"],
                }
            };
            badges.Add(badge);
        }
        BadgeList.ItemsSource = badges;
    }
}
