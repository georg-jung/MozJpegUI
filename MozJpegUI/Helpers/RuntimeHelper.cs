using System.Runtime.InteropServices;
using System.Text;

namespace MozJpegUI.Helpers;

public static class RuntimeHelper
{
    private static readonly Lazy<bool> _isMsix = new(DetermineIsMsix);

    public static bool IsMSIX => _isMsix.Value;

    private static bool DetermineIsMsix()
    {
        var length = 0;
        return GetCurrentPackageFullName(ref length, null) != 15700L;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);
}
