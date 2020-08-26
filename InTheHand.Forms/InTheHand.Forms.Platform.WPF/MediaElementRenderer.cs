﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaElementRenderer.cs" company="In The Hand Ltd">
//   Copyright (c) 2018-20 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly:ExportRenderer(typeof(InTheHand.Forms.MediaElement), typeof(InTheHand.Forms.Platform.WPF.MediaElementRenderer))]
namespace InTheHand.Forms.Platform.WPF
{
    public sealed class MediaElementRenderer : ViewRenderer<InTheHand.Forms.MediaElement, System.Windows.Controls.MediaElement>
    {
        IMediaElementController Controller => Element as IMediaElementController;
        MediaElementState _requestedState;

        protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                Element.SeekRequested -= ElementSeekRequested;
                Element.StateRequested -= ElementStateRequested;
                Element.PositionRequested -= ElementPositionRequested;

                if (Control != null)
                {
                    Control.BufferingStarted -= ControlBufferingStarted;
                    Control.BufferingEnded -= ControlBufferingEnded;
                    Control.MediaOpened -= ControlMediaOpened;
                    Control.MediaEnded -= ControlMediaEnded;
                    Control.MediaFailed -= ControlMediaFailed;
                }
            }

            if (e.NewElement != null)
            {
                SetNativeControl(new System.Windows.Controls.MediaElement());
                Control.HorizontalAlignment = HorizontalAlignment.Stretch;
                Control.VerticalAlignment = VerticalAlignment.Stretch;

                Control.LoadedBehavior = MediaState.Manual;
                Control.UnloadedBehavior = MediaState.Close;
                Control.Stretch = ToStretch(Element.Aspect);

                Control.BufferingStarted += ControlBufferingStarted;
                Control.BufferingEnded += ControlBufferingEnded;
                Control.MediaOpened += ControlMediaOpened;
                Control.MediaEnded += ControlMediaEnded;
                Control.MediaFailed += ControlMediaFailed;

                Element.SeekRequested += ElementSeekRequested;
                Element.StateRequested += ElementStateRequested;
                Element.PositionRequested += ElementPositionRequested;
                UpdateSource();
            }
        }

        void ElementPositionRequested(object sender, EventArgs e)
        {
            if (!(Control is null))
            {
                Controller.Position = Control.Position;
            }
        }

        void ElementStateRequested(object sender, StateRequested e)
        {
            _requestedState = e.State;

            switch (e.State)
            {
                case MediaElementState.Playing:
                    if (Element.KeepScreenOn)
                    {
                        DisplayRequestActive();
                    }

                    Control.Play();
                    Controller.CurrentState = _requestedState;
                    break;

                case MediaElementState.Paused:
                    if (Control.CanPause)
                    {
                        if (Element.KeepScreenOn)
                        {
                            DisplayRequestRelease();
                        }

                        Control.Pause();
                        Controller.CurrentState = _requestedState;
                    }
                    break;

                case MediaElementState.Stopped:
                    if (Element.KeepScreenOn)
                    {
                        DisplayRequestRelease();
                    }

                    Control.Stop();
                    Controller.CurrentState = _requestedState;
                    break;
            }

            Controller.Position = Control.Position;
        }

        void ElementSeekRequested(object sender, SeekRequested e)
        {
            Control.Position = e.Position;
            Controller.Position = Control.Position;
        }

        void UpdateSource()
        {
            if (Element.Source is null)
                return;

            if (Control.Clock != null)
                Control.Clock = null;

            if(!Element.Source.IsAbsoluteUri)
            {
                Control.Source = Element.Source;
            }
            else if (Element.Source.Scheme == "ms-appx")
            {
                Control.Source = new Uri(Element.Source.ToString().Replace("ms-appx:///", ""), UriKind.Relative);
            }
            else if (Element.Source.Scheme == "ms-appdata")
            {
                string filePath = string.Empty;

                if (Element.Source.LocalPath.StartsWith("/local"))
                {
                    // WPF doesn't have the concept of an app package local folder so using My Documents as a placeholder
                    filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Element.Source.LocalPath.Substring(7));
                }
                else if (Element.Source.LocalPath.StartsWith("/temp"))
                {
                    filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Element.Source.LocalPath.Substring(6));
                }
                else
                {
                    throw new ArgumentException("Invalid Uri", "Source");
                }

                Control.Source = new Uri(filePath);
            }
            else if (Element.Source.Scheme == "https")
            {
                throw new ArgumentException("WPF supports only HTTP remote sources and not the HTTPS URI scheme.", "Source");
            }
            else
            {
                Control.Source = Element.Source;
            }

            Controller.CurrentState = MediaElementState.Opening;

            if(Element.AutoPlay)
            {
                Control.Play();
            }
        }

        void ControlBufferingEnded(object sender, RoutedEventArgs e)
        {
            Controller.BufferingProgress = 1.0;
            if (Element.AutoPlay)
            {
                Controller.CurrentState = MediaElementState.Playing;
            }
            else
            {
                Controller.CurrentState = _requestedState; ;
            }
        }

        void ControlBufferingStarted(object sender, RoutedEventArgs e)
        {
            Controller.BufferingProgress = 0.0;
            Controller.CurrentState = MediaElementState.Buffering;
        }

        void ControlMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Controller.OnMediaFailed();
        }

        void ControlMediaEnded(object sender, RoutedEventArgs e)
        {
            if (Element.IsLooping)
            {
                Control.Position = TimeSpan.Zero;
                Control.Play();
            }
            else
            {
                _requestedState = MediaElementState.Stopped;
                Controller.CurrentState = MediaElementState.Stopped;
                Controller.OnMediaEnded();
            }

            Controller.Position = Control.Position;
        }

        void ControlMediaOpened(object sender, RoutedEventArgs e)
        {
            Controller.Duration = Control.NaturalDuration.HasTimeSpan ? Control.NaturalDuration.TimeSpan : (TimeSpan?)null;
            Controller.VideoHeight = Control.NaturalVideoHeight;
            Controller.VideoWidth = Control.NaturalVideoWidth;
            Controller.OnMediaOpened();

            if (Element.AutoPlay)
            {
                Control.Play();
                _requestedState = MediaElementState.Playing;
                Controller.CurrentState = MediaElementState.Playing;              
            }
            else
            {
                Controller.CurrentState = _requestedState;
            }
        }

        void ControlSeekCompleted(object sender, RoutedEventArgs e)
        {
            Controller.Position = Control.Position;
            Controller.OnSeekCompleted();
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MediaElement.Aspect):
                    Control.Stretch = ToStretch(Element.Aspect);
                    break;

                case nameof(MediaElement.KeepScreenOn):
                    if (Element.KeepScreenOn)
                    {
                        if (Element.CurrentState == MediaElementState.Playing)
                        {
                            DisplayRequestActive();
                        }
                    }
                    else
                    {
                        DisplayRequestRelease();
                    }
                    break;

                case nameof(MediaElement.Source):
                    UpdateSource();
                    break;

                case nameof(MediaElement.Volume):
                    Control.Volume = Element.Volume;
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }

        protected override void UpdateWidth()
        {
            Control.Width = Math.Max(0, Element.Width);
        }

        protected override void UpdateHeight()
        {
            Control.Height = Math.Max(0, Element.Height);
        }

        void DisplayRequestActive()
        {
            NativeMethods.SetThreadExecutionState(NativeMethods.EXECUTION_STATE.DISPLAY_REQUIRED | NativeMethods.EXECUTION_STATE.CONTINUOUS);
        }

        void DisplayRequestRelease()
        {
            NativeMethods.SetThreadExecutionState(NativeMethods.EXECUTION_STATE.CONTINUOUS);
        }

        static class NativeMethods
        {
            [DllImport("Kernel32", SetLastError = true)]
            internal static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

            internal enum EXECUTION_STATE : uint
            {
                /// <summary>
                /// Informs the system that the state being set should remain in effect until the next call that uses ES_CONTINUOUS and one of the other state flags is cleared.
                /// </summary>
                CONTINUOUS = 0x80000000,

                /// <summary>
                /// Forces the display to be on by resetting the display idle timer.
                /// </summary>
                DISPLAY_REQUIRED = 0x00000002,
            }
        }

        static System.Windows.Media.Stretch ToStretch(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Fill:
                    return System.Windows.Media.Stretch.Fill;
                case Aspect.AspectFill:
                    return System.Windows.Media.Stretch.UniformToFill;
                default:
                case Aspect.AspectFit:
                    return System.Windows.Media.Stretch.Uniform;
            }
        }
    }
}
