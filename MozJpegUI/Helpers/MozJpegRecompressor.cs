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
        public static byte[] CompressSingle(string file, int quality)
        {
            using var tjc = new MozJpegSharp.TJCompressor();
            byte[] compressed;
            PropertyItem[] exif;

            using (var bmp = new Bitmap(file))
            {
                exif = bmp.PropertyItems;
                compressed = tjc.Compress(bmp, MozJpegSharp.TJSubsamplingOption.Chrominance420, quality, MozJpegSharp.TJFlags.None);
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
    }
}
