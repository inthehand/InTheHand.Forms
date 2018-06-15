using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace InTheHand.Forms
{
    public sealed class ButtonLineBreakModeEffect : RoutingEffect
    {
        /// <summary>
        /// Gets or sets the LineBreakMode for the Button
        /// </summary>
        public LineBreakMode LineBreakMode { get; set; }

        public ButtonLineBreakModeEffect() : base(typeof(ButtonLineBreakModeEffect).FullName) { }
    }
}
