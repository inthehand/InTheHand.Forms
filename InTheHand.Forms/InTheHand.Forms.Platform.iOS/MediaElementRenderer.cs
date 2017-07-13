using AVFoundation;
using AVKit;
using CoreMedia;
using Foundation;
using InTheHand.Forms;
using System;
using System.IO;
using UIKit;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.iOS.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.iOS
{
    public sealed class MediaElementRenderer : Xamarin.Forms.Platform.iOS.ViewRenderer<MediaElement,UIView>, IMediaElementRenderer
    {
        private AVPlayerViewController _avPlayerViewController = new AVPlayerViewController();
        private NSObject _notificationHandle;
        private NSObject observer;

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
                e.OldElement.SetRenderer(null);
            }

            if (e.NewElement != null)
            {
                SetNativeControl(_avPlayerViewController.View);
                e.NewElement.SetRenderer(this);

                _avPlayerViewController.ShowsPlaybackControls = Element.AreTransportControlsEnabled;
                _avPlayerViewController.VideoGravity = AVLayerVideoGravity.ResizeAspect;
                if(Element.KeepScreenOn)
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
                if(_idleTimerDisabled)
                {
                    _idleTimerDisabled = false;
                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                }
            }
        }

        private void UpdateSource()
        {
            if (Element.Source != null)
            {
                AVAsset asset = null;
                if(Element.Source.Scheme == "ms-appx")
                {
                    // used for a file embedded in the application package
                    asset = AVAsset.FromUrl(NSUrl.FromFilename(Element.Source.LocalPath.Substring(1)));
                }
                else if (Element.Source.Scheme == "ms-appdata")
                {
                    asset = AVAsset.FromUrl(NSUrl.FromFilename(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Element.Source.LocalPath.Substring(1))));
                }
                else
                {
                    asset = AVAsset.FromUrl(NSUrl.FromString(Element.Source.ToString()));
                }

                AVPlayerItem item = new AVPlayerItem(asset);
                if(observer != null)
                {
                    if (_avPlayerViewController.Player != null && _avPlayerViewController.Player.CurrentItem != null)
                    {
                        _avPlayerViewController.Player.CurrentItem.RemoveObserver(observer, "status");
                    }

                    observer.Dispose();
                    observer = null;
                }

                observer = (NSObject)item.AddObserver("status", 0, ObserveStatus);

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
                    _avPlayerViewController.Player.Play();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_notificationHandle != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_notificationHandle);
                _notificationHandle = null;
            }

            if (observer != null)
            {
                _avPlayerViewController?.Player?.CurrentItem?.RemoveObserver(observer, "status");
                observer = null;
            }

            base.Dispose(disposing);
        }

        private void ObserveStatus(NSObservedChange e)
        {
            if (e.NewValue != null)
            {
                if(_avPlayerViewController.Player.Status == AVPlayerStatus.ReadyToPlay)
                {
                    Element.OnMediaOpened();
                }

                System.Diagnostics.Debug.WriteLine(e.NewValue.ToString());
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
                            _avPlayerViewController.Player.Play();
                            if(Element.KeepScreenOn)
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
                            break;
                    }

                    break;

                case "KeepScreenOn":
                    if (!Element.KeepScreenOn)
                    {
                        SetKeepScreenOn(false);
                    }
                    break;

                case "Position":
                    _avPlayerViewController.Player.Seek(new CoreMedia.CMTime(Convert.ToInt64(Element.Position.TotalMilliseconds), 1000), SeekComplete);
                    break;

                case "Stretch":
                    _avPlayerViewController.VideoGravity = StretchToGravity(Element.Stretch);
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }

        private static AVLayerVideoGravity StretchToGravity(Stretch stretch)
        {
            switch(stretch)
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