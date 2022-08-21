using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DimaTi.PathFinding
{
	[RequireComponent(typeof(GridManager))]
	public class ClosestCellsFinding : AFinding_Base<ClosestCellsFinding>
	{
		#region RequestsClases
		//----------------------------------------------------------------------------------------------------------------
		public class Request : ARequest<Request>
		{
			public Vector3 startWorldPos;
			public int cellsRange;
			public Action<Vector3[]> callback;

			public Request(Action<Request, Action<IRequestResult>> method, Vector3 startWorldPos, int cellsRange, Action<Vector3[]> callback) : base(method)
			{
				this.startWorldPos = startWorldPos;
				this.cellsRange = cellsRange;
				this.callback = callback;
			}
		}
		public class RequestResult : IRequestResult
		{
			Vector3[] path;
			Action<Vector3[]> callback;

			public RequestResult(Vector3[] path, Action<Vector3[]> callback)
			{
				this.path = path;
				this.callback = callback;
			}
			void IRequestResult.InvokeResult() => callback?.Invoke(path);
		}
		//----------------------------------------------------------------------------------------------------------------
		#endregion

		public static void Request_Cells(Vector3 posStart, int cellsRange, Action<Vector3[]> callback)
		{
			Request request = new Request(SearchClosestCells, posStart, cellsRange, callback);
			RequestManager.RequestPath(request);
		}

		static void SearchClosestCells(Request request, Action<IRequestResult> callback)
		{
			Node startNode = Instance.Grid.Get_NodeFromWorldPosition(request.startWorldPos);
			if (!startNode.walkable)
			{
				Debug.Log("startNode Cell do not walkable!");
				callback?.Invoke(new RequestResult(new Vector3[] { startNode.worldPosition }, request.callback));
				return;
			}

			startNode.hCost = 0;
			Queue<Node> openSet = new Queue<Node>();
			openSet.Enqueue(startNode);

			HashSet<Node> closedSet = new HashSet<Node>();

			while (openSet.Count > 0)
			{
				Node currentNode = openSet.Dequeue();
				if (currentNode.hCost > request.cellsRange)
					continue;

				closedSet.Add(currentNode);

				foreach (Node neighbour in Instance.Grid.Get_Neighbours(currentNode))
				{
					if (!neighbour.walkable || closedSet.Contains(neighbour) || openSet.Contains(neighbour))
						continue;
					neighbour.hCost = currentNode.hCost + 1;
					openSet.Enqueue(neighbour);
				}
			}
			callback?.Invoke(new RequestResult(RetraceCells(closedSet), request.callback));
		}

		static Vector3[] RetraceCells(HashSet<Node> cellsSet)
		{
			Vector3[] cells = new Vector3[cellsSet.Count];
			int i = 0;
			foreach (Node node in cellsSet)
			{
				cells[i] = node.worldPosition;
				i++;
			}
			return cells;
		}
	}
}
