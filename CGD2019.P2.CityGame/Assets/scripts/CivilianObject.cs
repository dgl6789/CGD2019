using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    [CreateAssetMenu(fileName = "Civilian", menuName = "Mobiles/Civilian", order = 0)]
    public class CivilianObject : MobileObject
    {
        [SerializeField] string civilianName;
        public string CivilianName { get { return civilianName; } }

        [SerializeField] float flockingWeight;
        public float FlockingWeight { get { return flockingWeight; } }

        [SerializeField] float neighborRange;
        public float NeighborRange { get { return neighborRange; } }

        public void SetValues(string spriteName, string mobileName, int speed, string civilianName, float flockingWeight, float neighborRange)
        {
            base.SetValues(spriteName, mobileName, speed);

            this.flockingWeight = flockingWeight;
            this.neighborRange = neighborRange;
        }
    }
}
