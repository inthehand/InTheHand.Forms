using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Forms;
using Xamarin.Forms;

namespace XamarinFormsNavigation
{
    public partial class FirstPage : ContentPage, IPageNavigation
    {
        public FirstPage()
        {
            InitializeComponent();
        }

        public void OnNavigatedFrom(InTheHand.Forms.NavigationEventArgs args)
        {
        }

        public void OnNavigatedTo(InTheHand.Forms.NavigationEventArgs args)
        {
            switch (args.NavigationMode)
            {
                case NavigationMode.New:
                    StatusLabel.Text = "Navigated forward to this page from " + (args.Page == null ? "<unknownpage>" : args.Page.GetType().ToString());
                    break;

                case NavigationMode.Back:
                    StatusLabel.Text = "Navigated backward to this page from " + (args.Page == null ? "<unknownpage>" : args.Page.GetType().ToString());
                    break;
            }
        }

        private async void Second_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SecondPage());
        }
    }
}
