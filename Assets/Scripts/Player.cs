using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    private Vector3 targetPosition;
    private Rigidbody rb;
    private const float positionThreshold = 0.01f;

    private void Start()
    {
        targetPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        // 回転と垂直方向の移動を制限
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        // 重力の影響を無効化
        rb.useGravity = false;
        // 物理演算による移動を無効化（衝突判定のみ使用）
        rb.isKinematic = true;
    }

    public void SetPosition(Vector3 position)
    {
        position.y = transform.position.y; // Y座標を維持
        transform.position = position;
        targetPosition = position;
    }

    public void MoveTo(Vector3 position)
    {
        position.y = transform.position.y; // Y座標を維持
        targetPosition = position;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition) > positionThreshold)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 newPosition = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            transform.position = newPosition;
        }
        else
        {
            transform.position = targetPosition;
        }
    }
}