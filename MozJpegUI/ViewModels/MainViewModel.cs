using System.Diagnostics;
using System.Windows.Input;
using MozJpegUI.Contracts.Services;
using MozJpegUI.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MozJpegUI.ViewModels;

public partial class MainViewModel : ObservableRecipient, IFilesDropped
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool _keepBackupFiles = true;

    [ObservableProperty]
    private int _jpegQuality = 85;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void OnFilesDropped(string[] files)
    {
        Debug.Print("---");
        foreach (var file in files)
        {
            Debug.Print($"{file}");
        }
    }

    [RelayCommand]
    public void ShowSettings()
    {
        _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    }
}
