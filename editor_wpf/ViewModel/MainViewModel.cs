using System;
using System.Windows.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;


using GalaSoft.MvvmLight;
using Newtonsoft.Json.Linq;

using Service = editor_wpf.Model.Model;

namespace editor_wpf.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		public class Instance : Dictionary<string, JProperty>
		{
			public string entity { get; set; }
			public string name { get; set;  }
			public string provider { get; set; }

			public ICollection<JProperty> data { 
				get {
					return this.Values;
				}
			}
		}

		public class Entity
		{
			public string name { get; set; }
			public string desc { get; set; }
			public string provider { get; set; }

			public IEnumerable<JProperty> props { get; set; }
			public ICollection<Instance> instances { get; set; }

			private IDictionary<string, Instance> _map = new Dictionary<string, Instance>();

			public void AddInstance(Instance instance) {
				_map[instance.name] = instance;
				instances.Add(instance);
			}

			public bool HasInstance(String name)
			{
				return _map.ContainsKey(name);
			}

			public Instance GetInstance(String name)
			{
				return _map[name];
			}

		}

		delegate void TokenDelegate(JToken token);
		delegate void ObjDelegate(JObject obj);

		public ObservableCollection<Entity> entities { get; set; }

		IDictionary<string, Entity> _entityIndex = new Dictionary<string, Entity>();
		Service _serv;
				

		public void InstanceFeedback(Service.Instance instance)
		{
			App.Current.Dispatcher.Invoke(new Service.AddInstance((Service.Instance src) =>
			{
				Entity entity = _entityIndex[src.entity];

				Instance obj;

				if (entity.HasInstance(src.name))
				{
					obj = entity.GetInstance(src.name);

					////update props
					//foreach (JProperty prop in src.data)
					//{
					//	if (existent.ContainsKey(prop.Name) && existent[prop.Name].Value != prop.Value)
					//	{
					//		existent[prop.Name].Value = prop.Value; 
					//	}
					//}					
				} 
				else
				{
					obj = new Instance { entity = src.entity, name = src.name };
					entity.AddInstance(obj);
				}

				foreach (JProperty prop in src.data)
				{
					JProperty newProp = new JProperty(prop);
					newProp.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
					{
						JObject data = new JObject(newProp);
						data["name"] = src.name;

						if (src.data["scene"] != null)
						{
							data["scene"] = src.data["scene"];
						}

						_serv.SetInstance(src.entity, data);
					};
					if (obj.ContainsKey(newProp.Name))
						obj[newProp.Name] = newProp;
					else
						obj.Add(newProp.Name, newProp);
				};


			}), instance);
		}

		public void Feedback(ICollection<Service.Entity> feed)
		{
			App.Current.Dispatcher.Invoke(new Service.AddEntities((ICollection<Service.Entity> collection) => {
				foreach (Service.Entity item in collection)
				{
					Entity entity = new Entity
					{
						name = item.name,
						desc = item.desc,
						props = item.props,
						provider = item.provider,
						instances = new ObservableCollection<Instance>()
					};

					_entityIndex[entity.name] = entity;
					entities.Add(entity);
				}
			}), feed);
		}
		
		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel()
		{
			entities = new ObservableCollection<Entity>();
			_serv = new Service(Feedback, InstanceFeedback);
			//isWaiting = true;
		}

		private bool _isWaiting = false;

		public bool isWaiting
		{
			get { return _isWaiting; }
			set { _isWaiting = value; RaisePropertyChanged("isWaiting"); }
		}

		public ICommand addEntity
		{
			get {
				return new AddEntityCommand(entities, _serv);  
			}
		}

		public ICommand copyEntity
		{
			get
			{
				return new CopyEntityCommand();
			}
		}

		public ICommand clearInstances
		{
			get
			{
				return new ClearInstancesCommand(_serv);
			}
		}

		public ICommand runScript
		{
			get
			{
				return new RunScriptCommand(_serv);
			}
		}


		class AddEntityCommand : ICommand
		{
			Service _serv;
			IEnumerable<Entity> _entities;
			public event EventHandler CanExecuteChanged;

			public AddEntityCommand(IEnumerable<Entity> entities, Service serv)
			{
				_entities = entities;
				_serv = serv;
			}

			public bool CanExecute(object parameter)
			{
				return true;
			}

			public void Execute(object parameter)
			{
				Entity selected = parameter as Entity;
				_serv.SetInstance(selected.name, new JObject(selected.props));
			}
		}

		/// <summary>
		/// copy entity serialization to clipboard
		/// </summary>
		class CopyEntityCommand : ICommand
		{
			public event EventHandler CanExecuteChanged;

			public bool CanExecute(object parameter)
			{
				return true;
			}

			public void Execute(object parameter)
			{
				Instance selected = parameter as Instance;

				if (selected != null)
				{
					Clipboard.SetText(new JObject(selected.data).ToString());
				}
			}
		}

		class ClearInstancesCommand : ICommand
		{
			private Service _serv;
			public event EventHandler CanExecuteChanged;

			public ClearInstancesCommand(Service serv)
			{
				_serv = serv;
			}

			public bool CanExecute(object parameter)
			{
				return true;
			}

			public void Execute(object parameter)
			{
				_serv.ClearAll();
			}

		}

		class RunScriptCommand : ICommand
		{
			private Service _serv;
			public event EventHandler CanExecuteChanged;

			public RunScriptCommand(Service serv)
			{
				_serv = serv;
			}

			public bool CanExecute(object parameter)
			{
				return true;
			}

			public void Execute(object parameter)
			{
				_serv.RunScript(parameter.ToString());
			}

		}

	}
}