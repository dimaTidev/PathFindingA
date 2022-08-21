using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

namespace DimaTi.PathFinding
{
	public class RequestManager : MonoBehaviour
	{
		Queue<IRequestResult> results = new Queue<IRequestResult>();

		static RequestManager s_instance;
		public static RequestManager Instance => s_instance ??= new GameObject("RequestManager_SINGLETON").AddComponent<RequestManager>();

		void Update()
		{
			if (results.Count > 0)
			{
				int itemsInQueue = results.Count;
				lock (results)
				{
					for (int i = 0; i < itemsInQueue; i++)
						results.Dequeue().InvokeResult();
				}
			}
		}
		public static void RequestPath(ARequest_base request)
		{
			ThreadStart threadStart = delegate
			{
				request.InvokeMethod(FinishedProcessingPath);
			};
			threadStart.Invoke();
		}

		static void FinishedProcessingPath(IRequestResult result)
		{
			lock (Instance.results)
			{
				Instance.results.Enqueue(result);
			}
		}
	}


	public interface IRequestResult
	{
		public void InvokeResult();
	}

	public abstract class ARequest_base
	{
		public abstract void InvokeMethod(Action<IRequestResult> callback);
	}

	public abstract class ARequest<T> : ARequest_base where T : ARequest<T>
	{
		protected Action<T, Action<IRequestResult>> method;
		public ARequest(Action<T, Action<IRequestResult>> method) => this.method = method;
		public override void InvokeMethod(Action<IRequestResult> callback) => method?.Invoke((T)this, callback);
	}
}
