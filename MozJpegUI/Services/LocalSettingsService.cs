using System.Text.Json;
using MozJpegUI.Contracts.Services;
using Windows.Storage;

namespace MozJpegUI.Services;

public class LocalSettingsService : ILocalSettingsService
{
    public int? MinSizeReduction
    {
        get => ReadSetting<int>(SettingsKeys.MinSizeReductionPercentage);
        set => SaveSetting(SettingsKeys.MinSizeReductionPercentage, value);
    }

    public T? ReadSetting<T>(string key)
    {
        if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
        {
            return JsonSerializer.Deserialize<T>((string)obj);
        }

        return default;
    }

    public void SaveSetting<T>(string key, T value)
    {
        ApplicationData.Current.LocalSettings.Values[key] = JsonSerializer.Serialize(value);
    }
}
