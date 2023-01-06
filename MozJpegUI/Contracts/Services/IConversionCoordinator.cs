using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MozJpegUI.ViewModels;

namespace MozJpegUI.Contracts.Services;

public interface IConversionCoordinator
{
    public void Add(ConversionViewModel conversion);

    public Task ProcessRemaining(Options options);

    public record Options(int JpegQuality, bool LosslessMode, double MaxNewRate, bool KeepOriginals);
}
