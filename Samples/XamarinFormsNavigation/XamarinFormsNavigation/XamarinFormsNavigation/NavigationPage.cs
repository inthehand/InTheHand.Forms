using System.Threading.Tasks;
using Xamarin.Forms;

namespace InTheHand.Forms
{
    public sealed class NavigationPage2 : Xamarin.Forms.NavigationPage
    {
        public NavigationPage2(Page p) : base(p)
        {
            HookEvents();
        }

        public NavigationPage2() : base()
        {
            HookEvents();
        }

        void HookEvents()
        {
            this.Pushed += NavigationPage_Pushed;
            this.Popped += NavigationPage_Popped;
            this.PoppedToRoot += NavigationPage_PoppedToRoot;
        }

        private void NavigationPage_PoppedToRoot(object sender, Xamarin.Forms.NavigationEventArgs e)
        {
            if (e.Page is IPageNavigation)
            {
                ((IPageNavigation)e.Page).OnNavigatedFrom(new NavigationEventArgs(e.Page, NavigationMode.Back));
            }

            if (this.Navigation.NavigationStack.Count > 0)
            {
                if (this.Navigation.NavigationStack[0] is IPageNavigation)
                {
                    ((IPageNavigation)this.Navigation.NavigationStack[0]).OnNavigatedTo(new NavigationEventArgs(e.Page, NavigationMode.Back));
                }
            }
        }

        private void NavigationPage_Popped(object sender, Xamarin.Forms.NavigationEventArgs e)
        {

            Page newTopPage = Navigation.NavigationStack[Navigation.NavigationStack.Count - 1];

            if (Device.OS == TargetPlatform.iOS)
            {
                newTopPage = Navigation.NavigationStack[Navigation.NavigationStack.Count - 2];
            }
            if (e.Page is IPageNavigation)
            {
                ((IPageNavigation)e.Page).OnNavigatedFrom(new NavigationEventArgs(newTopPage, NavigationMode.Back));
            }

            if (this.Navigation.NavigationStack.Count > 0)
            {
                if (newTopPage is IPageNavigation)
                {
                    ((IPageNavigation)newTopPage).OnNavigatedTo(new NavigationEventArgs(e.Page, NavigationMode.Back));
                }
            }
        }

        private async void NavigationPage_Pushed(object sender, Xamarin.Forms.NavigationEventArgs e)
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
                    ((IPageNavigation)previousPage).OnNavigatedFrom(new NavigationEventArgs(e.Page, NavigationMode.New));
                }
            }

            if (e.Page is IPageNavigation)
            {
                ((IPageNavigation)e.Page).OnNavigatedTo(new NavigationEventArgs(previousPage, NavigationMode.New));
            }
        }
    }

    public interface IPageNavigation
    {
        void OnNavigatedTo(NavigationEventArgs args);

        void OnNavigatedFrom(NavigationEventArgs args);
    }

    public enum NavigationMode
    {
        Back,
        //Forward,
        New,
    }

    public sealed class NavigationEventArgs
    {
        internal NavigationEventArgs(Page p, NavigationMode mode)
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
