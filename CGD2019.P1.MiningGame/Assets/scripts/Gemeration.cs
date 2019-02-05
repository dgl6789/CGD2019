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
        public int totalRockValue = 0;
        public int spawnRange = 0;
        [SerializeField] List<GameObject> gemPrefabs;
        [SerializeField] List<MineralItem> gemObjects;

        //gemerator position
        Vector3 gemOrigin;

        //list of created gems
        [SerializeField] List<GemBehavior> allGems;

        void Awake()
        {
            // Singleton intitialization.
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        //generate a number of gems
        public void GenerateGems()
        {
            ResetGemerationValues();

            Debug.Log("gem values are reset");

            do
            {
                Debug.Log("in gem generation loop");

                //randomly choose a gem
                MineralItem gemChosen = gemObjects[Random.Range(0, gemObjects.Count)];
                
                //check that this gem still fits within remaining value
                if (totalRockValue >= gemChosen.Value)
                {
                    SpawnGem(gemChosen);

                    //subtract from total value
                    totalRockValue -= gemChosen.Value;
                }
            } while (totalRockValue > 0);
        }

        //method to set up values for gemeration
        void ResetGemerationValues()
        {
            Debug.Log("resetting gemeration values");

            //clear out old gems
            ClearGems();

            Debug.Log("Setting rock value");

            totalRockValue = 1250 + Mathf.RoundToInt(Random.Range(0.0f, 7.5f) * 100.0f);
            //totalRockValue = 1500;

            Debug.Log("Rock Value = " + totalRockValue);

            gemOrigin = new Vector3(voxelGrid.X / 2.0f, voxelGrid.Y / 2.0f, voxelGrid.Z / 2.0f);

            spawnRange = Mathf.RoundToInt((voxelGrid.XBounds + voxelGrid.YBounds + voxelGrid.ZBounds) / 3.0f);
        }

        //method to spawn a specific gem
        void SpawnGem(MineralItem gemChosen)
        { 
            float valueMod = 1.0f - (gemChosen.Value / 1000.0f);

            //instantiate gem object
            GemBehavior thisGem = Instantiate(GetGemPrefab(gemChosen), voxelGrid.transform).GetComponent<GemBehavior>();

            //generate random position and normalize it
            Vector3 gemPosition = new Vector3(
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f)).normalized;

            //place position within specified range of origin
            float rangePercentage = Random.Range(valueMod / 2.0f, valueMod) * spawnRange;
            gemPosition = gemPosition * rangePercentage + gemOrigin;

            //correct for floating gems
            gemPosition = PullToRock(gemPosition);

            //set the gem's position
            thisGem.transform.position = gemPosition;

            // Set the gem's internal data
            thisGem.Initialize(gemChosen);

            //add gem to list
            allGems.Add(thisGem);
        }

        //method to get the appropraite model for a chosen gem
        GameObject GetGemPrefab(MineralItem chosenGem)
        {
            //loop through gem prefabs looking for specified one
            foreach(GameObject gem in gemPrefabs)
            {
                if (gem.name == chosenGem.ModelName)
                {
                    return gem;
                }
            }

            //return the first prefab by default
            return gemPrefabs[0];
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
        public bool InRock(Vector3 gemPosition)
        {
            //if origin is in grid then gem is embedded
            if (voxelGrid.GetData((int)gemPosition.x, (int)gemPosition.y, (int)gemPosition.z) >= 1)
            {
                return true;
            }

            return false;
        }

        //method to remove all gems
        public void ClearGems()
        {
            Debug.Log("clearing " + allGems.Count + " gems");

            if (allGems.Count == 0)
                return;

            foreach (GemBehavior gem in allGems)
            {
                Destroy(gem.gameObject);
            }

            allGems.Clear();
        }

        //method to remove a specific gem from the list
        public void RemoveGem(GemBehavior thisGem)
        {
            allGems.Remove(thisGem);
        }
    }
}