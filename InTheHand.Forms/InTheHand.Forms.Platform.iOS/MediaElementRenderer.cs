using AVFoundation;
using AVKit;
using CoreMedia;
using Foundation;
using InTheHand.Forms;
using System;
using UIKit;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.iOS.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.iOS
{
    public sealed class MediaElementRenderer : Xamarin.Forms.Platform.iOS.ViewRenderer<MediaElement,UIView>
    {
        private AVPlayerViewController _avPlayerViewController = new AVPlayerViewController();
        private NSObject _notificationHandle;
        private NSObject observer;

        protected override void OnElementChanged(Xamarin.Forms.Platform.iOS.ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;
            
            SetNativeControl(_avPlayerViewController.View);

            _avPlayerViewController.ShowsPlaybackControls = Element.AreTransportControlsEnabled;
            Control.BackgroundColor = UIColor.Black;

            if (Element.Source != null)
            {
                Element.SeekCompleted += Element_SeekCompleted;
                AVAsset asset = null;
                if (Element.Source.OriginalString.StartsWith("http"))
                {
                    asset = AVAsset.FromUrl(NSUrl.FromString(Element.Source.ToString()));
                }
                else
                {
                    asset = AVAsset.FromUrl(NSUrl.FromFilename(Element.Source.OriginalString.Substring(1)));                
                }

                AVPlayerItem item = new AVPlayerItem(asset);
                _avPlayerViewController.Player = new AVPlayer(item);
                _avPlayerViewController.VideoGravity = AVLayerVideoGravity.ResizeAspect;

                UIGestureRecognizer recog = new UITapGestureRecognizer(Touched);
                Control.GestureRecognizers = new UIGestureRecognizer[] { recog };
           
                _notificationHandle = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayedToEnd);
                observer = (NSObject)_avPlayerViewController.Player.CurrentItem.AddObserver("status", 0, ObserveStatus);
            }
        }

        protected override void Dispose(bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_notificationHandle);
            _avPlayerViewController.Player.CurrentItem.RemoveObserver(observer, "status");
            _notificationHandle = null;
            observer = null;
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
            try
            {
                Device.BeginInvokeOnMainThread(Element.OnMediaEnded);
            }
            catch { }
        }

        private void Element_SeekCompleted(object sender, EventArgs e)
        {
            _avPlayerViewController.Player.Seek(new CoreMedia.CMTime(Convert.ToInt64(Element.Position.TotalMilliseconds), 1000));
        }

        private void Touched()
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
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(_avPlayerViewController.Player.Status.ToString());

            switch (e.PropertyName)
            {
                case "AreTransportControlsEnabled":
                    _avPlayerViewController.ShowsPlaybackControls = Element.AreTransportControlsEnabled;
                    break;

                case "Width":
                case "Height":
                    System.Diagnostics.Debug.WriteLine(Element.Bounds);
                    break;
                case "Source":
                    if (Element.Source != null)
                    {
                        NSUrl url = null;

                        if (!Element.Source.OriginalString.StartsWith("/"))
                        {
                            url = NSUrl.FromString(Element.Source.ToString());
                        }
                        else
                        {
                            url = NSUrl.FromFilename(Element.Source.ToString().Substring(1));
                        }

                        _avPlayerViewController.Player.ReplaceCurrentItemWithPlayerItem(new AVPlayerItem(url));
                    }
                    
                    break;
                case "CurrentState":
                    System.Diagnostics.Debug.WriteLine(Element.CurrentState.ToString());
                    switch (Element.CurrentState)
                    {
                        case MediaElementState.Playing:
                            _avPlayerViewController.Player.Play();
                            System.Diagnostics.Debug.WriteLine(_avPlayerViewController.Player.Status.ToString());
                            break;

                        case MediaElementState.Paused:
                            _avPlayerViewController.Player.Pause();
                            break;

                        case MediaElementState.Stopped:
                            //ios has no stop...
                            _avPlayerViewController.Player.Pause();
                            _avPlayerViewController.Player.Seek(CMTime.Zero);
                            break;
                    }
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
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