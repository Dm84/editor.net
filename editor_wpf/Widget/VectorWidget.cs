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
	class VectorWidget : StackPanel
	{
		public VectorWidget(JProperty prop)
		{	
			this.Orientation = Orientation.Vertical;

			JArray array = prop.Value as JArray;

			for (int i = 0; i < array.Count; ++i)
			{
				FloatWidget widget = new FloatWidget(new ElementSource(prop, i));
				this.Children.Add(widget);
			}
		}
	}
}
