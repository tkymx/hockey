using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float mass = 2.0f;
    [SerializeField] private float pushForce = 5.0f;
    
    [Header("Strike Settings")]
    [SerializeField] private float strikeForce = 10f;
    [SerializeField] private float strikeRadius = 2f;
    [SerializeField] private float strikeDelay = 0.5f;
    [SerializeField] private LayerMask puckLayer;

    private Vector3 targetPosition;
    private bool canStrike = true;
    private float strikeTimer = 0f;
    private Rigidbody rb;
    private const float positionThreshold = 0.01f;

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
        rb.linearDamping = 1f;
        rb.angularDamping = 0.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false; // 物理挙動を有効化
    }

    private void Update()
    {
        // プレイヤーの移動
        if (Vector3.Distance(transform.position, targetPosition) > positionThreshold)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 force = direction * moveSpeed;
            rb.AddForce(force, ForceMode.Force);
            
            // 移動方向に回転
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
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
        if (canStrike && Input.GetMouseButtonDown(1)) // 右クリックでパックを打つ
        {
            Strike();
        }
    }

    public void SetPosition(Vector3 position)
    {
        position.y = transform.position.y; // Y座標を維持
        transform.position = position;
        targetPosition = position;
        rb.linearVelocity = Vector3.zero; // 速度をリセット
    }

    public void MoveTo(Vector3 position)
    {
        position.y = transform.position.y; // Y座標を維持
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
        // パックとの衝突処理
        Puck puck = collision.gameObject.GetComponent<Puck>();
        if (puck != null)
        {
            // 衝突点での相対速度を計算
            Vector3 relativeVelocity = collision.relativeVelocity;
            
            // プレイヤーの移動方向を考慮した追加の力
            Vector3 playerDirection = (targetPosition - transform.position).normalized;
            Vector3 additionalForce = playerDirection * pushForce;
            
            // パックに力を適用（衝突の力と追加の力を合わせる）
            Vector3 totalForce = (relativeVelocity + additionalForce).normalized * 
                               (relativeVelocity.magnitude + pushForce);
            
            puck.ApplyForce(totalForce);
        }
    }

    // 打撃範囲を可視化（デバッグ用）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, strikeRadius);
    }
}