// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPageNavigation.cs" company="In The Hand Ltd">
//   Copyright (c) 2016-2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace InTheHand.Forms
{
    public interface IPageNavigation
    {
        void OnNavigatedTo(NavigationEventArgs2 args);

        void OnNavigatedFrom(NavigationEventArgs2 args);
    }
}
