using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(InTheHand.Forms.Platform.Android.ButtonLineBreakModeEffect), nameof(InTheHand.Forms.ButtonLineBreakModeEffect))]
namespace InTheHand.Forms.Platform.Android
{
    public sealed class ButtonLineBreakModeEffect : PlatformEffect<Xamarin.Forms.Button,global::Android.Widget.Button>
    {
        protected override void OnAttached()
        {
            if (Control == null)
                return;

            var effect = (InTheHand.Forms.ButtonLineBreakModeEffect)Element.Effects.FirstOrDefault(item => item is InTheHand.Forms.ButtonLineBreakModeEffect);
            if (effect != null)
            {
                switch (effect.LineBreakMode)
                {
                    case LineBreakMode.HeadTruncation:
                        Control.Ellipsize = TextUtils.TruncateAt.Start;
                        Control.SetSingleLine(true);
                        break;

                    case LineBreakMode.MiddleTruncation:
                        Control.Ellipsize = TextUtils.TruncateAt.Middle;
                        Control.SetSingleLine(true);
                        break;

                    case LineBreakMode.TailTruncation:
                        Control.Ellipsize = TextUtils.TruncateAt.End;
                        Control.SetSingleLine(true);
                        break;

                    case LineBreakMode.NoWrap:
                        Control.Ellipsize = null;
                        Control.SetSingleLine(true);
                        break;

                    case LineBreakMode.CharacterWrap:
                        ((Xamarin.Forms.Button)Element).Text = ((Xamarin.Forms.Button)Element).Text.Replace(' ', '\u00A0');
                        Control.Ellipsize = null;
                        Control.SetSingleLine(false);
                        break;

                    case LineBreakMode.WordWrap:
                        Control.Ellipsize = null;
                        Control.SetSingleLine(false);
                        break;
                }
            }
        }

        protected override void OnDetached()
        {
        }
    }
}