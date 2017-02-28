// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NavigationPage2.cs" company="In The Hand Ltd">
//   Copyright (c) 2016-2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Xamarin.Forms;

namespace InTheHand.Forms
{
    /// <summary>
    /// Supports additional callbacks on pages exposing <see cref="IPageNavigation"/>.
    /// </summary>
    public sealed class NavigationPage2 : NavigationPage
    {
        public NavigationPage2(Page p) : base(p)
        {
            HookEvents();
        }

        public NavigationPage2() : base()
        {
            HookEvents();
        }

        private void HookEvents()
        {
            this.Pushed += NavigationPage_Pushed;
            this.Popped += NavigationPage_Popped;
            this.PoppedToRoot += NavigationPage_PoppedToRoot;
        }

        private void NavigationPage_PoppedToRoot(object sender, NavigationEventArgs e)
        {
            if (e.Page is IPageNavigation)
            {
                ((IPageNavigation)e.Page).OnNavigatedFrom(new NavigationEventArgs2(e.Page, NavigationMode.Back));
            }

            if (this.Navigation.NavigationStack.Count > 0)
            {
                if (this.Navigation.NavigationStack[0] is IPageNavigation)
                {
                    ((IPageNavigation)this.Navigation.NavigationStack[0]).OnNavigatedTo(new NavigationEventArgs2(e.Page, NavigationMode.Back));
                }
            }
        }

        private void NavigationPage_Popped(object sender, NavigationEventArgs e)
        {

            Page newTopPage = Navigation.NavigationStack[Navigation.NavigationStack.Count - 1];

            if (Device.OS == TargetPlatform.iOS)
            {
                newTopPage = Navigation.NavigationStack[Navigation.NavigationStack.Count - 2];
            }
            if (e.Page is IPageNavigation)
            {
                ((IPageNavigation)e.Page).OnNavigatedFrom(new NavigationEventArgs2(newTopPage, NavigationMode.Back));
            }

            if (this.Navigation.NavigationStack.Count > 0)
            {
                if (newTopPage is IPageNavigation)
                {
                    ((IPageNavigation)newTopPage).OnNavigatedTo(new NavigationEventArgs2(e.Page, NavigationMode.Back));
                }
            }
        }

        private void NavigationPage_Pushed(object sender, NavigationEventArgs e)
        {
            Page previousPage = Navigation.NavigationStack[0];

            if (Navigation.NavigationStack.Count > 2)
            {
                previousPage = Navigation.NavigationStack[Navigation.NavigationStack.Count - 2];
            }

            if (previousPage != null)
            {
                if (previousPage is IPageNavigation)
                {
                    ((IPageNavigation)previousPage).OnNavigatedFrom(new NavigationEventArgs2(e.Page, NavigationMode.New));
                }
            }

            if (e.Page is IPageNavigation)
            {
                ((IPageNavigation)e.Page).OnNavigatedTo(new NavigationEventArgs2(previousPage, NavigationMode.New));
            }
        }
    }
}
