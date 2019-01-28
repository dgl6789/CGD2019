﻿using UnityEngine;
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
        [SerializeField] GameObject gemPrefab;
        [SerializeField] List<MineralItem> gemObjects;

        //gemerator position
        Vector3 gemOrigin;

        void Awake()
        {
            // Singleton intitialization.
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        //generate a number of gems
        public void GenerateGems()
        {
            gemOrigin = FindOrigin();

            if (range <= 0)
            {
                range = Mathf.RoundToInt((voxelGrid.XBounds + voxelGrid.YBounds + voxelGrid.ZBounds) / 3.0f);
            }

            if (gemCount <= 0)
            {
                gemCount = Mathf.RoundToInt((voxelGrid.XBounds + voxelGrid.YBounds + voxelGrid.ZBounds) / 3.0f) * 2;
            }

            //loop to generate gems
            for (int i = 0; i < gemCount; i++)
            {
                //randomly choose a prefab
                GemBehavior thisGem = Instantiate(gemPrefab, voxelGrid.transform).GetComponent<GemBehavior>();
                
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

                // Set the gem's internal data
                thisGem.Initialize(gemObjects[Random.Range(0, gemObjects.Count)]);
            }
        }

        //method to find the center of the voxel grid
        Vector3 FindOrigin()
        {
            return new Vector3(voxelGrid.X / 2.0f, voxelGrid.Y / 2.0f, voxelGrid.Z / 2.0f);
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