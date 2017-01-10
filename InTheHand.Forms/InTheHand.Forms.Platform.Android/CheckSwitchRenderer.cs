using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;
using InTheHand.Forms;

[assembly: ExportRenderer(typeof(CheckSwitch), typeof(InTheHand.Forms.Platform.Android.CheckSwitchRenderer))]

namespace InTheHand.Forms.Platform.Android
{
    public sealed class CheckSwitchRenderer : ViewRenderer<CheckSwitch, CheckBox>
    {
        public CheckSwitchRenderer() : base() 
        {
            System.Diagnostics.Debug.WriteLine("Created");
        }
        protected override void OnElementChanged(ElementChangedEventArgs<CheckSwitch> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                this.SetNativeControl(new CheckBox(this.Context));
                
                Control.SetTextColor(e.NewElement.TextColor.ToAndroid());

                this.Control.Text = e.NewElement.Text;
                this.Control.Checked = e.NewElement.IsToggled;
                this.Control.CheckedChange += Control_CheckedChange;
            }
        }

        void Control_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Element.IsToggled = e.IsChecked;
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Text":
                    Control.Text = Element.Text;
                    break;

                case "IsToggled":
                    Control.Checked = Element.IsToggled;
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}