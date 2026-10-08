using UnityEngine;

public class VehicleMovement : MonoBehaviour
{
    [Header("=== WHEEL COLLIDERS ===")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("=== WHEEL MESHES ===")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;


    // =========================================================
    // ENGINE
    // =========================================================

    [Header("=== ENGINE ===")]

    public float motorTorque = 1600f;

    public float reverseTorque = 700f;

    public float maxSpeed = 160f;

    public float maxReverseSpeed = 35f;


    // =========================================================
    // STEERING
    // =========================================================

    [Header("=== STEERING ===")]

    public float maxSteerAngle = 32f;

    public float highSpeedSteerAngle = 12f;

    public float steeringReductionSpeed = 100f;

    public float steeringSmoothness = 8f;


    // =========================================================
    // BRAKES
    // =========================================================

    [Header("=== BRAKES ===")]

    public float normalBrakeTorque = 4500f;


    // =========================================================
    // DRIFT
    // =========================================================

    [Header("=== DRIFT ===")]

    public bool enableDrift = true;

    [Tooltip("Minimum speed required to start drifting.")]
    public float minimumDriftSpeed = 20f;

    [Tooltip("Normal rear tire lateral grip.")]
    public float rearNormalGrip = 1.8f;

    [Tooltip("Rear tire lateral grip during drift.")]
    public float rearDriftGrip = 1.05f;

    [Tooltip("Normal front tire lateral grip.")]
    public float frontNormalGrip = 1.75f;

    [Tooltip("Front tire lateral grip during drift.")]
    public float frontDriftGrip = 1.85f;

    [Tooltip("Small rear brake used to initiate the slide.")]
    public float driftBrakeTorque = 500f;

    [Tooltip("How quickly drift brake reaches its target.")]
    public float driftBrakeTransitionSpeed = 700f;

    [Tooltip("How quickly tire grip changes.")]
    public float driftGripTransitionSpeed = 1.5f;


    // =========================================================
    // SMOOTH DRIFT POWER
    // =========================================================

    [Header("=== SMOOTH DRIFT POWER ===")]

    [Tooltip("Maximum engine power multiplier during drift.")]
    public float driftPowerMultiplier = 1.05f;

    [Tooltip("How quickly power increases after starting drift.")]
    public float driftPowerIncreaseSpeed = 2.0f;

    [Tooltip("How quickly power returns to normal.")]
    public float driftPowerDecreaseSpeed = 3.0f;


    // =========================================================
    // TIRE FRICTION
    // =========================================================

    [Header("=== FRONT TIRE FRICTION ===")]

    public float frontExtremumSlip = 0.35f;

    public float frontExtremumValue = 1f;

    public float frontAsymptoteSlip = 0.8f;

    public float frontAsymptoteValue = 0.75f;


    [Header("=== REAR TIRE FRICTION ===")]

    public float rearExtremumSlip = 0.4f;

    public float rearExtremumValue = 1f;

    public float rearAsymptoteSlip = 0.9f;

    public float rearAsymptoteValue = 0.7f;


    // =========================================================
    // FORWARD GRIP
    // =========================================================

    [Header("=== FORWARD TIRE GRIP ===")]

    [Tooltip("Front longitudinal grip.")]
    public float frontForwardGrip = 1.4f;

    [Tooltip("Rear longitudinal grip. Keeps RWD power during drift.")]
    public float rearForwardGrip = 1.45f;


    // =========================================================
    // STABILITY
    // =========================================================

    [Header("=== STABILITY ===")]

    public float downforce = 80f;

    public float extraGravity = 20f;


    // =========================================================
    // ENGINE AUDIO
    // =========================================================

    [Header("=== ENGINE AUDIO ===")]

    public AudioSource engineAudio;

    public float minEnginePitch = 0.8f;

    public float maxEnginePitch = 2f;


    // =========================================================
    // DEBUG
    // =========================================================

    [Header("=== DEBUG ===")]

    public bool showDebug = true;


    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    private Rigidbody rb;

    private float throttleInput;

    private float steeringInput;

    private float currentSteerAngle;

    private float currentRearGrip;

    private float currentFrontGrip;

    private float currentDriftBrake;

    private float currentDriftPower = 1f;

    private float speedKmh;

    private bool isDrifting;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // IMPORTANT:
        // No center-of-mass modification here.
        //
        // You control the Rigidbody center of mass
        // manually or use Automatic Center Of Mass.
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentRearGrip = rearNormalGrip;

        currentFrontGrip = frontNormalGrip;

        currentDriftBrake = 0f;

        currentDriftPower = 1f;

        ApplyTireFriction();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        ReadInput();

        UpdateWheelVisuals();

        UpdateEngineAudio();
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        CalculateSpeed();

        HandleSteering();

        HandleDrift();

        HandleDrive();

        ApplyDownforce();

        ApplyExtraGravity();
    }


