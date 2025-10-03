using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToWayPoints : MonoBehaviour
{
    public Vector3 Offset;

    public float Speed;
    public float MaxSpeed;
    public float FirstLevelSpeed;
    public List<Transform> Waypoints;

    private int _curWaypointIndex = 0;
    private Transform _thisTransform;

    #region MONO

    private void Awake()
    {
        Speed = MaxSpeed;
        _thisTransform = transform;
    }

    #endregion

    #region BODY

    void Update()
    {
        if(_curWaypointIndex < Waypoints.Count)
        {
            _thisTransform.position = Vector3.MoveTowards(_thisTransform.position, Waypoints[_curWaypointIndex].position + Offset, Time.deltaTime * Speed / 5);
            _thisTransform.LookAt(Waypoints[_curWaypointIndex].position + Offset);
            if (Vector3.Distance(_thisTransform.position, Waypoints[_curWaypointIndex].position + Offset) < 0.0001f)
            {
                _curWaypointIndex++;
            }
        }
        else if (_curWaypointIndex == Waypoints.Count && Waypoints.Count != 0)
        {
            _curWaypointIndex++;
            GetComponent<Warrior>().SetNotMove();
        }
    }

    #endregion
}
