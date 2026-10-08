using UnityEngine;

public class AITruckController : MonoBehaviour
{
    // =========================================================
    // TRUCK REFERENCES
    // =========================================================

    [Header("Truck")]

    [SerializeField]
    private Rigidbody truckRigidbody;

    [SerializeField]
    private WheelCollider frontLeftWheel;

    [SerializeField]
    private WheelCollider frontRightWheel;

    [SerializeField]
    private WheelCollider rearLeftWheel;

    [SerializeField]
    private WheelCollider rearRightWheel;


    // =========================================================
    // PLAYER
    // =========================================================

    [Header("Player")]

    [SerializeField]
    private Transform playerCar;


    // =========================================================
    // TRAILER
    // =========================================================

    [Header("Trailer")]

    [SerializeField]
    private Transform trailerCenter;

    [SerializeField]
    private Transform trailerRearPoint;


    // =========================================================
    // WAYPOINTS
    // =========================================================

    [Header("Racing Line")]

    [SerializeField]
    private Transform[] waypoints;

    [SerializeField]
    private float waypointReachDistance = 8f;

    private int currentWaypoint = 0;


    // =========================================================
    // SPEED
    // =========================================================

    [Header("Speed")]

    [SerializeField]
    private float maximumSpeed = 55f;

    [SerializeField]
    private float acceleration = 900f;

    [SerializeField]
    private float brakeForce = 1800f;


    // =========================================================
    // STEERING
    // =========================================================

    [Header("Steering")]

    [SerializeField]
    private float maximumSteeringAngle = 30f;

    [SerializeField]
    private float steeringSmoothness = 4f;


    // =========================================================
    // DEFENSIVE AI
    // =========================================================

    [Header("Defensive AI")]

    [SerializeField]
    private float detectionDistance = 18f;

    [SerializeField]
    private float rearDetectionDistance = 25f;

    [Range(0f, 1f)]
    [SerializeField]
    private float defensiveChance = 0.65f;

    [SerializeField]
    private float defensiveCooldown = 3f;

    [SerializeField]
    private float defensiveDuration = 1.8f;

    [SerializeField]
    private float defensiveOffset = 4f;

    [SerializeField]
    private float defensiveSmoothness = 2.5f;


    // =========================================================
    // FINISH PROTECTION
    // =========================================================

    [Header("Finish Protection")]

    [SerializeField]
    private Transform finishProtectionPoint;

    [SerializeField]
    private float finishProtectionDistance = 35f;


    // =========================================================
    // OBSTACLE DETECTION
    // =========================================================

    [Header("Obstacle Avoidance")]

    [SerializeField]
    private float obstacleDetectionDistance = 12f;

    [SerializeField]
    private float obstacleDetectionRadius = 1.5f;

    [SerializeField]
    private float sideCheckDistance = 6f;

    [SerializeField]
    private float obstacleAvoidanceOffset = 5f;

    [SerializeField]
    private float obstacleAvoidanceSmoothness = 4f;

    [Tooltip("Assign the Obstracle layer here.")]
    [SerializeField]
    private LayerMask obstacleLayer;


    // =========================================================
    // COLLISION RECOVERY
    // =========================================================

    [Header("Collision Recovery")]

    [SerializeField]
    private float recoveryReverseTime = 1.2f;

    [SerializeField]
    private float recoveryReverseTorque = 700f;

    [SerializeField]
    private float recoverySteeringAngle = 25f;

    [SerializeField]
    private float recoveryCooldown = 2f;

    [SerializeField]
    private float recoveryStopTime = 0.25f;


    // =========================================================
    // AI STATES
    // =========================================================

    private enum AIState
    {
        Racing,
        Defensive,
        AvoidingObstacle,
        Returning,
        Recovery
    }

    private AIState currentState =
        AIState.Racing;


    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private float currentSteering;

    private float currentThrottle;

    private float currentBrake;

    private float defensiveTimer;

    private float cooldownTimer;

    private float currentOffset;

    private float targetOffset;

    private float defensiveSide;

    private float obstacleAvoidanceTimer;

    private float recoveryTimer;

    private float recoveryCooldownTimer;

    private float recoveryStopTimer;

    private float recoveryDirection;

    private bool recovering;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (truckRigidbody == null)
        {
            truckRigidbody =
                GetComponent<Rigidbody>();
        }


