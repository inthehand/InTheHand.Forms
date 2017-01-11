// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnPlatform2.cs" company="In The Hand Ltd">
//   Copyright (c) 2015-2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace InTheHand.Forms
{
    /// <summary>
    /// Replacement for Xamarin.Forms.OnPlatform which supports the Windows (WinRT) platforms.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class OnPlatform2<T>
    {
        public OnPlatform2()
        {
            Android = default(T);
            iOS = default(T);
            Tizen = default(T);
            WinPhone = default(T);
            Windows = default(T);
            Other = default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        public T Android{ get; set;}

        /// <summary>
        /// 
        /// </summary>
        public T iOS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public T Tizen { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public T WinPhone { get; set; }

        /// <summary>
        /// The value to use for WinRT (Windows Phone 8.1, Windows 8.1, Windows 10).
        /// </summary>
        public T Windows { get; set; }

        /// <summary>
        /// Currently unused.
        /// </summary>
        public T Other { get; set; }

        public static implicit operator T(InTheHand.Forms.OnPlatform2<T> onPlatform)
        {
            switch(Xamarin.Forms.Device.OS)
            {
                case Xamarin.Forms.TargetPlatform.Android:
                    return onPlatform.Android;

                case Xamarin.Forms.TargetPlatform.iOS:
                    return onPlatform.iOS;

                case (Xamarin.Forms.TargetPlatform)5:
                    return onPlatform.Tizen;

                case Xamarin.Forms.TargetPlatform.WinPhone:
                    return onPlatform.WinPhone;

                case Xamarin.Forms.TargetPlatform.Windows:
                    return onPlatform.Windows;

                default:
                    return onPlatform.Other;
            }
        }   
    }
}
