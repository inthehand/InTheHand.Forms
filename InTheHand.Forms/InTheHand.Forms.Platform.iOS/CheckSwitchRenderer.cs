using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using InTheHand.Forms;

[assembly: ExportRenderer(typeof(CheckSwitch), typeof(InTheHand.Forms.Platform.iOS.CheckSwitchRenderer))]

namespace InTheHand.Forms.Platform.iOS
{

    public sealed class CheckSwitchRenderer : Xamarin.Forms.Platform.iOS.ViewRenderer<CheckSwitch, UIKit.UIButton>
    {
        protected override void OnElementChanged(Xamarin.Forms.Platform.iOS.ElementChangedEventArgs<CheckSwitch> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;

            SetNativeControl(new UIButton());

            if(Element.TextColor != Color.Default)
            {
                Control.SetTitleColor(Element.TextColor.ToUIColor(), UIControlState.Normal);
            }

            Control.LineBreakMode = UILineBreakMode.WordWrap;
                Control.SetTitle((Element.IsToggled ? "☑" : "☐") + " " + Element.Text, UIControlState.Normal);
                Control.SetTitle((Element.IsToggled ? "☑" : "☐") + " " + Element.Text, UIControlState.Selected);
                Control.SetTitle((Element.IsToggled ? "☑" : "☐") + " " + Element.Text, UIControlState.Highlighted);
                Control.SetTitle((Element.IsToggled ? "☑" : "☐") + " " + Element.Text, UIControlState.Disabled);
                Control.TouchUpInside += Control_TouchUpInside;
            
        }

        void Control_TouchUpInside(object sender, EventArgs e)
        {
            Element.IsToggled = !Element.IsToggled;
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Text":
                case "IsToggled":
                    Control.SetTitle((Element.IsToggled ? "☑" : "☐") + " " + Element.Text, UIControlState.Normal);
                    Control.SetTitle((Element.IsToggled ? "☑" : "☐") + " " + Element.Text, UIControlState.Selected);
                    Control.SetTitle((Element.IsToggled ? "☑" : "☐") + " " + Element.Text, UIControlState.Highlighted);
                    Control.SetTitle((Element.IsToggled ? "☑" : "☐") + " " + Element.Text, UIControlState.Disabled);                    
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}