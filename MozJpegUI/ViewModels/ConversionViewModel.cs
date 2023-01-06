using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Humanizer;
using Microsoft.UI.Xaml;
using MozJpegUI.Helpers;

namespace MozJpegUI.ViewModels;

public partial class ConversionViewModel : ObservableRecipient
{
    /// <summary>
    /// This needs to be a valid member of the Microsoft.UI.Xaml.Controls.Symbol enumeration even if we don't want to show anything.
    /// </summary>
    private const string InvalidStatusSymbolPlaceholder = "Previous";

    /// <summary>
    /// This needs to be a valid brush value.
    /// </summary>
    private const string InvalidStatusBrushPlaceholder = "Black";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusSymbol))]
    [NotifyPropertyChangedFor(nameof(StatusSymbolBrush))]
    [NotifyPropertyChangedFor(nameof(StatusSymbolVisible))]
    [NotifyPropertyChangedFor(nameof(ProgressVisible))]
    private ConversionStatus _status;

    [ObservableProperty]
    private Exception? _error;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewRate))]
    [NotifyPropertyChangedFor(nameof(SizeDisplay))]
    private long? _oldSize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewRate))]
    [NotifyPropertyChangedFor(nameof(SizeDisplay))]
    private long? _newSize;

    private ConversionViewModel(string filePath)
    {
        FilePath = filePath;
        FileExtension = Path.GetExtension(filePath);

        // Include jpe and jfif?
        // See https://de.wikipedia.org/wiki/JPEG_File_Interchange_Format
        WasJpeg = ".jpg".Equals(FileExtension, StringComparison.OrdinalIgnoreCase) ||
            ".jpeg".Equals(FileExtension, StringComparison.OrdinalIgnoreCase);
    }

    public enum ConversionStatus
    {
        NotStarted,
        Processing,
        Skipped,
        Directory,
        Finished,
        Error,
    }

    public string FilePath { get; }

    public string FileExtension { get; }

    public bool WasJpeg { get; }

    public double? NewRate => NewSize.HasValue && OldSize.HasValue ? (double)NewSize.Value / OldSize.Value : null;

    public string SizeDisplay
    {
        get
        {
            if (NewSize.HasValue)
            {
                var percentageChange = (1.0 - NewRate!.Value) * -1;
                return $"{OldSize!.Value.Bytes()} ➔ {NewSize.Value.Bytes()} ({(percentageChange > 0 ? "+" : string.Empty)}{percentageChange:p})";
            }

            if (OldSize.HasValue)
            {
                return $"{OldSize!.Value.Bytes()}";
            }

            return string.Empty;
        }
    }

    public string? StatusSymbol => _status switch
    {
        ConversionStatus.NotStarted => InvalidStatusSymbolPlaceholder,
        ConversionStatus.Processing => InvalidStatusSymbolPlaceholder,
        ConversionStatus.Skipped => "Forward",
        ConversionStatus.Finished => "Accept",
        ConversionStatus.Error => "Important",
        _ => throw new NotImplementedException(),
    };

    public string? StatusSymbolBrush => _status switch
    {
        ConversionStatus.NotStarted => InvalidStatusBrushPlaceholder,
        ConversionStatus.Processing => InvalidStatusBrushPlaceholder,
        ConversionStatus.Skipped => "LightGray",
        ConversionStatus.Finished => "ForestGreen",
        ConversionStatus.Error => "Crimson",
        _ => throw new NotImplementedException(),
    };

    public string? StatusToolTip => _status switch
    {
        ConversionStatus.NotStarted => null,
        ConversionStatus.Processing => null,
        ConversionStatus.Skipped => "Skipped".GetLocalized(),
        ConversionStatus.Finished => "Finished".GetLocalized(),
        ConversionStatus.Error => "Error".GetLocalized(),
        _ => throw new NotImplementedException(),
    };

    public bool StatusSymbolVisible => _status != ConversionStatus.Processing && _status != ConversionStatus.NotStarted;

    public bool ProgressVisible => !StatusSymbolVisible;

    public bool ProgressIsActive => _status == ConversionStatus.Processing;

    public static ConversionViewModel Create(string filePath)
    {
        var x = new ConversionViewModel(filePath);
        if (Directory.Exists(filePath))
        {
            x.Status = ConversionStatus.Directory;
            return x;
        }

        if (!File.Exists(filePath))
        {
            x.Status = ConversionStatus.Error;
            x.Error = new FileNotFoundException("DroppedItemDoesNotExist".GetLocalized(), filePath);
            return x;
        }

        var fi = new FileInfo(filePath);
        x.OldSize = fi.Length;
        return x;
    }
}
