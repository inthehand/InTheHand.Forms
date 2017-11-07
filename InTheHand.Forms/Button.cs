// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormattedButton.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Xamarin.Forms;

namespace InTheHand.Forms
{
    /// <summary>
    /// <see cref="Button"/> with additional formatting options.
    /// </summary>
    public sealed class FormattedButton : Button
    {
        /// <summary>
        /// Backing store for the <see cref="LineBreakMode"/> bindable property.
        /// </summary>
        public static readonly BindableProperty LineBreakModeProperty = BindableProperty.Create("LineBreakMode", typeof(LineBreakMode), typeof(FormattedButton), LineBreakMode.NoWrap);

        /// <summary>
        /// Gets or sets the <see cref="LineBreakMode"/> for the Button.
        /// This is a bindable property.
        /// </summary>
        public LineBreakMode LineBreakMode
        {
            get { return (LineBreakMode)GetValue(LineBreakModeProperty); }
            set { SetValue(LineBreakModeProperty, value); }
        }
    }
}
