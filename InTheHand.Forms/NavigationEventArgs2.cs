// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NavigationEventArgs2.cs" company="In The Hand Ltd">
//   Copyright (c) 2016-2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Xamarin.Forms;

namespace InTheHand.Forms
{
    public sealed class NavigationEventArgs2
    {
        internal NavigationEventArgs2(Page p, NavigationMode mode)
        {
            Page = p;
            NavigationMode = mode;
        }

        public Page Page
        {
            get;
            private set;
        }

        public NavigationMode NavigationMode
        {
            get;
            private set;
        }
    }
}
