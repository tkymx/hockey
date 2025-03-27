using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float mass = 2.0f;
    [SerializeField] private float collisionForceMultiplier = 2.5f;

    private Vector3 previousPosition;
    private Vector3 currentVelocity;
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private Collider playerCollider;
    private Camera mainCamera;

    private void Awake()
    {
        previousPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        playerCollider = GetComponent<Collider>();
        mainCamera = Camera.main;

        // 初期状態では非表示かつ当たり判定無効
        SetActiveState(false);
        
        // 物理設定
        rb.mass = mass;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.None; // 補間を無効化
    }

    private void Update()
    {
        bool isPressed = Input.GetMouseButton(0);
        SetActiveState(isPressed);
    }

    private void SetActiveState(bool active)
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = active;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = active;
        }
        if (rb != null)
        {
            rb.isKinematic = !active; // アクティブでない時は物理演算を無効化
        }
    }

    public void SetPosition(Vector3 position)
    {
        Vector3 oldPosition = transform.position;
        position.y = transform.position.y;
        transform.position = position;
        
        // 位置が変更された場合の速度を計算
        currentVelocity = (position - oldPosition) / Time.deltaTime;
        previousPosition = position;
    }

    public void MoveTo(Vector3 position)
    {
        Vector3 oldPosition = transform.position;
        position.y = transform.position.y;
        transform.position = position;
        
        // 移動による速度を計算
        currentVelocity = (position - oldPosition) / Time.deltaTime;
        previousPosition = position;
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