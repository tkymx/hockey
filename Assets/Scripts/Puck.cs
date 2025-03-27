using UnityEngine;
using System;

public class Puck : MonoBehaviour
{
    [Header("Puck Properties")]
    [SerializeField] private float mass = 1.0f;
    [SerializeField] private float frictionCoefficient = 0.95f;
    [SerializeField] private float maxSpeed = 20.0f;

    [Header("Growth Settings")]
    [SerializeField] private int growthStage = 1;
    [SerializeField] private int maxGrowthStage = 3;
    [SerializeField] private float[] stageScales = { 0.8f, 1.0f, 1.2f };
    [SerializeField] private float[] stageMass = { 0.8f, 1.0f, 1.3f };
    [SerializeField] private float[] stageMaxSpeed = { 15.0f, 20.0f, 25.0f };
    [SerializeField] private float[] stageFriction = { 0.93f, 0.95f, 0.97f };

    public int GrowthStage => growthStage;
    
    public event Action<int> OnGrowthStageChanged;

    private Vector3 velocity = Vector3.zero;
    private bool isMoving = false;
    private Rigidbody rb;
    private SphereCollider sphereCollider;
    private PuckView puckView;
    private Player lastHitPlayer;

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

        puckView = GetComponent<PuckView>();
        
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

        // 成長段階の初期設定を適用
        ApplyGrowthStageSettings(growthStage);
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
    
    public void ApplyForce(Vector3 force, Player player = null)
    {
        if (player != null)
        {
            lastHitPlayer = player;
        }
        
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
    
    public void ResetPosition(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector3.zero;
        isMoving = false;
    }
    
    public Player GetLastHitPlayer()
    {
        return lastHitPlayer;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            lastHitPlayer = player;
        }

        if (puckView != null)
        {
            // 衝突点の位置でヒットエフェクトを再生
            ContactPoint contact = collision.GetContact(0);
            puckView.PlayHitEffect(contact.point);
        }
    }

    // 成長段階を更新する
    public void UpdateGrowthStage(int newStage)
    {
        if (newStage <= 0 || newStage > maxGrowthStage)
            return;

        if (growthStage != newStage)
        {
            growthStage = newStage;
            ApplyGrowthStageSettings(growthStage);
            OnGrowthStageChanged?.Invoke(growthStage);
        }
    }

    // 成長段階に応じた設定を適用する
    private void ApplyGrowthStageSettings(int stage)
    {
        int index = Mathf.Clamp(stage - 1, 0, stageScales.Length - 1);
        
        // スケールの更新
        float scale = stageScales[index];
        transform.localScale = new Vector3(scale, transform.localScale.y, scale);
        
        // 物理パラメータの更新
        mass = stageMass[index];
        maxSpeed = stageMaxSpeed[index];
        frictionCoefficient = stageFriction[index];
        
        if (rb != null)
        {
            rb.mass = mass;
        }
        
        // コライダーのサイズも更新
        if (sphereCollider != null)
        {
            sphereCollider.radius = transform.localScale.x / 2;
        }
    }
}