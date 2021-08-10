using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Controller2D))]
public class EnemyAI : MonoBehaviour
{

       
    [Header("Pathfinding")]
    public Transform target;
    public float activateDistance = 50f;
    public float pathUpdateSeconds = 0.5f;

    [Header("Physics")]
    public float maxJumpHeight = 3;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    public float moveSpeed = 2f;
    public float nextWaypointDistance = 3f;
    public float jumpNodeHeightRequirement = .6f;
    public float jumpCheckOffset = .1f;

    [Header("Custom Behavior")]
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;

    private Path path;
    private int directionX; 
    private int currentWaypoint = 0;
    float velocityXSmoothing;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    private float gravity;
    bool isGrounded = false;
    Seeker seeker;

    Controller2D controller;
    Vector3 velocity;

    public void Start()
    {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<Controller2D>();
        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
    }

    private void FixedUpdate()
    {
        if(TargetInDistance() && followEnabled)
        {
            PathFollow();

        }

    }
    private void UpdatePath()
    {
        if (followEnabled && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(transform.position, target.position, OnPathComplete);

        }
    }
    private void PathFollow()
    {
        if (path == null)
        {
            return;
        }

        //Reached end of path
        if (currentWaypoint >= path.vectorPath.Count)
        {
            return;
        }

        //see if colliding with anything
        Vector3 startOffset = transform.position - new Vector3(0f, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset);

        if (controller.collisions.below)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        //Direction Calculation
        Vector3 direction = ((Vector3)path.vectorPath[currentWaypoint] - transform.position).normalized;

        if (direction.x > 0)
        {
            directionX = 1;
        }
        else
        {
            directionX = -1;
        }

        float targetVelocitX = (directionX * moveSpeed);
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocitX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;

    

        //jump
        if (jumpEnabled && isGrounded)
        {
            if (controller.collisions.left || controller.collisions.right)
            {
                velocity.y = maxJumpHeight;
            }
        }


        //Movement
        controller.Move(velocity * Time.deltaTime, isGrounded);

        if (controller.collisions.above || controller.collisions.below && direction.y > jumpNodeHeightRequirement)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
                Debug.Log("Pigman slides a slop!");
            }
            else
            {
                velocity.y = 0;
            }
        }

        //Next Waypoint
        float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        //Direction Grpahics Handling
        if (directionLookEnabled)
        {
            if (velocity.x > 0)
            {
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            }else if(velocity.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
    private bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
    }
    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
}
