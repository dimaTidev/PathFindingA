using UnityEngine;
using System;

namespace DimaTi.PathFinding
{
	public abstract class AFinding_Base<T> : MonoBehaviour where T : AFinding_Base<T>
	{
		GridManager m_grid;
		protected GridManager Grid => m_grid ??= GetComponent<GridManager>();

		#region Singleton
		static T s_instance;
		public static T Instance => s_instance;

		protected virtual void Awake()
		{
			if (s_instance != null)
			{
				UnityEngine.Debug.LogWarning("Found a second Pathfinding.cs!");
				Destroy(this);
			}
			else
				s_instance = (T)this;
		}
		#endregion

	}
}
