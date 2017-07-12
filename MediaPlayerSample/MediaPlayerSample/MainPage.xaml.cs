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
            Debug.WriteLine(Media.NaturalVideoWidth + "x" + Media.NaturalVideoHeight);

            switch(Media.Stretch)
            {
                case InTheHand.Forms.Stretch.None:
                    Media.Stretch = InTheHand.Forms.Stretch.Fill;
                    break;

                case InTheHand.Forms.Stretch.Fill:
                    Media.Stretch = InTheHand.Forms.Stretch.Uniform;
                    break;

                case InTheHand.Forms.Stretch.Uniform:
                    Media.Stretch = InTheHand.Forms.Stretch.UniformToFill;
                    break;

                case InTheHand.Forms.Stretch.UniformToFill:
                    Media.Stretch = InTheHand.Forms.Stretch.None;
                    break;
            }
        }
    }
}
