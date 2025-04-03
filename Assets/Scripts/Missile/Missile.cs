using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour
{
    private MissileData data;
    private Rigidbody rb;
    private Player owner;
    
    public void Initialize(MissileData missileData, Player player)
    {
        data = missileData;
        owner = player;
        
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // 物理設定
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // コライダーをトリガーに設定
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // 近くの破壊可能オブジェクトを探索
        Transform target = FindNearestDestructibleObject();
        
        // ターゲットが見つかった場合、その方向に向かって発射
        if (target != null)
        {
            // ターゲットへの方向ベクトルを計算
            Vector3 direction = (target.position - transform.position).normalized;
            
            // ミサイルをターゲット方向に向ける
            transform.forward = direction;
            
            // ミサイルに初速度を設定
            rb.linearVelocity = direction * data.speed;
        }
        else
        {
            // ターゲットが見つからない場合は前方に発射
            rb.linearVelocity = transform.forward * data.speed;
        }
        
        // 存在時間後に自動破棄
        Destroy(gameObject, data.lifetime);
    }
    
    private Transform FindNearestDestructibleObject()
    {
        // 全ての破壊可能オブジェクトから探索
        DestructibleObject[] targets = Object.FindObjectsByType<DestructibleObject>(FindObjectsSortMode.None);
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (DestructibleObject destructible in targets)
        {
            if (destructible.IsDestroyed())
                continue;
            
            float distance = Vector3.Distance(transform.position, destructible.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = destructible.transform;
            }
        }
        
        return closestTarget;
    }
    
    private void Update()
    {
        // 進行方向に向きを合わせる（速度が十分ある場合）
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 破壊可能オブジェクトに衝突した場合
        DestructibleObject destructible = other.GetComponent<DestructibleObject>();
        if (destructible != null && !destructible.IsDestroyed())
        {
            // ダメージを与える
            destructible.TakeDamage(data.damage, owner ? owner.gameObject : gameObject);
            
            // ミサイルを破壊
            Destroy(gameObject);
        }
    }
}