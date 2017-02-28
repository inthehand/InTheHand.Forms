using System;
using System.Collections.Generic;
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
            this.Appearing += MainPage_Appearing;
        }

        private void MainPage_Appearing(object sender, EventArgs e)
        {
            //Device.OpenUri(new Uri("http://video.ch9.ms/ch9/334f/891b78a5-642d-40b4-8d02-ff40ffdd334f/LoginToLinkedinUSingXamarinAuth_mid.mp4"));
            /*if(Media.CurrentState != InTheHand.Forms.MediaElementState.Playing)
            {
                Media.Play();
            }*/
        }
    }
}
