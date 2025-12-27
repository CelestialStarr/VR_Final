using UnityEngine;
using UnityEditor;

[InitializeOnLoad]

public class WaypointEditor
{
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]

    public static void OnDrawSceneGizmos(Waypoint waypoint, GizmoType gismosType)
    {
        if ((gismosType & GizmoType.Selected) != 0)
        {
            Gizmos.color = Color.blue;
        }
        else//not selected / pickable
        {
            Gizmos.color = Color.blue * 0.5f;//light blue
        }

        Gizmos.DrawSphere(waypoint.transform.position, 0.1f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(waypoint.transform.position + (waypoint.transform.right * waypoint.waypointWidth / 2f), waypoint.transform.position - (waypoint.transform.right * waypoint.waypointWidth / 2f));

        if (waypoint.previousWaypoint != null)
        {
            Gizmos.color = Color.red;

            Vector3 offset = waypoint.transform.right * waypoint.waypointWidth / 2f;
            Vector3 offsetTo = waypoint.previousWaypoint.transform.right * waypoint.previousWaypoint.waypointWidth / 2f;

            Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.previousWaypoint.transform.position + offsetTo);
        }

        if (waypoint.nextWaypoint != null)
        {
            Gizmos.color = Color.green;

            Vector3 offset = waypoint.transform.right * -waypoint.waypointWidth / 2f;
            Vector3 offsetTo = waypoint.previousWaypoint.transform.right * -waypoint.previousWaypoint.waypointWidth / 2f;

            Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.previousWaypoint.transform.position + offsetTo);
        }
    }
}