using Microsoft.UI.Xaml;

namespace MozJpegUI.Contracts.Services;

public interface IThemeSelectorService
{
    ElementTheme Theme
    {
        get;
    }

    Task InitializeAsync();

    void SetTheme(ElementTheme theme);

    void SetRequestedTheme();
}
