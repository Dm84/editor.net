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
			JToken token = value as JToken;

			if (token != null)
			{				
				switch (token.Type)
				{
					case JTokenType.Float: 
						double fVal = (float)token; 
						return new Widget.FloatWidget { 
							Minimum = fVal - fVal * 0.5f, 
							Maximum = fVal + fVal * 0.5f, 
							Value = fVal, 
							Width = 150 };

					default: return new TextBox { Width = 150, Text = token.ToString() };
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
