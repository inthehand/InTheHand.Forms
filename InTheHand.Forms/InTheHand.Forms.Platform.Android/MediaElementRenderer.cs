// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaElementRenderer.cs" company="In The Hand Ltd">
//   Copyright (c) 2017-19 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Android.Content;
using Android.Media;
using Android.Views;
using Android.Widget;
using InTheHand.Forms;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(InTheHand.Forms.MediaElement), typeof(InTheHand.Forms.Platform.Android.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.Android
{
    internal sealed class MediaElementRenderer : FrameLayout, IVisualElementRenderer, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnInfoListener, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnErrorListener
    {
        bool _isDisposed;
        int? _defaultLabelFor;
        InTheHand.Forms.MediaElement MediaElement { get; set; }
        IMediaElementController Controller => MediaElement as IMediaElementController;

        VisualElementTracker _tracker;

        MediaController _controller;
        MediaPlayer _mediaPlayer;
        FormsVideoView _view;

        public MediaElementRenderer(Context context) : base(context)
        {
            _view = new FormsVideoView(Context);
            _view.SetZOrderMediaOverlay(true);
            _view.SetOnCompletionListener(this);
            _view.SetOnInfoListener(this);
            _view.SetOnPreparedListener(this);
            _view.SetOnErrorListener(this);
            _view.MetadataRetrieved += MetadataRetrieved;
            
            SetForegroundGravity(GravityFlags.Center);

            AddView(_view, -1, -1);

            _controller = new MediaController(Context);
            _controller.SetAnchorView(this);
            _view.SetMediaController(_controller);
        }

        public VisualElement Element => MediaElement;

        VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

        ViewGroup IVisualElementRenderer.ViewGroup => null;

        AView IVisualElementRenderer.View => this;

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
        public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

        SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            AView view = this;
            view.Measure(widthConstraint, heightConstraint);

            return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size(1,1));
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            UpdateLayoutParameters();
        }

        void IVisualElementRenderer.SetElement(VisualElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            if (!(element is MediaElement))
                throw new ArgumentException($"{nameof(element)} must be of type {nameof(MediaElement)}");

            MediaElement oldElement = MediaElement;
            MediaElement = (MediaElement)element;
            
            if (oldElement != null)
            {
                oldElement.PropertyChanged -= OnElementPropertyChanged;
                oldElement.SeekRequested -= SeekRequested;
                oldElement.StateRequested -= StateRequested;
                oldElement.PositionRequested -= OnPositionRequested;
            }

            Color currentColor = oldElement?.BackgroundColor ?? Color.Default;
            if (element.BackgroundColor != currentColor)
            {
                UpdateBackgroundColor();
            }

            MediaElement.PropertyChanged += OnElementPropertyChanged;
            MediaElement.SeekRequested += SeekRequested;
            MediaElement.StateRequested += StateRequested;
            MediaElement.PositionRequested += OnPositionRequested;

            if (_tracker is null)
            {
                // Can't set up the tracker in the constructor because it access the Element (for now)
                SetTracker(new VisualElementTracker(this));
            }

            OnElementChanged(new ElementChangedEventArgs<MediaElement>(oldElement as MediaElement, MediaElement));
        }

        private void OnPositionRequested(object sender, EventArgs e)
        {
            Controller.Position = TimeSpan.FromMilliseconds(_mediaPlayer.CurrentPosition);
        }

        void StateRequested(object sender, StateRequested e)
        {
            if (_mediaPlayer == null)
                return;

            switch (e.State)
            {
                case MediaElementState.Playing:
                    _view.Start();
                    UpdateVolume();
                    Controller.CurrentState = _view.IsPlaying ? MediaElementState.Playing : MediaElementState.Stopped;
                    break;

                case MediaElementState.Paused:
                    if (_view.CanPause())
                    {
                        _view.Pause();
                        Controller.CurrentState = MediaElementState.Paused;
                    }
                    break;

                case MediaElementState.Stopped:
                    _view.Pause();
                    _view.SeekTo(0);

                    Controller.CurrentState = _view.IsPlaying ? MediaElementState.Playing : MediaElementState.Stopped;
                    break;
            }

            UpdateLayoutParameters();
            Controller.Position = _view.Position;
        }

        void SeekRequested(object sender, SeekRequested e)
        {
            _mediaPlayer.SeekTo((int)e.Position.TotalMilliseconds);
            Controller.Position = _view.Position;
        }

        void IVisualElementRenderer.SetLabelFor(int? id)
        {
            if (_defaultLabelFor is null)
            {
                _defaultLabelFor = LabelFor;
            }

            LabelFor = (int)(id ?? _defaultLabelFor);
        }

        void SetTracker(VisualElementTracker tracker)
        {
            _tracker = tracker;
        }

        void UpdateBackgroundColor()
        {
            _view.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
            SetBackgroundColor(Element.BackgroundColor.ToAndroid());
        }

        void IVisualElementRenderer.UpdateLayout() => _tracker?.UpdateLayout();

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            ReleaseControl();

            if (disposing)
            {
                SetOnClickListener(null);
                SetOnTouchListener(null);

                _tracker?.Dispose();

                if (Element != null)
                {
                    Element.PropertyChanged -= OnElementPropertyChanged;
                }
            }

            base.Dispose(disposing);
        }

        void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            if (e.OldElement != null)
            {

            }

            if (e.NewElement != null)
            {
                this.EnsureId();

                UpdateKeepScreenOn();
                UpdateLayoutParameters();
                UpdateShowPlaybackControls();
                UpdateSource();
                UpdateBackgroundColor();
                UpdateVolume();
            }

            ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
        }

        void MetadataRetrieved(object sender, EventArgs e)
        {
            Controller.Duration = _view.DurationTimeSpan;
            Controller.VideoHeight = _view.VideoHeight;
            Controller.VideoWidth = _view.VideoWidth;

            Device.BeginInvokeOnMainThread(UpdateLayoutParameters);
        }

        void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MediaElement.Aspect):
                    UpdateLayoutParameters();
                    break;

                case nameof(MediaElement.IsLooping):
                    if (_mediaPlayer != null)
                    {
                        _mediaPlayer.Looping = MediaElement.IsLooping;
                    }
                    break;

                case nameof(MediaElement.KeepScreenOn):
                    UpdateKeepScreenOn();
                    break;

                case nameof(MediaElement.ShowsPlaybackControls):
                    UpdateShowPlaybackControls();
                    break;

                case nameof(MediaElement.Source):
                    UpdateSource();
                    break;

                case nameof(MediaElement.Volume):
                    UpdateVolume();
                    break;
            }

            ElementPropertyChanged?.Invoke(this, e);
        }
        
        void UpdateKeepScreenOn()
        {
            _view.KeepScreenOn = MediaElement.KeepScreenOn;
        }

        void UpdateShowPlaybackControls()
        {
            _controller.Visibility = MediaElement.ShowsPlaybackControls ? ViewStates.Visible : ViewStates.Gone;
        }

        void UpdateSource()
        {
            if (MediaElement.Source != null)
            {
                if (MediaElement.Source.Scheme == "ms-appx")
                {
                    // video resources should be in the raw folder with Build Action set to AndroidResource
                    string uri = "android.resource://" + Context.PackageName + "/raw/" + MediaElement.Source.LocalPath.Substring(1, MediaElement.Source.LocalPath.LastIndexOf('.') - 1).ToLower();
                    _view.SetVideoURI(global::Android.Net.Uri.Parse(uri));
                }
                else if (MediaElement.Source.Scheme == "ms-appdata")
                {
                    string filePath = ResolveMsAppDataUri(MediaElement.Source);

                    if (string.IsNullOrEmpty(filePath))
                        throw new ArgumentException("Invalid Uri", "Source");

                    _view.SetVideoPath(filePath);

                }
                else
                {
                    if (MediaElement.Source.IsFile)
                    {
                        _view.SetVideoPath(MediaElement.Source.AbsolutePath);
                    }
                    else
                    {
                        _view.SetVideoURI(global::Android.Net.Uri.Parse(MediaElement.Source.AbsoluteUri), MediaElement.HttpHeaders);
                    }
                }

                if (MediaElement.AutoPlay)
                {
                    _view.Start();
                    Controller.CurrentState = _view.IsPlaying ? MediaElementState.Playing : MediaElementState.Stopped;
                }

            }
            else if (_view.IsPlaying)
            {
                _view.StopPlayback();
                Controller.CurrentState = MediaElementState.Stopped;
            }
        }

        void UpdateVolume()
        {
            _mediaPlayer?.SetVolume((float)MediaElement.Volume, (float)MediaElement.Volume);
        }

        internal static string ResolveMsAppDataUri(Uri uri)
        {
            if (uri.Scheme == "ms-appdata")
            {
                string filePath = string.Empty;

                if (uri.LocalPath.StartsWith("/local"))
                {
                    filePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), uri.LocalPath.Substring(7));
                }
                else if (uri.LocalPath.StartsWith("/temp"))
                {
                    filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), uri.LocalPath.Substring(6));
                }
                else
                {
                    throw new ArgumentException("Invalid Uri", "Source");
                }

                return filePath;
            }
            else
            {
                throw new ArgumentException("uri");
            }

        }

        void MediaPlayer.IOnCompletionListener.OnCompletion(MediaPlayer mp)
        {
            Controller.Position = TimeSpan.FromMilliseconds(mp.CurrentPosition);
            Controller.OnMediaEnded();
        }

        void MediaPlayer.IOnPreparedListener.OnPrepared(MediaPlayer mp)
        {
            UpdateLayoutParameters();
            _mediaPlayer = mp;

            Device.BeginInvokeOnMainThread(() =>
            { 
                Controller.OnMediaOpened();
           
                mp.Looping = MediaElement.IsLooping;
                mp.SeekTo(0);

                if (MediaElement.AutoPlay)
                {
                    _mediaPlayer.Start();
                    Controller.CurrentState = MediaElementState.Playing;
                }
                else
                {
                    Controller.CurrentState = MediaElementState.Paused;
                }
            });
        }

        void UpdateLayoutParameters()
        {
            if (_view == null)
                return;

            if (_view.VideoWidth == 0 || _view.VideoHeight == 0)
            {
                _view.LayoutParameters = new FrameLayout.LayoutParams(Width, Height, GravityFlags.Fill);
                return;
            }

            float ratio = (float)_view.VideoWidth / (float)_view.VideoHeight;
            float controlRatio = (float)Width / Height;

            switch (MediaElement.Aspect)
            {
                case Aspect.Fill:
                    // TODO: this doesn't stretch like other platforms...
                    _view.LayoutParameters = new FrameLayout.LayoutParams(Width, Height, GravityFlags.Fill) { LeftMargin = 0, RightMargin = 0, TopMargin = 0, BottomMargin = 0 };
                    break;

                case Aspect.AspectFit:
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

                case Aspect.AspectFill:
                    if (ratio > controlRatio)
                    {
                        int requiredWidth = (int)(Height * ratio);
                        int horizMargin = (Width - requiredWidth) / 2;
                        _view.LayoutParameters = new FrameLayout.LayoutParams((int)(Height * ratio), Height, GravityFlags.CenterHorizontal | GravityFlags.FillVertical) { TopMargin = 0, BottomMargin = 0, LeftMargin = horizMargin, RightMargin = horizMargin };
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

        void ReleaseControl()
        {
            if (_view != null)
            {
                _view.MetadataRetrieved -= MetadataRetrieved;
                RemoveView(_view);
                _view.SetOnPreparedListener(null);
                _view.SetOnCompletionListener(null);
                _view.Dispose();
                _view = null;
            }

            if (_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }

            if (_mediaPlayer != null)
            {
                _mediaPlayer.Dispose();
                _mediaPlayer = null;
            }
        }

        bool MediaPlayer.IOnErrorListener.OnError(MediaPlayer mp, MediaError what, int extra)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Controller.OnMediaFailed();
            });

            return false;
        }

        bool MediaPlayer.IOnInfoListener.OnInfo(MediaPlayer mp, MediaInfo what, int extra)
        {
            if (_mediaPlayer == null)
            {
                _mediaPlayer = mp;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                switch (what)
                {
                    case MediaInfo.BufferingStart:
                        Controller.CurrentState = MediaElementState.Buffering;
                        mp.BufferingUpdate += Mp_BufferingUpdate;
                        break;

                    case MediaInfo.BufferingEnd:
                        mp.BufferingUpdate -= Mp_BufferingUpdate;
                        Controller.CurrentState = MediaElementState.Stopped;
                        break;

                    case MediaInfo.VideoRenderingStart:
                        _view.SetBackground(null);
                        Controller.CurrentState = MediaElementState.Playing;
                        break;
                }
            });
            
            return true;
        }

        void Mp_BufferingUpdate(object sender, MediaPlayer.BufferingUpdateEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Controller.BufferingProgress = e.Percent / 100f;
            });
        }
    }
}