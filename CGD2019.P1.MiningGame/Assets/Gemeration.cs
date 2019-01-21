using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gemeration : MonoBehaviour {

    //gemeration values
    public int gemCount = 0;
    public int range = 0;
    public List<GameObject> gemPrefabs = new List<GameObject>();

	// Use this for initialization
	void Start () {
        GenerateGems();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //generate a number of gems
    public void GenerateGems()
    {
        //loop to generate gems
        for (int i = 0; i < gemCount; i++)
        {
            //randomly choose a prefab
            GameObject thisGem = Instantiate(ChoosePrefab());

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
            gemPosition = gemPosition * rangePercentage;

            Debug.Log("Placing gem " + i + " at " + gemPosition);

            //set the gem's position
            thisGem.transform.position = gemPosition;
            Debug.Log("this gem's position = " + thisGem.transform.position + "\n");
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
}
