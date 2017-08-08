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
    public sealed class MediaElementRenderer : ViewRenderer<MediaElement, FrameLayout>, MediaPlayer.IOnCompletionListener, IMediaElementRenderer
    {
        private MediaController _controller;
        private VideoViewEx _view;

        double IMediaElementRenderer.BufferingProgress
        {
            get
            {
                return _view.BufferPercentage / 100;
            }
        }

        TimeSpan IMediaElementRenderer.NaturalDuration
        {
            get
            {
                return _view.Duration;
            }
        }

        int IMediaElementRenderer.NaturalVideoHeight
        {
            get
            {
                return _view.VideoHeight;
            }
        }

        int IMediaElementRenderer.NaturalVideoWidth
        {
            get
            {
                return _view.VideoWidth;
            }
        }

        void IMediaElementRenderer.Seek(TimeSpan time)
        {
            _view.SeekTo((int)time.TotalMilliseconds);
        }

        TimeSpan IMediaElementRenderer.Position
        {
            get
            {
                if (_view.IsPlaying)
                {
                    return TimeSpan.FromMilliseconds(_view.CurrentPosition);
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

                if (_view != null)
                {
                    _view.Prepared -= Control_Prepared;
                    _view.SetOnCompletionListener(null);
                    _view.Dispose();
                }

                if(_controller != null)
                {
                    _controller.Dispose();
                    _controller = null;
                }
            }

            if (e.NewElement != null)
            {
                if (!DesignMode.DesignModeEnabled)
                {
                    try
                    {
                        SetNativeControl(new FrameLayout(Context));// new VideoViewEx(Context));
                        _view = new VideoViewEx(Context);
                        Control.AddView(_view);
                        e.NewElement.SetRenderer(this);
                        _view.Prepared += Control_Prepared;
                        _view.SetOnCompletionListener(this);
                        _view.KeepScreenOn = Element.KeepScreenOn;

                        _controller = new MediaController(Context);
                        _controller.Visibility = Element.AreTransportControlsEnabled ? ViewStates.Visible : ViewStates.Gone;
                        _view.SetMediaController(_controller);
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
                    _view.SetVideoURI(global::Android.Net.Uri.Parse(uri));
                }
                else if (Element.Source.Scheme == "ms-appdata")
                {
                    _view.SetVideoPath(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),Element.Source.LocalPath.Substring(1)));
                }
                else
                {
                    if (Element.Source.IsFile)
                    {
                        _view.SetVideoPath(Element.Source.AbsolutePath);
                    }
                    else
                    {
                        _view.SetVideoURI(global::Android.Net.Uri.Parse(Element.Source.ToString()));
                    }
                }

                if(Element.AutoPlay)
                {
                    _view.Start();
                }

            }
        }

        private void Control_Prepared(object sender, EventArgs e)
        {
            Element?.RaiseMediaOpened();
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
                            _view.Start();
                            break;

                        case MediaElementState.Paused:
                            _view.Pause();
                            break;

                        case MediaElementState.Stopped:
                            _view.SeekTo(0);
                            _view.StopPlayback();
                            break;
                    }
                    break;

                case "KeepScreenOn":
                    _view.KeepScreenOn = Element.KeepScreenOn;
                    break;

                case "Stretch":
                    UpdateLayoutParameters();
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }

        private void UpdateLayoutParameters()
        {
            float ratio = (float)Element.NaturalVideoWidth / Element.NaturalVideoHeight;
            float controlRatio = (float)Control.Width / Control.Height;

            switch (Element.Stretch)
            {
                case Stretch.None:
                    _view.LayoutParameters = new FrameLayout.LayoutParams(Control.Width, Control.Height, GravityFlags.CenterHorizontal | GravityFlags.CenterVertical);
                    break;

                case Stretch.Fill:
                    // TODO: this doesn't stretch like other platforms...
                    _view.LayoutParameters = new FrameLayout.LayoutParams(Control.Width, Control.Height, GravityFlags.FillHorizontal | GravityFlags.FillVertical | GravityFlags.CenterHorizontal | GravityFlags.CenterVertical) { LeftMargin = 0, RightMargin = 0, TopMargin = 0, BottomMargin = 0 };
                    break;

                case Stretch.Uniform:
                    if (ratio > controlRatio)
                    {
                        _view.LayoutParameters = new FrameLayout.LayoutParams(Control.Width, (int)(Control.Width / ratio), GravityFlags.FillHorizontal | GravityFlags.CenterVertical);
                    }
                    else
                    {
                        _view.LayoutParameters = new FrameLayout.LayoutParams((int)(Control.Height * ratio), Control.Height, GravityFlags.CenterHorizontal | GravityFlags.FillVertical) { LeftMargin = 0, RightMargin = 0, TopMargin = 0, BottomMargin = 0 };
                    }
                    break;

                case Stretch.UniformToFill:
                    if (ratio > controlRatio)
                    {
                        _view.LayoutParameters = new FrameLayout.LayoutParams((int)(Control.Height * ratio), Control.Height, GravityFlags.CenterHorizontal | GravityFlags.FillVertical) { TopMargin = 0, BottomMargin = 0 };
                    }
                    else
                    {
                        _view.LayoutParameters = new FrameLayout.LayoutParams(Control.Width, (int)(Control.Width * ratio), GravityFlags.FillHorizontal | GravityFlags.CenterVertical) { LeftMargin = 0, RightMargin = 0, TopMargin = 0, BottomMargin = 0 };
                    }

                    break;
            }
        }

        void MediaPlayer.IOnCompletionListener.OnCompletion(MediaPlayer mp)
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