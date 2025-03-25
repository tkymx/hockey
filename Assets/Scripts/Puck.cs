using UnityEngine;

public class Puck : MonoBehaviour
{
    [Header("Puck Properties")]
    [SerializeField] private float mass = 1.0f;
    [SerializeField] private float frictionCoefficient = 0.95f;
    [SerializeField] private float maxSpeed = 20.0f;

    private Vector3 velocity = Vector3.zero;
    private bool isMoving = false;
    private Rigidbody rb;
    private SphereCollider sphereCollider;
    
    public float StrikeForceMultiplier { get; set; } = 10.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        
        // パックの物理設定
        rb.mass = mass;
        rb.linearDamping = 0;
        rb.angularDamping = 0.1f;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        // 当たり判定の設定
        sphereCollider.radius = transform.localScale.x / 2;
        sphereCollider.material = new PhysicsMaterial
        {
            bounciness = 0.8f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };
    }
    
    private void FixedUpdate()
    {
        // 現在の速度をチェックして摩擦を適用
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            isMoving = true;
            ApplyFriction();
        }
        else
        {
            if (isMoving)
            {
                rb.linearVelocity = Vector3.zero;
                isMoving = false;
            }
        }
    }
    
    public void ApplyForce(Vector3 force)
    {
        // 力を適用する前に既存の速度をリセット
        rb.linearVelocity = Vector3.zero;
        
        // 力を適用
        rb.AddForce(new Vector3(force.x, 0, force.z), ForceMode.Impulse);
        
        // 最大速度を制限
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        
        isMoving = true;
    }
    
    private void ApplyFriction()
    {
        // 摩擦係数を適用して速度を減衰
        rb.linearVelocity *= frictionCoefficient;
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public Vector3 GetVelocity()
    {
        return rb.linearVelocity;
    }
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    public void Reset(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector3.zero;
        isMoving = false;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // ここでは衝突イベントを検知するだけで、
        // 実際の処理は他のコンポーネント（DestructibleObjectなど）で行う
    }
}