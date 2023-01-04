using Humanizer;
using Microsoft.UI.Xaml.Controls;
using MozJpegUI.ViewModels;

namespace MozJpegUI.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    public MainViewModel ViewModel { get; }
}
