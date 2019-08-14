using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MediaPlayerSample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            //this.Appearing += MainPage_Appearing;
        }

        private void MainPage_Appearing(object sender, EventArgs e)
        {
            //Device.OpenUri(new Uri("http://video.ch9.ms/ch9/334f/891b78a5-642d-40b4-8d02-ff40ffdd334f/LoginToLinkedinUSingXamarinAuth_mid.mp4"));
            /*if(Media.CurrentState != InTheHand.Forms.MediaElementState.Playing)
            {
                Media.Play();
            }*/
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine(Media.VideoWidth + "x" + Media.VideoHeight);

            switch(Media.Aspect)
            {
                case Aspect.Fill:
                    Media.Aspect = Aspect.AspectFit;
                    break;

                case Aspect.AspectFit:
                    Media.Aspect = Aspect.AspectFill;
                    break;

                case Aspect.AspectFill:
                    Media.Aspect = Aspect.Fill;
                    break;
            }
        }

        private void RadioSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if(Radio1.IsToggled)
            {
                Radio2.IsToggled = true;
                Radio1.IsToggled = false;
            }
            else
            {
                Radio1.IsToggled = true;
                Radio2.IsToggled = false;
            }
        }
    }
}
