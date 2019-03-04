using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public class WayPoint : MonoBehaviour
    {
        public Transform[] adjacentWaypoints;

        public WayPoint GetNextWaypoint() { return adjacentWaypoints[Random.Range(0, adjacentWaypoints.Length)].GetComponent<WayPoint>(); }
    }
}
