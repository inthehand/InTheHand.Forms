// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaElementRenderer.cs" company="In The Hand Ltd">
//   Copyright (c) 2017-19 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
    public sealed class MediaElementRenderer : Xamarin.Forms.Platform.iOS.ViewRenderer<MediaElement, UIView>
    {
        IMediaElementController Controller => Element as IMediaElementController;

        private AVPlayerViewController _avPlayerViewController = new AVPlayerViewController();
        private NSObject _playedToEndObserver;
        private NSObject _rateObserver;
        private NSObject _statusObserver;


        TimeSpan Position
        {
            get
            {
                if (_avPlayerViewController.Player.CurrentTime.IsInvalid)
                    return TimeSpan.Zero;

                return TimeSpan.FromSeconds(_avPlayerViewController.Player.CurrentTime.Seconds);
            }
        }

        protected override void OnElementChanged(Xamarin.Forms.Platform.iOS.ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                e.OldElement.PropertyChanged -= OnElementPropertyChanged;
                e.OldElement.SeekRequested -= MediaElementSeekRequested;
                e.OldElement.StateRequested -= MediaElementStateRequested;
                e.OldElement.PositionRequested -= MediaElementPositionRequested;
                e.OldElement.VolumeRequested -= MediaElementVolumeRequested;

                if (_playedToEndObserver != null)
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(_playedToEndObserver);
                    _playedToEndObserver = null;
                }

                // stop video if playing
                if (_avPlayerViewController?.Player?.CurrentItem != null)
                {
                    RemoveStatusObserver();

                    _avPlayerViewController.Player.Pause();
                    _avPlayerViewController.Player.Seek(CMTime.Zero);
                    _avPlayerViewController.Player.ReplaceCurrentItemWithPlayerItem(null);
                    AVAudioSession.SharedInstance().SetActive(false);
                }
            }

            if (e.NewElement != null)
            {
                SetNativeControl(_avPlayerViewController.View);

                Element.PropertyChanged += OnElementPropertyChanged;
                Element.SeekRequested += MediaElementSeekRequested;
                Element.StateRequested += MediaElementStateRequested;
                Element.PositionRequested += MediaElementPositionRequested;
                Element.VolumeRequested += MediaElementVolumeRequested;

                _avPlayerViewController.ShowsPlaybackControls = Element.ShowsPlaybackControls;
                _avPlayerViewController.VideoGravity = AspectToGravity(Element.Aspect);
                if (Element.KeepScreenOn)
                {
                    SetKeepScreenOn(true);
                }

                _playedToEndObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayedToEnd);

                UpdateBackgroundColor();
                UpdateSource();
            }
        }

        void UpdateBackgroundColor()
        {
            Control.BackgroundColor = Element.BackgroundColor.ToUIColor();
        }

        private bool _idleTimerDisabled = false;
        private void SetKeepScreenOn(bool value)
        {
            if (value)
            {
                if (!UIApplication.SharedApplication.IdleTimerDisabled)
                {
                    _idleTimerDisabled = true;
                    UIApplication.SharedApplication.IdleTimerDisabled = true;
                }
            }
            else
            {
                if (_idleTimerDisabled)
                {
                    _idleTimerDisabled = false;
                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                }
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

        private void UpdateSource()
        {
            if (Element.Source != null)
            {
                AVAsset asset = null;
                if (Element.Source.Scheme == null)
                {
                    // file path
                    asset = AVAsset.FromUrl(NSUrl.FromFilename(Element.Source.OriginalString));
                }
                else if (Element.Source.Scheme == "ms-appx")
                {
                    // used for a file embedded in the application package
                    asset = AVAsset.FromUrl(NSUrl.FromFilename(Element.Source.LocalPath.Substring(1)));
                }
                else if (Element.Source.Scheme == "ms-appdata")
                {
                    string filePath = ResolveMsAppDataUri(Element.Source);

                    if (string.IsNullOrEmpty(filePath))
                        throw new ArgumentException("Invalid Uri", "Source");

                    asset = AVAsset.FromUrl(NSUrl.FromFilename(filePath));
                }
                else if (Element.Source.IsFile)
                {
                    asset = AVAsset.FromUrl(NSUrl.FromFilename(Element.Source.LocalPath));
                }
                else
                {
                    asset = AVUrlAsset.Create(NSUrl.FromString(Element.Source.AbsoluteUri), GetOptionsWithHeaders(Element.HttpHeaders));
                }

                AVPlayerItem item = new AVPlayerItem(asset);

                RemoveStatusObserver();

                _statusObserver = (NSObject)item.AddObserver("status", NSKeyValueObservingOptions.New, ObserveStatus);

                if (_avPlayerViewController.Player != null)
                {
                    _avPlayerViewController.Player.ReplaceCurrentItemWithPlayerItem(item);
                }
                else
                {
                    _avPlayerViewController.Player = new AVPlayer(item);
                }

                if (Element.AutoPlay)
                {
                    Play();
                }
            }
            else
            {
                if (Element.CurrentState == MediaElementState.Playing || Element.CurrentState == MediaElementState.Buffering)
                {
                    Element.Stop();
                }
            }
        }

        internal static string ResolveMsAppDataUri(Uri uri)
        {
            if (uri.Scheme == "ms-appdata")
            {
                string filePath;

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
            if (_playedToEndObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_playedToEndObserver);
                _playedToEndObserver = null;
            }

            if (_rateObserver != null)
            {
                _rateObserver.Dispose();
                _rateObserver = null;
            }

            RemoveStatusObserver();

            _avPlayerViewController?.Player?.Pause();
            _avPlayerViewController?.Player?.ReplaceCurrentItemWithPlayerItem(null);

            base.Dispose(disposing);
        }

        private void RemoveStatusObserver()
        {
            if (_statusObserver != null)
            {
                try
                {
                    _avPlayerViewController?.Player?.CurrentItem?.RemoveObserver(_statusObserver, "status");
                }
                catch { }
                finally
                {
                    _statusObserver = null;
                }
            }
        }

        private void MediaElementVolumeRequested(object sender, EventArgs e)
        {
            Controller.Volume = _avPlayerViewController.Player.Volume;
        }

        void MediaElementPositionRequested(object sender, EventArgs e)
        {
            Controller.Position = Position;
        }

        void MediaElementSeekRequested(object sender, SeekRequested e)
        {
            if (_avPlayerViewController.Player.Status != AVPlayerStatus.ReadyToPlay || _avPlayerViewController.Player.CurrentItem == null)
                return;

            NSValue[] ranges = _avPlayerViewController.Player.CurrentItem.SeekableTimeRanges;
            CMTime seekTo = new CMTime(Convert.ToInt64(e.Position.TotalMilliseconds), 1000);
            foreach (NSValue v in ranges)
            {
                if (seekTo >= v.CMTimeRangeValue.Start && seekTo < (v.CMTimeRangeValue.Start + v.CMTimeRangeValue.Duration))
                {
                    _avPlayerViewController.Player.Seek(seekTo, SeekComplete);
                    break;
                }
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
                    if (Element.KeepScreenOn)
                    {
                        SetKeepScreenOn(false);
                    }

                    if (_avPlayerViewController.Player != null)
                    {
                        _avPlayerViewController.Player.Pause();
                        Controller.CurrentState = MediaElementState.Paused;
                    }
                    break;

                case MediaElementState.Stopped:
                    if (Element.KeepScreenOn)
                    {
                        SetKeepScreenOn(false);
                    }
                    // ios has no stop...
                    _avPlayerViewController?.Player.Pause();
                    _avPlayerViewController?.Player.Seek(CMTime.Zero);
                    Controller.CurrentState = MediaElementState.Stopped;

                    NSError err = AVAudioSession.SharedInstance().SetActive(false);
                    break;
            }

            Controller.Position = Position;
        }

        void ObserveRate(NSObservedChange e)
        {
            if (Controller is object)
            {
                switch (_avPlayerViewController.Player.Rate)
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
        }

        private void ObserveStatus(NSObservedChange e)
        {
            if (e.NewValue != null)
            {
                if (_avPlayerViewController.Player.Status == AVPlayerStatus.ReadyToPlay)
                {
                    Controller?.OnMediaOpened();
                }
            }
        }

        void Play()
        {
            var audioSession = AVAudioSession.SharedInstance();
            NSError err = audioSession.SetCategory(AVAudioSession.CategoryPlayback);

            audioSession.SetMode(AVAudioSession.ModeMoviePlayback, out err);

            err = audioSession.SetActive(true);

            if (_avPlayerViewController.Player != null)
            {
                _avPlayerViewController.Player.Play();
                Controller.CurrentState = MediaElementState.Playing;
            }

            if (Element.KeepScreenOn)
            {
                SetKeepScreenOn(true);
            }
        }

        private void PlayedToEnd(NSNotification notification)
        {
            if (Element.IsLooping)
            {
                _avPlayerViewController.Player.Seek(CMTime.Zero);
                _avPlayerViewController.Player.Play();
            }
            else
            {
                SetKeepScreenOn(false);

                try
                {
                    Device.BeginInvokeOnMainThread(Controller.OnMediaEnded);
                }
                catch { }
            }
        }

        /*private void Touched()
        {
            if (_avPlayerViewController.Player.Rate == 1.0)
            {
                Element.Pause();
                //player.Pause();
            }
            else
            {
                if(_avPlayerViewController.Player.CurrentTime == _avPlayerViewController.Player.CurrentItem.Duration)
                {
                    _avPlayerViewController.Player.Seek(CMTime.FromSeconds(0,1));
                }
                Element.Play();
                //player.Play();
            }
        }*/

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ShowsPlaybackControls":
                    _avPlayerViewController.ShowsPlaybackControls = Element.ShowsPlaybackControls;
                    break;

                case "BackgroundColor":
                    UpdateBackgroundColor();
                    break;

                case "Source":
                    UpdateSource();
                    break;

                case "CurrentState":
                    switch (Element.CurrentState)
                    {
                        case MediaElementState.Playing:
                            Play();
                            break;

                        case MediaElementState.Paused:
                            if (Element.KeepScreenOn)
                            {
                                SetKeepScreenOn(false);
                            }
                            _avPlayerViewController.Player.Pause();
                            break;

                        case MediaElementState.Stopped:
                            if (Element.KeepScreenOn)
                            {
                                SetKeepScreenOn(false);
                            }

                            // ios has no stop...
                            _avPlayerViewController.Player.Pause();
                            _avPlayerViewController.Player.Seek(CMTime.Zero);

                            var err = AVAudioSession.SharedInstance().SetActive(false);
                            break;
                    }

                    break;

                case "KeepScreenOn":
                    if (!Element.KeepScreenOn)
                    {
                        SetKeepScreenOn(false);
                    }
                    break;

                case nameof(Aspect):
                    _avPlayerViewController.VideoGravity = AspectToGravity(Element.Aspect);
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
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

        private void SeekComplete(bool finished)
        {
            if (finished)
            {
                Controller?.OnSeekCompleted();
            }
        }
    }
}

/*using AVFoundation;
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
        VisualElementPackager _packager;
        EventTracker _events;
        VisualElementTracker _tracker;
#pragma warning restore 0414

        NSObject _playToEndObserver;
        NSObject _statusObserver;
        NSObject _rateObserver;

        VisualElement IVisualElementRenderer.Element => MediaElement;

        UIView IVisualElementRenderer.NativeView
        {
            get
            {
                if (_isDisposed)
                    return new UIView();

                return View;
            }
        }

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

        
        private bool _isDisposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                Player?.Pause();

                if (_playToEndObserver != null)
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(_playToEndObserver);
                    _playToEndObserver.Dispose();
                    _playToEndObserver = null;
                }

                if (_rateObserver != null)
                {
                    _rateObserver.Dispose();
                    _rateObserver = null;
                }

                RemoveStatusObserver();

                Player?.Dispose();
                Player = null;

                View.RemoveFromSuperview();
                View.Dispose();

                if (disposing)
                {
                    _events?.Dispose();
                    _tracker?.Dispose();
                    _packager?.Dispose();
                }

                System.Diagnostics.Debug.WriteLine("BeforeDispose");
                base.Dispose(disposing);
                System.Diagnostics.Debug.WriteLine("AfterDispose");

            }
        }

        void RemoveStatusObserver()
        {
            if (_statusObserver != null)
            {
                _statusObserver.Dispose();
                _statusObserver = null;
            }
        }

        void ObserveRate(NSObservedChange e)
        {
            if (!_isDisposed && Controller is object)
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
        }

        void ObserveStatus(NSObservedChange e)
        {
            if (!_isDisposed && Controller is object)
            {
                Controller.Volume = Player.Volume;

                switch (Player.Status)
                {
                    case AVPlayerStatus.Failed:
                        Controller.OnMediaFailed();
                        break;

                    case AVPlayerStatus.ReadyToPlay:
                        if (!Player.CurrentItem.Duration.IsInvalid && !Player.CurrentItem.Duration.IsIndefinite)
                        {
                            Controller.Duration = TimeSpan.FromSeconds(Player.CurrentItem.Duration.Seconds);
                        }

                        Controller.VideoHeight = (int)Player.CurrentItem.Asset.NaturalSize.Height;
                        Controller.VideoWidth = (int)Player.CurrentItem.Asset.NaturalSize.Width;
                        Controller.OnMediaOpened();
                        Controller.Position = Position;
                        break;
                }
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
            if (_isDisposed)
                return;

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
                    // ios has no stop...
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
            if (_isDisposed)
                return new SizeRequest(new Size(widthConstraint, heightConstraint));

            return View.GetSizeRequest(widthConstraint, heightConstraint, 240, 180);
        }

        void IVisualElementRenderer.SetElement(VisualElement element)
        {
            MediaElement oldElement = MediaElement;

            if (oldElement != null)
            {
                oldElement.PropertyChanged -= OnElementPropertyChanged;
                oldElement.SeekRequested -= MediaElementSeekRequested;
                oldElement.StateRequested -= MediaElementStateRequested;
                oldElement.PositionRequested -= MediaElementPositionRequested;
                oldElement.VolumeRequested -= MediaElementVolumeRequested;
            }

            MediaElement = (MediaElement)element;

            if (element != null)
            {
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

                _packager = new VisualElementPackager(this);
                _packager.Load();

                _events = new EventTracker(this);
                _events.LoadEvents(View);
            }

            OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
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
            Layout.LayoutChildIntoBoundingRegion(MediaElement, new Rectangle(MediaElement.X, MediaElement.Y, size.Width, size.Height));
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (!_isDisposed)
            {
                // This is a temporary fix to stop zero width/height on resize or control expanding beyond page dimensions
                View.Frame = new CoreGraphics.CGRect(View.Frame.Left, View.Frame.Top, Math.Min(Math.Max(View.Frame.Width, View.Superview.Bounds.Width - View.Frame.X), View.Superview.Bounds.Width), Math.Min(Math.Max(View.Frame.Height, View.Superview.Bounds.Height - View.Frame.Y), View.Superview.Bounds.Height));
            }
        }
        
        void UpdateBackgroundColor()
        {
            View.BackgroundColor = MediaElement.BackgroundColor.ToUIColor();
        }
    }
}*/