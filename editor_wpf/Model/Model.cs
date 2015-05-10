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

		public delegate void SetScriptResult(string scriptResult);

		/// <summary>
		/// индекс для поиска сущностей по имени
		/// </summary>
		private IDictionary<string, Entity> _entityIndex = new Dictionary<string, Entity>();

		/// <summary>
		/// веб-сокет сервис
		/// </summary>
		private WsService _ws;

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
				JToken defaultParams = token["params"];
				
				if (defaultParams is JObject)
				{
					props = (defaultParams as JObject).Properties();
				}
				else 
					props = new LinkedList<JProperty>();
			}
		}

		public class Args 
		{
			public Args(AddEntities	entityFeedback, AddInstance	instanceFeedback, SetScriptResult setScriptResult)
			{
				this.entityFeedback = entityFeedback;
				this.instanceFeedback = instanceFeedback;
				this.setScriptResult = setScriptResult;
			}

			public AddEntities		entityFeedback;
			public AddInstance		instanceFeedback;
			public SetScriptResult	setScriptResult;
		}

		private Args _args;

		public Model(Args args)
		{
			_args = args;

			_ws = new WsService();
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
				if (ret is JArray)
				{
					var array = ret as JArray;
					var entities = new List<Entity>();

					foreach (JToken token in array)
					{
						var entity = new Entity(token);
						_entityIndex[entity.name] = entity;
						entities.Add(entity);
					}

					_args.entityFeedback(entities);
				}
			});

			_ws.CallGet("instances", new JObject(), (JToken ret) =>
			{				
				if (ret is JArray)
				{
					var array = ret as JArray;
					foreach (JObject obj in array)
						_args.instanceFeedback(new Instance(obj["entity"].Value<string>(), obj["data"]));
				}
			});

		}

		public void OnSetMethod(Object sender, JObject obj)
		{
			if (obj["data"].HasValues)
				_args.instanceFeedback(new Instance(obj["method"].Value<string>(), obj["data"]));
		}

		public void InterpreterReset()
		{
			var obj = new JObject();
			_ws.CallGet("interpreter_reset", obj, new WsService.RpcCallback((JToken token) =>
			{
				throw new NotImplementedException();
			}));
		}

		public void RunScript(string filename)
		{
			var obj = new JObject();
			obj.Add("filename", filename);
			_ws.CallGet("interpreter_execute_file", obj, new WsService.RpcCallback((JToken token) => {

				string result = "";
				foreach (JToken log in token.AsJEnumerable())
				{
					result += log["result"].Value<string>() +"\n";	
				}
				_args.setScriptResult(result);
				
			}));
		}

	}
}
