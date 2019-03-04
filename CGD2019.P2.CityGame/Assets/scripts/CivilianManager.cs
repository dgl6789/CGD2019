using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public class CivilianManager : MonoBehaviour
    {
        //instance
        public static CivilianManager Instance;

        //waypoints for debug
        public Transform debugWaypoint;

        [SerializeField] Transform civilianParent;

        //civilian objects
        public List<CivilianObject> civilianObjectList;
        public GameObject civilianPrefab;

        //civilians
        [SerializeField] List<CivilianMovement> civilianList = new List<CivilianMovement>();
        public List<CivilianMovement> CivilianList
        {
            get { return civilianList; }
        }
        //trig lookup tables
        float[] sinLookup = new float[360];
        float[] cosLookup = new float[360];
        public float SinLookup(int index) { return sinLookup[index]; }
        public float CosLookup(int index) { return cosLookup[index]; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            CalcLookupTables();
        }

        // Update is called once per frame
        public void DoUpdateStep()
        {
            if (Random.Range(0, 100) == 0) //Debug.Log("1/100");
                SpawnCivilian();
        }

        //method to spawn a single civilian
        public void SpawnCivilian()
        {

            CivilianMovement civ = Instantiate(civilianPrefab, civilianParent).GetComponent<CivilianMovement>();
            Vector3 pos = new Vector3(10, 10, 0);
            civ.transform.position = pos;

            CivilianObject civObj = civilianObjectList[Random.Range(0, civilianObjectList.Count)];
            civ.CivilianData = civObj;

            civ.nextWaypoint = debugWaypoint;

            civilianList.Add(civ);
        }

        //method to remove a civilian
        public void RemoveCivilian(CivilianMovement thisCivilian, bool replace = true)
        {
            civilianList.Remove(thisCivilian);

            Destroy(thisCivilian.gameObject);

            if (replace)
                SpawnCivilian();
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
                    
                //add to list if in range
                if (thisCivilian.WithinDist(civilian.Position, thisCivilian.CivilianData.NeighborRange))
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
