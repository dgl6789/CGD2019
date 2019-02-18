using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianMovement : MonoBehaviour {

    //waypoints
    GameObject nextWaypoint;

    //movement values
    Vector2 position;
    Vector2 velocity;
    Vector2 acceleration;
    Vector2 direction;

    float speed;
    float maxSpeed;
    float maxForce;
    float mass;
    float flockingWeight;

    //world bounds
    Vector2 worldBounds = new Vector2(50, 50);

    //wandering
    int wanderAngle = 0;
    int deltaAngle = 5;

    //collision detection
    float collisionRadius;

	// Use this for initialization
	void Start () {
        position = transform.position;
        velocity = Vector2.zero;
        direction = Vector2.zero;
        acceleration = Vector2.zero;

        maxSpeed = 10;
        maxForce = 10;

        collisionRadius = 0.5f;

        wanderAngle = Random.Range(0, 360);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //method to calculate steering forces
    void CalculateSteeringForces ()
    {

    }

    //update position based on steering forces
    void UpdatePosition()
    {
        Vector2.ClampMagnitude(acceleration, maxForce);

        velocity += acceleration * Time.deltaTime;

        Vector2.ClampMagnitude(velocity, maxSpeed);

        position += velocity * Time.deltaTime;

        transform.position = position;

        direction = velocity.normalized;
        acceleration = Vector2.zero;
    }

    //method to check if the civilian is out of the world
    bool OutOfBounds ()
    {
        return (position.x > worldBounds.x || position.x < 0 || position.y > worldBounds.y || position.y < 0);
    }

    //method to apply a force to the acceleration
    void ApplyForce (Vector2 force)
    {
        acceleration += force / mass * speed;
    }

    //method to determine seeking forces towards a position
    Vector2 Seek(Vector2 targetPos)
    {
        Vector2 desiredVelocity = targetPos - position;

        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        Vector2 steeringForce = desiredVelocity - velocity;

        return steeringForce;
    }

    //method to determine feeling forces away from a position
    Vector2 Flee(Vector2 targetPos)
    {
        Vector2 desiredVelocity = position - targetPos;

        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        Vector2 steeringForce = desiredVelocity - velocity;

        return steeringForce;
    }

    void Flock()
    {

    }

    //method to move towards the center of the flock
    void Cohesion()
    {

    }

    Vector2 Separation (Vector2 targetPos)
    {
        float dist = CalcDistSqr(targetPos);

        if (dist < 2f * 2f)
        {
            Vector2 separationForce = 2f * Flee(targetPos);

            if (dist < 1f * 1f)
            {
                separationForce *= 4f;
            }

            return separationForce;
        }

        return Vector2.zero;
    }

    void Wander()
    {

    }

    float CalcDistSqr (Vector2 targetPos)
    {
        float distX = (position.x - targetPos.x) * (position.x - targetPos.x);
        float distY = (position.y - targetPos.y) * (position.y - targetPos.y);

        return distX + distY;
    }
}
