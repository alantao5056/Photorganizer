using System.Collections.Generic;
using Alan.Sortify.App.Services;
using Xunit;

namespace Alan.Sortify.Tests
{
    public class LocalSettingsServiceTests
    {
        private static LocalSettingsService CreateService(Dictionary<string, object>? seed = null)
            => new(seed ?? new Dictionary<string, object>());

        // ── IsDarkTheme ──

        [Fact]
        public void IsDarkTheme_DefaultsToFalse()
        {
            var sut = CreateService();
            Assert.False(sut.IsDarkTheme);
        }

        [Fact]
        public void IsDarkTheme_RoundTrips()
        {
            var sut = CreateService();
            sut.IsDarkTheme = true;
            Assert.True(sut.IsDarkTheme);

            sut.IsDarkTheme = false;
            Assert.False(sut.IsDarkTheme);
        }

        [Fact]
        public void IsDarkTheme_IgnoresNonBoolValues()
        {
            var store = new Dictionary<string, object> { ["IsDarkTheme"] = "true" };
            var sut = CreateService(store);
            Assert.False(sut.IsDarkTheme);
        }

        // ── CurrentFormat ──

        [Fact]
        public void CurrentFormat_DefaultsToYyyyMmDd()
        {
            var sut = CreateService();
            Assert.Equal("yyyy-MM-dd", sut.CurrentFormat);
        }

        [Fact]
        public void CurrentFormat_RoundTrips()
        {
            var sut = CreateService();
            sut.CurrentFormat = "yyyy/MM/dd";
            Assert.Equal("yyyy/MM/dd", sut.CurrentFormat);
        }

        [Fact]
        public void CurrentFormat_IgnoresNonStringValues()
        {
            var store = new Dictionary<string, object> { ["CurrentFormat"] = 42 };
            var sut = CreateService(store);
            Assert.Equal("yyyy-MM-dd", sut.CurrentFormat);
        }

        // ── CustomFormat ──

        [Fact]
        public void CustomFormat_DefaultsToEmpty()
        {
            var sut = CreateService();
            Assert.Equal(string.Empty, sut.CustomFormat);
        }

        [Fact]
        public void CustomFormat_RoundTrips()
        {
            var sut = CreateService();
            sut.CustomFormat = "dd-MMM-yyyy";
            Assert.Equal("dd-MMM-yyyy", sut.CustomFormat);
        }

        // ── Persisted seed values ──

        [Fact]
        public void ReadsPreExistingValues()
        {
            var store = new Dictionary<string, object>
            {
                ["IsDarkTheme"] = true,
                ["CurrentFormat"] = "MM/dd/yyyy",
                ["CustomFormat"] = "custom!"
            };
            var sut = CreateService(store);

            Assert.True(sut.IsDarkTheme);
            Assert.Equal("MM/dd/yyyy", sut.CurrentFormat);
            Assert.Equal("custom!", sut.CustomFormat);
        }

        [Fact]
        public void SetterWritesBackToStore()
        {
            var store = new Dictionary<string, object>();
            var sut = CreateService(store);

            sut.IsDarkTheme = true;
            sut.CurrentFormat = "yyyy_MM";
            sut.CustomFormat = "my fmt";

            Assert.Equal(true, store["IsDarkTheme"]);
            Assert.Equal("yyyy_MM", store["CurrentFormat"]);
            Assert.Equal("my fmt", store["CustomFormat"]);
        }
    }
}
