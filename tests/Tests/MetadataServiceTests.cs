using System;
using System.Globalization;
using Alan.Photorganizer.App.Services;
using Xunit;

namespace Alan.Photorganizer.Tests
{
    public class MetadataServiceTests
    {
        [Theory]
        [InlineData("2024:01:15 10:30:00", 2024, 1, 15, 10, 30, 0)]
        [InlineData("2024-01-15 10:30:00", 2024, 1, 15, 10, 30, 0)]
        [InlineData("2024-01-15T10:30:00", 2024, 1, 15, 10, 30, 0)]
        [InlineData("Fri Jan 09 19:49:55 2026", 2026, 1, 9, 19, 49, 55)]
        [InlineData("Mon Dec 25 08:00:00 2023", 2023, 12, 25, 8, 0, 0)]
        [InlineData("Thu Jul 04 12:30:45 2024", 2024, 7, 4, 12, 30, 45)]
        public void TryParseDate_ValidFormats_ReturnsTrue(
            string input, int year, int month, int day, int hour, int minute, int second)
        {
            var success = MetadataService.TryParseDate(input, out var result);

            Assert.True(success, $"Failed to parse: \"{input}\"");
            Assert.Equal(new DateTime(year, month, day, hour, minute, second), result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not a date")]
        public void TryParseDate_InvalidInput_ReturnsFalse(string? input)
        {
            var success = MetadataService.TryParseDate(input, out _);

            Assert.False(success);
        }
    }
}
