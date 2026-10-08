using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore;

public class SecondVehicleController : MonoBehaviour
{
    // =========================================================
    // WHEEL COLLIDERS
    // =========================================================

    [Header("Wheel Colliders")]

    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;

    public WheelCollider backLeftWheel;
    public WheelCollider backRightWheel;


    // =========================================================
    // WHEEL MESHES
    // =========================================================

    [Header("Wheel Meshes")]

    public Transform frontLeftMesh;
    public Transform frontRightMesh;

    public Transform backLeftMesh;
    public Transform backRightMesh;


    // =========================================================
    // CAR SETTINGS
    // =========================================================

    [Header("Car Power, Brake, Steering Settings")]

    public float motorTorque = 1500f;

    public float steeringAngle = 30f;

    public float brakeForce = 2000f;


    // =========================================================
    // DRIFT SETTINGS
    // =========================================================

    [Header("Drift Settings")]

    [Tooltip("Normal rear tire sideways grip.")]
    public float normalRearGrip = 1.8f;

    [Tooltip("Rear tire sideways grip while drifting.")]
    public float driftRearGrip = 1.05f;

    [Tooltip("How quickly the rear tire grip changes.")]
    public float gripChangeSpeed = 1.5f;

    [Tooltip("Small rear brake force used during drift.")]
    public float driftBrakeForce = 300f;

    [Tooltip("How quickly the drift brake is applied.")]
    public float driftBrakeChangeSpeed = 500f;


    // =========================================================
    // FRONT GRIP
    // =========================================================

    [Header("Front Tire Grip")]

    public float normalFrontGrip = 1.75f;

    public float driftFrontGrip = 1.85f;


    // =========================================================
    // SMOOTH DRIFT POWER
    // =========================================================

    [Header("Smooth Drift Power")]

    [Tooltip("Maximum power multiplier during drift.")]
    public float driftPowerMultiplier = 1.05f;

    [Tooltip("How quickly power increases when drift starts.")]
    public float driftPowerIncreaseSpeed = 1.5f;

    [Tooltip("How quickly power returns to normal.")]
    public float driftPowerDecreaseSpeed = 2.5f;


    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    private float motorInput;

    private float steeringInput;

    private float currentBrakeForce;

    private float currentRearGrip;

    private float currentFrontGrip;

    private float currentDriftBrake;

    private float currentPowerMultiplier = 1f;

    private bool isDrifting;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentRearGrip = normalRearGrip;

        currentFrontGrip = normalFrontGrip;

        currentDriftBrake = 0f;

        currentPowerMultiplier = 1f;

        ApplyTireGrip();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // -----------------------------------------------------
        // INPUT
        // -----------------------------------------------------

        motorInput =
            Input.GetAxis("Vertical");

        steeringInput =
            Input.GetAxis("Horizontal");


        // -----------------------------------------------------
        // SHIFT = BRAKE
        // -----------------------------------------------------

