using UnityEngine;

public class ZoneWall : MonoBehaviour
{
    [SerializeField] private int requiredLevel;
    
    public int CurrentLevel { get; set; }
    public int RequiredLevel { get => requiredLevel; set => requiredLevel = value; }

    private void Awake()
    {
        SetupWallPhysics();
    }

    private void SetupWallPhysics()
    {
        // 各子オブジェクトに物理マテリアルを適用
        PhysicsMaterial wallPhysicsMaterial = new PhysicsMaterial
        {
            bounciness = 1f,        // 完全な反射
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum,
            dynamicFriction = 0f,
            staticFriction = 0f
        };

        foreach (Transform child in transform)
        {
            var meshCollider = child.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.convex = true;
                meshCollider.isTrigger = false;
                meshCollider.material = wallPhysicsMaterial;
            }
        }
    }

    public void SetWallState(bool active)
    {
        foreach (Transform child in transform)
        {
            var renderer = child.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = active;
            }

            var collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = active;
            }
        }
        
        if (!active)
        {
            PlayDisappearEffect();
        }
    }

    private void PlayDisappearEffect()
    {
        // 壁消失時のエフェクト再生
        // パーティクルシステム等を実装
    }

    private void OnCollisionEnter(Collision collision)
    {
        // パックとの衝突処理
        Puck puck = collision.gameObject.GetComponent<Puck>();
        if (puck != null)
        {
            HandlePuckCollision(puck, collision);
        }
    }

    private void HandlePuckCollision(Puck puck, Collision collision)
    {
        // パックのレベルチェック
        if (CurrentLevel < RequiredLevel)
        {
            // レベルが不足している場合は完全に反射
            Vector3 normal = collision.contacts[0].normal;
            Vector3 incomingVelocity = puck.Rigidbody.linearVelocity;
            Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, normal);
            
            // 反射速度を維持
            puck.Rigidbody.linearVelocity = reflectedVelocity.normalized * incomingVelocity.magnitude;
            
            // 視覚的なフィードバック
            ShowReflectionEffect(collision.contacts[0].point);
        }
    }

    private void ShowReflectionEffect(Vector3 position)
    {
        // 反射エフェクトの実装（パーティクルなど）
        // 必要に応じて実装
    }
}
