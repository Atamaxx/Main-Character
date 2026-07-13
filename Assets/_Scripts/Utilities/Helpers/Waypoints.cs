using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class WaypointsCreator : MonoBehaviour
{
    [SerializeField, BoxGroup("WAYPOINTS")] private bool _useUpdateWaypoints = false;
    [BoxGroup("WAYPOINTS")] public List<GameObject> Waypoints = new();
    [BoxGroup("WAYPOINTS")] public List<Vector2> WaypointsPosition = new();
    [SerializeField, BoxGroup("WAYPOINTS")] private GameObject _waypointParent;

    [SerializeField, BoxGroup("PREVIEW")] private Sprite _waypointSprite;
    [SerializeField, BoxGroup("PREVIEW"), Range(0, 2)] private float _spriteScale = 0.5f;
    [SerializeField, BoxGroup("PREVIEW")] private bool _showPreview = false;
    [SerializeField, ShowIf("_showPreview"), BoxGroup("PREVIEW")] private BoxCollider2D _previewCollider;
    [SerializeField, ShowIf("_showPreview"), BoxGroup("PREVIEW")] private Color _previewColor = Color.yellow;
    [SerializeField, ShowIf("_showPreview"), BoxGroup("PREVIEW")] private bool _loopPreviewPath = false;

    public float WayLength;

    #region U_LIFECYCLE
    private void Start()
    {
        WaypointsPosition.Clear();

        foreach (GameObject waypoint in Waypoints)
        {
            WaypointsPosition.Add(waypoint.transform.position);
            waypoint.SetActive(false);
        }
        WayLength = My.Line.CalculateLength(WaypointsPosition);
    }


    private void Update()
    {
        if (_useUpdateWaypoints)
        {
            WaypointsPosition.Clear();
            foreach (GameObject waypoint in Waypoints)
            {
                WaypointsPosition.Add(waypoint.transform.position);
            }
            WayLength = My.Line.CalculateLength(WaypointsPosition);
        }
    }
    #endregion


    #region PUBLIC
    public bool OutOfClamp(float value, float bottomClamp, float topClamp)
    {
        if (value <= bottomClamp)
        {
            transform.position = WaypointsPosition[0];
            return true;
        }
        else if (value >= topClamp)
        {
            transform.position = WaypointsPosition[^1];
            return true;
        }

        return false;
    }
    #endregion


    #region EDITOR
#if UNITY_EDITOR


    [Button]
    private void CreateWaypoint()
    {
        if (_waypointParent == null)
        {
            _waypointParent = new("WP - " + name);
            _waypointParent.transform.position = transform.position;
            _waypointParent.transform.parent = transform.parent;
        }
        GameObject waypoint = new("Waypoint_" + name + Waypoints.Count);
        SpriteRenderer spriteRen = waypoint.AddComponent<SpriteRenderer>();
        spriteRen.sprite = _waypointSprite;
        spriteRen.color = _previewColor;
        waypoint.transform.localScale *= _spriteScale;
        waypoint.transform.parent = _waypointParent.transform;
        waypoint.transform.position = transform.position;
        Waypoints.Add(waypoint);
    }
    [Button("Add Parent's Waypoints")]
    private void AddParentWaypoints()
    {
        if (_waypointParent == null) return;
        for (int i = 0; i < _waypointParent.transform.childCount; i++)
        {
            Transform child = _waypointParent.transform.GetChild(i);
            if (!Waypoints.Contains(child.gameObject))
            {
                Waypoints.Add(child.gameObject);
            }
        }
    }
    [Button]
    private void DeleteLastWaypoint()
    {
        if (Waypoints.Count == 0 && _waypointParent == null) return;

        Waypoints.RemoveAt(Waypoints.Count - 1);

        int childCount = _waypointParent.transform.childCount;
        DestroyImmediate(_waypointParent.transform.GetChild(childCount - 1).gameObject);
    }

    [Button]
    private void DeleteWaypoints()
    {
        if (_waypointParent == null) return;

        Waypoints.Clear();
        int childCount = _waypointParent.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(_waypointParent.transform.GetChild(i).gameObject);
        }
    }
    [Button]
    private void ReverseWaypoints()
    {
        if (Waypoints.Count == 0) return;
        Waypoints.Reverse();
    }
    [Button]
    private void ResetPosition()
    {
        if (Waypoints.Count == 0) return;
        transform.position = Waypoints[0].transform.position;
    }

    [BoxGroup("EDITOR"), SerializeField] private bool _changePosition;
    [BoxGroup("EDITOR"), ShowIf("_changePosition"), SerializeField] private int _moveToWaypointNum;
    [Button, ShowIf("_changePosition")]
    private void ChangePosition()
    {
        if (Waypoints.Count < _moveToWaypointNum) return;
        transform.position = Waypoints[_moveToWaypointNum].transform.position;
    }




    //private float g_angle = 0f;
    void OnDrawGizmos()
    {
        if (_showPreview && Waypoints != null && Waypoints.Count > 1)
        {
            for (int i = 0; i < Waypoints.Count; i++)
            {
                if (Waypoints[i] != null)
                {
                    //if (rotatingPlatform != null) g_angle = rotatingPlatform.g_angle;
                    //else g_platformBounds = GetComponent<BoxCollider2D>().bounds;

                    // Draw a line between Waypoints
                    if (i < Waypoints.Count - 1 && Waypoints[i + 1] != null)
                    {
                        Debug.DrawLine(Waypoints[i].transform.position, Waypoints[i + 1].transform.position, _previewColor);
                    }

                    if (_previewCollider == null) return;
                    List<Vector2> corners;

                    // Draw an outline of the platform at each waypoint
                    Vector2 waypointPos = Waypoints[i].transform.position;

                    corners = My.Trans.GetEdgePointsShift(_previewCollider, waypointPos);

                    Debug.DrawLine(corners[0], corners[1], _previewColor);
                    Debug.DrawLine(corners[1], corners[2], _previewColor);
                    Debug.DrawLine(corners[2], corners[3], _previewColor);
                    Debug.DrawLine(corners[3], corners[0], _previewColor);

                }
            }

            // Draw a line from the last waypoint to the first one
            if (_loopPreviewPath && Waypoints[0] != null && Waypoints[^1] != null)
            {
                Debug.DrawLine(Waypoints[0].transform.position, Waypoints[^1].transform.position, _previewColor);
            }
        }

    }

#endif

    #endregion
}
