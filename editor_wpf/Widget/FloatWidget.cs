using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json.Linq;

namespace editor_wpf.Widget
{
	public class FloatWidget : StackPanel, IWidget
	{
		public object Value { get { return _val; } }

		JToken _prop;
		double _val;

		TextBox _textBox;
		Slider _slider;

		const string FORMAT = "N2";

		public FloatWidget(JToken prop)
		{
			_prop = prop;
			_val = _prop.Value<double>();

			_textBox = new TextBox { Text = _val.ToString(FORMAT), Width = 50 };
			_slider = new Slider { Width = 150 };			

			this.Orientation = Orientation.Horizontal;

			this.Children.Add(_textBox);
			this.Children.Add(_slider);

			UpdateControl();

			this._slider.ValueChanged += SliderChanged;
		}

		private void UpdateControl()
		{
			double tresh = _val == 0.0 ? 0.5 : _val;
			_slider.Minimum = _val - tresh * 0.5;
			_slider.Maximum = _val + tresh * 0.5;
			_slider.Value = _val;
		}

		public void SliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			_val = e.NewValue;
			_prop = _val;

			_textBox.Text = _val.ToString(FORMAT);			
		}
	}
}
