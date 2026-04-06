using System.Collections.Generic;
using Windows.Storage;

namespace Alan.Sortify.App.Services;

public sealed class LocalSettingsService : ISettingsService
{
    private readonly IDictionary<string, object> _values;

    public LocalSettingsService()
        : this(ApplicationData.Current.LocalSettings.Values) { }

    internal LocalSettingsService(IDictionary<string, object> values)
    {
        _values = values;
    }

    public bool IsDarkTheme
    {
        get => _values.TryGetValue(nameof(IsDarkTheme), out var v) && v is bool b && b;
        set => _values[nameof(IsDarkTheme)] = value;
    }

    public string CurrentFormat
    {
        get => _values.TryGetValue(nameof(CurrentFormat), out var v) && v is string s ? s : "yyyy-MM-dd";
        set => _values[nameof(CurrentFormat)] = value;
    }

    public string CustomFormat
    {
        get => _values.TryGetValue(nameof(CustomFormat), out var v) && v is string s ? s : string.Empty;
        set => _values[nameof(CustomFormat)] = value;
    }
}
