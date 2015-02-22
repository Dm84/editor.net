using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

using Newtonsoft.Json.Linq;

namespace editor_wpf.Widget
{
	public class TextWidget : TextBox
	{
		JToken _val;
		string _text;

		public TextWidget(JToken val)
		{
			_val = val;
			_text = val.Value<string>();

			this.KeyUp += OnChange;
			this.Width = 190;
			this.Text = _text;
		}

		public void OnChange(Object sender, KeyEventArgs e)
		{
			_val = this.Text;
		}
	}
}
