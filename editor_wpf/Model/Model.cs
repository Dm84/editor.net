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
		/// <summary>
		/// обратная связь для передачи сущностей обработчику
		/// </summary>
		/// <param name="entities"></param>
		public delegate void AddEntities(ICollection<Entity> entities);

		/// <summary>
		/// ... для объектов
		/// </summary>
		/// <param name="instance"></param>
		public delegate void AddInstance(Instance instance);

		/// <summary>
		/// индекс для поиска сущностей по имени
		/// </summary>
		private IDictionary<string, Entity> _entityIndex = new Dictionary<string, Entity>();

		/// <summary>
		/// веб-сокет сервис
		/// </summary>
		private WsService _ws;

		/// <summary>
		/// функции обр. вызова регистрируются в момент создания модели
		/// </summary>
		private AddEntities _entityFeedback;
		private AddInstance _instanceFeedback;

		/// <summary>
		/// объект сущности
		/// </summary>
		public class Instance
		{
			public string name;
			public string entity;
			public JToken data;

			public Instance(string entity, JToken data)
			{
				this.entity = entity;
				this.name = data["name"].Value<string>();
				this.data = data;
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

		public void SetInstance(string entity, JObject data) {

			_ws.CallSet(entity, data);
		}

		public void ClearAll()
		{
			_ws.CallSet("clear_all", new JObject());
		}

		/// <summary>
		/// запрашиваем доступные сущности и уже созданные объекты после открытия соединения
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

			_ws.CallGet("instances", new JObject(), (JToken ret) =>
			{
				JArray array = ret as JArray;
				if (array != null)
				{
					foreach (JObject obj in array)
					{
						_instanceFeedback(new Instance(obj["entity"].Value<string>(), obj["data"]));
					}
					
				}
			});

		}

		public void OnSetMethod(Object sender, JObject obj)
		{
			if (obj["data"].HasValues)
				_instanceFeedback(new Instance(obj["method"].Value<string>(), obj["data"]));
		}

		public void RunScript(string filename)
		{
			var obj = new JObject();
			obj.Add("filename", filename);
			_ws.CallGet("execute_file", obj, new WsService.RpcCallback((JToken token) => {

				Console.WriteLine("executed: " + token.ToString());

			}));
		}

	}
}
