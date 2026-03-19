using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alan.Photorganizer.App.Models
{
    public class FileItem
    {
        public string Name { get; set; } = "";
        public string Format { get; set; } = "";
        public string CaptureTime { get; set; } = "";
        public string DestFolder { get; set; } = "";
        public bool HasExif { get; set; } = true;

        public SolidColorBrush FormatFg { get; set; } = new();
        public SolidColorBrush FormatBg { get; set; } = new();
        public SolidColorBrush FormatBd { get; set; } = new();
        public string DestIcon { get; set; } = "\uE8B7";
        public SolidColorBrush DestFg { get; set; } = new();
        public string StatusText { get; set; } = "Ready";
        public SolidColorBrush StatusFg { get; set; } = new();
        public SolidColorBrush StatusBg { get; set; } = new();
        public SolidColorBrush StatusBd { get; set; } = new();
    }
}
