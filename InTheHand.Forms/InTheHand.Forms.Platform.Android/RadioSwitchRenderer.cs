using Android.Content;
using Android.Widget;
using InTheHand.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RadioSwitch), typeof(InTheHand.Forms.Platform.Android.RadioSwitchRenderer))]

namespace InTheHand.Forms.Platform.Android
{
    public sealed class RadioSwitchRenderer : ViewRenderer<RadioSwitch, RadioButton>
    {
        public RadioSwitchRenderer(Context c) : base(c)
        { }

        protected override void OnElementChanged(ElementChangedEventArgs<RadioSwitch> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                this.SetNativeControl(new RadioButton(this.Context));

                Control.SetTextColor(e.NewElement.TextColor.ToAndroid());
                this.Control.Text = e.NewElement.Text;
                this.Control.Checked = e.NewElement.IsToggled;
                this.Control.CheckedChange += Control_CheckedChange;
                Control.Click += Control_Click;
            }
        }

        private void Control_Click(object sender, System.EventArgs e)
        {
            Element.IsToggled = Control.Checked;
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
                    Control.SetText(Element.Text, TextView.BufferType.Normal);
                    break;

                case "IsToggled":
                    Control.Checked = Element.IsToggled;
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}