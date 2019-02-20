using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public class CivilianManager : MonoBehaviour
    {

        public static CivilianManager Instance;

        //civilians
        List<CivilianMovement> civilianList = new List<CivilianMovement>();

        //trig lookup tables
        float[] sinLookup = new float[360];
        float[] cosLookup = new float[360];
        public float SinLookup(int index) { return sinLookup[index]; }
        public float CosLookup(int index) { return cosLookup[index]; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            CalcLookupTables();
        }

        // Update is called once per frame
        void Update()
        {

        }

        //method to spawn a single civilian
        public void SpawnCivilian()
        {

        }

        //method to find each civilian's neighbor
        public List<CivilianMovement> FindNeighbors(CivilianMovement thisCivilian)
        {
            List<CivilianMovement> neighbors = new List<CivilianMovement>();

            //check each civilian
            foreach (CivilianMovement civilian in civilianList)
            {
                //don't check against self
                if (civilian == thisCivilian)
                    continue;

                //get distance between civilians and the neighbor range of this one
                float dist = thisCivilian.CalcDistSqr(civilian.transform.position);
                float range = thisCivilian.CivilianData.NeighborRange;
    
                //check that they are within range
                if (dist <= range * range)
                {
                    neighbors.Add(civilian);
                }
            }

            return neighbors;
        }

        void CalcLookupTables()
        {
            for (int i = 0; i < 360; i++)
            {
                cosLookup[i] = Mathf.Cos(i);
                sinLookup[i] = Mathf.Sin(i);
            }
        }
    }
}
