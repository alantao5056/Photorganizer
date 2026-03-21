namespace Alan.Photorganizer.App.Models
{
    public class FileItem
    {
        public string Name { get; set; } = "";
        public string Format { get; set; } = "";
        public string CaptureTime { get; set; } = "";
        public string DestFolder { get; set; } = "";
        public bool HasExif { get; set; } = true;
        public string StatusText { get; set; } = "Ready";
    }
}
