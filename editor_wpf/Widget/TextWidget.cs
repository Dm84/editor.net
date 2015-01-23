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
		JProperty _prop;
		string _val;

		public TextWidget(JProperty prop)
		{
			_prop = prop;
			_val = prop.Value.Value<string>();

			this.KeyUp += OnChange;
			this.Width = 190;
			this.Text = _val;
		}

		public void OnChange(Object sender, KeyEventArgs e)
		{
			_prop.Value = this.Text;
		}
	}
}
