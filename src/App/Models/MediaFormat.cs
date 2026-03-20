using System;

namespace Alan.Photorganizer.App.Models
{
    public enum MediaFormat
    {
        JPG,
        HEIC,
        PNG,
        MOV
    }

    public static class MediaFormatExtensions
    {
        public static MediaFormat? FromExtension(string extension) =>
            extension.ToUpperInvariant() switch
            {
                ".JPG" or ".JPEG" => MediaFormat.JPG,
                ".HEIC" => MediaFormat.HEIC,
                ".PNG" => MediaFormat.PNG,
                ".MOV" => MediaFormat.MOV,
                _ => null
            };

        public static bool IsSupportedExtension(string extension) =>
            FromExtension(extension) != null;
    }
}
