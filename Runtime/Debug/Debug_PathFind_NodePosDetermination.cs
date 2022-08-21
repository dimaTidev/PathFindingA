using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimaTi.PathFinding
{
    public class Debug_PathFind_NodePosDetermination : MonoBehaviour
    {
        [SerializeField] float gizmosSphereSize = 0.1f;
        Vector3 gizmosPosition;
        bool isWalkable;

        void Update()
        {
            gizmosPosition = GridManager.instance?.Get_PositionOnGrid(transform.position) ?? transform.position;
            isWalkable = GridManager.instance?.Check_isWalkable(transform.position) ?? false;
        }

        private void OnDrawGizmos()
        {
            // Gizmos.color = Color.grey;
            // Gizmos.DrawSphere(transform.position, gizmosSphereSize);

            Gizmos.color = isWalkable ? Color.green : Color.red;
            Gizmos.DrawSphere(gizmosPosition, gizmosSphereSize);
            Gizmos.DrawLine(gizmosPosition, transform.position);
        }
    }
}
