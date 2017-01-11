// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CheckSwitch.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Xamarin.Forms;

namespace InTheHand.Forms
{
    /// <summary>
    /// Checkbox control derived from Switch.
    /// </summary>
    [ContentProperty("Text")]
    public sealed class CheckSwitch : Xamarin.Forms.Switch 
    {
        /// <summary>
        /// <see cref="BindableProperty"/>. Backing store for the Text bindable property.
        /// </summary>
        public static readonly BindableProperty TextProperty =
          BindableProperty.Create("Text", typeof(string), typeof(CheckSwitch), string.Empty);

        /// <summary>
        /// <see cref="BindableProperty"/>. Backing store for the TextColor bindable property.
        /// </summary>
        public static readonly BindableProperty TextColorProperty =
          BindableProperty.Create("TextColor", typeof(Color), typeof(CheckSwitch), Color.Default);

        /// <summary>
        /// The text that will appear on the CheckSwitch.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// <see cref="Color"/>. Gets or sets the Color for the text of this Label.
        /// This is a bindable property.
        /// </summary>
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }
    }
}
