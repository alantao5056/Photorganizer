namespace Alan.Photorganizer.App.Services;

public interface ISettingsService
{
    bool IsDarkTheme { get; set; }
    string CurrentFormat { get; set; }
    string CustomFormat { get; set; }
}
