using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportEffect(typeof(InTheHand.Forms.Platform.UWP.ButtonLineBreakModeEffect), nameof(InTheHand.Forms.ButtonLineBreakModeEffect))]
namespace InTheHand.Forms.Platform.UWP
{
    [Preserve]
    public class ButtonLineBreakModeEffect : PlatformEffect
    {
        private InTheHand.Forms.ButtonLineBreakModeEffect _effect;

        protected override void OnAttached()
        {
            var control = Control as Windows.UI.Xaml.Controls.Button;

            if (control == null)
                return;

            control.Loaded += Control_Loaded;
            _effect = (InTheHand.Forms.ButtonLineBreakModeEffect)Element.Effects.FirstOrDefault(item => item is InTheHand.Forms.ButtonLineBreakModeEffect);
            
            
        }

        private void Control_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_effect != null)
            {
                var control = sender as Windows.UI.Xaml.Controls.Button;

                FrameworkElement element = control;
                TextBlock tb = null;

                while (tb == null)
                {
                    for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                    {
                        var child = VisualTreeHelper.GetChild(element, i);
                        if (child is TextBlock)
                        {
                            tb = child as TextBlock;
                            break;
                        }
                        else if (child is FrameworkElement)
                        {
                            element = child as FrameworkElement;
                        }
                    }
                }
                
                switch (_effect.LineBreakMode)
                {
                    case LineBreakMode.WordWrap:
                        tb.TextWrapping = Windows.UI.Xaml.TextWrapping.WrapWholeWords;
                        tb.TextTrimming = TextTrimming.None;
                        break;

                    case LineBreakMode.NoWrap:
                        tb.TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
                        tb.TextTrimming = TextTrimming.CharacterEllipsis;
                        break;

                    case LineBreakMode.CharacterWrap:
                        tb.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
                        tb.TextTrimming = TextTrimming.None;
                        break;

                    case LineBreakMode.TailTruncation:
                        tb.TextWrapping = TextWrapping.NoWrap;
                        tb.TextTrimming = Windows.UI.Xaml.TextTrimming.Clip;
                        break;

                }
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
