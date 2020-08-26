// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColorExtensions.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace InTheHand.Forms
{
    public static class ColorExtensions
    {
        [Obsolete("Use Xamarin.Forms.Platform.UWP.ColorExtensions.ToWindowsColor()")]
        public static Windows.UI.Color ToWindows(this Xamarin.Forms.Color color)
        {
            return Windows.UI.Color.FromArgb(Convert.ToByte(color.A * 255), Convert.ToByte(color.R * 255), Convert.ToByte(color.G * 255), Convert.ToByte(color.B * 255));
        }
    }
}
