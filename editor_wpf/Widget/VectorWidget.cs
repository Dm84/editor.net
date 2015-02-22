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
		private FloatWidget[] _vector;

		public VectorWidget(JToken prop)
		{	
			this.Orientation = Orientation.Vertical;
			foreach (JValue val in prop)
			{
				FloatWidget widget = new FloatWidget(val);
				this.Children.Add(widget);
			}
		}
	}
}
