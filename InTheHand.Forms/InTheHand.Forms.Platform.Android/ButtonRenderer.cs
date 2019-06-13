// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormattedButtonRenderer.cs" company="In The Hand Ltd">
//   Copyright (c) 2017-19 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Android.Content;
using Android.Text;

using InTheHand.Forms;
using InTheHand.Forms.Platform.Android;

using System.ComponentModel;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FormattedButton), typeof(FormattedButtonRenderer))]
namespace InTheHand.Forms.Platform.Android
{
    public class FormattedButtonRenderer : Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer
    {
        public FormattedButtonRenderer(Context c) : base(c)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                SetLineBreakMode();
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
            FormattedButton b = Element as FormattedButton;

            switch (b.LineBreakMode)
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
                    Element.Text = Element.Text.Replace(' ', '\u00A0');
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
}