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
        public List<WayPoint> spawnPoints;

        [SerializeField] Transform civilianParent;

        //civilian objects
        public CivilianObject civObj;
        //public List<CivilianObject> civilianObjectList;
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

            WayPoint startPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            civ.transform.position = startPoint.transform.position;

            civ.nextWaypoint = startPoint.GetNextWaypoint();

            Debug.Log("Starting at " + startPoint + " and moving to " + civ.nextWaypoint);

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

        //method to calculate sin and cosines to reduce math later
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
