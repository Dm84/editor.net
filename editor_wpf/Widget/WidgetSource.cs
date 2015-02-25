using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace editor_wpf.Widget
{
	public interface FloatSource
	{
		double Value { get; set; }
	}

	public class SingleSource : FloatSource
	{
		private JProperty _prop;

		public SingleSource(JProperty prop)
		{
			_prop = prop;
		}

		public double Value
		{
			get
			{
				return _prop.Value.Value<double>();
			}

			set
			{
				_prop.Value = value;
			}
		}
	}

	public class ElementSource : FloatSource
	{
		private JProperty _prop;
		private int _index;

		public ElementSource(JProperty prop, int index)
		{
			_prop = prop;
			_index = index;
		}

		public double Value
		{
			get
			{
				return _prop.Value[_index].Value<double>();
			}
			set
			{
				//JToken newVal = new JValue(_prop.Value);
				_prop.Value[_index] = value;
				_prop.Value = _prop.Value;
			}
		}
	}
		
}
