using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace My
{
    public class Trans : MonoBehaviour
    {
        #region Moving By Waypoints
        public static void MoveConstant(List<GameObject> waypoints, ref int waypointIndex, Transform objTransform, float speed)
        {
            if (waypoints.Count == 0) return;

            if (Vector2.Distance(waypoints[waypointIndex].transform.position, objTransform.position) < 0.01f)
            {
                waypointIndex++;
                if (waypointIndex >= waypoints.Count)
                {
                    waypointIndex = 0;
                }
            }

            objTransform.position = Vector2.MoveTowards(objTransform.position, waypoints[waypointIndex].transform.position, speed * Time.deltaTime);
        }

        public static void MoveTimeDistance(List<GameObject> waypoints, ref int waypointIndex, Transform objTransform, float speed)
        {

        }


        #endregion

        #region Info
        
        #endregion
       
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }

        public static List<Vector2> GetEdgePoints(BoxCollider2D collider)
        {
            List<Vector2> edgePoints = new ();

            // Local points without rotation
            Vector2 halfSize = collider.size * 0.5f;
            Vector2 topLeft = new Vector2(-halfSize.x, halfSize.y) + collider.offset;
            Vector2 topRight = new Vector2(halfSize.x, halfSize.y) + collider.offset;
            Vector2 bottomRight = new Vector2(halfSize.x, -halfSize.y) + collider.offset;
            Vector2 bottomLeft = new Vector2(-halfSize.x, -halfSize.y) + collider.offset;

            // Convert local points to global points considering rotation
            edgePoints.Add(collider.transform.TransformPoint(topLeft));
            edgePoints.Add(collider.transform.TransformPoint(topRight));
            edgePoints.Add(collider.transform.TransformPoint(bottomRight));
            edgePoints.Add(collider.transform.TransformPoint(bottomLeft));

            return edgePoints;
        }

        public static List<Vector2> GetEdgePointsShift(BoxCollider2D collider, Vector2 center)
        {
            List<Vector2> edgePoints = new();
            Vector2 shift = center - ((Vector2)collider.transform.position + collider.offset);
            
            // Local points without rotation
            Vector2 halfSize = collider.size * 0.5f;
            Vector2 topLeft = new Vector2(-halfSize.x, halfSize.y) + collider.offset;
            Vector2 topRight = new Vector2(halfSize.x, halfSize.y) + collider.offset;
            Vector2 bottomRight = new Vector2(halfSize.x, -halfSize.y) + collider.offset;
            Vector2 bottomLeft = new Vector2(-halfSize.x, -halfSize.y) + collider.offset;

            // Convert local points to global points considering rotation
            edgePoints.Add(collider.transform.TransformPoint(topLeft));
            edgePoints.Add(collider.transform.TransformPoint(topRight));
            edgePoints.Add(collider.transform.TransformPoint(bottomRight));
            edgePoints.Add(collider.transform.TransformPoint(bottomLeft));
            edgePoints[0] = shift + edgePoints[0];
            edgePoints[1] = shift + edgePoints[1];
            edgePoints[2] = shift + edgePoints[2];
            edgePoints[3] = shift + edgePoints[3];

            return edgePoints;
        }
    }
}

