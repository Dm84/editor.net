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
	class InstanceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			MainViewModel.Instance instance = value as MainViewModel.Instance;

			if (instance != null)
			{
				return instance.data;
			}
			else return null;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return new JValue(value);
		}
	}
}
