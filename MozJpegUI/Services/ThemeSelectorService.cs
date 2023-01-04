using Microsoft.UI.Xaml;
using MozJpegUI.Contracts.Services;
using MozJpegUI.Helpers;

namespace MozJpegUI.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string SettingsKey = "AppBackgroundRequestedTheme";

    private readonly ILocalSettingsService _localSettingsService;

    public ThemeSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    public Task InitializeAsync()
    {
        Theme = LoadThemeFromSettings();
        return Task.CompletedTask;
    }

    public void SetTheme(ElementTheme theme)
    {
        Theme = theme;

        SetRequestedTheme();
        SaveThemeInSettings(Theme);
    }

    public void SetRequestedTheme()
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;

            TitleBarHelper.UpdateTitleBar(Theme);
        }
    }

    private ElementTheme LoadThemeFromSettings()
    {
        var themeName = _localSettingsService.ReadSetting<string>(SettingsKey);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    private void SaveThemeInSettings(ElementTheme theme)
    {
        _localSettingsService.SaveSetting(SettingsKey, theme.ToString());
    }
}