    // =========================================================
    // INPUT
    // =========================================================

    private void ReadInput()
    {
        throttleInput = Input.GetAxis("Vertical");

        steeringInput = Input.GetAxis("Horizontal");
    }


    // =========================================================
    // SPEED
    // =========================================================

    private void CalculateSpeed()
    {
        speedKmh = rb.linearVelocity.magnitude * 3.6f;
    }


    // =========================================================
    // STEERING
    // =========================================================

    private void HandleSteering()
    {
        float speedFactor = Mathf.InverseLerp(
            0f,
            steeringReductionSpeed,
            speedKmh
        );

        float availableSteering = Mathf.Lerp(
            maxSteerAngle,
            highSpeedSteerAngle,
            speedFactor
        );

        float targetSteering =
            steeringInput * availableSteering;

        currentSteerAngle = Mathf.Lerp(
            currentSteerAngle,
            targetSteering,
            steeringSmoothness * Time.fixedDeltaTime
        );

        frontLeft.steerAngle = currentSteerAngle;

        frontRight.steerAngle = currentSteerAngle;
    }


    // =========================================================
    // DRIFT
    // =========================================================

    private void HandleDrift()
    {
        bool spacePressed =
            Input.GetKey(KeyCode.Space);

        bool enoughSpeed =
            speedKmh >= minimumDriftSpeed;

        bool hasThrottle =
            Mathf.Abs(throttleInput) > 0.1f;

        bool canDrift =
            enableDrift &&
            spacePressed &&
            enoughSpeed &&
            hasThrottle;


        // -----------------------------------------
        // DRIFT STATE
        // -----------------------------------------

        if (canDrift)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }


        // -----------------------------------------
        // TARGET REAR GRIP
        // -----------------------------------------

        float targetRearGrip =
            isDrifting
                ? rearDriftGrip
                : rearNormalGrip;


        // -----------------------------------------
        // TARGET FRONT GRIP
        // -----------------------------------------

        float targetFrontGrip =
            isDrifting
                ? frontDriftGrip
                : frontNormalGrip;


        // -----------------------------------------
        // SMOOTH REAR GRIP
        // -----------------------------------------

        currentRearGrip = Mathf.MoveTowards(
            currentRearGrip,
            targetRearGrip,
            driftGripTransitionSpeed *
            Time.fixedDeltaTime
        );


        // -----------------------------------------
        // SMOOTH FRONT GRIP
        // -----------------------------------------

        currentFrontGrip = Mathf.MoveTowards(
            currentFrontGrip,
            targetFrontGrip,
            driftGripTransitionSpeed *
            Time.fixedDeltaTime
        );


        // -----------------------------------------
        // DRIFT BRAKE
        // -----------------------------------------

        float targetDriftBrake =
            isDrifting
                ? driftBrakeTorque
                : 0f;


        currentDriftBrake = Mathf.MoveTowards(
            currentDriftBrake,
            targetDriftBrake,
            driftBrakeTransitionSpeed *
            Time.fixedDeltaTime
        );


        // -----------------------------------------
        // SMOOTH DRIFT POWER
        // -----------------------------------------

        float targetDriftPower =
            isDrifting
                ? driftPowerMultiplier
                : 1f;


        float powerTransitionSpeed =
            isDrifting
                ? driftPowerIncreaseSpeed
                : driftPowerDecreaseSpeed;


        currentDriftPower = Mathf.MoveTowards(
            currentDriftPower,
            targetDriftPower,
            powerTransitionSpeed *
            Time.fixedDeltaTime
        );


        // -----------------------------------------
        // APPLY DRIFT BRAKE
        // -----------------------------------------

