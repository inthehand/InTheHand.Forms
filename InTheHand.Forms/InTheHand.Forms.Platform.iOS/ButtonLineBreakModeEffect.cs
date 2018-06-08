using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using System.Linq;

[assembly: ExportEffect(typeof(InTheHand.Forms.Platform.iOS.ButtonLineBreakModeEffect), nameof(InTheHand.Forms.ButtonLineBreakModeEffect))]
namespace InTheHand.Forms.Platform.iOS
{
    public class ButtonLineBreakModeEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var control = Control as UILabel;

            if (control == null)
                return;

            var effect = (InTheHand.Forms.ButtonLineBreakModeEffect)Element.Effects.FirstOrDefault(item => item is InTheHand.Forms.ButtonLineBreakModeEffect);
            if (effect != null)
            {
                switch (effect.LineBreakMode)
                {
                    case LineBreakMode.HeadTruncation:
                        control.LineBreakMode = UILineBreakMode.HeadTruncation;
                        break;

                    case LineBreakMode.MiddleTruncation:
                        control.LineBreakMode = UILineBreakMode.MiddleTruncation;
                        break;

                    case LineBreakMode.TailTruncation:
                        control.LineBreakMode = UILineBreakMode.TailTruncation;
                        break;

                    case LineBreakMode.NoWrap:
                        control.LineBreakMode = UILineBreakMode.Clip;
                        break;

                    case LineBreakMode.CharacterWrap:
                        control.LineBreakMode = UILineBreakMode.CharacterWrap;
                        break;

                    case LineBreakMode.WordWrap:
                        control.LineBreakMode = UILineBreakMode.WordWrap;
                        break;
                }
            }
        }


        protected override void OnDetached()
        {
        }
    }
}