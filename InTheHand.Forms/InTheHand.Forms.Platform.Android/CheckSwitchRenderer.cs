using Android.Content;
using Android.Widget;
using InTheHand.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CheckSwitch), typeof(InTheHand.Forms.Platform.Android.CheckSwitchRenderer))]

namespace InTheHand.Forms.Platform.Android
{
    public sealed class CheckSwitchRenderer : ViewRenderer<CheckSwitch, global::Android.Widget.CheckBox>
    {
        public CheckSwitchRenderer(Context c) : base(c)
        { }

        protected override void OnElementChanged(ElementChangedEventArgs<CheckSwitch> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                this.SetNativeControl(new global::Android.Widget.CheckBox(this.Context));
                
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