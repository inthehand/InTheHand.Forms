using System.ComponentModel;
using System.Windows.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(InTheHand.Forms.Platform.WPF.ActivityIndicatorRenderer))]

namespace InTheHand.Forms.Platform.WPF
{
    public sealed class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, ProgressRing>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
        {
            base.OnElementChanged(e);

            if(e.NewElement != null)
            {
                SetNativeControl(new ProgressRing());
                Control.Foreground = new SolidColorBrush(Element.Color.ToMediaColor());
                Control.IsActive = Element.IsRunning;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(ActivityIndicator.Color):
                    Control.Foreground = new SolidColorBrush(Element.Color.ToMediaColor());
                    break;
                case nameof(ActivityIndicator.IsRunning):
                    Control.IsActive = Element.IsRunning;
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}
