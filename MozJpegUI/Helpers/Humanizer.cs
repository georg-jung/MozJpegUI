using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;

namespace MozJpegUI.Helpers
{
    public class Humanizer
    {
        public static string? HumanizeFileBytes(long? bytes) => bytes != null ? bytes.Value.Bytes().ToString() : null;
    }
}
