﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MozJpegUI.ViewModels;

public partial class FolderDroppedDialogViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool _includeSubdirectories;
}
