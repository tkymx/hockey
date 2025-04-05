using UnityEngine;
using System;
using Hockey.Data;

public class Puck : MonoBehaviour
{
    [Header("Puck Properties")]
    private float mass;
    private float frictionCoefficient;
    private float airResistance;
    private float minVelocityThreshold;

    [Header("Growth Settings")]
    private int growthStage = 1;
    private int maxGrowthStage;
    private float[] stageScales;
    private float[] stageMass;
    private float[] stageMaxSpeed;
    private float[] stageMaxForce;
    private float[] stageFriction;

    public int GrowthStage => growthStage;

    public event Action<int> OnGrowthStageChanged;

    private bool isMoving = false;
    private Rigidbody rb;
    private SphereCollider sphereCollider;
    private PuckView puckView;
    private Player lastHitPlayer;

    public Rigidbody Rigidbody => rb;

    private Func<float> maxSpeed = null;
    private Func<float> maxForce = null;

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
    }

    public void Initialize(PuckData puckData)
    {
        // パックの物理設定
        mass = puckData.mass;
        frictionCoefficient = puckData.frictionCoefficient;
        airResistance = puckData.airResistance;
        minVelocityThreshold = puckData.minVelocityThreshold;
        maxGrowthStage = puckData.maxGrowthStage;
        stageScales = puckData.stageScales;
        stageMass = puckData.stageMass;
        stageMaxSpeed = puckData.stageMaxSpeed;
        stageMaxForce = puckData.stageMaxForce;
        stageFriction = puckData.stageFriction;

        rb.mass = mass;
        rb.linearDamping = 0; // ダンピングはなし
        rb.angularDamping = 0.05f; // 回転の減衰もわずかに
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // 高速移動時の衝突検出を改善

        // 当たり判定の設定
        PhysicsMaterial puckMaterial = new PhysicsMaterial
        {
            dynamicFriction = 0.01f, // ほとんど摩擦なし
            staticFriction = 0.01f, // ほとんど摩擦なし
            bounciness = 0.9f, // 弾性を少し高く
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };
        sphereCollider.material = puckMaterial;

        // 成長段階の初期設定を適用
        ApplyGrowthStageSettings(growthStage);
    }

    private void FixedUpdate()
    {
        // 現在の速度をチェックして空気抵抗と微小な摩擦を適用
        if (rb.linearVelocity.sqrMagnitude > minVelocityThreshold * minVelocityThreshold)
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

        // 外力の大きさを制限
        float forceMagnitude = force.magnitude;
        if (forceMagnitude > maxForce?.Invoke())
        {
            force = force.normalized * (maxForce?.Invoke() ?? 0);
        }

        rb.AddForce(new Vector3(force.x, 0, force.z), ForceMode.Impulse);

        isMoving = true;
    }

    private void ApplyFriction()
    {
        // エアホッケーでは、摩擦はほとんどなく、主に空気抵抗で徐々に減速
        rb.linearVelocity *= airResistance; // まず空気抵抗を適用

        // 非常に小さな摩擦
        rb.linearVelocity *= frictionCoefficient;

        // 最大速度を制限
        if (rb.linearVelocity.magnitude > maxSpeed?.Invoke())
        {
            rb.linearVelocity = rb.linearVelocity.normalized * (maxSpeed?.Invoke() ?? 0);
        }
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
        maxSpeed = () => stageMaxSpeed[index];
        maxForce = () => stageMaxForce[index]; // 最大外力も更新
        frictionCoefficient = stageFriction[index];

        if (rb != null)
        {
            rb.mass = mass;
        }
    }

    // 成長段階をリセットするメソッド
    public void ResetGrowth()
    {
        // 成長段階を1にリセット
        if (growthStage != 1)
        {
            UpdateGrowthStage(1);
        }
    }
}