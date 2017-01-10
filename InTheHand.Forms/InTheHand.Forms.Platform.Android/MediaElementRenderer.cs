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
        protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                SetNativeControl(new VideoView(this.Context));
                this.Control.Touch += Control_Touch;
                this.Control.KeepScreenOn = true;
                this.Control.Prepared += Control_Prepared;
                this.Control.SetOnCompletionListener(this);

                if (e.NewElement.Source != null)
                {
                    if (!e.NewElement.Source.OriginalString.StartsWith("/"))
                    {
                        this.Control.SetVideoURI(global::Android.Net.Uri.Parse(e.NewElement.Source.ToString()));
                    }
                    else
                    {
                        String path = "android.resource://" + Context.PackageName + "/" + Resources.GetIdentifier(System.IO.Path.GetFileNameWithoutExtension(e.NewElement.Source.ToString()), "raw", Context.PackageName).ToString();

                        this.Control.SetVideoPath(path);
                    }

                }
            }
        }

        private void Control_Prepared(object sender, EventArgs e)
        {
            Element.OnMediaOpened();
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            Control.RequestLayout();
        }

        void Control_Touch(object sender, TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Up:
                    if (Control.IsPlaying)
                    {
                        if (this.Control.CanPause())
                        {
                            Element.Pause();
                            this.Control.Pause();
                            this.Control.KeepScreenOn = false;
                        }
                    }
                    else
                    {
                        Element.Play();
                        this.Control.Start();
                        this.Control.KeepScreenOn = true;
                    }
                    break;
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Source":
                    if (!Element.Source.OriginalString.StartsWith("/"))
                    {
                        Control.SetVideoURI(global::Android.Net.Uri.Parse(Element.Source.ToString()));
                    }
                    else
                    {
                        string path = "android.resource://" + Context.PackageName + "/" + Resources.GetIdentifier(System.IO.Path.GetFileNameWithoutExtension(Element.Source.ToString()), "raw", Context.PackageName).ToString();

                        Control.SetVideoPath(path);
                    }
                    break;
                case "CurrentState":
                    switch(Element.CurrentState)
                    {
                        case MediaElementState.Playing:
                            Control.Start();
                            break;
                        case MediaElementState.Paused:
                            Control.Pause();
                            break;
                        case MediaElementState.Stopped:
                            Control.SeekTo(0);
                            Control.StopPlayback();
                            break;
                    }
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }

        private MediaPlayer player = null;

        public void OnCompletion(MediaPlayer mp)
        {
            player = mp;
            this.Element.OnMediaEnded();
        }
    }
}