﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

using Newtonsoft.Json.Linq;

namespace editor_wpf.Widget
{
	public class FloatWidget : StackPanel
	{
		public object Value { get { return _val; } }

		FloatSource _prop;
		double _val;

		TextBox			_textBox;
		SliderWidget	_slider;
		bool			_isProgrammatic = false;

		const string FORMAT = "N2";

		public FloatWidget(FloatSource prop)
		{
			Console.WriteLine("construct");

			_prop = prop;
			_val = _prop.Value;

			_textBox = new TextBox { Text = _val.ToString(), Width = 50 };
			_slider = new SliderWidget { Width = 150 };			

			this.Orientation = Orientation.Horizontal;

			this.Children.Add(_textBox);
			this.Children.Add(_slider);

			UpdateSlider();

			this._slider.ValueChanged += SliderChanged;
			this._textBox.LostKeyboardFocus += FocusLost;
			this._slider.dragCompleted += DragCompleted;
		}

		private void FocusLost(object sender, KeyboardFocusChangedEventArgs e)
		{			
			try
			{
				_val = double.Parse(this._textBox.Text);
				Console.WriteLine("changed: " + _val.ToString());

				UpdateSlider();
			} catch
			{
				this._textBox.Undo();
			}

		}

		private void UpdateSlider()
		{
			double tresh = _val == 0.0 ? 1.0 : _val;

			_slider.Value = _val;
			_slider.Minimum = _val - tresh * 0.5;
			_slider.Maximum = _val + tresh * 0.5;
			
		}

		public void DragCompleted(object sender, DragCompletedEventArgs e)
		{
			UpdateSlider();
		}

		public void SliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_val != e.NewValue)
			{
				Console.WriteLine("slider changed:" + e.NewValue.ToString());

				_val = e.NewValue;
				_prop.Value = _val;

				_isProgrammatic = true;
				_textBox.Text = _val.ToString();
			}
		}
	}
}
