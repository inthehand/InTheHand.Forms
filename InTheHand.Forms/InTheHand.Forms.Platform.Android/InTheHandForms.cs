// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InTheHandForms.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#if __ANDROID__
namespace InTheHand.Forms.Platform.Android
#elif __IOS__
namespace InTheHand.Forms.Platform.iOS
#elif WINDOWS_UWP
namespace InTheHand.Forms.Platform.UWP
#endif
{
    public static class InTheHandForms
    {
        public static void Init()
        {
            //this call is necessary just to ensure the platform library is loaded so the renderers will be used.
            var t = new MediaElementRenderer();
        }
    }
}