        if (waypoints == null ||
            waypoints.Length == 0)
        {
            Debug.LogError(
                "AI Truck: No waypoints assigned."
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (waypoints == null ||
            waypoints.Length == 0)
        {
            return;
        }


        UpdateTimers();


        // Recovery has complete control
        // over the truck.
        if (currentState ==
            AIState.Recovery)
        {
            UpdateRecovery();

            return;
        }


        CheckForObstacle();

        CheckPlayer();

        UpdateState();

        ApplySteering();

        ApplyMotor();

        ApplyBrakes();

        CheckWaypoint();
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        if (currentState ==
            AIState.Recovery)
        {
            ApplyRecoveryPhysics();
        }
    }


    // =========================================================
    // TIMERS
    // =========================================================

    private void UpdateTimers()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -=
                Time.deltaTime;
        }


        if (defensiveTimer > 0f)
        {
            defensiveTimer -=
                Time.deltaTime;
        }


        if (obstacleAvoidanceTimer > 0f)
        {
            obstacleAvoidanceTimer -=
                Time.deltaTime;
        }


        if (recoveryCooldownTimer > 0f)
        {
            recoveryCooldownTimer -=
                Time.deltaTime;
        }


        if (recoveryStopTimer > 0f)
        {
            recoveryStopTimer -=
                Time.deltaTime;
        }
    }


    // =========================================================
    // OBSTACLE DETECTION
    // =========================================================

    private void CheckForObstacle()
    {
        if (currentState ==
            AIState.Recovery)
        {
            return;
        }


        Vector3 origin =
            transform.position +
            transform.forward * 2f +
            Vector3.up * 0.8f;


        bool hit =
            Physics.SphereCast(
                origin,
                obstacleDetectionRadius,
                transform.forward,
                out RaycastHit hitInfo,
                obstacleDetectionDistance,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            );


        if (hit)
        {
            Debug.DrawLine(
                origin,
                hitInfo.point,
                Color.red
            );


            HandleObstacle();
        }
        else
        {
            Debug.DrawRay(
                origin,
                transform.forward *
                obstacleDetectionDistance,
                Color.green
            );
        }
    }


    // =========================================================
    // HANDLE OBSTACLE
    // =========================================================

    private void HandleObstacle()
    {
        if (currentState ==
            AIState.AvoidingObstacle)
        {
            return;
        }


        if (IsNearFinish())
        {
            return;
        }


        bool leftClear =
            IsSideClear(-1f);


        bool rightClear =
            IsSideClear(1f);


        if (leftClear &&
            rightClear)
        {
            defensiveSide =
                currentOffset >= 0f
                ? 1f
                : -1f;


            StartObstacleAvoidance();

            return;
        }


        if (leftClear)
        {
            defensiveSide = -1f;

            StartObstacleAvoidance();

            return;
        }


        if (rightClear)
        {
            defensiveSide = 1f;

            StartObstacleAvoidance();

            return;
        }


        // Both sides blocked.
        // Do not force the truck forward.
        currentThrottle = 0f;

        targetOffset = 0f;
    }


    // =========================================================
    // SIDE CHECK
    // =========================================================

    private bool IsSideClear(
        float side
    )
    {
        Vector3 origin =
            transform.position +
            Vector3.up * 0.8f;


        Vector3 direction =
            transform.right *
            side;


        bool hit =
            Physics.SphereCast(
                origin,
                obstacleDetectionRadius,
                direction,
                out RaycastHit hitInfo,
                sideCheckDistance,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            );


        if (hit)
        {
            Debug.DrawLine(
                origin,
                hitInfo.point,
                Color.red
            );

            return false;
        }


        Debug.DrawRay(
            origin,
            direction *
            sideCheckDistance,
            Color.green
        );


        return true;
    }


    // =========================================================
    // START OBSTACLE AVOIDANCE
    // =========================================================

    private void StartObstacleAvoidance()
    {
        currentState =
            AIState.AvoidingObstacle;


        obstacleAvoidanceTimer =
            1.5f;


        targetOffset =
            defensiveSide *
            obstacleAvoidanceOffset;
    }


    // =========================================================
    // PLAYER DETECTION
    // =========================================================

    private void CheckPlayer()
    {
        if (playerCar == null ||
            trailerCenter == null)
        {
            return;
        }


        if (currentState !=
            AIState.Racing)
        {
            return;
        }


        if (cooldownTimer > 0f)
        {
            return;
        }


        if (IsNearFinish())
        {
            return;
        }


        Vector3 toPlayer =
            playerCar.position -
            trailerCenter.position;


        float distance =
            toPlayer.magnitude;


        if (distance >
            detectionDistance)
        {
            return;
        }


        Vector3 localPlayerPosition =
            trailerCenter.InverseTransformPoint(
                playerCar.position
            );


        // Player must be behind trailer.
        if (localPlayerPosition.z > 2f)
        {
            return;
        }


        if (Mathf.Abs(
                localPlayerPosition.x
            ) > 10f)
        {
            return;
        }


        if (Random.value <=
            defensiveChance)
        {
            StartDefensiveManeuver();
        }
    }


    // =========================================================
    // START DEFENSIVE MANEUVER
    // =========================================================

    private void StartDefensiveManeuver()
    {
        defensiveSide =
            Random.value < 0.5f
            ? -1f
            : 1f;


        // Make sure chosen direction is clear.
        if (!IsSideClear(
                defensiveSide
            ))
        {
            float opposite =
                -defensiveSide;


            if (IsSideClear(opposite))
            {
                defensiveSide =
                    opposite;
            }
            else
            {
                return;
            }
        }


        currentState =
            AIState.Defensive;


        defensiveTimer =
            defensiveDuration;


        cooldownTimer =
            defensiveCooldown;


        targetOffset =
            defensiveSide *
            defensiveOffset;
    }


    // =========================================================
    // STATE UPDATE
    // =========================================================

    private void UpdateState()
    {
        switch (currentState)
        {
            case AIState.Racing:

                targetOffset = 0f;

                currentOffset =
                    Mathf.Lerp(
                        currentOffset,
                        targetOffset,
                        Time.deltaTime *
                        defensiveSmoothness
                    );

                break;


            case AIState.Defensive:

                UpdateDefensiveMovement();

                break;


            case AIState.AvoidingObstacle:

                UpdateObstacleAvoidance();

                break;


            case AIState.Returning:

                targetOffset = 0f;

                currentOffset =
                    Mathf.Lerp(
                        currentOffset,
                        targetOffset,
                        Time.deltaTime *
                        defensiveSmoothness
                    );


                if (Mathf.Abs(
                        currentOffset
                    ) < 0.15f)
                {
                    currentOffset = 0f;

                    currentState =
                        AIState.Racing;
                }

                break;


            case AIState.Recovery:

                break;
        }
    }


    // =========================================================
    // DEFENSIVE MOVEMENT
    // =========================================================

    private void UpdateDefensiveMovement()
    {
        currentOffset =
            Mathf.Lerp(
                currentOffset,
                targetOffset,
                Time.deltaTime *
                defensiveSmoothness
            );


        // If obstacle suddenly appears,
        // switch to obstacle avoidance.
        if (CheckObstacleImmediately())
        {
            HandleObstacle();

            return;
        }


        if (defensiveTimer <= 0f)
        {
            currentState =
                AIState.Returning;
        }
    }


    // =========================================================
    // OBSTACLE AVOIDANCE
    // =========================================================

    private void UpdateObstacleAvoidance()
    {
        currentOffset =
            Mathf.Lerp(
                currentOffset,
                targetOffset,
                Time.deltaTime *
                obstacleAvoidanceSmoothness
            );


        if (obstacleAvoidanceTimer <= 0f)
        {
            currentState =
                AIState.Returning;
        }
    }


    // =========================================================
    // IMMEDIATE OBSTACLE CHECK
    // =========================================================

    private bool CheckObstacleImmediately()
    {
        Vector3 origin =
            transform.position +
            transform.forward * 2f +
            Vector3.up * 0.8f;


        return Physics.SphereCast(
            origin,
            obstacleDetectionRadius,
            transform.forward,
            out _,
            obstacleDetectionDistance,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );
    }


    // =========================================================
    // STEERING
    // =========================================================

    private void ApplySteering()
    {
        if (currentState ==
            AIState.Recovery)
        {
            return;
        }


        Transform waypoint =
            waypoints[currentWaypoint];


        if (waypoint == null)
        {
            return;
        }


        Vector3 targetPosition =
            waypoint.position;


        targetPosition +=
            transform.right *
            currentOffset;


        Vector3 localTarget =
            transform.InverseTransformPoint(
                targetPosition
            );


        float targetSteering =
            localTarget.x /
            Mathf.Max(
                localTarget.magnitude,
                0.01f
            );


        targetSteering =
            Mathf.Clamp(
                targetSteering,
                -1f,
                1f
            );


        currentSteering =
            Mathf.Lerp(
                currentSteering,
                targetSteering,
                Time.deltaTime *
                steeringSmoothness
            );


        float steeringAngle =
            currentSteering *
            maximumSteeringAngle;


        frontLeftWheel.steerAngle =
            steeringAngle;


        frontRightWheel.steerAngle =
            steeringAngle;
    }


    // =========================================================
    // MOTOR
    // =========================================================

    private void ApplyMotor()
    {
        if (truckRigidbody == null)
        {
            return;
        }


        float speed =
            truckRigidbody.linearVelocity.magnitude *
            3.6f;


        if (speed <
            maximumSpeed)
        {
            currentThrottle =
                Mathf.Lerp(
                    currentThrottle,
                    1f,
                    Time.deltaTime *
                    2f
                );
        }
        else
        {
            currentThrottle =
                Mathf.Lerp(
                    currentThrottle,
                    0f,
                    Time.deltaTime *
                    3f
                );
        }


        float torque =
            acceleration *
            currentThrottle;


        rearLeftWheel.motorTorque =
            torque;


        rearRightWheel.motorTorque =
            torque;
    }


    // =========================================================
    // BRAKES
    // =========================================================

    private void ApplyBrakes()
    {
        if (truckRigidbody == null ||
            currentState ==
            AIState.Recovery)
        {
            return;
        }


        float speed =
            truckRigidbody.linearVelocity.magnitude *
            3.6f;


        float targetBrake = 0f;


        if (speed >
            maximumSpeed + 5f)
        {
            targetBrake =
                brakeForce;
        }


        currentBrake =
            Mathf.Lerp(
                currentBrake,
                targetBrake,
                Time.deltaTime *
                5f
            );


        frontLeftWheel.brakeTorque =
            currentBrake;

        frontRightWheel.brakeTorque =
            currentBrake;

        rearLeftWheel.brakeTorque =
            currentBrake;

        rearRightWheel.brakeTorque =
            currentBrake;
    }


    // =========================================================
    // COLLISION DETECTION
    // =========================================================

    private void OnCollisionEnter(
        Collision collision
    )
    {
        if (recovering)
        {
            return;
        }


        if (recoveryCooldownTimer > 0f)
        {
            return;
        }


        // Check the actual collision object's layer.
        if (((1 << collision.gameObject.layer)
             & obstacleLayer) == 0)
        {
            return;
        }


        StartRecovery(
            collision
        );
    }


    // =========================================================
    // START RECOVERY
    // =========================================================

    private void StartRecovery(
        Collision collision
    )
    {
        recovering = true;


        currentState =
            AIState.Recovery;


        recoveryTimer =
            recoveryReverseTime;


        recoveryCooldownTimer =
            recoveryCooldown;


        recoveryStopTimer =
            recoveryStopTime;


        currentThrottle = 0f;

        currentBrake = 0f;


        // Determine which direction
        // the obstacle is relative to truck.
        Vector3 localHit =
            transform.InverseTransformPoint(
                collision.contacts[0].point
            );


        // If obstacle is on the left,
        // reverse slightly toward right.
        if (localHit.x < 0f)
        {
            recoveryDirection = 1f;
        }
        else
        {
            recoveryDirection = -1f;
        }


        Debug.Log(
            "AI TRUCK: OBSTACLE HIT - REVERSING"
        );
    }


    // =========================================================
    // RECOVERY UPDATE
    // =========================================================

    private void UpdateRecovery()
    {
        if (truckRigidbody == null)
        {
            return;
        }


        // =============================================
        // STOP MOMENT
        // =============================================

        if (recoveryStopTimer > 0f)
        {
            StopTruck();

            return;
        }


        // =============================================
        // REVERSE TIMER
        // =============================================

        recoveryTimer -=
            Time.deltaTime;


        if (recoveryTimer <= 0f)
        {
            FinishRecovery();

            return;
        }
    }


    // =========================================================
    // RECOVERY PHYSICS
    // =========================================================

    private void ApplyRecoveryPhysics()
    {
        if (truckRigidbody == null)
        {
            return;
        }


        if (recoveryStopTimer > 0f)
        {
            StopTruck();

            return;
        }


        // =============================================
        // REVERSE
        // =============================================

        frontLeftWheel.steerAngle =
            recoveryDirection *
            recoverySteeringAngle;


        frontRightWheel.steerAngle =
            recoveryDirection *
            recoverySteeringAngle;


        rearLeftWheel.motorTorque =
            -recoveryReverseTorque;


        rearRightWheel.motorTorque =
            -recoveryReverseTorque;


        frontLeftWheel.brakeTorque = 0f;

        frontRightWheel.brakeTorque = 0f;

        rearLeftWheel.brakeTorque = 0f;

        rearRightWheel.brakeTorque = 0f;
    }


    // =========================================================
    // STOP TRUCK
    // =========================================================

    private void StopTruck()
    {
        rearLeftWheel.motorTorque = 0f;

        rearRightWheel.motorTorque = 0f;


        frontLeftWheel.brakeTorque =
            brakeForce;

        frontRightWheel.brakeTorque =
            brakeForce;

        rearLeftWheel.brakeTorque =
            brakeForce;

        rearRightWheel.brakeTorque =
            brakeForce;
    }


    // =========================================================
    // FINISH RECOVERY
    // =========================================================

    private void FinishRecovery()
    {
        recovering = false;


        currentState =
            AIState.Returning;


        currentOffset = 0f;

        targetOffset = 0f;


        rearLeftWheel.motorTorque = 0f;

        rearRightWheel.motorTorque = 0f;


        frontLeftWheel.brakeTorque = 0f;

        frontRightWheel.brakeTorque = 0f;

        rearLeftWheel.brakeTorque = 0f;

        rearRightWheel.brakeTorque = 0f;


        Debug.Log(
            "AI TRUCK: RECOVERED - RETURNING TO RACE"
        );
    }


    // =========================================================
    // WAYPOINT
    // =========================================================

    private void CheckWaypoint()
    {
        if (waypoints == null ||
            waypoints.Length == 0)
        {
            return;
        }


        Transform waypoint =
            waypoints[currentWaypoint];


        if (waypoint == null)
        {
            return;
        }


        float distance =
            Vector3.Distance(
                transform.position,
                waypoint.position
            );


        if (distance <=
            waypointReachDistance)
        {
            currentWaypoint++;


            if (currentWaypoint >=
                waypoints.Length)
            {
                currentWaypoint = 0;
            }
        }
    }


    // =========================================================
    // FINISH PROTECTION
    // =========================================================

    private bool IsNearFinish()
    {
        if (finishProtectionPoint == null)
        {
            return false;
        }


        float distance =
            Vector3.Distance(
                transform.position,
                finishProtectionPoint.position
            );


        return distance <=
               finishProtectionDistance;
    }


    // =========================================================
    // DEBUG GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // =============================================
        // OBSTACLE DETECTION
        // =============================================

        Vector3 origin =
            transform.position +
            transform.forward * 2f +
            Vector3.up * 0.8f;


        Gizmos.color =
            Color.yellow;


        Gizmos.DrawLine(
            origin,
            origin +
            transform.forward *
            obstacleDetectionDistance
        );


        Gizmos.DrawWireSphere(
            origin +
            transform.forward *
            obstacleDetectionDistance,
            obstacleDetectionRadius
        );


        // =============================================
        // WAYPOINTS
        // =============================================

        if (waypoints != null)
        {
            for (
                int i = 0;
                i < waypoints.Length;
                i++
            )
            {
                if (waypoints[i] == null)
                {
                    continue;
                }


                Gizmos.color =
                    Color.cyan;


                Gizmos.DrawSphere(
                    waypoints[i].position,
                    0.5f
                );


                int next =
                    (i + 1) %
                    waypoints.Length;


                if (waypoints[next] != null)
                {
                    Gizmos.DrawLine(
                        waypoints[i].position,
                        waypoints[next].position
                    );
                }
            }
        }
    }
}