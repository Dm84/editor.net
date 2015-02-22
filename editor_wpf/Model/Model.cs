using System;
using System.Windows.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace editor_wpf.Model
{
	/// <summary>
	/// incapsulates data exchange protocol
	/// </summary>
	public class Model
	{
		public delegate void AddEntities(ICollection<Entity> entities);
		public delegate void AddInstance(Instance instance);

		private IDictionary<string, Entity> _entityIndex = new Dictionary<string, Entity>();
		private WsService _ws;
		private AddEntities _entityFeedback;
		private AddInstance _instanceFeedback;

		public class Instance
		{
			public string name;
			public string entity;
			public string provider;
			public JToken data;

			public Instance(JObject src)
			{
				string[] fullName = src["method"].ToString().Split('_');
				provider = fullName.Length > 1 ? fullName[0] : "";
				entity = fullName[fullName.Length - 1];

				name = src["data"]["name"].ToString();
				data = src["data"];
			}
		}

		public class Entity
		{
			public string name;
			public string desc;
			public string provider;

			public IEnumerable<JProperty> props;		

			public Entity(JToken token)
			{
				name = token["name"].ToString();
				desc = token["desc"].ToString();
				provider = token["provider"].ToString();
				JToken def = token["params"];
				
				JObject defObj = def as JObject;

				if (defObj != null)
				{
					props = defObj.Properties();
				}
				else props = new LinkedList<JProperty>();
			}
		}
		

		public Model(AddEntities entityFeedback, AddInstance instanceFeedback)
		{
			_ws = new WsService();
			_entityFeedback = entityFeedback;
			_instanceFeedback = instanceFeedback;

			_ws.Opened += new EventHandler(OnOpenConnection);
			_ws.SetEvent += new EventHandler<JObject>(OnSetMethod);
			_ws.Open();
		}

		public void SetInstance(string provider, string entity, JObject data) {
			_ws.CallSet(provider + "_" + entity, data);
		}

		public void OnOpenConnection(Object sender, EventArgs e)
		{
			_ws.CallGet("entities", new JObject(), (JToken ret) =>
			{
				JArray array = ret as JArray;
				if (array != null)
				{
					List<Entity> entities = new List<Entity>();

					foreach (JToken token in array)
					{
						Entity entity = new Entity(token);
						_entityIndex[entity.name] = entity;
						entities.Add(entity);
					}

					_entityFeedback(entities);
				}
			});
		}

		public void OnSetMethod(Object sender, JObject obj)
		{
			_instanceFeedback(new Instance(obj));
		}



	}
}
