using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using WebSocket4Net;

namespace editor_wpf.Model
{
	public class WsService : WebSocket
	{
		private const int SLEEP_PERIOD_MS = 200;

		public delegate void RpcCallback(JToken ret);

		private Dictionary<int, RpcCallback> _callbacks = new Dictionary<int, RpcCallback>();
		private Dictionary<int, JToken> _answers = new Dictionary<int, JToken>();

		private int _queryId = 0;

		public event EventHandler<JObject> OnSetEvent;

		public WsService() : base("ws://localhost:" + (0xDE10).ToString()) {
			this.MessageReceived += new EventHandler<MessageReceivedEventArgs>(OnMessage);
		}

		public void Reconnect()
		{
			base.Open();
		}

		private void HandleRequestAnswer(int answerId, JToken data)
		{
			if (_callbacks.ContainsKey(answerId))
			{
				_callbacks[answerId](data);
				_callbacks.Remove(answerId);
			}
			else
			{
				_answers[answerId] = data;
			}
		}

		private bool IsFromRequest(JObject answer)
		{
			return answer["id"] != null;
		}

		private void OnMessage(object obj, MessageReceivedEventArgs args) {
			JObject answer = JObject.Parse(args.Message);
			if (IsFromRequest(answer))
			{
				HandleRequestAnswer((int)answer["id"], answer["data"]);
			}
			else
			{
				OnSetEvent(this, answer);	
			}
		}
/*
		public Task<JToken> CallGetAsync(string method, JObject args)
		{
			return CallAsync(method, "get", args);
		}

		private Task<JToken> CallAsync(string method, string type, JObject args)
		{
			int id = _queryId++;

			SendObj(id, method, "get", args);

			Task<JToken> task = new Task<JToken>(() => 
			{
				while (!_answers.ContainsKey(id))
				{
					Thread.Sleep(TimeSpan.FromMilliseconds(SLEEP_PERIOD_MS));
				}
				return _answers[id];
			});

			return task;
		}
*/
		private void SendObj(int id, string method, string type, JObject args) {
			JObject json = new JObject();

			json.Add("id", id);
			json.Add("method", method);
			json.Add("type", type);
			json.Add("params", args);

			Send(json.ToString());
		}

		public void CallGet(string method, JObject args, RpcCallback callback) 
		{			
			_callbacks.Add(_queryId, callback);
			SendObj(_queryId++, method, "get", args);
		}

		public void CallSet(string method, JObject args)
		{
			SendObj(_queryId++, method, "set", args);
		}

	}
}
