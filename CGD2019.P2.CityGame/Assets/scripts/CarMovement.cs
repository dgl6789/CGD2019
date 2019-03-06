using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{

    public class CarMovement : MonoBehaviour
    {

        float speed;
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        Vector2 direction;
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        Bounds worldBounds;

        // Use this for initialization
        void Start()
        {
            Rotate();

            worldBounds = OrthographicBounds();
        }

        // Update is called once per frame
        void Update()
        {
            if (OutOfBounds())
                Destroy(gameObject);

            Move();
        }

        //method to move the car
        void Move()
        {
            Vector3 currentPos = transform.position;

            currentPos.x += direction.x * speed;
            currentPos.y += direction.y * speed;

            transform.position = currentPos;
        }

        //method to rotate sprite
        void Rotate()
        {
            if (direction.x == 1)
                transform.Rotate(Vector3.forward * 180);
            else if (direction.y == 1)
                transform.Rotate(Vector3.forward * -90);
            else if (direction.y == -1)
                transform.Rotate(Vector3.forward * 90);
}

        //method to determine if the car is out of bounds
        bool OutOfBounds()
        {
            return false;
            return (transform.position.x > worldBounds.max.x || transform.position.x < worldBounds.min.x || transform.position.y > worldBounds.max.y || transform.position.y < worldBounds.min.y);
        }

        //method to determine bounds of screen
        public static Bounds OrthographicBounds()
        {
            return CivilianManager.Instance.worldRenderer.sprite.bounds;
        }
    }
}
