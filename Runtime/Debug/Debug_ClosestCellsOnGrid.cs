using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimaTi.PathFinding
{
    public class Debug_ClosestCellsOnGrid : MonoBehaviour
    {
        [SerializeField, Range(1, 10)] int cellsRange = 1;
        [SerializeField] float gizmosRadius = 0.1f;

        [SerializeField] bool performSearch = false;
        bool isSearching;

        Vector3[] cells;

        private void Update()
        {
            if (performSearch && !isSearching)
            {
                ClosestCellsFinding.Request_Cells(transform.position, cellsRange, OnFindPoints);
                performSearch = false;
                isSearching = true;
            }
        }

        void OnFindPoints(Vector3[] points)
        {
            isSearching = false;
            cells = points;
        }

        private void OnDrawGizmos()
        {
            if (cells == null)
                return;

            foreach (var cell in cells)
                Gizmos.DrawSphere(cell, gizmosRadius);
        }
    }
}
