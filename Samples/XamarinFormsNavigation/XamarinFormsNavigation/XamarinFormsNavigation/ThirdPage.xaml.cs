using InTheHand.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace XamarinFormsNavigation
{
    public partial class ThirdPage : ContentPage, IPageNavigation
    {
        public ThirdPage()
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

        private async void First_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FirstPage());
        }
    }
}
