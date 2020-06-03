﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaElementRenderer.cs" company="In The Hand Ltd">
//   Copyright (c) 2017-19 In The Hand Ltd, All rights reserved.
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
using Controls = Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.System.Display;

[assembly: ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.WinRT.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.WinRT
{
    public sealed class MediaElementRenderer : VisualElementRenderer<MediaElement, Windows.UI.Xaml.Controls.MediaElement>
    {
        long _bufferingProgressChangedToken;
        long _positionChangedToken;
        DisplayRequest _request = new DisplayRequest();

        void ReleaseControl()
        {
            if (Control is null)
                return;

            if (_bufferingProgressChangedToken != 0)
            {
                Control.UnregisterPropertyChangedCallback(Controls.MediaElement.BufferingProgressProperty, _bufferingProgressChangedToken);
                _bufferingProgressChangedToken = 0;
            }

            if (_positionChangedToken != 0)
            {
                Control.UnregisterPropertyChangedCallback(Controls.MediaElement.PositionProperty, _positionChangedToken);
                _positionChangedToken = 0;
            }

            Element.SeekRequested -= SeekRequested;
            Element.StateRequested -= StateRequested;
            Element.PositionRequested -= PositionRequested;

            Control.CurrentStateChanged -= ControlCurrentStateChanged;
            Control.SeekCompleted -= ControlSeekCompleted;
            Control.MediaOpened -= ControlMediaOpened;
            Control.MediaEnded -= ControlMediaEnded;
            Control.MediaFailed -= ControlMediaFailed;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ReleaseControl();
        }

        protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                ReleaseControl();
            }

            if (e.NewElement != null)
            {
                SetNativeControl(new Controls.MediaElement());
                Control.HorizontalAlignment = HorizontalAlignment.Stretch;
                Control.VerticalAlignment = VerticalAlignment.Stretch;

                Control.AreTransportControlsEnabled = Element.ShowsPlaybackControls;
                Control.AutoPlay = Element.AutoPlay;
                Control.IsLooping = Element.IsLooping;
                Control.Stretch = ToStretch(Element.Aspect);

                _bufferingProgressChangedToken = Control.RegisterPropertyChangedCallback(Controls.MediaElement.BufferingProgressProperty, BufferingProgressChanged);
                _positionChangedToken = Control.RegisterPropertyChangedCallback(Controls.MediaElement.PositionProperty, PositionChanged);

                Element.SeekRequested += SeekRequested;
                Element.StateRequested += StateRequested;
                Element.PositionRequested += PositionRequested;
                Control.SeekCompleted += ControlSeekCompleted;
                Control.CurrentStateChanged += ControlCurrentStateChanged;
                Control.MediaOpened += ControlMediaOpened;
                Control.MediaEnded += ControlMediaEnded;
                Control.MediaFailed += ControlMediaFailed;

                UpdateSource();
            }
        }

        void PositionRequested(object sender, EventArgs e)
        {
            if (!(Control is null))
            {
                Controller.Position = Control.Position;
            }
        }

        IMediaElementController Controller => Element as IMediaElementController;

        void StateRequested(object sender, StateRequested e)
        {
            if (!(Control is null))
            {
                switch (e.State)
                {
                    case MediaElementState.Playing:
                        Control.Play();
                        break;

                    case MediaElementState.Paused:
                        if (Control.CanPause)
                        {
                            Control.Pause();
                        }
                        break;

                    case MediaElementState.Stopped:
                        Control.Stop();
                        break;
                }

                Controller.Position = Control.Position;
            }
        }

        void SeekRequested(object sender, SeekRequested e)
        {
            if (!(Control is null) && Control.CanSeek)
            {
                Control.Position = e.Position;
                Controller.Position = Control.Position;
            }
        }

        void ControlMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Controller?.OnMediaFailed();
        }

        void ControlMediaEnded(object sender, RoutedEventArgs e)
        {
            if (!(Control is null))
            {
                Controller.Position = Control.Position;
            }

            Controller.CurrentState = MediaElementState.Stopped;
            Controller.OnMediaEnded();
        }

        void ControlMediaOpened(object sender, RoutedEventArgs e)
        {
            Controller.Duration = Control.NaturalDuration.HasTimeSpan ? Control.NaturalDuration.TimeSpan : (TimeSpan?)null;
            Controller.VideoHeight = Control.NaturalVideoHeight;
            Controller.VideoWidth = Control.NaturalVideoWidth;

            Controller.OnMediaOpened();
        }


        void ControlCurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (Element is null || Control is null)
                return;

            switch (Control.CurrentState)
            {
                case Windows.UI.Xaml.Media.MediaElementState.Playing:
                    if (Element.KeepScreenOn)
                    {
                        _request.RequestActive();
                    }
                    break;

                case Windows.UI.Xaml.Media.MediaElementState.Paused:
                case Windows.UI.Xaml.Media.MediaElementState.Stopped:
                case Windows.UI.Xaml.Media.MediaElementState.Closed:
                    if (Element.KeepScreenOn)
                    {
                        _request.RequestRelease();
                    }
                    break;
            }

            Controller.CurrentState = FromWindowsMediaElementState(Control.CurrentState);
        }

        static MediaElementState FromWindowsMediaElementState(Windows.UI.Xaml.Media.MediaElementState state)
        {
            switch (state)
            {
                case Windows.UI.Xaml.Media.MediaElementState.Buffering:
                    return MediaElementState.Buffering;

                case Windows.UI.Xaml.Media.MediaElementState.Closed:
                    return MediaElementState.Closed;

                case Windows.UI.Xaml.Media.MediaElementState.Opening:
                    return MediaElementState.Opening;

                case Windows.UI.Xaml.Media.MediaElementState.Paused:
                    return MediaElementState.Paused;

                case Windows.UI.Xaml.Media.MediaElementState.Playing:
                    return MediaElementState.Playing;

                case Windows.UI.Xaml.Media.MediaElementState.Stopped:
                    return MediaElementState.Stopped;
            }

            throw new ArgumentOutOfRangeException();
        }

        void BufferingProgressChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (!(Control is null))
            {
                Controller.BufferingProgress = Control.BufferingProgress;
            }
        }

        void PositionChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (!(Control is null))
            {
                Controller.Position = Control.Position;
            }
        }

        void ControlSeekCompleted(object sender, RoutedEventArgs e)
        {
            if (!(Control is null))
            {
                Controller.Position = Control.Position;
                Controller.OnSeekCompleted();
            }
        }
        private static Stretch ToStretch(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Fill:
                    return Stretch.Fill;

                case Aspect.AspectFill:
                    return Stretch.UniformToFill;

                case Aspect.AspectFit:
                default:
                    return Stretch.Uniform;
            }
        }
        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MediaElement.Aspect):
                    Control.Stretch = ToStretch(Element.Aspect);
                    break;

                case nameof(MediaElement.AutoPlay):
                    Control.AutoPlay = Element.AutoPlay;
                    break;

                case nameof(MediaElement.IsLooping):
                    Control.IsLooping = Element.IsLooping;
                    break;

                case nameof(MediaElement.KeepScreenOn):
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

                case nameof(MediaElement.ShowsPlaybackControls):
                    Control.AreTransportControlsEnabled = Element.ShowsPlaybackControls;
                    break;

                case nameof(MediaElement.Source):
                    UpdateSource();
                    break;

                case nameof(MediaElement.Width):
                    Width = Math.Max(0, Element.Width);
                    break;

                case nameof(MediaElement.Height):
                    Height = Math.Max(0, Element.Height);
                    break;

                case nameof(MediaElement.Volume):
                    Control.Volume = Element.Volume;
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }

        void UpdateSource()
        {
            if (Element.Source is null)
                return;

            if (!Element.Source.IsAbsoluteUri)
            {
                Control.Source = new Uri(new Uri("ms-appx:///"), Element.Source);
            }
            else
            {
                Control.Source = Element.Source;
            }
        }
    }
}
