using AVFoundation;
using AVKit;
using CoreMedia;
using Foundation;
using InTheHand.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.iOS.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.iOS
{
    public sealed class MediaElementRenderer : AVPlayerViewController, IVisualElementRenderer
    {
        MediaElement MediaElement { get; set; }
        IMediaElementController Controller => MediaElement as IMediaElementController;

#pragma warning disable 0414
        VisualElementTracker _tracker;
#pragma warning restore 0414

        NSObject _playToEndObserver;
        NSObject _statusObserver;
        NSObject _rateObserver;

        VisualElement IVisualElementRenderer.Element => MediaElement;

        UIView IVisualElementRenderer.NativeView => View;

        UIViewController IVisualElementRenderer.ViewController => this;

        bool _idleTimerDisabled = false;

        public MediaElementRenderer()
        {
            _playToEndObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayedToEnd);
            View.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            View.ContentMode = UIViewContentMode.ScaleToFill;
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }

        void SetKeepScreenOn(bool value)
        {
            if (value)
            {
                if (!UIApplication.SharedApplication.IdleTimerDisabled)
                {
                    _idleTimerDisabled = true;
                    UIApplication.SharedApplication.IdleTimerDisabled = true;
                }
            }
            else if (_idleTimerDisabled)
            {
                _idleTimerDisabled = false;
                UIApplication.SharedApplication.IdleTimerDisabled = false;
            }
        }

        private AVUrlAssetOptions GetOptionsWithHeaders(IDictionary<string, string> headers)
        {
            var nativeHeaders = new NSMutableDictionary();

            foreach (var header in headers)
            {
                nativeHeaders.Add((NSString)header.Key, (NSString)header.Value);
            }

            var nativeHeadersKey = (NSString)"AVURLAssetHTTPHeaderFieldsKey";

            var options = new AVUrlAssetOptions(NSDictionary.FromObjectAndKey(
                nativeHeaders,
                nativeHeadersKey
            ));

            return options;
        }

        void UpdateSource()
        {
            if (MediaElement.Source != null)
            {
                AVAsset asset = null;

                    if (MediaElement.Source.Scheme == "ms-appx")
                    {
                        // used for a file embedded in the application package
                        asset = AVAsset.FromUrl(NSUrl.FromFilename(MediaElement.Source.LocalPath.Substring(1)));
                    }
                    else if (MediaElement.Source.Scheme == "ms-appdata")
                    {
                        string filePath = ResolveMsAppDataUri(MediaElement.Source);

                        if (string.IsNullOrEmpty(filePath))
                            throw new ArgumentException("Invalid Uri", "Source");

                        asset = AVAsset.FromUrl(NSUrl.FromFilename(filePath));
                    }
                    else
                    {
                        asset = AVUrlAsset.Create(NSUrl.FromString(MediaElement.Source.AbsoluteUri), GetOptionsWithHeaders(MediaElement.HttpHeaders));
                    }


                AVPlayerItem item = new AVPlayerItem(asset);
                RemoveStatusObserver();

                _statusObserver = (NSObject)item.AddObserver("status", NSKeyValueObservingOptions.New, ObserveStatus);


                if (Player != null)
                {
                    Player.ReplaceCurrentItemWithPlayerItem(item);
                }
                else
                {
                    Player = new AVPlayer(item);
                    _rateObserver = (NSObject)Player.AddObserver("rate", NSKeyValueObservingOptions.New, ObserveRate);
                }

                if (MediaElement.AutoPlay)
                    Play();
            }
            else
            {
                if (MediaElement.CurrentState == MediaElementState.Playing || MediaElement.CurrentState == MediaElementState.Buffering)
                {
                    Player.Pause();
                    Controller.CurrentState = MediaElementState.Stopped;
                }
            }
        }
        internal static string ResolveMsAppDataUri(Uri uri)
        {
            if (uri.Scheme == "ms-appdata")
            {
                string filePath = string.Empty;

                if (uri.LocalPath.StartsWith("/local"))
                {
                    var libraryPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
                    filePath = Path.Combine(libraryPath, uri.LocalPath.Substring(7));
                }
                else if (uri.LocalPath.StartsWith("/temp"))
                {
                    filePath = Path.Combine(Path.GetTempPath(), uri.LocalPath.Substring(6));
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

        protected override void Dispose(bool disposing)
        {
            if (_playToEndObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_playToEndObserver);
                _playToEndObserver = null;
            }

            if (_rateObserver != null)
            {
                Player.RemoveObserver(_rateObserver, "rate");
                _rateObserver = null;
            }

            RemoveStatusObserver();

            Player?.Pause();
            Player?.ReplaceCurrentItemWithPlayerItem(null);

            base.Dispose(disposing);
        }

        void RemoveStatusObserver()
        {
            if (_statusObserver != null)
            {
                try
                {
                    Player?.CurrentItem?.RemoveObserver(_statusObserver, "status");
                }
                finally
                {

                    _statusObserver = null;
                }
            }
        }

        void ObserveRate(NSObservedChange e)
        {
            switch (Player.Rate)
            {
                case 0.0f:
                    Controller.CurrentState = MediaElementState.Paused;
                    break;

                case 1.0f:
                    Controller.CurrentState = MediaElementState.Playing;
                    break;
            }

            Controller.Position = Position;
        }

        void ObserveStatus(NSObservedChange e)
        {
            Controller.Volume = Player.Volume;

            switch (Player.Status)
            {
                case AVPlayerStatus.Failed:
                    Controller.OnMediaFailed();
                    break;

                case AVPlayerStatus.ReadyToPlay:
                    Controller.Duration = TimeSpan.FromSeconds(Player.CurrentItem.Duration.Seconds);
                    Controller.VideoHeight = (int)Player.CurrentItem.Asset.NaturalSize.Height;
                    Controller.VideoWidth = (int)Player.CurrentItem.Asset.NaturalSize.Width;
                    Controller.OnMediaOpened();
                    Controller.Position = Position;
                    break;
            }
        }

        TimeSpan Position
        {
            get
            {
                if (Player.CurrentTime.IsInvalid)
                    return TimeSpan.Zero;

                return TimeSpan.FromSeconds(Player.CurrentTime.Seconds);
            }
        }

        void PlayedToEnd(NSNotification notification)
        {
            if (MediaElement.IsLooping)
            {
                Player.Seek(CMTime.Zero);
                Controller.Position = Position;
                Player.Play();
            }
            else
            {
                SetKeepScreenOn(false);
                Controller.Position = Position;

                try
                {
                    Device.BeginInvokeOnMainThread(Controller.OnMediaEnded);
                }
                catch { }
            }
        }

        void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MediaElement.Aspect):
                    VideoGravity = AspectToGravity(MediaElement.Aspect);
                    break;

                case nameof(MediaElement.KeepScreenOn):
                    if (!MediaElement.KeepScreenOn)
                    {
                        SetKeepScreenOn(false);
                    }
                    else if (MediaElement.CurrentState == MediaElementState.Playing)
                    {
                        // only toggle this on if property is set while video is already running
                        SetKeepScreenOn(true);
                    }
                    break;

                case nameof(MediaElement.ShowsPlaybackControls):
                    ShowsPlaybackControls = MediaElement.ShowsPlaybackControls;
                    break;

                case nameof(MediaElement.Source):
                    UpdateSource();
                    break;

                case nameof(MediaElement.Volume):
                    Player.Volume = (float)MediaElement.Volume;
                    break;
            }
        }

        void MediaElementSeekRequested(object sender, SeekRequested e)
        {
            if (Player.Status != AVPlayerStatus.ReadyToPlay || Player.CurrentItem == null)
                return;

            NSValue[] ranges = Player.CurrentItem.SeekableTimeRanges;
            CMTime seekTo = new CMTime(Convert.ToInt64(e.Position.TotalMilliseconds), 1000);
            foreach (NSValue v in ranges)
            {
                if (seekTo >= v.CMTimeRangeValue.Start && seekTo < (v.CMTimeRangeValue.Start + v.CMTimeRangeValue.Duration))
                {
                    Player.Seek(seekTo, SeekComplete);
                    break;
                }
            }
        }

        void Play()
        {
            var audioSession = AVAudioSession.SharedInstance();
            NSError err = audioSession.SetCategory(AVAudioSession.CategoryPlayback);
            
            audioSession.SetMode(AVAudioSession.ModeMoviePlayback, out err);

            err = audioSession.SetActive(true);

            if (Player != null)
            {
                Player.Play();
                Controller.CurrentState = MediaElementState.Playing;
            }

            if (MediaElement.KeepScreenOn)
            {
                SetKeepScreenOn(true);
            }
        }

        void MediaElementStateRequested(object sender, StateRequested e)
        {
            MediaElementVolumeRequested(this, EventArgs.Empty);

            switch (e.State)
            {
                case MediaElementState.Playing:
                    Play();
                    break;

                case MediaElementState.Paused:
                    if (MediaElement.KeepScreenOn)
                    {
                        SetKeepScreenOn(false);
                    }

                    if (Player != null)
                    {
                        Player.Pause();
                        Controller.CurrentState = MediaElementState.Paused;
                    }
                    break;

                case MediaElementState.Stopped:
                    if (MediaElement.KeepScreenOn)
                    {
                        SetKeepScreenOn(false);
                    }
                    //ios has no stop...
                    Player.Pause();
                    Player.Seek(CMTime.Zero);
                    Controller.CurrentState = MediaElementState.Stopped;

                    NSError err = AVAudioSession.SharedInstance().SetActive(false);
                    break;
            }

            Controller.Position = Position;
        }

        static AVLayerVideoGravity AspectToGravity(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Fill:
                    return AVLayerVideoGravity.Resize;

                case Aspect.AspectFill:
                    return AVLayerVideoGravity.ResizeAspectFill;

                default:
                    return AVLayerVideoGravity.ResizeAspect;
            }
        }

        void SeekComplete(bool finished)
        {
            if (finished)
            {
                Controller.OnSeekCompleted();
            }
        }

        SizeRequest IVisualElementRenderer.GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            return View.GetSizeRequest(widthConstraint, heightConstraint, 240, 180);
        }

        void IVisualElementRenderer.SetElement(VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (!(element is MediaElement))
            {
                throw new ArgumentException($"{nameof(element)} must be of type {nameof(MediaElement)}");
            }

            MediaElement oldElement = MediaElement;
            MediaElement = (MediaElement)element;
            
            if (oldElement != null)
            {
                oldElement.PropertyChanged -= OnElementPropertyChanged;
                oldElement.SeekRequested -= MediaElementSeekRequested;
                oldElement.StateRequested -= MediaElementStateRequested;
                oldElement.PositionRequested -= MediaElementPositionRequested;
                oldElement.VolumeRequested -= MediaElementVolumeRequested;
            }

            Color currentColor = oldElement?.BackgroundColor ?? Color.Default;
            if (element.BackgroundColor != currentColor)
            {
                UpdateBackgroundColor();
            }

            MediaElement.PropertyChanged += OnElementPropertyChanged;
            MediaElement.SeekRequested += MediaElementSeekRequested;
            MediaElement.StateRequested += MediaElementStateRequested;
            MediaElement.PositionRequested += MediaElementPositionRequested;
            MediaElement.VolumeRequested += MediaElementVolumeRequested;

            UpdateSource();
            VideoGravity = AspectToGravity(MediaElement.Aspect);

            _tracker = new VisualElementTracker(this);

            OnElementChanged(new VisualElementChangedEventArgs(oldElement, MediaElement));
        }

        private void MediaElementVolumeRequested(object sender, EventArgs e)
        {
            Controller.Volume = Player.Volume;
        }

        void MediaElementPositionRequested(object sender, EventArgs e)
        {
            Controller.Position = Position;
        }

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        void OnElementChanged(VisualElementChangedEventArgs e)
        {
            ElementChanged?.Invoke(this, e);
        }

        void IVisualElementRenderer.SetElementSize(Size size)
        {
            MediaElement.Layout(new Rectangle(MediaElement.X, MediaElement.Y, size.Width, size.Height));
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            View.Frame = new CoreGraphics.CGRect(View.Frame.Left, View.Frame.Top, Math.Min(View.Frame.Width, View.Superview.Bounds.Width), Math.Min(View.Frame.Height, View.Superview.Bounds.Height));
        }
        
        void UpdateBackgroundColor()
        {
            View.BackgroundColor = MediaElement.BackgroundColor.ToUIColor();
        }
    }
}