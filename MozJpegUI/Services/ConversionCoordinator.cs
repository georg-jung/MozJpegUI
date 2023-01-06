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
                if (options.LosslessMode)
                {
                    result = await Task.Run(() => MozJpegRecompressor.OptimizeSingleAsync(conv.FilePath)).ConfigureAwait(true);
                }
                else
                {
                    result = await Task.Run(() => MozJpegRecompressor.RecompressSingle(conv.FilePath, options.JpegQuality)).ConfigureAwait(true);
                }

                conv.NewSize = result.Length;

                if (conv.WasJpeg && conv.NewRate!.Value > options.MaxNewRate)
                {
                    conv.Status = ConversionViewModel.ConversionStatus.Skipped;
                }
                else
                {
                    async Task WriteOutput(string path)
                    {
                        using var f = File.Open(path, FileMode.CreateNew);
                        await f.WriteAsync(result).ConfigureAwait(false);
                        await f.FlushAsync().ConfigureAwait(false);
                        await f.DisposeAsync().ConfigureAwait(false);
                    }

                    var parentDir = Path.GetDirectoryName(conv.FilePath) ?? throw new InvalidOperationException($"{conv.FilePath} is no valid full qualified path.");
                    var fileWoExt = Path.GetFileNameWithoutExtension(conv.FilePath);
                    var ext = Path.GetExtension(conv.FilePath);
                    var filePathWoExt = Path.Combine(parentDir, fileWoExt);
                    var outputPath = filePathWoExt + ".jpg";

                    try
                    {
                        await WriteOutput(outputPath).ConfigureAwait(true);
                    }
                    catch (IOException)
                    {
                        // target exists
                        if (options.KeepOriginals)
                        {
                            var origFile = Path.Combine(parentDir, "Original_" + fileWoExt + ext);
                            var cnt = 2;
                            while (File.Exists(origFile))
                            {
                                origFile = Path.Combine(parentDir, $"Original{cnt}_" + fileWoExt + ext);
                            }

                            File.Move(outputPath, origFile);
                            await WriteOutput(outputPath).ConfigureAwait(true);
                        }
                        else
                        {
                            // we need to keep a bak though until write was successfull, otherwise full loss might happen
                            var bakFile = outputPath + ".bak";
                            while (File.Exists(bakFile))
                            {
                                bakFile += ".bak";
                            }

                            File.Move(outputPath, bakFile);
                            await WriteOutput(outputPath).ConfigureAwait(true);
                            File.Delete(bakFile);
                        }
                    }

                    // this is relevant for non-jpgs
                    // check path equality https://stackoverflow.com/a/7345023/1200847
                    if (!options.KeepOriginals && !string.Equals(Path.GetFullPath(conv.FilePath), Path.GetFullPath(outputPath)))
                    {
                        File.Delete(conv.FilePath);
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
