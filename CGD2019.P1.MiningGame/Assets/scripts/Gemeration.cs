using UnityEngine;
using App.Gameplay;
using System.Collections.Generic;

namespace App
{
    public class Gemeration : MonoBehaviour
    {
        /// Reference to the rock's voxel grid.
        [SerializeField] VoxelGrid voxelGrid;

        // Singleton instance (reference this class' members via StateManager.Instance from any context that is 'using App;')
        public static Gemeration Instance;

        //gemeration values
        public int gemCount = 1;
        public int range = 0;
        public List<GameObject> gemPrefabs = new List<GameObject>();

        //gemerator position
        Vector3 gemOrigin;

        void Awake()
        {
            // Singleton intitialization.
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        //generate a number of gems
        public void GenerateGems()
        {
            gemOrigin = FindOrigin();

            if (range <= 0)
            {
                range = FindDefaultRange();
            }

            if (gemCount <= 0)
                gemCount = 1;

            //loop to generate gems
            for (int i = 0; i < gemCount; i++)
            {
                //randomly choose a prefab
                GameObject thisGem = Instantiate(ChoosePrefab(), voxelGrid.transform);

                //if there are no prefabs, exit
                if (thisGem == null)
                {
                    Debug.Log("ERROR: No gem prefabs found during gemeration process\n");
                    return;
                }

                //generate random position and normalize it
                Vector3 gemPosition = new Vector3(
                    Random.Range(-1.0f, 1.0f),
                    Random.Range(-1.0f, 1.0f),
                    Random.Range(-1.0f, 1.0f)).normalized;

                //place position within specified range of origin
                float rangePercentage = Random.Range(0.0f, 1.0f) * range;
                gemPosition = gemPosition * rangePercentage + gemOrigin;

                //correct for floating gems
                gemPosition = PullToRock(gemPosition);

                //set the gem's position
                thisGem.transform.position = gemPosition;
            }
        }

        //choose a gem prefab to use
        private GameObject ChoosePrefab()
        {
            //null if there are no prefabs set
            if (gemPrefabs.Count == 0)
            {
                return null;
            }

            GameObject thisGem;

            //if there's only one prefab, choose that one
            if (gemPrefabs.Count == 1)
            {
                thisGem = gemPrefabs[0];
            }
            else
            {
                //choose a prefab at random if there are prefabs to choose from
                int chosenPrefab = Random.Range(0, gemPrefabs.Count);

                thisGem = gemPrefabs[chosenPrefab];
            }

            return thisGem;
        }

        //method to find the center of the voxel grid
        Vector3 FindOrigin()
        {
            return new Vector3(voxelGrid.X / 2.0f, voxelGrid.Y / 2.0f, voxelGrid.Z / 2.0f);
        }

        //method to find default range value using voxel grid dimensions
        int FindDefaultRange()
        {
            return (int)((voxelGrid.X + voxelGrid.Y + voxelGrid.Z) / 3.0f);
        }

        //method to correct for floating gems
        Vector3 PullToRock(Vector3 gemPosition, bool firstPass = true)
        {
            //return if gem is in rock
            if (InRock(gemPosition))
            {
                return gemPosition;
            }
            else if (firstPass) //correct for position outside of rock grid
            {
                if (gemPosition.x > voxelGrid.X)
                    gemPosition.x = voxelGrid.X;
                else if (gemPosition.x < 0)
                    gemPosition.x = 0;

                if (gemPosition.y > voxelGrid.Y)
                    gemPosition.y = voxelGrid.Y;
                else if (gemPosition.y < 0)
                    gemPosition.y = 0;

                if (gemPosition.z > voxelGrid.Z)
                    gemPosition.z = voxelGrid.Z;
                else if (gemPosition.z < 0)
                    gemPosition.z = 0;
                
                return PullToRock(gemPosition, false);
            }
            else if (Mathf.Abs(gemPosition.x - gemOrigin.x) <= 0.5f && Mathf.Abs(gemPosition.y - gemOrigin.y) <= 0.5f && Mathf.Abs(gemPosition.z - gemOrigin.z) <= 0.5f) //check for close enough to the origin to be safe
            {
                return gemPosition;
            }
            else //gradually pull gem in closer if it's not in the rock
            {
                Vector3 newPosition = gemPosition - gemOrigin;
                newPosition = newPosition * 0.75f + gemOrigin;

                return PullToRock(newPosition, false);
            }
        }

        //method to determine if gem is embedded in rock
        bool InRock(Vector3 gemPosition)
        {
            //if origin is in grid then gem is embedded
            if (voxelGrid.GetData((int)gemPosition.x, (int)gemPosition.y, (int)gemPosition.z) == 1)
            {
                return true;
            }

            return false;
        }
    }
}