namespace MozJpegUI.Contracts.Services;

public interface ILocalSettingsService
{
    int MinSizeReduction { get; set; }

    T? ReadSetting<T>(string key);

    void SaveSetting<T>(string key, T value);
}
