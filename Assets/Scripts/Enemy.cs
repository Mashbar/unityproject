using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]

public class Enemy : RaycastController
{

    public Transform player;

    public bool simple = true;
    public bool patrol = false;
    public bool cyclic = false;
    public bool ignoreLedge = true;
    private bool facingRight;

    public float waitTime;

    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    public float maxJumpVelocity = 1.5f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;
    float velocityXSmoothing;
    public float easeAmount;

    private int directionX;
    private float gravity;
    public float moveSpeed = 4f;
    Vector3 velocity;


    Controller2D controller;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        controller = GetComponent<Controller2D>();
        gravity = -5f;
        facingRight = true;

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (simple)
        {
            UpdateRaycastOrigins();
            DetectCollisionState();
            CalculateEnemySimpleMovement();
            FlipSprite();
            controller.Move(velocity * Time.deltaTime, false);
        }
        else if(patrol){

            UpdateRaycastOrigins();
            ClaculateEnemyPatrolMovement();
            controller.Move(velocity * Time.deltaTime, false);

        }


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);


    }

    void FlipSprite()
    {
        if (velocity.x >= 0.01f)
        {
            transform.localScale = new Vector3(-1, 1f, 1f);
        }
        else if (velocity.x <= 0.01f)
        {
            transform.localScale = new Vector3(1, 1f, 1f);
        }
    }

    void DetectCollisionState()
    { 

        if (controller.collisions.right || controller.collisions.left)
        {
        moveSpeed = -moveSpeed;
        }
    }

    void ClaculateEnemyPatrolMovement()
    {

        //Check Waypoint
        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        Vector3 target = globalWaypoints[toWaypointIndex];

        //keep waypoint at enemy height
        target.y = transform.position.y;

        //find move direcation
        Vector3 moveDirection = target - transform.position;
        if (moveDirection.x > 0)
        {
            directionX = 1;
        }
        else
        {
            directionX = -1;
        }

        if (moveDirection.magnitude < 0.5)
        {
            if (nextMoveTime == 0)
                nextMoveTime = Time.time; // Pause over the Waypoint
            if ((Time.time - nextMoveTime) >= waitTime)
            {
                fromWaypointIndex++;
                nextMoveTime = 0;
            }
            
        }
        //calculate velocity
        float targetVelocitX = (directionX * moveSpeed);
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocitX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }
    void CalculateEnemySimpleMovement()
    {
        float targetVelocitX = moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocitX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }


    void Jump()
    {
        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (directionX != -Mathf.Sign(controller.collisions.slopeNormal.x))
                { //Not jumping against max slope
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }

        }

    }

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    void OnDrawGizmos()
    {

        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }

    }

}
