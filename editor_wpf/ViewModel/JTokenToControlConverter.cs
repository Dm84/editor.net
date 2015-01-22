using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;

using Newtonsoft.Json.Linq;



namespace editor_wpf.ViewModel
{
	class JTokenToControlConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			JProperty prop = value as JProperty;

			if (prop != null)
			{
				JToken val = prop.Value;
				switch (prop.Value.Type)
				{
					case JTokenType.Float: 
						float fVal = (float)val; 
						return new Slider { SelectionStart = fVal - fVal * 0.5f, SelectionEnd = fVal + fVal * 0.5f, Value = fVal, Width = 150 };

					default: return new TextBox { Width = 150, Text = val.ToString() };
				}
			} else return null;			
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return new JValue(value);
		}
	}
}
