using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MozJpegUI.Contracts.Services;
using MozJpegUI.Helpers;
using Nito.AsyncEx;

namespace MozJpegUI.ViewModels;

public partial class MainViewModel : ObservableRecipient, IFilesDropped
{
    private readonly INavigationService _navigationService;
    private readonly IConversionCoordinator _conversionCoordinator;
    private readonly ILocalSettingsService _localSettingsService;

    [ObservableProperty]
    private bool _keepOriginals = true;

    [ObservableProperty]
    private int _jpegQuality = 85;

    [ObservableProperty]
    private bool _losslessMode;

    [ObservableProperty]
    private bool _qualitySettingsVisible;

    [ObservableProperty]
    private ObservableCollection<ConversionViewModel>? _conversions;

    public MainViewModel(INavigationService navigationService, IConversionCoordinator conversionCoordinator, ILocalSettingsService localSettingsService)
    {
        _navigationService = navigationService;
        _conversionCoordinator = conversionCoordinator;
        _localSettingsService = localSettingsService;
        LosslessMode = _localSettingsService.LosslessOptimizeOnly;
        QualitySettingsVisible = !LosslessMode;
    }

    private enum DirectoryPolicy
    {
        Skip,
        IncludeDirectChildren,
        IncludeSubdirectoryTree,
    }

    public async void OnFilesDropped(string[] files)
    {
        var dirHandling = new AsyncLazy<DirectoryPolicy>(AskForDirectoryPolicy, AsyncLazyFlags.ExecuteOnCallingThread);
        var maxNewRate = (100 - _localSettingsService.MinSizeReduction) / 100.0;
        var options = new IConversionCoordinator.Options(JpegQuality, _localSettingsService.LosslessOptimizeOnly, maxNewRate, KeepOriginals);

        Conversions ??= new();
        foreach (var file in files)
        {
            var c = ConversionViewModel.Create(file);

            if (c.Status == ConversionViewModel.ConversionStatus.Directory)
            {
                // var policy = await dirHandling;
                c.Status = ConversionViewModel.ConversionStatus.Skipped;
            }

            if (c.Status == ConversionViewModel.ConversionStatus.NotStarted)
            {
                _conversionCoordinator.Add(c);
            }

            // c.Status = (ConversionViewModel.ConversionStatus)Random.Shared.Next(5);
            // c.NewSize = c.OldSize.HasValue ? (long)(c.OldSize.Value * Random.Shared.NextDouble()) : null;
            Conversions.Add(c);
        }

        // leave one core untouched, or two if there are >= 8
        var untouched = Environment.ProcessorCount >= 8 ? 2 : 1;
        var threads = Math.Max(1, Environment.ProcessorCount - untouched);
        var tasks = Enumerable.Range(0, threads).Select(x => _conversionCoordinator.ProcessRemaining(options));
        await Task.WhenAll(tasks);
    }

    [RelayCommand]
    public void ShowSettings()
    {
        _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    }

    private async Task<DirectoryPolicy> AskForDirectoryPolicy()
    {
        var vm = new FolderDroppedDialogViewModel();
        var diag = new ContentDialog
        {
            XamlRoot = App.MainWindow.Content.XamlRoot,
            PrimaryButtonText = "Skip",
            SecondaryButtonText = "Include",
            DefaultButton = ContentDialogButton.Primary,
            Content = new Views.FolderDroppedDialogPage(vm),
        };
        var res = await diag.ShowAsync();

        return (res, vm.IncludeSubdirectories) switch
        {
            (ContentDialogResult.Primary, _) => DirectoryPolicy.Skip,
            (ContentDialogResult.Secondary, true) => DirectoryPolicy.IncludeSubdirectoryTree,
            (ContentDialogResult.Secondary, false) => DirectoryPolicy.IncludeDirectChildren,
            _ => throw new NotImplementedException(),
        };
    }
}
