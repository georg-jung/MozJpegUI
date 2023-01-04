using System;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MozJpegUI.Contracts.Services;
using MozJpegUI.Helpers;
using Windows.ApplicationModel;

namespace MozJpegUI.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private int _selectedMinSizeReduction = 15;

    public SettingsViewModel(IThemeSelectorService themeSelectorService, INavigationService navigationService)
    {
        _themeSelectorService = themeSelectorService;
        _navigationService = navigationService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
    }

    public IReadOnlyCollection<int> SizeReductionSteps => new[] { 1, 5, 10, 15, 20, 30, 40, 50 };

    private static string? GetInformationalVersion() =>
        Assembly
            .GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

    private static string GetVersionDescription()
    {
        string versionStr;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            Version version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            versionStr = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        else
        {
            versionStr = GetInformationalVersion() ?? "UNKNOWN VERSION";
        }
        return $"{"AppDisplayName".GetLocalized()} - {versionStr}";
    }

    [RelayCommand]
    private async Task SwitchThemeAsync(ElementTheme param)
    {
        if (ElementTheme != param)
        {
            ElementTheme = param;
            await _themeSelectorService.SetThemeAsync(param);
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        _navigationService.GoBack();
    }
}
