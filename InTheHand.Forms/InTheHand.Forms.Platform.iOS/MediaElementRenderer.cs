using AVFoundation;
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
        private AVPlayer player = null;
        private AVPlayerLayer layer = null;
        private bool layerAdded = false;
        private NSObject _notificationHandle;
        private NSObject observer;

        protected override void OnElementChanged(Xamarin.Forms.Platform.iOS.ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if(e.OldElement != null)
            {
                if (player != null)
                {
                    try
                    {
                        player.Pause();
                    }
                    catch { }
                }
            }

            if (e.OldElement != null || this.Element == null)
                return;
            
            SetNativeControl(new UIView());
            NativeView.BackgroundColor = UIColor.Black;

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
                player = new AVPlayer(item);
                layer = AVPlayerLayer.FromPlayer(player);
                layer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
                UIGestureRecognizer recog = new UITapGestureRecognizer(Touched);
                Control.GestureRecognizers = new UIGestureRecognizer[] { recog };
           
                _notificationHandle = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayedToEnd);
                observer = (NSObject)player.CurrentItem.AddObserver("status", 0, ObserveStatus);
            }
        }

        protected override void Dispose(bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_notificationHandle);
            player.CurrentItem.RemoveObserver(observer, "status");
            _notificationHandle = null;
            observer = null;
            base.Dispose(disposing);
        }

        private void ObserveStatus(NSObservedChange e)
        {
            if (e.NewValue != null)
            {
                if(player.Status == AVPlayerStatus.ReadyToPlay)
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
            player.Seek(new CoreMedia.CMTime(Convert.ToInt64(Element.Position.TotalMilliseconds), 1000));
        }

        private void Touched()
        {
            if (player.Rate == 1.0)
            {
                Element.Pause();
                //player.Pause();
            }
            else
            {
                if(player.CurrentTime == player.CurrentItem.Duration)
                {
                    player.Seek(CMTime.FromSeconds(0,1));
                }
                Element.Play();
                //player.Play();
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(player.Status.ToString());

            switch (e.PropertyName)
            {
                case "Width":
                case "Height":
                    System.Diagnostics.Debug.WriteLine(Element.Bounds);
                    Control.Frame = new CoreGraphics.CGRect(0, 0, Element.Width, Element.Height);
                    if (layer != null)
                    {
                        layer.Frame = Control.Frame;
                        layer.ZPosition = 0;
                    }
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

                        player = AVPlayer.FromUrl(url);
                    }

                    layer = AVPlayerLayer.FromPlayer(player);
                    break;
                case "CurrentState":
                    System.Diagnostics.Debug.WriteLine(Element.CurrentState.ToString());
                    switch (Element.CurrentState)
                    {
                        case MediaElementState.Playing:
                            layer.Frame = Control.Frame;
                            if (!layerAdded)
                            {
                                layerAdded = true;
                                Control.Layer.AddSublayer(layer);
                            }
                            player.Play();
                            System.Diagnostics.Debug.WriteLine(player.Status.ToString());
                            break;

                        case MediaElementState.Paused:
                            player.Pause();
                            break;

                        case MediaElementState.Stopped:
                            //ios has no stop...
                            player.Pause();
                            player.Seek(CMTime.Zero);
                            break;
                    }
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }

        public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if(!double.IsInfinity(widthConstraint) && widthConstraint > 0)
            {
                return new SizeRequest(new Size(320,180));
            }
            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }
    }
}