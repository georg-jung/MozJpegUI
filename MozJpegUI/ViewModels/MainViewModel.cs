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

    [ObservableProperty]
    private bool _keepBackupFiles = true;

    [ObservableProperty]
    private int _jpegQuality = 85;

    [ObservableProperty]
    private ObservableCollection<ConversionViewModel>? _conversions;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void OnFilesDropped(string[] files)
    {
        Conversions ??= new();
        foreach (var file in files)
        {
            var c = new ConversionViewModel
            {
                FilePath = file,
            };
            Conversions.Add(c);
        }
    }

    [RelayCommand]
    public void ShowSettings()
    {
        _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    }
}
