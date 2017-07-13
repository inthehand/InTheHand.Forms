// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaElementRenderer.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using InTheHand.Forms;
using System;
#if WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;
#else
using Xamarin.Forms.Platform.WinRT;
#endif
using Windows.UI.Xaml;
using System.Diagnostics;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.WinRT.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.WinRT
{
    public sealed class MediaElementRenderer : VisualElementRenderer<MediaElement, Windows.UI.Xaml.Controls.MediaElement>, IMediaElementRenderer
    {
        private Windows.System.Display.DisplayRequest _request = new Windows.System.Display.DisplayRequest();

#if WINDOWS_UWP
        private long _bufferingProgressChangedToken;

        private long _positionChangedToken;
#endif

        double IMediaElementRenderer.BufferingProgress
        {
            get
            {
                return Control.BufferingProgress;
            }
        }

        TimeSpan IMediaElementRenderer.NaturalDuration
        {
            get
            {
                if (Control.NaturalDuration.HasTimeSpan)
                {
                    return Control.NaturalDuration.TimeSpan;
                }

                return TimeSpan.Zero;
            }
        }

        int IMediaElementRenderer.NaturalVideoHeight
        {
            get
            {
                return Control.NaturalVideoHeight;
            }
        }

        int IMediaElementRenderer.NaturalVideoWidth
        {
            get
            {
                return Control.NaturalVideoWidth;
            }
        }

        TimeSpan IMediaElementRenderer.Position
        {
            get
            {
                return Control.Position;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                if (Control != null)
                {
#if WINDOWS_UWP
                    if (_positionChangedToken != 0)
                    {
                        Control.UnregisterPropertyChangedCallback(Windows.UI.Xaml.Controls.MediaElement.PositionProperty, _positionChangedToken);
                        _positionChangedToken = 0;
                    }
#endif
                    Control.CurrentStateChanged -= Control_CurrentStateChanged;
                    Control.SeekCompleted -= Control_SeekCompleted;
                    
                }

                e.OldElement.SetRenderer(null);
            }

            if (e.NewElement != null)
            {
                this.SetNativeControl(new Windows.UI.Xaml.Controls.MediaElement());
                e.NewElement.SetRenderer(this);
                Control.Stretch = Windows.UI.Xaml.Media.Stretch.Uniform;
                Control.AreTransportControlsEnabled = Element.AreTransportControlsEnabled;
                Control.AutoPlay = Element.AutoPlay;
                Control.IsLooping = Element.IsLooping;
                Control.Stretch = (Windows.UI.Xaml.Media.Stretch)Element.Stretch;
#if WINDOWS_UWP
                _bufferingProgressChangedToken = Control.RegisterPropertyChangedCallback(Windows.UI.Xaml.Controls.MediaElement.BufferingProgressProperty, BufferingProgressChanged);
                _positionChangedToken = Control.RegisterPropertyChangedCallback(Windows.UI.Xaml.Controls.MediaElement.PositionProperty, PositionChanged);
#endif
                Control.SeekCompleted += Control_SeekCompleted;
                Control.CurrentStateChanged += Control_CurrentStateChanged;

                if (Element.Source != null)
                {
                    if (Element.Source.IsAbsoluteUri)
                    {
                        Control.Source = Element.Source;
                    }
                    else
                    {
                        Control.Source = new Uri("ms-appx:///" + Element.Source.ToString());
                    }
                }
            }
        }

        private void Control_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch(Control.CurrentState)
            {
                case Windows.UI.Xaml.Media.MediaElementState.Playing:
                    if(Element.KeepScreenOn)
                    {
                        _request.RequestActive();
                    }
                    break;

                case Windows.UI.Xaml.Media.MediaElementState.Paused:
                case Windows.UI.Xaml.Media.MediaElementState.Stopped:
                case Windows.UI.Xaml.Media.MediaElementState.Closed:
                    if(Element.KeepScreenOn)
                    {
                        _request.RequestRelease();
                    }
                    break;
            }
            Element.CurrentState = (MediaElementState)((int)Control.CurrentState);
            //((IElementController)Element).SetValueFromRenderer(MediaElement.CurrentStateProperty, (MediaElementState)((int)Control.CurrentState));
            Element.RaiseCurrentStateChanged();
        }

#if WINDOWS_UWP
        private void BufferingProgressChanged(DependencyObject sender, DependencyProperty dp)
        {
            Debug.WriteLine("BufferingProgress");
            ((IElementController)Element).SetValueFromRenderer(MediaElement.BufferingProgressProperty, Control.BufferingProgress);
        }
        
        private void PositionChanged(DependencyObject sender, DependencyProperty dp)
        {
            Debug.WriteLine("Position");

            ((IElementController)Element).SetValueFromRenderer(MediaElement.PositionProperty, Control.Position);
        }     
#endif  

        private void Control_SeekCompleted(object sender, RoutedEventArgs e)
        {
            Element.RaiseSeekCompleted();
        }
               
        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AreTransportControlsEnabled":
                    Control.AreTransportControlsEnabled = Element.AreTransportControlsEnabled;
                    break;

                case "AutoPlay":
                    Control.AutoPlay = Element.AutoPlay;
                    break;

                case "CurrentState":
                    switch (Element.CurrentState)
                    {
                        case MediaElementState.Playing:
                            Control.Play();
                            break;

                        case MediaElementState.Paused:
                            Control.Pause();
                            break;

                        case MediaElementState.Stopped:
                            Control.Stop();
                            break;
                    }
                    break;

                case "IsLooping":
                    Control.IsLooping = Element.IsLooping;
                    break;

                case "KeepScreenOn":
                    if (Element.KeepScreenOn)
                    {
                        if (Control.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
                        {
                            _request.RequestActive();
                        }
                    }
                    else
                    {
                        _request.RequestRelease();
                    }
                    break;
                         
                case "Position":
                    Control.Position = (TimeSpan)Element.GetValue(MediaElement.PositionProperty);
                    break;

                case "Source":
                    Control.Source = Element.Source;
                    break;

                case "Stretch":
                    Control.Stretch = (Windows.UI.Xaml.Media.Stretch)Element.Stretch;
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}
