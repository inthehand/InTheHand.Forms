using Xamarin.Forms;

namespace InTheHand.Forms
{
    public sealed class CheckSwitch : Xamarin.Forms.Switch 
    {
        //Bindable property for the text
        public static readonly BindableProperty TextProperty =
          BindableProperty.Create("Text", typeof(string), typeof(CheckSwitch), string.Empty);

        public static readonly BindableProperty TextColorProperty =
          BindableProperty.Create("TextColor", typeof(Color), typeof(CheckSwitch), Color.Default);

        //Gets or sets the text
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        //Gets or sets the text color
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }
    }
}
