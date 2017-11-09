using CoreGraphics;
using InTheHand.Forms;
using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(FormattedButton), typeof(InTheHand.Forms.Platform.iOS.ButtonRenderer))]
namespace InTheHand.Forms.Platform.iOS
{
    public class ButtonRenderer : Xamarin.Forms.Platform.iOS.ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                SetLineBreakMode();
                Control.TitleEdgeInsets = new UIEdgeInsets(0, 8, 0, 8);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                case "Text":
                case "LineBreakMode":
                    SetLineBreakMode();
                    break;
            }
        }

        private void SetLineBreakMode()
        {
            var b = Element as FormattedButton;

            switch (b.LineBreakMode)
            {
                case LineBreakMode.HeadTruncation:
                    Control.LineBreakMode = UILineBreakMode.HeadTruncation;
                    break;

                case LineBreakMode.MiddleTruncation:
                    Control.LineBreakMode = UILineBreakMode.MiddleTruncation;
                    break;

                case LineBreakMode.TailTruncation:
                    Control.LineBreakMode = UILineBreakMode.TailTruncation;
                    break;

                case LineBreakMode.NoWrap:
                    Control.LineBreakMode = UILineBreakMode.Clip;
                    break;

                case LineBreakMode.CharacterWrap:
                    Control.LineBreakMode = UILineBreakMode.CharacterWrap;
                    break;

                case LineBreakMode.WordWrap:
                    Control.LineBreakMode = UILineBreakMode.WordWrap;
                    break;
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            var s = base.SizeThatFits(size);
            // as we are adding an 8pt border left/right deduct this when calculating fit
            var c = Control.TitleLabel.SizeThatFits(new CGSize(nfloat.IsInfinity(size.Width) ? size.Width : Math.Min(size.Width, s.Width) - 8, nfloat.IsInfinity(size.Height) ? size.Height : Math.Min(size.Height, s.Height)));
            // because this is the label size add some height for standard margins to match regular buttons
            return new CGSize(c.Width, c.Height + 24);
        }
    }
}