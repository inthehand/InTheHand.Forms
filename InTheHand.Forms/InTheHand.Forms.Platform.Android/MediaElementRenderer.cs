using Android.Content;
using Android.Media;
using Android.Views;
using Android.Widget;
using InTheHand.Forms;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.Android.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.Android
{
    public sealed class MediaElementRenderer : ViewRenderer<MediaElement, VideoView>, MediaPlayer.IOnCompletionListener
    {
        private MediaController _controller;

        protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if(e.OldElement != null)
            {
                if (Control != null)
                {
                    Control.Prepared -= Control_Prepared;
                    Control.SetOnCompletionListener(null);
                    Control.Dispose();
                }
                if(_controller != null)
                {
                    _controller.Dispose();
                    _controller = null;
                }
            }

            if (e.NewElement != null)
            {
                SetNativeControl(new VideoView(Context));
                this.Control.KeepScreenOn = true;
                this.Control.Prepared += Control_Prepared;
                this.Control.SetOnCompletionListener(this);

                _controller = new MediaController(Context);
                _controller.Visibility = Element.AreTransportControlsEnabled ? ViewStates.Visible : ViewStates.Gone;
                this.Control.SetMediaController(_controller);

                UpdateSource();
            }
        }

        private void UpdateSource()
        {
            if (Element.Source != null)
            {
                if (!Element.Source.OriginalString.StartsWith("/"))
                {
                    Control.SetVideoURI(global::Android.Net.Uri.Parse(Element.Source.ToString()));
                }
                else
                {
                    string path = "android.resource://" + Context.PackageName + "/" + Resources.GetIdentifier(System.IO.Path.GetFileNameWithoutExtension(Element.Source.ToString()), "raw", Context.PackageName).ToString();
                    Control.SetVideoPath(path);
                }

                if(Element.AutoPlay)
                {
                    Control.Start();
                }

            }
        }

        private void Control_Prepared(object sender, EventArgs e)
        {
            Element.OnMediaOpened();
        }
        
        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "AreTransportControlsEnabled":
                    _controller.Visibility = Element.AreTransportControlsEnabled ? ViewStates.Visible : ViewStates.Gone;
                    break;

                case "Source":
                    UpdateSource();
                    break;

                case "CurrentState":
                    switch(Element.CurrentState)
                    {
                        case MediaElementState.Playing:
                            Control.Start();
                            this.Control.KeepScreenOn = true;
                            break;
                        case MediaElementState.Paused:
                            Control.Pause();
                            this.Control.KeepScreenOn = false;
                            break;
                        case MediaElementState.Stopped:
                            Control.SeekTo(0);
                            Control.StopPlayback();
                            this.Control.KeepScreenOn = false;
                            break;
                    }
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }

        //private MediaPlayer player = null;

        public void OnCompletion(MediaPlayer mp)
        {
            //player = mp;
            this.Element.OnMediaEnded();
        }
    }
}