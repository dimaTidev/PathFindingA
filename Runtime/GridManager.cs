using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace DimaTi.PathFinding
{
	public class GridManager : MonoBehaviour
	{
		public static GridManager instance;


		public enum GridAxis { _2D, _3D };
		[SerializeField] GridAxis gridAxis = GridAxis._3D;
		[SerializeField] Vector2Int gridWorldSize = new Vector2Int(5, 5);
		[SerializeField] float nodeSize = 1;
		[SerializeField] bool isWalkingOnlyByCross = false;
		Node[,] grid;


		[Header("Walkable params")]
		[SerializeField] bool isBlurPenaltyMap = false;
		[SerializeField] LayerMask unwalkableMask;
		[SerializeField] TerrainType[] walkableRegions;
		[SerializeField] int obstacleProximityPenalty = 10;
		Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
		LayerMask walkableMask;
		int penaltyMin = int.MaxValue;
		int penaltyMax = int.MinValue;


		void Awake()
		{
			if (instance != null)
			{
				Debug.Log("Scene has a few GridManager!!!");
				return;
			}

			instance = this;

			foreach (TerrainType region in walkableRegions)
			{
				walkableMask.value |= region.terrainMask.value;
				walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
			}

			CreateGrid();
		}

		public int MaxSize => gridWorldSize.x * gridWorldSize.y;
		Vector3 StartCorner
		{
			get
			{
				Vector3 origin = transform.position;
				if (gridAxis == GridAxis._2D)
					origin = (Vector2)transform.position - (Vector2.right * gridWorldSize.x * nodeSize / 2) - (Vector2.up * gridWorldSize.y * nodeSize / 2);
				else if (gridAxis == GridAxis._3D)
					origin = transform.position - (Vector3.right * gridWorldSize.x * nodeSize / 2) - (Vector3.forward * gridWorldSize.y * nodeSize / 2);
				return origin;
			}
		}
		Vector3 Get_LocalCellPos_3d(int x, int y) => Vector3.right * (x * nodeSize + nodeSize / 2) + Vector3.forward * (y * nodeSize + nodeSize / 2);

		#region GridCreation
		//-----------------------------------------------------------------------------------------------------------------
		void CreateGrid()
		{
			grid = new Node[gridWorldSize.x, gridWorldSize.y];

			if (gridAxis == GridAxis._2D)
				CheckGrid2D(grid);
			else if (gridAxis == GridAxis._3D)
				CheckGrid3D(grid);

			if (isBlurPenaltyMap)
				BlurPenaltyMap(3);
		}

		//Only check first object for walkable
		void CheckGrid3D(Node[,] grid)
		{
			Vector3 worldBottomLeft = StartCorner;
			for (int x = 0; x < gridWorldSize.x; x++)
			{
				for (int y = 0; y < gridWorldSize.y; y++)
				{
					Vector3 worldPoint = worldBottomLeft + Get_LocalCellPos_3d(x, y);
					bool walkable = !Physics.CheckBox(worldPoint, Vector3.one * nodeSize * 0.5f, Quaternion.identity, unwalkableMask);

					int movementPenalty = 0;

					Collider[] hitCol = Physics.OverlapBox(worldPoint, Vector3.one * nodeSize, Quaternion.identity);
					if (hitCol.Length > 0)
						walkableRegionsDictionary.TryGetValue(hitCol[0].gameObject.layer, out movementPenalty);

					if (!walkable)
						movementPenalty += obstacleProximityPenalty;

					grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
				}
			}
		}
		void CheckGrid2D(Node[,] grid)
		{
			Vector2 worldBottomLeft = StartCorner;
			for (int x = 0; x < gridWorldSize.x; x++)
			{
				for (int y = 0; y < gridWorldSize.y; y++)
				{
					Vector3 worldPoint = worldBottomLeft + Vector2.right * (x * nodeSize + nodeSize / 2) + Vector2.up * (y * nodeSize + nodeSize / 2);
					//bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
					bool walkable = !Physics2D.OverlapBox(worldPoint, Vector2.one * nodeSize, 0, unwalkableMask);

					int movementPenalty = 0;

					Collider2D hitCol = Physics2D.OverlapBox(worldPoint, Vector2.one * nodeSize, 0);
					if (hitCol)
						walkableRegionsDictionary.TryGetValue(hitCol.gameObject.layer, out movementPenalty);

					if (!walkable)
						movementPenalty += obstacleProximityPenalty;

					grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
				}
			}
		}
		void BlurPenaltyMap(int blurSize)
		{
			int kernelSize = blurSize * 2 + 1;
			int kernelExtents = (kernelSize - 1) / 2;

			int[,] penaltiesHorizontalPass = new int[gridWorldSize.x, gridWorldSize.y];
			int[,] penaltiesVerticalPass = new int[gridWorldSize.x, gridWorldSize.y];

			for (int y = 0; y < gridWorldSize.y; y++)
			{
				for (int x = -kernelExtents; x <= kernelExtents; x++)
				{
					int sampleX = Mathf.Clamp(x, 0, kernelExtents);
					penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
				}

				for (int x = 1; x < gridWorldSize.x; x++)
				{
					int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridWorldSize.x);
					int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridWorldSize.x - 1);

					penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
				}
			}

			for (int x = 0; x < gridWorldSize.x; x++)
			{
				for (int y = -kernelExtents; y <= kernelExtents; y++)
				{
					int sampleY = Mathf.Clamp(y, 0, kernelExtents);
					penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
				}

				int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
				grid[x, 0].movementPenalty = blurredPenalty;

				for (int y = 1; y < gridWorldSize.y; y++)
				{
					int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridWorldSize.y);
					int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridWorldSize.y - 1);

					penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
					blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
					grid[x, y].movementPenalty = blurredPenalty;

					if (blurredPenalty > penaltyMax)
					{
						penaltyMax = blurredPenalty;
					}
					if (blurredPenalty < penaltyMin)
					{
						penaltyMin = blurredPenalty;
					}
				}
			}

		}
		//-----------------------------------------------------------------------------------------------------------------
		#endregion

		public List<Node> Get_Neighbours(Node node)
		{
			List<Node> neighbours = new List<Node>();

			Func<int, int, bool> isOutOfBounds;

			if (!isWalkingOnlyByCross)
				isOutOfBounds = (x, y) => x == 0 && y == 0;  //cubic Neighbours
			else
				isOutOfBounds = (x, y) => x == 0 && y == 0 || x == -1 && y == -1 || x == -1 && y == 1 || x == 1 && y == -1 || x == 1 && y == 1; //cross Neighbours

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (isOutOfBounds(x, y))
						continue;

					int checkX = node.gridX + x;
					int checkY = node.gridY + y;

					if (checkX >= 0 && checkX < gridWorldSize.x && checkY >= 0 && checkY < gridWorldSize.y)
						neighbours.Add(grid[checkX, checkY]);
				}
			}
			return neighbours;
		}
		public Node Get_NodeFromWorldPosition(Vector3 worldPosition)
		{
			float xWorld = worldPosition.x;
			float yWorld = gridAxis == GridAxis._2D ? worldPosition.y : worldPosition.z;

			xWorld -= nodeSize / 2;
			yWorld -= nodeSize / 2;

			float xMin = transform.position.x - (gridWorldSize.x / 2) * nodeSize;
			float xMax = xMin + gridWorldSize.x * nodeSize;
			float percentX = Mathf.InverseLerp(xMin, xMax, xWorld);

			float yMin = transform.position.y - (gridWorldSize.y / 2) * nodeSize;
			float yMax = yMin + gridWorldSize.y * nodeSize;
			float percentY = Mathf.InverseLerp(yMin, yMax, yWorld);

			int x = Mathf.RoundToInt(gridWorldSize.x * percentX);
			int y = Mathf.RoundToInt(gridWorldSize.y * percentY);

			if (x >= gridWorldSize.x) x = gridWorldSize.x - 1;
			if (y >= gridWorldSize.y) y = gridWorldSize.y - 1;

			return grid[x, y];
		}

		public bool Check_isWalkable(Vector3 worldPosition)
		{
			Node targetNode = Get_NodeFromWorldPosition(worldPosition);
			return targetNode != null ? targetNode.walkable : false;
		}

		public Vector3 Get_PositionOnGrid(Vector3 worldPosition)
		{
			Node targetNode = Get_NodeFromWorldPosition(worldPosition);
			return targetNode != null ? targetNode.worldPosition : worldPosition;
		}

