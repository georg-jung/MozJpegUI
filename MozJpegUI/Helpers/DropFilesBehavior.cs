using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;

namespace MozJpegUI.Helpers;

// based on https://stackoverflow.com/a/65266427/1200847
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "More readable here because the interface is so small.")]
public interface IFilesDropped
{
    void OnFilesDropped(string[] files);
}

public class DropFilesBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(DropFilesBehavior), PropertyMetadata.Create(default(bool), OnIsEnabledChanged));

    public static readonly DependencyProperty FileDropTargetProperty = DependencyProperty.RegisterAttached(
        "FileDropTarget", typeof(IFilesDropped), typeof(DropFilesBehavior), null);

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetFileDropTarget(DependencyObject obj, IFilesDropped value)
    {
        obj.SetValue(FileDropTargetProperty, value);
    }

    public static IFilesDropped GetFileDropTarget(DependencyObject obj)
    {
        return (IFilesDropped)obj.GetValue(FileDropTargetProperty);
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var fe = d as FrameworkElement ?? throw new InvalidOperationException();

        if ((bool)e.NewValue)
        {
            fe.AllowDrop = true;
            fe.Drop += OnDrop;
            fe.DragOver += OnDragOver;
        }
        else
        {
            fe.AllowDrop = false;
            fe.Drop -= OnDrop;
            fe.DragOver -= OnDragOver;
        }
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move; // or Link/Copy
        e.Handled = true;
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        var dobj = (DependencyObject)sender;
        var target = dobj.GetValue(FileDropTargetProperty);
        var filesDropped = target switch
        {
            IFilesDropped fd => fd,
            null => throw new InvalidOperationException("File drop target is not set."),
            _ => throw new InvalidOperationException($"Binding error, '{target.GetType().Name}' doesn't implement '{nameof(IFilesDropped)}'."),
        };

        if (filesDropped == null)
        {
            return;
        }

        var files = e.DataView.GetStorageItemsAsync().GetAwaiter().GetResult();
        if (files.Count == 0)
        {
            return;
        }

        filesDropped.OnFilesDropped(files.Select(f => f.Path).ToArray());
    }
}
