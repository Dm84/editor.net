using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

using Newtonsoft.Json.Linq;

namespace editor_wpf.ViewModel
{
	class JTokenToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			JToken token = value as JToken;

			parameter = "Ok";

			return token;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{


			return new JValue(value);
		}

	}
}
