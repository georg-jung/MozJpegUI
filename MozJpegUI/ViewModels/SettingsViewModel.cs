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
    private readonly ILocalSettingsService _localSettingsService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private int _selectedMinSizeReduction;

    [ObservableProperty]
    private bool _losslessOptimizationOnly;

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        INavigationService navigationService,
        ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _navigationService = navigationService;
        _localSettingsService = localSettingsService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
        _selectedMinSizeReduction = _localSettingsService.MinSizeReduction;
        _losslessOptimizationOnly = _localSettingsService.LosslessOptimizeOnly;
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
    private void SwitchTheme(ElementTheme param)
    {
        if (ElementTheme != param)
        {
            ElementTheme = param;
            _themeSelectorService.SetTheme(param);
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        _navigationService.GoBack();
    }

    partial void OnSelectedMinSizeReductionChanged(int value) => _localSettingsService.MinSizeReduction = value;

    partial void OnLosslessOptimizationOnlyChanged(bool value) => _localSettingsService.LosslessOptimizeOnly = value;
}
