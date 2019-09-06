using System;
using System.ComponentModel;
using AVFoundation;
using AVKit;
using Xamarin.Forms.Platform.MacOS;
using Xamarin.Forms;
using InTheHand.Forms;
using InTheHand.Forms.Platform.MacOS;
using System.Collections.Generic;
using Foundation;
using CoreMedia;
using System.IO;
using AppKit;

[assembly:ExportRenderer(typeof(MediaElement), typeof(MediaElementRenderer))]
namespace InTheHand.Forms.Platform.MacOS
{
    public class MediaElementRenderer : Xamarin.Forms.Platform.MacOS.ViewRenderer<MediaElement, AVPlayerView>
    {
        IMediaElementController Controller => Element as IMediaElementController;

        NSObject _playToEndObserver;
        NSObject _statusObserver;
        NSObject _rateObserver;

        protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if(e.NewElement != null)
            {

                _playToEndObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayedToEnd);

                if (Control == null)
                {
                    SetNativeControl(new AVPlayerView());
                }

                Control.VideoGravity = AspectToGravity(Element.Aspect);
                UpdateSource();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch(e.PropertyName)
            {
                case nameof(MediaElement.Aspect):
                    Control.VideoGravity = AspectToGravity(Element.Aspect);
                    break;
                case nameof(MediaElement.Source):
                    UpdateSource();
                    break;
            }
        }

        static string AspectToGravity(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Fill:
                    return "AVLayerVideoGravityResize";

                case Aspect.AspectFill:
                    return "AVLayerVideoGravityResizeAspectFill";

                default:
                    return "AVLayerVideoGravityResizeAspect";
            }
        }

        void RemoveStatusObserver()
        {
            if (_statusObserver != null)
            {
                try
                {
                    Control.Player?.CurrentItem?.RemoveObserver(_statusObserver, "status");
                }
                finally
                {

                    _statusObserver = null;
                }
            }
        }

        void ObserveRate(NSObservedChange e)
        {
            switch (Control.Player.Rate)
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
            Controller.Volume = Control.Player.Volume;

            switch (Control.Player.Status)
            {
                case AVPlayerStatus.Failed:
                    Controller.OnMediaFailed();
                    break;

                case AVPlayerStatus.ReadyToPlay:
                    if (!Control.Player.CurrentItem.Duration.IsIndefinite)
                    {
                        Controller.Duration = TimeSpan.FromSeconds(Control.Player.CurrentItem.Duration.Seconds);
                    }
                    Controller.VideoHeight = (int)Control.Player.CurrentItem.Asset.NaturalSize.Height;
                    Controller.VideoWidth = (int)Control.Player.CurrentItem.Asset.NaturalSize.Width;
                    Controller.OnMediaOpened();
                    Controller.Position = Position;
                    break;
            }
        }

        TimeSpan Position
        {
            get
            {
                if (Control.Player.CurrentTime.IsInvalid)
                    return TimeSpan.Zero;

                return TimeSpan.FromSeconds(Control.Player.CurrentTime.Seconds);
            }
        }

        void PlayedToEnd(NSNotification notification)
        {
            if (Element.IsLooping)
            {
                Control.Player.Seek(CMTime.Zero);
                Controller.Position = Position;
                Control.Player.Play();
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

        void Play()
        {

            if (Control.Player != null)
            {
                Control.Player.Play();
                Controller.CurrentState = MediaElementState.Playing;
            }

            if (Element.KeepScreenOn)
            {
                SetKeepScreenOn(true);
            }
        }

        void SetKeepScreenOn(bool value)
        {
        }

        void UpdateSource()
        {
            if (Element.Source != null)
            {
                AVAsset asset = null;

                if (Element.Source.Scheme == "ms-appx")
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
                else
                {
                    asset = AVUrlAsset.Create(NSUrl.FromString(Element.Source.AbsoluteUri), GetOptionsWithHeaders(Element.HttpHeaders));
                }


                AVPlayerItem item = new AVPlayerItem(asset);
                RemoveStatusObserver();

                _statusObserver = (NSObject)item.AddObserver("status", NSKeyValueObservingOptions.New, ObserveStatus);


                if (Control.Player != null)
                {
                    Control.Player.ReplaceCurrentItemWithPlayerItem(item);
                }
                else
                {
                    Control.Player = new AVPlayer(item);
                    _rateObserver = (NSObject)Control.Player.AddObserver("rate", NSKeyValueObservingOptions.New, ObserveRate);
                }

                if (Element.AutoPlay)
                    Play();
            }
            else
            {
                if (Element.CurrentState == MediaElementState.Playing || Element.CurrentState == MediaElementState.Buffering)
                {
                    Control.Player.Pause();
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
                Control?.Player.RemoveObserver(_rateObserver, "rate");
                _rateObserver = null;
            }

            RemoveStatusObserver();

            Control?.Player?.Pause();
            Control?.Player?.ReplaceCurrentItemWithPlayerItem(null);

            base.Dispose(disposing);
        }
    }
}
