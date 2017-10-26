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
    public sealed class MediaElementRenderer : ViewRenderer<MediaElement, FrameLayout>, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnPreparedListener, IMediaElementRenderer
    {
        private MediaController _controller;
        private VideoViewEx _view;
        private MediaPlayer _mediaPlayer;

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
                try
                {
                    return TimeSpan.FromMilliseconds(_view.CurrentPosition);
                }
                catch
                {
                    return TimeSpan.Zero;
                }
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                e.OldElement.SetRenderer(null);

                if (_view != null)
                {
                    _view.Prepared -= Control_Prepared;
                    _view.SetOnCompletionListener(null);
                    _view.Dispose();
                }

                if (_controller != null)
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
                        e.NewElement.SetRenderer(this);

                        _view = new VideoViewEx(Context);
                        SetNativeControl(new FrameLayout(Context));// new VideoViewEx(Context));
                        Control.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                        //_view.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

                        //_view.SetZOrderOnTop(true);
                        _view.SetZOrderMediaOverlay(true);
                        _view.Prepared += Control_Prepared;
                        _view.SetOnCompletionListener(this);
                        _view.SetOnPreparedListener(this);
                        _view.KeepScreenOn = Element.KeepScreenOn;

                        Control.AddView(_view);
                        //Control.ForceLayout();

                        _controller = new MediaController(Context);
                        _controller.SetAnchorView(_view);
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
                if(Element.Source.Scheme == null)
                {
                    _view.SetVideoPath(Element.Source.AbsolutePath);
                }
                else if (Element.Source.Scheme == "ms-appx")
                {
                    // video resources should be in the raw folder with Build Action set to AndroidResource
                    string uri = "android.resource://" + Context.PackageName + "/raw/" + Element.Source.LocalPath.Substring(1, Element.Source.LocalPath.LastIndexOf('.') - 1).ToLower();
                    _view.SetVideoURI(global::Android.Net.Uri.Parse(uri));
                }
                else if (Element.Source.Scheme == "ms-appdata")
                {
                    _view.SetVideoPath(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Element.Source.LocalPath.Substring(1)));
                }
                else
                {
                    if (Element.Source.IsFile)
                    {
                        _view.SetVideoPath(Element.Source.AbsolutePath);
                    }
                    else
                    {
                        _view.SetVideoURI(global::Android.Net.Uri.Parse(Element.Source.ToString()), Element.HttpHeaders);
                    }
                }

                if (Element.AutoPlay)
                {
                    _view.Start();
                    Element.CurrentState = MediaElementState.Playing;
                }

            }
        }

        private void Control_Prepared(object sender, EventArgs e)
        {
            Element?.RaiseMediaOpened();
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AreTransportControlsEnabled":
                    _controller.Visibility = Element.AreTransportControlsEnabled ? ViewStates.Visible : ViewStates.Gone;
                    break;

                case "Source":
                    UpdateSource();
                    break;

                case "CurrentState":
                    switch (Element.CurrentState)
                    {
                        case MediaElementState.Playing:
                            if (!_view.IsPlaying)
                            {
                                _view.Start();
                            }
                            Element.CurrentState = _view.IsPlaying ? MediaElementState.Playing : MediaElementState.Stopped;
                            break;

                        case MediaElementState.Paused:
                            _view.Pause();
                            break;

                        case MediaElementState.Stopped:
                            if (_view.IsPlaying)
                            {
                                _view.SeekTo(0);
                                _view.StopPlayback();
                            }
                            Element.CurrentState = _view.IsPlaying ? MediaElementState.Playing : MediaElementState.Stopped;
                            break;
                    }
                    
                    break;

                case "IsLooping":
                    if(_mediaPlayer != null)
                    {
                        _mediaPlayer.Looping = Element.IsLooping;
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

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            UpdateLayoutParameters();
        }

        private void UpdateLayoutParameters()
        {
            if(Element.NaturalVideoHeight == 0)
            {
                _view.LayoutParameters = new FrameLayout.LayoutParams(Width, Height, GravityFlags.Fill);
                return;
            }

            float ratio = (float)Element.NaturalVideoWidth / Element.NaturalVideoHeight;
            float controlRatio = (float)Width / Height;

            switch (Element.Stretch)
            {
                case Stretch.None:
                    _view.LayoutParameters = new FrameLayout.LayoutParams(Width, Height, GravityFlags.CenterHorizontal | GravityFlags.CenterVertical);
                    break;

                case Stretch.Fill:
                    // TODO: this doesn't stretch like other platforms...
                    _view.LayoutParameters = new FrameLayout.LayoutParams(Width, Height, GravityFlags.FillHorizontal | GravityFlags.FillVertical | GravityFlags.CenterHorizontal | GravityFlags.CenterVertical) { LeftMargin = 0, RightMargin = 0, TopMargin = 0, BottomMargin = 0 };
                    break;

                case Stretch.Uniform:
                    if (ratio > controlRatio)
                    {
                        int requiredHeight = (int)(Width / ratio);
                        int vertMargin = (Height - requiredHeight) / 2;
                        _view.LayoutParameters = new FrameLayout.LayoutParams(Width, requiredHeight, GravityFlags.FillHorizontal | GravityFlags.CenterVertical) { LeftMargin = 0, RightMargin = 0, TopMargin = vertMargin, BottomMargin = vertMargin };
                    }
                    else
                    {
                        int requiredWidth = (int)(Height * ratio);
                        int horizMargin = (Width - requiredWidth) / 2;
                        _view.LayoutParameters = new FrameLayout.LayoutParams(requiredWidth, Height, GravityFlags.CenterHorizontal | GravityFlags.FillVertical) { LeftMargin = horizMargin, RightMargin = horizMargin, TopMargin = 0, BottomMargin = 0 };
                    }
                    break;

                case Stretch.UniformToFill:
                    if (ratio > controlRatio)
                    {
                        int requiredWidth = (int)(Height / ratio);
                        int horizMargin = (Width - requiredWidth) / 2;
                        _view.LayoutParameters = new FrameLayout.LayoutParams((int)(Height * ratio), Height, GravityFlags.CenterHorizontal | GravityFlags.FillVertical) { TopMargin = 0, BottomMargin = 0, LeftMargin=horizMargin, RightMargin=horizMargin };
                    }
                    else
                    {
                        int requiredHeight = (int)(Width / ratio);
                        int vertMargin = (Height - requiredHeight) / 2;
                        _view.LayoutParameters = new FrameLayout.LayoutParams(Width, requiredHeight, GravityFlags.FillHorizontal | GravityFlags.CenterVertical) { LeftMargin = 0, RightMargin = 0, TopMargin = vertMargin, BottomMargin = vertMargin };
                    }

                    break;
            }
        }

        void MediaPlayer.IOnCompletionListener.OnCompletion(MediaPlayer mp)
        {
            mp.SeekTo(0);
            this.Element.OnMediaEnded();
        }

        public void OnPrepared(MediaPlayer mp)
        {
            _mediaPlayer = mp;
            mp.Looping = Element.IsLooping;
        }
    }
}
