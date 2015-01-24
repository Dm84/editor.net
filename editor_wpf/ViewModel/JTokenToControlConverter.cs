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
	using Property = MainViewModel.Property;

	class JTokenToControlConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			Property prop = value as Property;	

			if (prop != null)
			{
				prop.prop.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
				{
					Console.WriteLine("changed");

				};

				JToken token = prop.prop.Value;
				switch (token.Type)
				{
					case JTokenType.Float: 
						return new Widget.FloatWidget(prop.prop);

					default: return new Widget.TextWidget(prop.prop);
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
