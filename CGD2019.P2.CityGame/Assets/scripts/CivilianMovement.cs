using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace App
{
    public class CivilianMovement : MonoBehaviour {

        //data
        CivilianObject civilianData;
        public CivilianObject CivilianData
        {
            get { return civilianData; }
            set { civilianData = value; }
        }
        Vector3 averageFlockPosition;

        //waypoints
        GameObject nextWaypoint;

        //movement values
        Vector3 position;
        public Vector3 Position { get { return position; } }
        Vector3 velocity;
        Vector3 acceleration;
        Vector3 direction;
        
        float maxSpeed;
        float maxForce;

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
            //look for neighbors
            neighborList = CivilianManager.Instance.FindNeighbors(this);

            //determine which way to move
            CalculateSteeringForces();

            //update position based on movement forces
            UpdatePosition();

            //delete if out of bounds
            if (OutOfBounds())
                CivilianManager.Instance.RemoveCivilian(this);
        }

        //method to calculate steering forces
        void CalculateSteeringForces()
        {
            FindAvgPosition();

            foreach (CivilianMovement neighbor in neighborList)
            {
                Vector3 neighborPos = neighbor.position; //should be neighbor.position?

                ApplyForce(Align(neighborPos, neighbor.direction));
                ApplyForce(Separation(neighborPos));
            }

            ApplyForce(Cohesion() * civilianData.FlockingWeight);

            if (nextWaypoint != null)
                ApplyForce(Seek(nextWaypoint.transform.position));
            else
                ApplyForce(Wander());
        }

        //update position based on steering forces
        void UpdatePosition()
        {
            //update position based on actual position
            position = transform.position;

            //keep acceleration contained
            Vector3.ClampMagnitude(acceleration, maxForce);

            //change velocity based on acceleration
            velocity += acceleration * Time.deltaTime;

            //keep velocity contained
            Vector3.ClampMagnitude(velocity, maxSpeed);

            //change position based on velocity
            position += velocity * Time.deltaTime;

            //correct z component of position
            position.z = 0.0f;

            //move to new position
            transform.position = position;

            //reset values
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
            acceleration += force * civilianData.Speed;
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

        //method to align direction with neighbors
        Vector3 Align(Vector3 targetPos, Vector3 targetDir)
        {
            if (WithinDist(targetPos, CivilianData.NeighborRange))
            {
                Vector3 alignmentForce = Seek(targetPos + targetDir);

                return alignmentForce;
            }

            return Vector3.zero;
        }

        //method to move towards the center of the flock
        Vector3 Cohesion()
        {
            Vector3 cohesionForce = Seek(averageFlockPosition);

            return cohesionForce;
        }

        Vector3 Separation(Vector3 targetPos)
        {
            if (WithinDist(targetPos, 2.0f))
            {
                Vector3 separationForce = 2f * Flee(targetPos);

                if (WithinDist(targetPos, 1.0f))
                {
                    separationForce *= 4f;
                }

                return separationForce;
            }

            return Vector3.zero;
        }

        Vector3 Wander()
        {
            Vector3 centerPoint = position + direction;

            Vector3 wanderPoint = centerPoint;

            if (wanderAngle < 0)
                wanderAngle = 360 - wanderAngle;

            if (wanderAngle >= 360)
                wanderAngle = wanderAngle % 360;

            wanderPoint.x += 3 * CivilianManager.Instance.CosLookup(wanderAngle);
            wanderPoint.y += 3 * CivilianManager.Instance.SinLookup(wanderAngle);

            Vector3 wanderForce = Seek(wanderPoint);

            if (Random.Range(0, 2) == 0)
                wanderAngle -= deltaAngle;
            else
                wanderAngle += deltaAngle;

            wanderAngle = wanderAngle % 360;

            return wanderForce;
        }

        void FindAvgPosition()
        {
            averageFlockPosition = Vector3.zero;

            foreach(CivilianMovement neighbor in neighborList)
            {
                averageFlockPosition = averageFlockPosition + neighbor.position; //should be neighbor.position?
            }

            averageFlockPosition /= neighborList.Count;
        }

        public bool WithinDist(Vector3 targetPos, float range)
        {
            return (CalcDistSqr(targetPos) < range * range);
        }

        float CalcDistSqr(Vector3 targetPos)
        {
            float distX = (position.x - targetPos.x) * (position.x - targetPos.x);
            float distY = (position.y - targetPos.y) * (position.y - targetPos.y);

            return distX + distY;
        }
    }
}
