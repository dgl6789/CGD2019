using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace App
{
    public class CivilianMovement : MonoBehaviour {

        //data
        CivilianObject civilianData;
        public CivilianObject CivilianData { get { return civilianData; } }
        Vector3 averageFlockPosition;

        //waypoints
        GameObject nextWaypoint;

        //movement values
        Vector3 position;
        Vector3 velocity;
        Vector3 acceleration;
        Vector3 direction;

        float speed;
        float maxSpeed;
        float maxForce;
        float mass;
        float flockingWeight;

        //world bounds
        Vector3 worldBounds = new Vector3(50, 50);

        //neighbors
        List<CivilianMovement> neighborList = new List<CivilianMovement>();

        //wandering
        int wanderAngle = 0;
        int deltaAngle = 5;

        //collision detection
        float collisionRadius;

        // Use this for initialization
        void Start() {
            position = transform.position;
            velocity = Vector3.zero;
            direction = Vector3.zero;
            acceleration = Vector3.zero;

            maxSpeed = 10;
            maxForce = 10;

            collisionRadius = 0.5f;

            wanderAngle = Random.Range(0, 360);
        }

        // Update is called once per frame
        void Update() {

        }

        //method to calculate steering forces
        void CalculateSteeringForces()
        {

        }

        //update position based on steering forces
        void UpdatePosition()
        {
            Vector3.ClampMagnitude(acceleration, maxForce);

            velocity += acceleration * Time.deltaTime;

            Vector3.ClampMagnitude(velocity, maxSpeed);

            position += velocity * Time.deltaTime;

            transform.position = position;

            direction = velocity.normalized;
            acceleration = Vector3.zero;
        }

        //method to check if the civilian is out of the world
        bool OutOfBounds()
        {
            return (position.x > worldBounds.x || position.x < 0 || position.y > worldBounds.y || position.y < 0);
        }

        //method to apply a force to the acceleration
        void ApplyForce(Vector3 force)
        {
            acceleration += force / mass * speed;
        }

        //method to determine seeking forces towards a position
        Vector3 Seek(Vector3 targetPos)
        {
            Vector3 desiredVelocity = targetPos - position;

            desiredVelocity = desiredVelocity.normalized * maxSpeed;

            Vector3 steeringForce = desiredVelocity - velocity;

            return steeringForce;
        }

        //method to determine feeling forces away from a position
        Vector3 Flee(Vector3 targetPos)
        {
            Vector3 desiredVelocity = position - targetPos;

            desiredVelocity = desiredVelocity.normalized * maxSpeed;

            Vector3 steeringForce = desiredVelocity - velocity;

            return steeringForce;
        }

        void Flock()
        {

        }

        //method to move towards the center of the flock
        Vector3 Cohesion()
        {
            Vector3 cohesionForce = Seek(averageFlockPosition);

            return cohesionForce;
        }

        Vector3 Separation(Vector3 targetPos)
        {
            float dist = CalcDistSqr(targetPos);

            if (dist < 2f * 2f)
            {
                Vector3 separationForce = 2f * Flee(targetPos);

                if (dist < 1f * 1f)
                {
                    separationForce *= 4f;
                }

                return separationForce;
            }

            return Vector3.zero;
        }

        void Wander()
        {
            Vector3 centerPoint = position + direction;

            Vector3 wanderPoint = centerPoint;

            if (wanderAngle < 0)
                wanderAngle = 360 - wanderAngle;

            if (wanderAngle <= 360)
                wanderAngle = wanderAngle % 360;
        }

        void FindAvgPosition()
        {
            averageFlockPosition = Vector3.zero;

            foreach(CivilianMovement neighbor in neighborList)
            {
                averageFlockPosition = averageFlockPosition + neighbor.transform.position; //should be neighbor.position?
            }

            averageFlockPosition /= neighborList.Count;
        }

        public float CalcDistSqr(Vector3 targetPos)
        {
            float distX = (position.x - targetPos.x) * (position.x - targetPos.x);
            float distY = (position.y - targetPos.y) * (position.y - targetPos.y);

            return distX + distY;
        }
    }
}
