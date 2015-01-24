using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Collections.Specialized;

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
				JToken token = prop.Value;
				switch (token.Type)
				{
					case JTokenType.Float: 
						return new Widget.FloatWidget(prop);

					default: return new Widget.TextWidget(prop);
				}
			} else return null;			
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			Console.WriteLine("ConvertBack");

			Widget.IWidget w = value as Widget.IWidget;
			return w != null ? new JValue(w.Value) : null;
		}
	}
}
