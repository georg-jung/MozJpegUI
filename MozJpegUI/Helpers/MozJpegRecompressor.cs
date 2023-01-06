using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MozJpegSharp;

namespace MozJpegUI.Helpers
{
    internal static class MozJpegRecompressor
    {
        public static byte[] RecompressSingle(string file, int quality)
        {
            using var tjc = new MozJpegSharp.TJCompressor();
            byte[] compressed;
            PropertyItem[] exif;

            // This is what mozjpeg/libjpeg-turbo defaults to. See
            // https://github.com/mozilla/mozjpeg/blob/fd569212597dcc249752bd38ea58a4e2072da24f/rdswitch.c#L561-L562
            var targetChrominance = quality switch
            {
                >= 90 => TJSubsamplingOption.Chrominance444,
                >= 80 => TJSubsamplingOption.Chrominance422,
                _ => TJSubsamplingOption.Chrominance420,
            };

            using (var bmp = new Bitmap(file))
            {
                exif = bmp.PropertyItems;
                compressed = tjc.Compress(bmp, targetChrominance, quality, MozJpegSharp.TJFlags.None);
            }

            using var msCompressed = new MemoryStream(compressed);
            using var img = Image.FromStream(msCompressed, false, false);
            foreach (var item in exif)
            {
                img.SetPropertyItem(item);
            }

            using var msOutput = new MemoryStream(msCompressed.Capacity);
            img.Save(msOutput, ImageFormat.Jpeg);
            return msOutput.ToArray();
        }

        public static async Task<byte[]> OptimizeSingleAsync(string file)
        {
            var content = await File.ReadAllBytesAsync(file);
            return MozJpeg.LosslessOptimize(content);
        }
    }
}
