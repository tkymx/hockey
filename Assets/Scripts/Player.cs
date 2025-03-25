using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float mass = 2.0f;
    [SerializeField] private float pushForce = 20.0f; // 押し出す力を増加
    
    [Header("Strike Settings")]
    [SerializeField] private float strikeForce = 10f;
    [SerializeField] private float strikeRadius = 2f;
    [SerializeField] private float strikeDelay = 0.5f;
    [SerializeField] private LayerMask puckLayer;

    private Vector3 targetPosition;
    private bool canStrike = true;
    private float strikeTimer = 0f;
    private Rigidbody rb;

    private void Awake()
    {
        targetPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // 物理設定
        rb.mass = mass;
        rb.linearDamping = 0.5f; // 空気抵抗を少し追加
        rb.angularDamping = 0.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false;
    }

    private void Update()
    {
        // プレイヤーの位置を即座に更新
        Vector3 currentPos = transform.position;
        transform.position = Vector3.Lerp(currentPos, new Vector3(targetPosition.x, currentPos.y, targetPosition.z), 0.5f);

        // 移動方向を向く
        Vector3 direction = (targetPosition - transform.position);
        if (direction.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction.normalized),
                0.3f
            );
        }

        // パックを打つクールダウン処理
        if (!canStrike)
        {
            strikeTimer -= Time.deltaTime;
            if (strikeTimer <= 0)
            {
                canStrike = true;
            }
        }

        // パックを打つ入力処理
        if (canStrike && Input.GetMouseButtonDown(1))
        {
            Strike();
        }
    }

    public void SetPosition(Vector3 position)
    {
        position.y = transform.position.y;
        transform.position = position;
        targetPosition = position;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void MoveTo(Vector3 position)
    {
        position.y = transform.position.y;
        targetPosition = position;
    }

    // パックを打つ処理
    public void Strike()
    {
        if (!canStrike) return;

        // 近くにあるパックを検索
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, strikeRadius, puckLayer);

        foreach (var hitCollider in hitColliders)
        {
            Puck puck = hitCollider.GetComponent<Puck>();
            if (puck != null)
            {
                // パックへの方向と力を計算
                Vector3 direction = (puck.transform.position - transform.position).normalized;
                
                // パックに力を加える
                Vector3 force = direction * strikeForce;
                puck.ApplyForce(force);
                
                // クールダウン設定
                canStrike = false;
                strikeTimer = strikeDelay;
                
                // 打った方向に少し回転させる
                Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
                
                break; // 最も近いパックだけ打つ
            }
        }
    }

    // プレイヤーの打撃力を取得
    public float GetStrikeForce()
    {
        return strikeForce;
    }

    // 打撃力を設定
    public void SetStrikeForce(float force)
    {
        strikeForce = force;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Puck puck = collision.gameObject.GetComponent<Puck>();
        if (puck != null)
        {
            // 衝突点を取得
            ContactPoint contact = collision.GetContact(0);
            Vector3 collisionNormal = contact.normal;
            
            // プレイヤーとパックの速度を取得
            Vector3 playerVelocity = rb.linearVelocity;
            Rigidbody puckRb = puck.GetComponent<Rigidbody>();
            Vector3 puckVelocity = puckRb.linearVelocity;
            
            // 衝突の強さを計算
            float restitution = 0.8f;
            Vector3 relativeVelocity = playerVelocity - puckVelocity;
            float normalVelocity = Vector3.Dot(relativeVelocity, collisionNormal);
            
            if (normalVelocity < 0)
            {
                // 運動量による衝突力を計算
                float j = -(1 + restitution) * normalVelocity;
                j /= (1 / mass) + (1 / puckRb.mass);
                Vector3 impulse = j * collisionNormal;
                
                // プレイヤーの移動方向の力を追加
                Vector3 playerDirection = (targetPosition - transform.position).normalized;
                Vector3 additionalForce = playerDirection * pushForce;
                
                // 衝突力を増幅して適用
                Vector3 totalForce = (impulse + additionalForce) * 25f; // 全体の力を2.5倍に増幅
                puck.ApplyForce(totalForce);
            }
        }
    }

    // 打撃範囲を可視化（デバッグ用）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, strikeRadius);
    }
}