        if (isDrifting && throttleInput >= 0f)
        {
            rearLeft.brakeTorque =
                currentDriftBrake;

            rearRight.brakeTorque =
                currentDriftBrake;
        }
        else if (!isDrifting)
        {
            if (throttleInput >= -0.05f)
            {
                rearLeft.brakeTorque = 0f;

                rearRight.brakeTorque = 0f;
            }
        }


        // -----------------------------------------
        // UPDATE FRICTION
        // -----------------------------------------

        ApplyTireFriction();
    }


    // =========================================================
    // DRIVE
    // =========================================================

    private void HandleDrive()
    {
        float forwardSpeed =
            GetSignedForwardSpeed();


        // =====================================================
        // FORWARD
        // =====================================================

        if (throttleInput > 0.05f)
        {
            if (speedKmh < maxSpeed)
            {
                // -----------------------------------------
                // RWD
                // -----------------------------------------
                //
                // Rear wheels ALWAYS receive the power.
                //
                // During drift, the power increases
                // gradually through currentDriftPower.
                //

                float torque =
                    motorTorque *
                    currentDriftPower;


                rearLeft.motorTorque =
                    torque * throttleInput;

                rearRight.motorTorque =
                    torque * throttleInput;


                // No normal braking while accelerating.

                frontLeft.brakeTorque = 0f;

                frontRight.brakeTorque = 0f;


                // Drift brake is controlled by HandleDrift().
                if (!isDrifting)
                {
                    rearLeft.brakeTorque = 0f;

                    rearRight.brakeTorque = 0f;
                }
            }
            else
            {
                rearLeft.motorTorque = 0f;

                rearRight.motorTorque = 0f;
            }

            return;
        }


        // =====================================================
        // BRAKE / REVERSE
        // =====================================================

        if (throttleInput < -0.05f)
        {
            // -----------------------------------------
            // MOVING FORWARD = BRAKE
            // -----------------------------------------

            if (forwardSpeed > 1f)
            {
                rearLeft.motorTorque = 0f;

                rearRight.motorTorque = 0f;


                frontLeft.brakeTorque =
                    normalBrakeTorque;

                frontRight.brakeTorque =
                    normalBrakeTorque;

                rearLeft.brakeTorque =
                    normalBrakeTorque;

                rearRight.brakeTorque =
                    normalBrakeTorque;
            }

            // -----------------------------------------
            // ALMOST STOPPED = REVERSE
            // -----------------------------------------

            else
            {
                frontLeft.brakeTorque = 0f;

                frontRight.brakeTorque = 0f;

                rearLeft.brakeTorque = 0f;

                rearRight.brakeTorque = 0f;


                if (speedKmh < maxReverseSpeed)
                {
                    rearLeft.motorTorque =
                        -reverseTorque *
                        Mathf.Abs(throttleInput);

                    rearRight.motorTorque =
                        -reverseTorque *
                        Mathf.Abs(throttleInput);
                }
                else
                {
                    rearLeft.motorTorque = 0f;

                    rearRight.motorTorque = 0f;
                }
            }

            return;
        }


        // =====================================================
        // NO THROTTLE
        // =====================================================

        rearLeft.motorTorque = 0f;

        rearRight.motorTorque = 0f;

        frontLeft.brakeTorque = 0f;

        frontRight.brakeTorque = 0f;


        if (!isDrifting)
        {
            rearLeft.brakeTorque = 0f;

            rearRight.brakeTorque = 0f;
        }
    }


    // =========================================================
    // TIRE FRICTION
    // =========================================================

    private void ApplyTireFriction()
    {
        // =====================================================
        // FRONT SIDEWAYS FRICTION
        // =====================================================

        WheelFrictionCurve frontSideways =
            frontLeft.sidewaysFriction;


        frontSideways.extremumSlip =
            frontExtremumSlip;

        frontSideways.extremumValue =
            frontExtremumValue;

        frontSideways.asymptoteSlip =
            frontAsymptoteSlip;

        frontSideways.asymptoteValue =
            frontAsymptoteValue;

        frontSideways.stiffness =
            currentFrontGrip;


        frontLeft.sidewaysFriction =
            frontSideways;

        frontRight.sidewaysFriction =
            frontSideways;


        // =====================================================
        // FRONT FORWARD FRICTION
        // =====================================================

        WheelFrictionCurve frontForward =
            frontLeft.forwardFriction;

        frontForward.stiffness =
            frontForwardGrip;

        frontLeft.forwardFriction =
            frontForward;

        frontRight.forwardFriction =
            frontForward;


        // =====================================================
        // REAR SIDEWAYS FRICTION
        // =====================================================

        WheelFrictionCurve rearSideways =
            rearLeft.sidewaysFriction;


        rearSideways.extremumSlip =
            rearExtremumSlip;

        rearSideways.extremumValue =
            rearExtremumValue;

        rearSideways.asymptoteSlip =
            rearAsymptoteSlip;

        rearSideways.asymptoteValue =
            rearAsymptoteValue;

        rearSideways.stiffness =
            currentRearGrip;


        rearLeft.sidewaysFriction =
            rearSideways;

        rearRight.sidewaysFriction =
            rearSideways;


        // =====================================================
        // REAR FORWARD FRICTION
        // =====================================================
        //
        // IMPORTANT:
        //
        // We do NOT reduce this during drift.
        //
        // This allows the rear wheels to continue
        // pushing the car forward.
        //

        WheelFrictionCurve rearForward =
            rearLeft.forwardFriction;

        rearForward.stiffness =
            rearForwardGrip;

        rearLeft.forwardFriction =
            rearForward;

        rearRight.forwardFriction =
            rearForward;
    }


    // =========================================================
    // DOWNFORCE
    // =========================================================

    private void ApplyDownforce()
    {
        float speed =
            rb.linearVelocity.magnitude;


        if (speed > 1f)
        {
            rb.AddForce(
                -transform.up *
                speed *
                downforce,
                ForceMode.Force
            );
        }
    }


    // =========================================================
    // EXTRA GRAVITY
    // =========================================================

    private void ApplyExtraGravity()
    {
        rb.AddForce(
            Vector3.down *
            extraGravity,
            ForceMode.Force
        );
    }


    // =========================================================
    // FORWARD SPEED
    // =========================================================

    private float GetSignedForwardSpeed()
    {
        return Vector3.Dot(
            rb.linearVelocity,
            transform.forward
        ) * 3.6f;
    }


    // =========================================================
    // WHEEL VISUALS
    // =========================================================

    private void UpdateWheelVisuals()
    {
        UpdateSingleWheel(
            frontLeft,
            frontLeftMesh
        );

        UpdateSingleWheel(
            frontRight,
            frontRightMesh
        );

        UpdateSingleWheel(
            rearLeft,
            rearLeftMesh
        );

        UpdateSingleWheel(
            rearRight,
            rearRightMesh
        );
    }


    private void UpdateSingleWheel(
        WheelCollider wheelCollider,
        Transform wheelMesh
    )
    {
        if (wheelCollider == null ||
            wheelMesh == null)
        {
            return;
        }


        Vector3 position;

        Quaternion rotation;


        wheelCollider.GetWorldPose(
            out position,
            out rotation
        );


        wheelMesh.position =
            position;

        wheelMesh.rotation =
            rotation;
    }


    // =========================================================
    // ENGINE AUDIO
    // =========================================================

    private void UpdateEngineAudio()
    {
        if (engineAudio == null)
            return;


        float normalizedSpeed =
            Mathf.Clamp01(
                speedKmh / maxSpeed
            );


        float targetPitch =
            Mathf.Lerp(
                minEnginePitch,
                maxEnginePitch,
                normalizedSpeed
            );


        if (Mathf.Abs(throttleInput) > 0.1f)
        {
            targetPitch += 0.15f;
        }


        engineAudio.pitch =
            Mathf.Lerp(
                engineAudio.pitch,
                targetPitch,
                Time.deltaTime * 5f
            );
    }


    // =========================================================
    // DEBUG
    // =========================================================

    private void OnGUI()
    {
        if (!showDebug)
            return;


        GUI.Label(
            new Rect(20, 20, 350, 25),
            "Speed: " +
            speedKmh.ToString("0") +
            " km/h"
        );


        GUI.Label(
            new Rect(20, 45, 350, 25),
            "Rear Grip: " +
            currentRearGrip.ToString("0.00")
        );


        GUI.Label(
            new Rect(20, 70, 350, 25),
            "Drift Power: " +
            currentDriftPower.ToString("0.00")
        );


        GUI.Label(
            new Rect(20, 95, 350, 25),
            "Drifting: " +
            isDrifting
        );
    }
}