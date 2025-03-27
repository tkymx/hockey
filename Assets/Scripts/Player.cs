using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float mass = 2.0f;
    [SerializeField] private float collisionForceMultiplier = 2.5f;
    [SerializeField] private float positionSmoothTime = 0.05f; // 位置の補間時間

    private Vector3 targetPosition;
    private Vector3 previousPosition;
    private Vector3 currentVelocity;
    private Vector3 smoothVelocity;
    private Rigidbody rb;

    private void Awake()
    {
        targetPosition = transform.position;
        previousPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // 物理設定
        rb.mass = mass;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        // マウス位置への移動
        Vector3 newPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref smoothVelocity,
            positionSmoothTime
        );
        
        // 現在のフレームでの速度を計算
        currentVelocity = (newPosition - previousPosition) / Time.deltaTime;
        
        // 位置を更新
        transform.position = newPosition;
        
        // 移動方向を向く
        if (currentVelocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(currentVelocity.normalized),
                0.3f
            );
        }

        // 前フレームの位置を更新
        previousPosition = transform.position;
    }

    public void SetPosition(Vector3 position)
    {
        position.y = transform.position.y;
        transform.position = position;
        previousPosition = position;
        targetPosition = position;
        currentVelocity = Vector3.zero;
        smoothVelocity = Vector3.zero;
    }

    public void MoveTo(Vector3 position)
    {
        position.y = transform.position.y;
        targetPosition = position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Puck puck = collision.gameObject.GetComponent<Puck>();
        if (puck != null)
        {
            // 現在の速度から衝突力を計算
            Vector3 collisionForce = currentVelocity * collisionForceMultiplier * mass;
            
            // パックに力を適用
            puck.ApplyForce(collisionForce);
        }
    }
}