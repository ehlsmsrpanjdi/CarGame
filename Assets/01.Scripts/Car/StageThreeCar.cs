using UnityEngine;

public class StageThreeCar : MonoBehaviour
{
    [Header("Wheel Transforms")]
    public Transform wheelFL; // Front Left
    public Transform wheelFR; // Front Right
    public Transform wheelRL; // Rear Left
    public Transform wheelRR; // Rear Right

    [Header("Wheel Physics Settings")]
    [Tooltip("각 바퀴의 기본 구동력")]
    public float motorTorque = 3000f;

    [Tooltip("바퀴 1개만 작동 시 힘 배율 (1.0보다 크면 개별 바퀴가 더 강함)")]
    public float singleWheelMultiplier = 1.5f;

    [Tooltip("전체 최대 힘 제한")]
    public float maxTotalForce = 4000f;

    [Tooltip("종방향 마찰 (앞뒤 미끄러짐 방지)")]
    public float forwardFriction = 10f;

    [Tooltip("횡방향 마찰 (좌우 미끄러짐 방지, 높을수록 회전이 강함)")]
    public float sidewaysFriction = 10f;

    [Tooltip("최대 속도 제한")]
    public float maxSpeed = 15f;

    [Tooltip("지면 감지 거리")]
    public float groundCheckDistance = 0.5f;

    [Header("Weight Distribution")]
    [Tooltip("각 바퀴에 실리는 무게 비율 (전체 합 = 1.0)")]
    public float wheelWeight = 0.25f;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    private Rigidbody rb;
    private CarInputActions inputActions;

    // 각 바퀴의 현재 입력 값
    private float inputFL = 0f;
    private float inputFR = 0f;
    private float inputRL = 0f;
    private float inputRR = 0f;

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

        // ✅ 앞바퀴만 입력 받기
        inputActions.Car.WheelFL_Forward.performed += ctx => inputFL = 1f;
        inputActions.Car.WheelFL_Forward.canceled += ctx => inputFL = 0f;
        inputActions.Car.WheelFL_Backward.performed += ctx => inputFL = -1f;
        inputActions.Car.WheelFL_Backward.canceled += ctx => inputFL = 0f;

        inputActions.Car.WheelFR_Forward.performed += ctx => inputFR = 1f;
        inputActions.Car.WheelFR_Forward.canceled += ctx => inputFR = 0f;
        inputActions.Car.WheelFR_Backward.performed += ctx => inputFR = -1f;
        inputActions.Car.WheelFR_Backward.canceled += ctx => inputFR = 0f;

        // 뒷바퀴 입력 없음!
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void FixedUpdate()
    {
        // 최대 속도 제한
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // 활성 바퀴 개수 계산
        int activeWheels = 0;
        if (!Mathf.Approximately(inputFL, 0f)) activeWheels++;
        if (!Mathf.Approximately(inputFR, 0f)) activeWheels++;
        if (!Mathf.Approximately(inputRL, 0f)) activeWheels++;
        if (!Mathf.Approximately(inputRR, 0f)) activeWheels++;

        // 바퀴 개수에 따른 힘 스케일 계산
        float forceScale = CalculateForceScale(activeWheels);

        // 각 바퀴에 물리 적용
        ApplyWheelPhysics(wheelFL, inputFL, forceScale);
        ApplyWheelPhysics(wheelFR, inputFR, forceScale);
        ApplyWheelPhysics(wheelRL, inputRL, forceScale);
        ApplyWheelPhysics(wheelRR, inputRR, forceScale);
    }

    // 바퀴 개수에 따른 힘 스케일 계산
    float CalculateForceScale(int activeWheels)
    {
        if (activeWheels == 0) return 0f;
        if (activeWheels == 1) return singleWheelMultiplier;

        // 여러 바퀴가 작동 시: 전체 힘이 maxTotalForce를 넘지 않도록 조정
        float totalForce = motorTorque * singleWheelMultiplier * activeWheels;
        if (totalForce > maxTotalForce)
        {
            return maxTotalForce / (motorTorque * activeWheels);
        }

        return singleWheelMultiplier;
    }

    void ApplyWheelPhysics(Transform wheel, float input, float forceScale)
    {
        if (wheel == null) return;


        // Raycast로 지면 확인
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(wheel.position, -transform.up, out hit, groundCheckDistance);

        if (!isGrounded) return;

        // === 1. 종방향 힘 (Forward Force) ===
        // 바퀴가 구르는 방향으로 힘 적용 (forceScale 적용)
        Vector3 forwardDir = transform.forward;
        float forwardForce = input * motorTorque * forceScale;

        // 바퀴 위치에서 차체에 힘 적용 (회전 모멘트 자동 생성)
        rb.AddForceAtPosition(forwardDir * forwardForce, wheel.position);

        // === 2. 횡방향 마찰 (Sideways Friction) ===
        // 바퀴가 옆으로 미끄러지는 것을 방지
        Vector3 sidewaysDir = transform.right;

        // 바퀴의 횡방향 속도 계산
        Vector3 wheelVelocity = rb.GetPointVelocity(wheel.position);
        float sidewaysVelocity = Vector3.Dot(wheelVelocity, sidewaysDir);

        // 횡방향 마찰력 = 미끄러지는 속도에 반비례하는 힘
        float lateralFrictionForce = -sidewaysVelocity * sidewaysFriction * rb.mass * wheelWeight;
        rb.AddForceAtPosition(sidewaysDir * lateralFrictionForce, wheel.position);


        // === 3. 종방향 마찰/저항 (Forward Friction) ===
        // 바퀴가 구르지 않을 때 (input == 0) 앞뒤 마찰 적용
        if (Mathf.Approximately(input, 0f))
        {
            float forwardVelocity = Vector3.Dot(wheelVelocity, forwardDir);
            float forwardFrictionForce = -forwardVelocity * forwardFriction * rb.mass * wheelWeight;
            rb.AddForceAtPosition(forwardDir * forwardFrictionForce, wheel.position);
        }

        // === 디버그 시각화 ===
        if (showDebugGizmos)
        {
            // 종방향 힘 (초록색)
            if (!Mathf.Approximately(input, 0f))
            {
                Debug.DrawRay(wheel.position, forwardDir * input * 2f, Color.green);
            }

            // 횡방향 마찰 (빨간색)
            if (Mathf.Abs(sidewaysVelocity) > 0.1f)
            {
                Debug.DrawRay(wheel.position, sidewaysDir * -sidewaysVelocity * 0.5f, Color.red);
            }

            // 지면 체크 (노란색)
            Debug.DrawRay(wheel.position, -transform.up * groundCheckDistance, Color.yellow);
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // 바퀴 위치 표시
        Gizmos.color = Color.cyan;
        if (wheelFL) Gizmos.DrawWireSphere(wheelFL.position, 0.15f);
        if (wheelFR) Gizmos.DrawWireSphere(wheelFR.position, 0.15f);
        if (wheelRL) Gizmos.DrawWireSphere(wheelRL.position, 0.15f);
        if (wheelRR) Gizmos.DrawWireSphere(wheelRR.position, 0.15f);

        // 차체의 forward, right 방향 표시
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right * 2f);
        }
    }
}