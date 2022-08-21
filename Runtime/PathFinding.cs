using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace DimaTi.PathFinding
{
	[RequireComponent(typeof(GridManager))]
	public class PathFinding : AFinding_Base<PathFinding>
	{
		#region RequestsClases
		//----------------------------------------------------------------------------------------------------------------
		public class Request : ARequest<Request>
		{
			public Vector3 previousPosition;
			public Vector3 pathStart;
			public Vector3 pathEnd;
			public Action<Vector3[], bool> callback;

			public Request(Action<Request, Action<IRequestResult>> method, Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback) : base(method)
			{
				previousPosition = _start;
				pathStart = _start;
				pathEnd = _end;
				callback = _callback;
			}
		}
		public class RequestResult : IRequestResult
		{
			Vector3[] path;
			bool success;
			Action<Vector3[], bool> callback;

			public RequestResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
			{
				this.path = path;
				this.success = success;
				this.callback = callback;
			}
			void IRequestResult.InvokeResult() => callback?.Invoke(path, success);
		}
		//----------------------------------------------------------------------------------------------------------------
		#endregion

		public static void FindPath_Request(Vector3 posStart, Vector3 posEnd, Action<Vector3[], bool> callback)
		{
			Request request = new Request(FindPath, posStart, posEnd, callback);
			RequestManager.RequestPath(request);
		}

		#region Private
		//----------------------------------------------------------------------------------------------------------------
		static void FindPath(Request request, Action<IRequestResult> callback)
		{
			Vector3[] waypoints = new Vector3[0];
			bool pathSuccess = false;

			if (!Instance)
			{
				callback?.Invoke(new RequestResult(waypoints, pathSuccess, request.callback));
				return;
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();

			Node startNode = Instance.Grid.Get_NodeFromWorldPosition(request.pathStart);
			if (!startNode.walkable)
				startNode = Instance.Grid.Get_NodeFromWorldPosition(request.previousPosition);
			Node targetNode = Instance.Grid.Get_NodeFromWorldPosition(request.pathEnd);

			if (startNode == null || targetNode == null)
			{
				callback?.Invoke(new RequestResult(waypoints, pathSuccess, request.callback));
				sw.Stop();
				return;
			}

			startNode.parent = startNode;

			if (startNode.walkable && targetNode.walkable)
			{
				Heap<Node> openSet = new Heap<Node>(Instance.Grid.MaxSize);
				HashSet<Node> closedSet = new HashSet<Node>();
				openSet.Add(startNode);

				while (openSet.Count > 0)
				{
					Node currentNode = openSet.RemoveFirst();
					closedSet.Add(currentNode);

					if (currentNode == targetNode)
					{
						sw.Stop();
						//print ("Path found: " + sw.ElapsedMilliseconds + " ms");
						pathSuccess = true;
						break;
					}

					foreach (Node neighbour in Instance.Grid.Get_Neighbours(currentNode))
					{
						if (!neighbour.walkable || closedSet.Contains(neighbour))
							continue;

						int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
						if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
						{
							neighbour.gCost = newMovementCostToNeighbour;
							neighbour.hCost = GetDistance(neighbour, targetNode);
							neighbour.parent = currentNode;

							if (!openSet.Contains(neighbour))
								openSet.Add(neighbour);
							else
								openSet.UpdateItem(neighbour);
						}
					}
				}
			}

			if (pathSuccess)
			{
				waypoints = RetracePath(startNode, targetNode);
				pathSuccess = waypoints.Length > 0;
			}
			else
			{
				print("!!! Path NOT found: " + sw.ElapsedMilliseconds + " ms!!!!!!!!!!!");
			}

			callback?.Invoke(new RequestResult(waypoints, pathSuccess, request.callback));

		}


		static Vector3[] RetracePath(Node startNode, Node endNode)
		{
			List<Node> path = new List<Node>();
			Node currentNode = endNode;

			while (currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.parent;
			}
			Vector3[] waypoints = SimplifyPath(path);
			Array.Reverse(waypoints);
			return waypoints;

		}

		static Vector3[] SimplifyPath(List<Node> path)
		{
			List<Vector3> waypoints = new List<Vector3>();
			//Vector2 directionOld = Vector2.zero;

			//for (int i = 1; i < path.Count; i ++) 
			for (int i = 0; i < path.Count; i++)
			{
				//Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX,path[i-1].gridY - path[i].gridY);
				//if (directionNew != directionOld) 
				waypoints.Add(path[i].worldPosition);

				//directionOld = directionNew;
			}
			return waypoints.ToArray();
		}

		static int GetDistance(Node nodeA, Node nodeB)
		{
			int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
			int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

			if (dstX > dstY)
				return 14 * dstY + 10 * (dstX - dstY);
			return 14 * dstX + 10 * (dstY - dstX);
		}
		//----------------------------------------------------------------------------------------------------------------
		#endregion

	}
}
