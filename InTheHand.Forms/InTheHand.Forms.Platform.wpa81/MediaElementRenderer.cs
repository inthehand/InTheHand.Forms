// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaElementRenderer.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using InTheHand.Forms;
using System;
using Xamarin.Forms.Platform.WinRT;

[assembly: ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.WinRT.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.WinRT
{
    public sealed class MediaElementRenderer : VisualElementRenderer<MediaElement, Windows.UI.Xaml.Controls.MediaElement>, IMediaElementRenderer
    {
        public double BufferingProgress
        {
            get
            {
                return Control.BufferingProgress;
            }
        }

        public TimeSpan Position
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

                case "IsLooping":
                    Control.IsLooping = Element.IsLooping;
                    break;

                case "Source":
                    Control.Source = Element.Source;
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

                case "Position":
                    Control.Position = (TimeSpan)Element.GetValue(MediaElement.PositionProperty);
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}
