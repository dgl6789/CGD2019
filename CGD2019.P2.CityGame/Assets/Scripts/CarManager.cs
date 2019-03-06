using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public class CarManager : MonoBehaviour
    {
        //car values
        public GameObject carPrefab;
        public Transform carParent;
        public List<CarSpawn> carSpawnPoints = new List<CarSpawn>();

        //instance
        public static CarManager Instance;

        //car values
        float speedMax = 1.5f;
        float speedMin = 0.5f;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        // Update is called once per frame
        public void DoUpdateStep()
        {
            if (Random.Range(0, 50) == 0) //Debug.Log("1/100");
                SpawnCar();
        }

        //method to spawn a car
        void SpawnCar()
        {
            CarMovement car = Instantiate(carPrefab, carParent).GetComponent<CarMovement>();

            CarSpawn startPoint = carSpawnPoints[Random.Range(0, carSpawnPoints.Count)];

            //Debug.Log("spawning car at " + startPoint);

            car.transform.position = startPoint.transform.position;
            car.Direction = startPoint.direction;
            car.Speed = Random.Range(speedMin, speedMax);
        }
    }
}
