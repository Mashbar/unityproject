using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class GroundEnemyAI : RaycastController
{
    public LayerMask passengerMask;
    public Transform player;

    public float agroRange;

    public float maxJumpHeight = 4;
    public float timeToJumpApex = .4f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    public float moveSpeed = 6;
    private bool facingRight;
    public bool chasePlayer;

    float gravity;
    float velocityXSmoothing;

    Controller2D controller;

    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    public float speed;
    public bool cyclic;
    public float waitTime;
    [Range(0, 2)]
    public float easeAmount;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;
    List<EnemyMovement> enemyMovement;
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();


    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();


        controller = GetComponent<Controller2D>();

        chasePlayer = false;
        facingRight = true;
        gravity = -18.75f;

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRaycastOrigins();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);



        if (!chasePlayer)
        {
            Vector3 velocity = CalculateEnemyMovement();
            //Vector3 velocity = Vector3.MoveTowards(transform.position, player.position, moveSpeed);
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime, false); 
        }
        else
        {
            Vector3 velocity = CalculateChaseEnemyMovement();
            controller.Move(velocity * Time.deltaTime, true);
        }

        
    }


    Vector3 CalculateChaseEnemyMovement()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        Vector3 newPos = Vector3.Lerp(transform.position, player.position, 1f);
        return newPos;
     
   }
    Vector3 CalculateEnemyMovement()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

    

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }
        return newPos - transform.position;
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
    struct EnemyMovement
    {
        public Transform transform;
        public Vector3 velocity;


        public EnemyMovement(Transform _transform, Vector3 _velocity)
        {
            transform = _transform;
            velocity = _velocity;
        }
    }
}
