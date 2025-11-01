using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Stage 1 - 완벽한 자동차
/// WASD로 자동 제어되는 일반 자동차
/// </summary>
public class StageOneCar : MonoBehaviour
{
    [Header("Wheel Transforms")]
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelRL;
    public Transform wheelRR;

    [Header("Car Settings")]
    [Tooltip("자동차 가속 힘")]
    public float motorTorque = 3000f;

    [Tooltip("회전 힘")]
    public float turnTorque = 2000f;

    [Tooltip("최대 속도")]
    public float maxSpeed = 30f;

    [Header("Physics")]
    public float sidewaysFriction = 4f;
    public float forwardFriction = 3f;
    public float groundCheckDistance = 0.5f;
    public float wheelWeight = 0.25f;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    private Rigidbody rb;
    private CarInputActions inputActions;

    // WASD 입력
    private float throttleInput = 0f;  // W/S
    private float steerInput = 0f;     // A/D


    private void Reset()
    {
        wheelFL = this.TryFindChild("wheelFL").transform;
        wheelFR = this.TryFindChild("wheelFR").transform;
        wheelRL = this.TryFindChild("wheelRL").transform;
        wheelRR = this.TryFindChild("wheelRR").transform;
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new CarInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();

        // WASD 바인딩
        // W = 전진
        inputActions.Car.WheelFR_Forward.performed += ctx => throttleInput = 1f;
        inputActions.Car.WheelFR_Forward.canceled += ctx => throttleInput = 0f;

        // S = 후진
        inputActions.Car.WheelFR_Backward.performed += ctx => throttleInput = -1f;
        inputActions.Car.WheelFR_Backward.canceled += ctx => throttleInput = 0f;

        // A = 좌회전
        inputActions.Car.WheelFL_Forward.performed += ctx => steerInput = -1f;
        inputActions.Car.WheelFL_Forward.canceled += ctx => steerInput = 0f;

        // D = 우회전
        inputActions.Car.WheelFR_Backward.performed += ctx => steerInput = 1f;
        inputActions.Car.WheelFR_Backward.canceled += ctx => steerInput = 0f;
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void FixedUpdate()
    {
        // 속도 제한
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // 4개 바퀴 모두에 물리 적용
        ApplyAutomatedControl(wheelFL);
        ApplyAutomatedControl(wheelFR);
        ApplyAutomatedControl(wheelRL);
        ApplyAutomatedControl(wheelRR);
    }

    void ApplyAutomatedControl(Transform wheel)
    {
        if (wheel == null) return;

        // 지면 체크
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(wheel.position, -transform.up, out hit, groundCheckDistance);
        if (!isGrounded) return;

        // === 1. 전진/후진 힘 (자동) ===
        Vector3 forwardDir = transform.forward;
        float forwardForce = throttleInput * motorTorque;
        rb.AddForceAtPosition(forwardDir * forwardForce, wheel.position);

        // === 2. 회전 힘 (자동) ===
        // 좌회전(A): 왼쪽 바퀴 감속, 오른쪽 바퀴 가속
        // 우회전(D): 오른쪽 바퀴 감속, 왼쪽 바퀴 가속
        float turnMultiplier = 0f;

        if (wheel == wheelFL || wheel == wheelRL)
        {
            // 왼쪽 바퀴
            turnMultiplier = -steerInput;  // A누르면 -1
        }
        else if (wheel == wheelFR || wheel == wheelRR)
        {
            // 오른쪽 바퀴
            turnMultiplier = steerInput;   // D누르면 +1
        }

        float turnForce = turnMultiplier * turnTorque;
        rb.AddForceAtPosition(forwardDir * turnForce, wheel.position);

        // === 3. 횡방향 마찰 ===
        Vector3 sidewaysDir = transform.right;
        Vector3 wheelVelocity = rb.GetPointVelocity(wheel.position);
        float sidewaysVelocity = Vector3.Dot(wheelVelocity, sidewaysDir);
        float lateralFrictionForce = -sidewaysVelocity * sidewaysFriction * rb.mass * wheelWeight;
        rb.AddForceAtPosition(sidewaysDir * lateralFrictionForce, wheel.position);

        // === 4. 종방향 마찰 ===
        if (Mathf.Approximately(throttleInput, 0f))
        {
            float forwardVelocity = Vector3.Dot(wheelVelocity, forwardDir);
            float forwardFrictionForce = -forwardVelocity * forwardFriction * rb.mass * wheelWeight;
            rb.AddForceAtPosition(forwardDir * forwardFrictionForce, wheel.position);
        }

        // === 디버그 ===
        if (showDebugGizmos)
        {
            if (!Mathf.Approximately(throttleInput, 0f))
            {
                Debug.DrawRay(wheel.position, forwardDir * throttleInput * 2f, Color.green);
            }
            Debug.DrawRay(wheel.position, -transform.up * groundCheckDistance, Color.yellow);
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = Color.cyan;
        if (wheelFL) Gizmos.DrawWireSphere(wheelFL.position, 0.15f);
        if (wheelFR) Gizmos.DrawWireSphere(wheelFR.position, 0.15f);
        if (wheelRL) Gizmos.DrawWireSphere(wheelRL.position, 0.15f);
        if (wheelRR) Gizmos.DrawWireSphere(wheelRR.position, 0.15f);

        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}