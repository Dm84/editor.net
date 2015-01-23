using System;
using System.Windows.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

using GalaSoft.MvvmLight;
using Newtonsoft.Json.Linq;

using Service = editor_wpf.Model.Model;

namespace editor_wpf.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		public class Instance
		{
			public string entity { get; set; }
			public string name { get; set;  }
			public string provider { get; set; }
			public JToken data { get; set; }
		}

		public class Entity
		{
			public string name { get; set; }
			public string desc { get; set; }
			public string provider { get; set; }

			public IEnumerable<JProperty> props { get; set; }
			public ObservableCollection<Instance> instances { get; set; }
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
				entity.instances.Add(new Instance { entity = src.entity, provider = src.provider, name = src.name, data = src.data });
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
				_serv.SetInstance(selected.provider, selected.name, new JObject(selected.props));
			}
		}

	}
}