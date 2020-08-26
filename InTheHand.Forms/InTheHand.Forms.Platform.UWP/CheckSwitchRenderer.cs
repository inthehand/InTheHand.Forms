// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CheckSwitchRenderer.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using InTheHand.Forms;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;

#if WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;
#else
using Xamarin.Forms.Platform.WinRT;
#endif

[assembly: ExportRenderer(typeof(CheckSwitch), typeof(InTheHand.Forms.Platform.WinRT.CheckSwitchRenderer))]

namespace InTheHand.Forms.Platform.WinRT
{
    public sealed class CheckSwitchRenderer : VisualElementRenderer<CheckSwitch, Windows.UI.Xaml.Controls.CheckBox>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<CheckSwitch> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                this.SetNativeControl(new Windows.UI.Xaml.Controls.CheckBox());
                Control.Content = e.NewElement.Text;
                // set the initial state
                Control.IsChecked = e.NewElement.IsToggled;
                if (e.NewElement.TextColor != Color.Default)
                {
                    Control.BorderBrush = new Windows.UI.Xaml.Media.SolidColorBrush(Element.TextColor.ToWindowsColor());
                    Control.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Element.TextColor.ToWindowsColor());
                }
                Control.Checked += Control_Checked;
                Control.Unchecked += Control_Unchecked;
            }
        }

        void Control_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Element.IsToggled = false;
        }

        void Control_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Element.IsToggled = true;
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Text":
                    Control.Content = Element.Text;
                    break;

                case "TextColor":
                    Control.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Element.TextColor.ToWindowsColor());
                    break;

                case "IsToggled":
                    Control.IsChecked = Element.IsToggled;
                    break;
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}
