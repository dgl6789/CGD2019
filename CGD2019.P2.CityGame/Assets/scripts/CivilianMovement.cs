using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace App
{
    public class CivilianMovement : MonoBehaviour {

        //flocking
        float flockingWeight;
        Vector3 averageFlockPosition;

        //waypoints
        private bool checkWaypoint = true;
        public WayPoint nextWaypoint;
        public WayPoint prevWaypoint;

        //movement values
        Vector3 position;
        public Vector3 Position { get { return position; } }
        Vector3 velocity;
        Vector3 acceleration;
        Vector3 direction;
        float speed;
        float maxSpeed;
        float maxForce;

        Bounds worldBounds;

        //neighbors
        float neighborRange;
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

            speed = 1;
            maxSpeed = 10;
            maxForce = 10;
            
            flockingWeight = 0.75f;
            neighborRange = 2.0f;

            wanderAngle = Random.Range(0, 360);

            worldBounds = OrthographicBounds();
        }

        // Update is called once per frame
        void Update() {
            //look for neighbors
            ScanNearby();

            //determine which way to move
            CalculateSteeringForces();

            //update position based on movement forces
            UpdatePosition();

            //check waypoint if it exists
            if (nextWaypoint) UpdateWaypoint();

            //delete if out of bounds
            if (OutOfBounds())
                CivilianManager.Instance.RemoveCivilian(this);
        }

        //method to switch to the next waypoint
        void UpdateWaypoint()
        {
            if (WithinDist(nextWaypoint.transform.position, neighborRange))
                nextWaypoint = nextWaypoint.GetNextWaypoint();
        }

        //method to calculate steering forces
        void CalculateSteeringForces()
        {
            FindAvgPosition();

            foreach (CivilianMovement neighbor in neighborList)
            {
                Vector3 neighborPos = neighbor.position;

                ApplyForce(Align(neighborPos, neighbor.direction));
                ApplyForce(Separation(neighborPos));
            }

            ApplyForce(Cohesion() * flockingWeight);

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

        //method to scan nearby area
        void ScanNearby()
        {
            //empty neigbor list to prevent overlap
            neighborList.Clear();

            //scan nearby area for rigidbodies
            RaycastHit2D[] results = Physics2D.CircleCastAll(
                new Vector2(position.x, position.y),
                neighborRange,
                Vector2.zero);

            //values for finding nearest waypoints
            WayPoint nearestWaypoint = null;
            float nearestDistSqr = float.PositiveInfinity;

            //loop through nearby rigidbodies
            for(int i = 0; i < results.Length; i++)
            {
                //this specific rigidbody
                GameObject thisHit = results[i].collider.gameObject;

                //find relevant components
                CivilianMovement thisCiv = thisHit.GetComponent<CivilianMovement>();
                WayPoint thisWaypoint = thisHit.GetComponent<WayPoint>();

                if (thisCiv != null)
                {
                    //if this is a civilian, add it to the neighbor list
                    neighborList.Add(thisCiv);
                }
                else if (thisWaypoint != null && thisWaypoint != prevWaypoint && nextWaypoint == null)
                {
                    //if this is a waypoint and NOT the previous waypoint, check if it's the nearest

                    //find distance to waypoint
                    float thisDistSqr = CalcDistSqr(thisWaypoint.transform.position);

                    //check if it's closer than the previous one
                    if (thisDistSqr < nearestDistSqr)
                    {
                        //make this one the closest
                        nearestWaypoint = thisWaypoint;
                        nearestDistSqr = thisDistSqr;
                    }
                }
            }

            //if a closer waypoint was found, go to that one
            if (nearestWaypoint != null && nextWaypoint == null)
            {
                prevWaypoint = nextWaypoint;
                nextWaypoint = nearestWaypoint;
            }
        }

        //method to check if the civilian is out of the world
        bool OutOfBounds()
        {
            return (position.x > worldBounds.max.x || position.x < worldBounds.min.x || position.y > worldBounds.max.y || position.y < worldBounds.min.y) && CivilianManager.Instance.spawnPoints.Contains(nextWaypoint);
        }

        //method to apply a force to the acceleration
        void ApplyForce(Vector3 force)
        {
            acceleration += force * speed;
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
            if (WithinDist(targetPos, neighborRange))
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

        //method to keep civilians from hitting each other
        Vector3 Separation(Vector3 targetPos)
        {
            if (WithinDist(targetPos, neighborRange))
            {
                Vector3 separationForce = Flee(targetPos);

                //if (WithinDist(targetPos, 1.0f))
                //{
                //    separationForce *= 4f;
                //}

                return separationForce;
            }

            return Vector3.zero;
        }

        //method for wandering behavior for when seeking is unable to happen
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

        //method to find the average position of the flock
        void FindAvgPosition()
        {
            averageFlockPosition = Vector3.zero;

            foreach(CivilianMovement neighbor in neighborList)
            {
                averageFlockPosition = averageFlockPosition + neighbor.position;
            }

            averageFlockPosition /= neighborList.Count;
        }

        //method to determine whether a specific position is within a given range
        public bool WithinDist(Vector3 targetPos, float range)
        {
            return (CalcDistSqr(targetPos) < range * range);
        }

        //method to calculate the distance squared between this and a target position
        //distance squared to reduce math calls
        float CalcDistSqr(Vector3 targetPos)
        {
            float distX = (position.x - targetPos.x) * (position.x - targetPos.x);
            float distY = (position.y - targetPos.y) * (position.y - targetPos.y);

            return distX + distY;
        }

        //method to determine bounds of screen
        public static Bounds OrthographicBounds() {
            SpriteRenderer r = CivilianManager.Instance.worldRenderer;

            return new Bounds(r.transform.position, 
                new Vector3(r.sprite.bounds.extents.x * 2 * r.transform.localScale.x, r.sprite.bounds.extents.y * 2 * r.transform.localScale.y, 1));
        }
    }
}
