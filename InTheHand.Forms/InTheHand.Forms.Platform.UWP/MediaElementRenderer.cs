using InTheHand.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.UWP;

[assembly:ExportRenderer(typeof(MediaElement), typeof(InTheHand.Forms.Platform.UWP.MediaElementRenderer))]

namespace InTheHand.Forms.Platform.UWP
{
    public sealed class MediaElementRenderer : VisualElementRenderer<MediaElement, Windows.UI.Xaml.Controls.MediaElement>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                this.SetNativeControl(new Windows.UI.Xaml.Controls.MediaElement());
                Control.Stretch = Windows.UI.Xaml.Media.Stretch.Uniform;
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
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}
