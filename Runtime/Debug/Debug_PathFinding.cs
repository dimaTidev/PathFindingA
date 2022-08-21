using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimaTi.PathFinding
{
    public class Debug_PathFinding : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float updateTime = 1f;
        bool isSearchingPath;

        void Start() => StartCoroutine(UpdatePath());
        IEnumerator UpdatePath()
        {
            while (true)
            {
                if (!isSearchingPath && target)
                {
                    PathFinding.Request_FindPath(transform.position, target.position, OnPathFound);
                    isSearchingPath = true;
                }
                yield return new WaitForSeconds(updateTime);
            }
        }


        Vector3[] path;
        void OnPathFound(Vector3[] path, bool isFind)
        {
            isSearchingPath = false;
            this.path = path;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (path == null)
                return;

            for (int i = 0; i < path.Length - 1; i++)
                Gizmos.DrawLine(path[i], path[i + 1]);
        }
    }
}
