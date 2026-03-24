using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Alan.Photorganizer.App.Models
{
    public class FileItem : INotifyPropertyChanged
    {
        private string _captureTime = "";
        private string _statusText = "Pending";
        private bool _hasExif = true;

        public string Name { get; set; } = "";
        public string Format { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string DestFolder { get; set; } = "";
        public DateTime? DateTaken { get; set; }

        public string CaptureTime
        {
            get => _captureTime;
            set { _captureTime = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public bool HasExif
        {
            get => _hasExif;
            set { _hasExif = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
