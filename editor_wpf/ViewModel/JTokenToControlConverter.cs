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
using editor_wpf.Widget;

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
						return new Widget.FloatWidget(new SingleSource(prop));
					case JTokenType.Array:
						return new Widget.VectorWidget(prop);

					default: return new Widget.TextWidget(prop);
				}
			} else return null;			
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			Console.WriteLine("ConvertBack");
			return null;
		}
	}
}
