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

using Service = editor_wpf.Model.OperationModel;

namespace editor_wpf.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		/// <summary>
		/// объект с точки зрения представления
		/// </summary>
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

		public ObservableCollection<Entity> entities { get; set; }		

		IDictionary<string, Entity> _entityIndex = new Dictionary<string, Entity>();
		Service _serv;

		public void InstanceFeedback(Service.Instance instance)
		{
			App.Current.Dispatcher.Invoke(new Service.SendInstance((Service.Instance src) =>
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
			App.Current.Dispatcher.Invoke(new Service.SendEntities((ICollection<Service.Entity> collection) => {
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

		public void SetScriptResult(string result)
		{
			runResult += result;
		}
		
		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel()
		{
			entities = new ObservableCollection<Entity>();
			_serv = new Service(new Service.Args(Feedback, InstanceFeedback, SetScriptResult));
		}

		private bool _isWaiting = false;

		public bool isWaiting
		{
			get { return _isWaiting; }
			set { _isWaiting = value; RaisePropertyChanged("isWaiting"); }
		}

		public ICommand shutdownHost
		{
			get
			{
				return new ShutdownHostCommand(_serv);
			}
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

		public ICommand interpreterReset
		{
			get
			{
				return new InterpreterResetCommand(_serv);
			}
		}

		private abstract class Command : ICommand
		{
			public event EventHandler CanExecuteChanged;

			public bool CanExecute(object parameter)
			{
				return true;
			}

			public abstract void Execute(object parameter);
		}

		private abstract class ServCommand : Command
		{
			public ServCommand(Service serv)
			{
				_serv = serv;
			}

			override public abstract void Execute(object parameter);

			protected Service _serv;
		}

		private class ShutdownHostCommand : ServCommand
		{
			public ShutdownHostCommand(Service serv)
				: base(serv)
			{
			}

			public override void Execute(object parameter)
			{
				_serv.Shutdown();
			}
		}

		private class InterpreterResetCommand : ServCommand 
		{			
			public InterpreterResetCommand(Service serv) 
				: base(serv)
			{
			}

			override public void Execute(object parameter)
			{
				_serv.InterpreterReset();
			}
		}

		private class AddEntityCommand : ServCommand
		{
			Service _serv;
			IEnumerable<Entity> _entities;

			public AddEntityCommand(IEnumerable<Entity> entities, Service serv) 
				: base(serv)
			{
				_entities = entities;				
			}

			override public void Execute(object parameter)
			{
				if (parameter is Entity)
				{
					var selected = parameter as Entity;
					_serv.SetInstance(selected.name, new JObject(selected.props));
				}
				else
					throw new InvalidCastException();
			}
		}

		/// <summary>
		/// copy entity serialization to clipboard
		/// </summary>
		private class CopyEntityCommand : Command
		{
			override public void Execute(object parameter)
			{
				if (parameter is Instance)
				{
					var selected = parameter as Instance;
					Clipboard.SetText(new JObject(selected.data).ToString());
				}
				else
					throw new InvalidCastException();
			}
		}

		private class ClearInstancesCommand : ServCommand
		{
			public ClearInstancesCommand(Service serv)
				: base(serv)
			{
			}

			override public void Execute(object parameter)
			{
				_serv.ClearAll();
			}

		}

		private string _runResult;
		public string runResult {
			get { return _runResult;  }
			set
			{
				_runResult = value;
				RaisePropertyChanged("runResult");
			} 
		}

		class RunScriptCommand : ServCommand
		{
			public RunScriptCommand(Service serv)
				: base(serv)
			{
			}

			override public void Execute(object parameter)
			{
				_serv.RunScript(parameter.ToString());				
			}
		}

	}
}