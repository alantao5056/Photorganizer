using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Alan.Photorganizer.App.Services
{
    public static class MetadataService
    {
        private static readonly string[] DateFormats =
        [
            "yyyy:MM:dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy:MM:dd HH:mm:sszzz",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:sszzz",
        ];

        /// <summary>
        /// Extracts the date-taken from a media file's metadata.
        /// Returns null if no date-taken metadata is found.
        /// </summary>
        public static DateTime? ExtractDateTaken(string filePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                // Try EXIF DateTimeOriginal first (photos)
                var exifSub = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (exifSub != null)
                {
                    var dateStr = exifSub.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
                    if (TryParseDate(dateStr, out var dt))
                        return dt;
                }

                // Try EXIF DateTimeDigitized
                if (exifSub != null)
                {
                    var dateStr = exifSub.GetDescription(ExifDirectoryBase.TagDateTimeDigitized);
                    if (TryParseDate(dateStr, out var dt))
                        return dt;
                }

                // Try EXIF DateTime (IFD0)
                var exifIfd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
                if (exifIfd0 != null)
                {
                    var dateStr = exifIfd0.GetDescription(ExifDirectoryBase.TagDateTime);
                    if (TryParseDate(dateStr, out var dt))
                        return dt;
                }

                // Try QuickTime creation time (MOV/MP4)
                var qt = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                if (qt != null)
                {
                    var dateStr = qt.GetDescription(QuickTimeMovieHeaderDirectory.TagCreated);
                    if (TryParseDate(dateStr, out var dt))
                        return dt;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool TryParseDate(string? dateStr, out DateTime result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(dateStr))
                return false;

            return DateTime.TryParseExact(
                dateStr.Trim(),
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result);
        }
    }
}
