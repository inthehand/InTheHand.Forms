using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ChatList
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

            this.BindingContext = new List<string> { "First message", "Second message, much longer than the first and will require significantly more space", "Third message, fairly short", "Forth message, didn't really think this through so just making up text to fill up a lot of space just to illustrate different item heights" };
		}
	}
}
