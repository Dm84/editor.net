using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace editor_wpf.Widget
{
	class SliderWidget : Slider
	{
		public event DragCompletedEventHandler dragCompleted;

		protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
		{
			dragCompleted(this, e);
		}
	}
}
