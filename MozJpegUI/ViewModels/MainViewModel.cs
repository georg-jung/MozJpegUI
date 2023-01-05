using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MozJpegUI.Contracts.Services;
using MozJpegUI.Helpers;

namespace MozJpegUI.ViewModels;

public partial class MainViewModel : ObservableRecipient, IFilesDropped
{
    private readonly INavigationService _navigationService;
    private readonly IConversionCoordinator _conversionCoordinator;
    private readonly ILocalSettingsService _localSettingsService;
    [ObservableProperty]
    private bool _keepBackupFiles = true;

    [ObservableProperty]
    private int _jpegQuality = 85;

    [ObservableProperty]
    private ObservableCollection<ConversionViewModel>? _conversions;

    public MainViewModel(INavigationService navigationService, IConversionCoordinator conversionCoordinator, ILocalSettingsService localSettingsService)
    {
        _navigationService = navigationService;
        _conversionCoordinator = conversionCoordinator;
        _localSettingsService = localSettingsService;
    }

    public void OnFilesDropped(string[] files)
    {
        var maxNewRate = (100 - _localSettingsService.MinSizeReduction) / 100.0;
        var options = new IConversionCoordinator.Options(JpegQuality, maxNewRate, KeepBackupFiles);

        Conversions ??= new();
        foreach (var file in files)
        {
            var c = ConversionViewModel.Create(file);

            // c.Status = (ConversionViewModel.ConversionStatus)Random.Shared.Next(5);
            // c.NewSize = c.OldSize.HasValue ? (long)(c.OldSize.Value * Random.Shared.NextDouble()) : null;
            Conversions.Add(c);
            _conversionCoordinator.Add(c);
        }

        // leave one core untouched, or two if there are >= 8
        var untouched = Environment.ProcessorCount >= 8 ? 2 : 1;
        for (var i = 0; i < Math.Max(1, Environment.ProcessorCount - untouched); i++)
        {
            _conversionCoordinator.ProcessRemaining(options);
        }
    }

    [RelayCommand]
    public void ShowSettings()
    {
        _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    }
}
