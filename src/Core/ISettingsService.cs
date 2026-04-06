namespace Alan.Sortify.App.Services;

public interface ISettingsService
{
    bool IsDarkTheme { get; set; }
    string CurrentFormat { get; set; }
    string CustomFormat { get; set; }
}
