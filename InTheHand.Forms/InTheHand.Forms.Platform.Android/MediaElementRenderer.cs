using Android.Content;
using Android.Media;
using Android.Views;
using Android.Widget;
using InTheHand.Forms;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;
using Android.Runtime;

[assembly: ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.Android.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.Android
{
    public sealed class MediaElementRenderer : ViewRenderer<MediaElement, VideoView>, MediaPlayer.IOnCompletionListener, IMediaElementRenderer
    {
        private MediaController _controller;

        public double BufferingProgress
        {
            get
            {
                return Control.BufferPercentage / 100;
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (Control.IsPlaying)
                {
                    return TimeSpan.FromMilliseconds(Control.CurrentPosition);
                }

                return TimeSpan.Zero;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if(e.OldElement != null)
            {
                e.OldElement.SetRenderer(null);

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
                if (Application.Current != null)
                {
                    try
                    {
                        SetNativeControl(new VideoView(Context));
                        e.NewElement.SetRenderer(this);
                        Control.Prepared += Control_Prepared;
                        Control.SetOnCompletionListener(this);
                        Control.KeepScreenOn = Element.KeepScreenOn;

                        _controller = new MediaController(Context);
                        _controller.Visibility = Element.AreTransportControlsEnabled ? ViewStates.Visible : ViewStates.Gone;
                        Control.SetMediaController(_controller);
                        UpdateSource();
                    }
                    catch
                    { }
                }
            }
        }

        private void UpdateSource()
        {
            if (Element.Source != null)
            {
                if (Element.Source.Scheme == "ms-appx")
                {
                    // video resources should be in the raw folder with Build Action set to AndroidResource
                    string uri = "android.resource://" + Context.PackageName + "/raw/" + Element.Source.LocalPath.Substring(1, Element.Source.LocalPath.LastIndexOf('.') - 1).ToLower();
                    Control.SetVideoURI(global::Android.Net.Uri.Parse(uri));
                }
                else if (Element.Source.Scheme == "ms-appdata")
                {
                    Control.SetVideoPath(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),Element.Source.LocalPath.Substring(1)));
                }
                else
                {
                    Control.SetVideoURI(global::Android.Net.Uri.Parse(Element.Source.ToString()));
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
                            //this.Control.KeepScreenOn = true;
                            break;
                        case MediaElementState.Paused:
                            Control.Pause();
                            //this.Control.KeepScreenOn = false;
                            break;
                        case MediaElementState.Stopped:
                            Control.SeekTo(0);
                            Control.StopPlayback();
                            //this.Control.KeepScreenOn = false;
                            break;
                    }
                    break;

                case "KeepScreenOn":
                    Control.KeepScreenOn = Element.KeepScreenOn;
                    break;

                case "Position":
                    Control.SeekTo((int)((TimeSpan)Element.GetValue(MediaElement.PositionProperty)).TotalMilliseconds);
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }
        

        public void OnCompletion(MediaPlayer mp)
        {
            if (Element.IsLooping)
            {
                mp.SeekTo(0);
                mp.Start();
            }
            else
            {
                this.Element.OnMediaEnded();
            }
        }
    }
}