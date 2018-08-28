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

[assembly: ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.iOS.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.iOS
{
    public sealed class MediaElementRenderer : Xamarin.Forms.Platform.iOS.ViewRenderer<MediaElement, UIView>, IMediaElementRenderer
    {
        private AVPlayerViewController _avPlayerViewController = new AVPlayerViewController();
        private NSObject _notificationHandle;
        private NSObject _observer;

        public double BufferingProgress
        {
            get
            {
                return _avPlayerViewController.Player.Status == AVPlayerStatus.ReadyToPlay ? 1.0 : 0.0;
            }
        }

        public TimeSpan Position
        {
            get
            {
                return TimeSpan.FromSeconds(_avPlayerViewController.Player.CurrentTime.Seconds);
            }
        }

        TimeSpan IMediaElementRenderer.NaturalDuration
        {
            get
            {
                return TimeSpan.FromSeconds(_avPlayerViewController.Player.CurrentItem != null ? _avPlayerViewController.Player.CurrentItem.Asset.Duration.Seconds : 0);
            }
        }

        int IMediaElementRenderer.NaturalVideoHeight
        {
            get
            {
                return (int)_avPlayerViewController.Player?.CurrentItem.Asset.NaturalSize.Height;
            }
        }

        int IMediaElementRenderer.NaturalVideoWidth
        {
            get
            {
                return (int)_avPlayerViewController.Player?.CurrentItem.Asset.NaturalSize.Width;
            }
        }

        protected override void OnElementChanged(Xamarin.Forms.Platform.iOS.ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                System.Diagnostics.Debug.WriteLine("OnElementChanged e.OldElement != null");

                e.OldElement.SetRenderer(null);

                if(_notificationHandle != null)
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(_notificationHandle);
                    _notificationHandle = null;
                }

                //stop video if playing
                if (_avPlayerViewController?.Player?.CurrentItem != null)
                {
                    RemoveStatusObserver();

                    _avPlayerViewController?.Player?.Pause();
                    _avPlayerViewController?.Player?.Seek(CMTime.Zero);
                    _avPlayerViewController?.Player?.ReplaceCurrentItemWithPlayerItem(null);
                    AVAudioSession.SharedInstance().SetActive(false);
                }
            }

            if (e.NewElement != null)
            {
                SetNativeControl(_avPlayerViewController.View);
                e.NewElement.SetRenderer(this);

                _avPlayerViewController.ShowsPlaybackControls = Element.AreTransportControlsEnabled;
                _avPlayerViewController.VideoGravity = AVLayerVideoGravity.ResizeAspect;
                if (Element.KeepScreenOn)
                {
                    SetKeepScreenOn(true);
                }

                _notificationHandle = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayedToEnd);

                UpdateSource();
            }
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
            System.Diagnostics.Debug.WriteLine("UpdateSource");


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
                    string filePath = string.Empty;

                    if (Element.Source.LocalPath.StartsWith("/local"))
                    {
                        filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Element.Source.LocalPath.Substring(7));
                    }
                    else if (Element.Source.LocalPath.StartsWith("/temp"))
                    {
                        filePath = Path.Combine(Path.GetTempPath(), Element.Source.LocalPath.Substring(6));
                    }

                    asset = AVAsset.FromUrl(NSUrl.FromFilename(filePath));
                }
                else
                {
                    asset = AVUrlAsset.Create(NSUrl.FromString(Element.Source.AbsoluteUri), GetOptionsWithHeaders(Element.HttpHeaders));
                }

                AVPlayerItem item = new AVPlayerItem(asset);

                RemoveStatusObserver();

                _observer = (NSObject)item.AddObserver("status", NSKeyValueObservingOptions.New, ObserveStatus);

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
                    var audioSession = AVAudioSession.SharedInstance();
                    NSError err = audioSession.SetCategory(AVAudioSession.CategoryPlayback);
                    audioSession.SetMode(AVAudioSession.ModeMoviePlayback, out err);
                    err = audioSession.SetActive(true);

                    _avPlayerViewController.Player.Play();
                    Element.CurrentState = MediaElementState.Playing;
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

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine(DateTimeOffset.Now + " Dispose " + this.GetHashCode());

            if (_notificationHandle != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_notificationHandle);
                _notificationHandle = null;
            }

            RemoveStatusObserver();

            _avPlayerViewController?.Player?.Pause();
            _avPlayerViewController?.Player?.ReplaceCurrentItemWithPlayerItem(null);

            base.Dispose(disposing);
        }

        private void RemoveStatusObserver()
        {
            if (_observer != null)
            {
                try
                {
                    _avPlayerViewController?.Player?.CurrentItem?.RemoveObserver(_observer, "status");
                }
                catch { }
                finally
                {
                    
                    _observer = null;
                }
            }
        }

        private void ObserveStatus(NSObservedChange e)
        {
            if (e.NewValue != null)
            {
                if (_avPlayerViewController.Player.Status == AVPlayerStatus.ReadyToPlay)
                {
                    Element?.RaiseMediaOpened();
                }

                System.Diagnostics.Debug.WriteLine(DateTimeOffset.Now + " " + e.NewValue.ToString());
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
                    Device.BeginInvokeOnMainThread(Element.OnMediaEnded);
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
                case "AreTransportControlsEnabled":
                    _avPlayerViewController.ShowsPlaybackControls = Element.AreTransportControlsEnabled;
                    break;

                /*case "Width":
                case "Height":
                    System.Diagnostics.Debug.WriteLine(Element.Bounds);
                    break;*/

                case "Source":
                    UpdateSource();
                    break;

                case "CurrentState":
                    System.Diagnostics.Debug.WriteLine(Element.CurrentState.ToString());
                    switch (Element.CurrentState)
                    {
                        case MediaElementState.Playing:
                            var audioSession = AVAudioSession.SharedInstance();
                            NSError err = audioSession.SetCategory(AVAudioSession.CategoryPlayback);
                            audioSession.SetMode(AVAudioSession.ModeMoviePlayback, out err);
                            err = audioSession.SetActive(true);

                            _avPlayerViewController.Player.Play();
                            if (Element.KeepScreenOn)
                            {
                                SetKeepScreenOn(true);
                            }
                            System.Diagnostics.Debug.WriteLine(_avPlayerViewController.Player.Status.ToString());
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
                            //ios has no stop...
                            _avPlayerViewController.Player.Pause();
                            _avPlayerViewController.Player.Seek(CMTime.Zero);

                            err = AVAudioSession.SharedInstance().SetActive(false);
                            break;
                    }

                    break;

                case "KeepScreenOn":
                    if (!Element.KeepScreenOn)
                    {
                        SetKeepScreenOn(false);
                    }
                    break;

                case "Stretch":
                    _avPlayerViewController.VideoGravity = StretchToGravity(Element.Stretch);
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }

        void IMediaElementRenderer.Seek(TimeSpan time)
        {
            if (_avPlayerViewController.Player.Status == AVPlayerStatus.ReadyToPlay)
            {
                if (_avPlayerViewController.Player.CurrentItem != null)
                {
                    NSValue[] ranges = _avPlayerViewController.Player.CurrentItem.SeekableTimeRanges;
                    CMTime seekTo = new CMTime(Convert.ToInt64(time.TotalMilliseconds), 1000);
                    bool canSeek = false;
                    foreach (NSValue v in ranges)
                    {
                        if (seekTo >= v.CMTimeRangeValue.Start && seekTo < (v.CMTimeRangeValue.Start + v.CMTimeRangeValue.Duration))
                        {
                            canSeek = true;
                            break;
                        }
                    }

                    if (canSeek)
                    {
                        _avPlayerViewController.Player.Seek(seekTo, SeekComplete);
                    }
                }
            }
        }

        private static AVLayerVideoGravity StretchToGravity(Stretch stretch)
        {
            switch (stretch)
            {
                case Stretch.Fill:
                    return AVLayerVideoGravity.Resize;

                case Stretch.UniformToFill:
                    return AVLayerVideoGravity.ResizeAspectFill;

                default:
                    return AVLayerVideoGravity.ResizeAspect;
            }
        }

        private void SeekComplete(bool finished)
        {
            if (finished)
            {
                Element.RaiseSeekCompleted();
            }
        }

        /*public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if(!double.IsInfinity(widthConstraint) && widthConstraint > 0)
            {
                return new SizeRequest(new Size(320,180));
            }
            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }*/
    }
}