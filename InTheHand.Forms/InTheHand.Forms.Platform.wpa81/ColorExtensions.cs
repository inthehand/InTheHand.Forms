using System;

namespace InTheHand.Forms.Platform.WinRT
{
    public static class ColorExtensions
    {
        public static Windows.UI.Color ToWindows(this Xamarin.Forms.Color color)
        {
            return Windows.UI.Color.FromArgb(Convert.ToByte(color.A * 255), Convert.ToByte(color.R * 255), Convert.ToByte(color.G * 255), Convert.ToByte(color.B * 255));
        }
    }
}