#if UNITY_EDITOR
		[Header("__Gizmos__")]
		[SerializeField] bool gizmos_displayGrid;
		[SerializeField] bool gizmos_displayGridCells;
		[SerializeField] float gizmos_SphereSize = 1;

		void OnDrawGizmos()
		{
			Gizmos_DrawBorder();
			Gizmos_DrawWalkaleNodes();
			Gizmos_DrawGrid();
		}


		void Gizmos_DrawBorder() => Gizmos.DrawWireCube(transform.position, gridAxis == GridAxis._2D ? new Vector3(gridWorldSize.x, gridWorldSize.y) : new Vector3(gridWorldSize.x, 0, gridWorldSize.y));
		void Gizmos_DrawWalkaleNodes()
		{
			if (grid != null && gizmos_displayGridCells)
			{
				foreach (Node n in grid)
				{
					Gizmos.color = Color.clear;// Color.Lerp (Color.white, Color.black, Mathf.InverseLerp (penaltyMin, penaltyMax, n.movementPenalty));
					Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
					Vector3 gizmoSize = Vector3.one * nodeSize;
					gizmoSize.y *= gridAxis == GridAxis._3D ? 0.1f : 1;
					gizmoSize.z *= gridAxis == GridAxis._2D ? 0.1f : 1;
					Gizmos.DrawCube(n.worldPosition, gizmoSize * 0.9f);
				}
			}
		}
		void Gizmos_DrawGrid()
		{
			if (gizmos_displayGrid)
			{
				Gizmos.color = Color.blue;

				Vector3 origin = StartCorner;

				Gizmos.DrawSphere(origin, gizmos_SphereSize);

				if (gridAxis == GridAxis._2D) //2D (vertical)
				{
					for (int i = 0; i <= gridWorldSize.x; i++)
					{
						Gizmos.DrawLine(origin + new Vector3(i * nodeSize, 0), origin + new Vector3(i * nodeSize, gridWorldSize.y * nodeSize));
						for (int k = 0; k <= gridWorldSize.y; k++)
							Gizmos.DrawLine(origin + new Vector3(i * nodeSize, k * nodeSize), origin + new Vector3(gridWorldSize.x * nodeSize, k * nodeSize));
					}
				}
				else if (gridAxis == GridAxis._3D) //3D (horizontal)
				{
					for (int i = 0; i <= gridWorldSize.x; i++)
					{
						Gizmos.DrawLine(origin + new Vector3(i * nodeSize, 0, 0), origin + new Vector3(i * nodeSize, 0, gridWorldSize.y * nodeSize));
						for (int k = 0; k <= gridWorldSize.y; k++)
							Gizmos.DrawLine(origin + new Vector3(i * nodeSize, 0, k * nodeSize), origin + new Vector3(gridWorldSize.x * nodeSize, 0, k * nodeSize));
					}
				}

			}
		}

		[ContextMenu("Log Nodes length")]
		void Debug_NodesLength() => Debug.Log(grid.Length);
#endif

		/// <summary>
		/// Left-Down corner
		/// </summary>

		[System.Serializable]
		public class TerrainType
		{
			public LayerMask terrainMask;
			public int terrainPenalty;
		}
	}
}