// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MozJpegUI.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace MozJpegUI.Views;

public sealed partial class FolderDroppedDialogPage : Page
{
    public FolderDroppedDialogPage(FolderDroppedDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    public FolderDroppedDialogViewModel ViewModel { get; }
}
