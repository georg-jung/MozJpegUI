using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MozJpegUI.Contracts.Services;
using MozJpegUI.Helpers;
using MozJpegUI.ViewModels;

namespace MozJpegUI.Services;

internal class ConversionCoordinator : IConversionCoordinator
{
    private readonly ConcurrentQueue<ConversionViewModel> queue = new();

    public void Add(ConversionViewModel conversion)
    {
        queue.Enqueue(conversion);
    }

    public async Task ProcessRemaining(IConversionCoordinator.Options options)
    {
        while (queue.TryDequeue(out var conv))
        {
            conv.Status = ConversionViewModel.ConversionStatus.Processing;
            byte[] result;
            try
            {
                result = await Task.Run(() => MozJpegRecompressor.CompressSingle(conv.FilePath, options.JpegQuality)).ConfigureAwait(true);
                conv.NewSize = result.Length;

                if (conv.WasJpeg && conv.NewRate!.Value > options.MaxNewRate)
                {
                    conv.Status = ConversionViewModel.ConversionStatus.Skipped;
                }
                else
                {
                    if (conv.WasJpeg)
                    {
                        if (options.KeepBackup)
                        {
                            var bakFile = conv.FilePath;
                            while (File.Exists(bakFile))
                            {
                                bakFile += ".bak";
                            }

                            File.Move(conv.FilePath, bakFile);
                        }

                        await File.WriteAllBytesAsync(conv.FilePath, result).ConfigureAwait(true);
                    }
                    else
                    {
                        var extLess = conv.FilePath.Substring(0, conv.FilePath.Length - conv.FileExtension.Length);
                        var outputPath = extLess + ".jpg";
                        using var f = File.Open(outputPath, FileMode.CreateNew);
                        await f.WriteAsync(result).ConfigureAwait(true);
                        await f.FlushAsync().ConfigureAwait(true);
                        await f.DisposeAsync().ConfigureAwait(true);
                    }

                    conv.Status = ConversionViewModel.ConversionStatus.Finished;
                }
            }
            catch (Exception ex)
            {
                conv.Status = ConversionViewModel.ConversionStatus.Error;
                conv.Error = ex;
            }
        }
    }
}