        if (Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift))
        {
            currentBrakeForce =
                brakeForce;
        }
        else
        {
            currentBrakeForce = 0f;
        }


        // -----------------------------------------------------
        // SPACE = DRIFT
        // -----------------------------------------------------

        HandleDrift();


        // -----------------------------------------------------
        // UPDATE WHEEL MESHES
        // -----------------------------------------------------

        UpdateWheelPose();
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        // -----------------------------------------------------
        // MOTOR
        // -----------------------------------------------------

        ApplyMotor();


        // -----------------------------------------------------
        // STEERING
        // -----------------------------------------------------

        ApplySteering();


        // -----------------------------------------------------
        // BRAKES
        // -----------------------------------------------------

        ApplyBrake();
    }


    // =========================================================
    // MOTOR
    // =========================================================

    private void ApplyMotor()
    {
        // -----------------------------------------------------
        // SMOOTH DRIFT POWER
        // -----------------------------------------------------

        float targetPower;

        if (isDrifting)
        {
            targetPower =
                driftPowerMultiplier;
        }
        else
        {
            targetPower = 1f;
        }


        float powerChangeSpeed;

        if (isDrifting)
        {
            powerChangeSpeed =
                driftPowerIncreaseSpeed;
        }
        else
        {
            powerChangeSpeed =
                driftPowerDecreaseSpeed;
        }


        currentPowerMultiplier =
            Mathf.MoveTowards(
                currentPowerMultiplier,
                targetPower,
                powerChangeSpeed *
                Time.fixedDeltaTime
            );


        // -----------------------------------------------------
        // CALCULATE TORQUE
        // -----------------------------------------------------

        float torque =
            motorTorque *
            motorInput *
            currentPowerMultiplier;


        // -----------------------------------------------------
        // RWD
        // -----------------------------------------------------
        //
        // ONLY rear wheels receive engine power.
        //
        // This does NOT change during drift.
        //

        backLeftWheel.motorTorque =
            torque;

        backRightWheel.motorTorque =
            torque;


        // Front wheels are not powered.

        frontLeftWheel.motorTorque =
            0f;

        frontRightWheel.motorTorque =
            0f;
    }


    // =========================================================
    // STEERING
    // =========================================================

    private void ApplySteering()
    {
        float steering =
            steeringInput *
            steeringAngle;


        frontLeftWheel.steerAngle =
            steering;

        frontRightWheel.steerAngle =
            steering;
    }


    // =========================================================
    // DRIFT
    // =========================================================

    private void HandleDrift()
    {
        // -----------------------------------------------------
        // SPACE = DRIFT
        // -----------------------------------------------------
        //
        // NO SPEED CHECK.
        //
        // NO ACCELERATION CHECK.
        //
        // Space alone controls the drift mode.
        //

        bool spacePressed =
            Input.GetKey(KeyCode.Space);


        isDrifting =
            spacePressed;


        // =====================================================
        // REAR GRIP
        // =====================================================

        float targetRearGrip;

        if (isDrifting)
        {
            targetRearGrip =
                driftRearGrip;
        }
        else
        {
            targetRearGrip =
                normalRearGrip;
        }


        currentRearGrip =
            Mathf.MoveTowards(
                currentRearGrip,
                targetRearGrip,
                gripChangeSpeed *
                Time.deltaTime
            );


        // =====================================================
        // FRONT GRIP
        // =====================================================

        float targetFrontGrip;

        if (isDrifting)
        {
            targetFrontGrip =
                driftFrontGrip;
        }
        else
        {
            targetFrontGrip =
                normalFrontGrip;
        }


        currentFrontGrip =
            Mathf.MoveTowards(
                currentFrontGrip,
                targetFrontGrip,
                gripChangeSpeed *
                Time.deltaTime
            );


        // =====================================================
        // DRIFT BRAKE
        // =====================================================

        float targetDriftBrake;

        if (isDrifting)
        {
            targetDriftBrake =
                driftBrakeForce;
        }
        else
        {
            targetDriftBrake =
                0f;
        }


        currentDriftBrake =
            Mathf.MoveTowards(
                currentDriftBrake,
                targetDriftBrake,
                driftBrakeChangeSpeed *
                Time.deltaTime
            );


        // =====================================================
        // APPLY DRIFT BRAKE
        // =====================================================

        if (isDrifting)
        {
            backLeftWheel.brakeTorque =
                currentDriftBrake;

            backRightWheel.brakeTorque =
                currentDriftBrake;
        }


        // =====================================================
        // APPLY TIRE GRIP
        // =====================================================

        ApplyTireGrip();
    }


    // =========================================================
    // TIRE GRIP
    // =========================================================

    private void ApplyTireGrip()
    {
        // -----------------------------------------------------
        // FRONT TIRES
        // -----------------------------------------------------

        WheelFrictionCurve frontFriction =
            frontLeftWheel.sidewaysFriction;


        frontFriction.stiffness =
            currentFrontGrip;


        frontLeftWheel.sidewaysFriction =
            frontFriction;

        frontRightWheel.sidewaysFriction =
            frontFriction;


        // -----------------------------------------------------
        // REAR TIRES
        // -----------------------------------------------------

        WheelFrictionCurve rearFriction =
            backLeftWheel.sidewaysFriction;


        rearFriction.stiffness =
            currentRearGrip;


        backLeftWheel.sidewaysFriction =
            rearFriction;

        backRightWheel.sidewaysFriction =
            rearFriction;
    }


    // =========================================================
    // BRAKE
    // =========================================================

    private void ApplyBrake()
    {
        // -----------------------------------------------------
        // SHIFT BRAKE
        // -----------------------------------------------------

        if (currentBrakeForce > 0f)
        {
            frontLeftWheel.brakeTorque =
                currentBrakeForce;

            frontRightWheel.brakeTorque =
                currentBrakeForce;

            backLeftWheel.brakeTorque =
                currentBrakeForce;

            backRightWheel.brakeTorque =
                currentBrakeForce;
        }
        else
        {
            // Front brakes off.

            frontLeftWheel.brakeTorque =
                0f;

            frontRightWheel.brakeTorque =
                0f;


            // -------------------------------------------------
            // REAR BRAKES
            // -------------------------------------------------
            //
            // Don't remove the progressive drift brake.
            //

            if (!isDrifting)
            {
                backLeftWheel.brakeTorque =
                    0f;

                backRightWheel.brakeTorque =
                    0f;
            }
        }
    }


    // =========================================================
    // WHEEL POSE
    // =========================================================

    private void UpdateWheelPose()
    {
        UpdateSingleWheel(
            frontLeftWheel,
            frontLeftMesh
        );

        UpdateSingleWheel(
            frontRightWheel,
            frontRightMesh
        );

        UpdateSingleWheel(
            backLeftWheel,
            backLeftMesh
        );

        UpdateSingleWheel(
            backRightWheel,
            backRightMesh
        );
    }


    // =========================================================
    // SINGLE WHEEL POSE
    // =========================================================

    private void UpdateSingleWheel(
        WheelCollider wheel,
        Transform mesh)
    {
        if (wheel == null ||
            mesh == null)
        {
            return;
        }


        Vector3 position;

        Quaternion rotation;


        wheel.GetWorldPose(
            out position,
            out rotation
        );


        mesh.position =
            position;

        mesh.rotation =
            rotation;
    }



}
