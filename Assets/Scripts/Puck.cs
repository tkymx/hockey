using UnityEngine;
using System;
using System.Collections.Generic;
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

    // 貫通関連
    private List<GameObject> penetratedObjects = new List<GameObject>();

    public Rigidbody Rigidbody => rb;

    private Func<float> maxSpeed = null;
    private Func<float> maxForce = null;

    // 成長段階に対応する基本スケール値を保持
    private Vector3 currentBaseScale;

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
        penetratedObjects.Clear();
    }

    public Player GetLastHitPlayer()
    {
        return lastHitPlayer;
    }

    // プレイヤーとの物理衝突処理
    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤーとの衝突
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            lastHitPlayer = player;

            // プレイヤーから押された方向に力を適用
            Vector3 direction = transform.position - player.transform.position;
            direction.y = 0;
            direction.Normalize();

            // 力を計算（プレイヤーの速度をベースに）
            float playerSpeed = player.GetCurrentVelocity().magnitude;
            Vector3 force = direction * playerSpeed * player.GetCollisionForceMultiplier() * player.GetMass();

            // 力を適用
            ApplyForce(force, player);

            // 効果音やエフェクトの再生
            if (puckView != null)
            {
                Vector3 contactPoint = collision.contacts[0].point;
                puckView.PlayHitEffect(contactPoint);
            }
        }
    }

    // トリガー領域に進入時の処理を改善
    private void OnTriggerEnter(Collider other)
    {
        // DestructibleObjectとの衝突
        DestructibleObject destructible = other.GetComponent<DestructibleObject>();
        if (destructible != null && !destructible.IsDestroyed())
        {
            // 貫通スキル処理
            PuckSkillController skillController = GetComponent<PuckSkillController>();
            if (skillController != null)
            {
                if (skillController.ProcessDestructibleCollision(destructible))
                {
                    // 貫通した場合は反射しない
                    if (!penetratedObjects.Contains(other.gameObject))
                    {
                        // リストに追加
                        penetratedObjects.Add(other.gameObject);
                    }

                    // ダメージを与える - 乗算方式の攻撃力を使用
                    float damageMultiplier = skillController != null ? skillController.GetDamageMultiplier() : 1.0f;
                    float attackPower = lastHitPlayer != null ? lastHitPlayer.GetAttackPowerMultiplied(damageMultiplier) : 100.0f;
                    destructible.TakeDamage(attackPower, lastHitPlayer?.gameObject);

                    return;
                }
            }

            // 貫通していない場合は通常の破壊処理
            {
                // 通常の破壊処理と反射
                // ダメージを与える - 乗算方式の攻撃力を使用
                float damageMultiplier = skillController != null ? skillController.GetDamageMultiplier() : 1.0f;
                float attackPower = lastHitPlayer != null ? lastHitPlayer.GetAttackPowerMultiplied(damageMultiplier) : 100.0f;
                bool destroyed = destructible.TakeDamage(attackPower, lastHitPlayer?.gameObject);

            }

            // 反射処理
            HandleReflection(other);

            // 効果音やエフェクト
            if (puckView != null)
            {
                Vector3 contactPoint = other.ClosestPoint(transform.position);
                puckView.PlayHitEffect(contactPoint);
            }

            return;
        }

        // その他の障害物との衝突（壁など）
        HandleReflection(other);

        if (puckView != null)
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            puckView.PlayHitEffect(contactPoint);
        }
    }

    // 物体からの反射を処理
    private void HandleReflection(Collider other)
    {
        // 衝突点を取得
        Vector3 contactPoint = other.ClosestPoint(transform.position);

        // 反射方向を計算
        Vector3 normal = (transform.position - contactPoint).normalized;
        normal.y = 0; // Y軸方向の反射は無視

        if (normal.magnitude < 0.01f)
        {
            // 法線ベクトルが不正な場合はランダムな方向に反射
            normal = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
        }

        // 入射ベクトル（現在の速度）
        Vector3 incident = rb.linearVelocity.normalized;

        // 反射ベクトルの計算 (R = I - 2 * N * dot(I, N))
        Vector3 reflection = Vector3.Reflect(incident, normal);

        // 反射後の速度計算（勢いは少し失われる）
        float speed = rb.linearVelocity.magnitude * 0.9f; // 10%減衰

        // 新しい速度を設定
        rb.linearVelocity = reflection * speed;
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
        // 現在の基本スケールを保存
        currentBaseScale = new Vector3(scale, transform.localScale.y, scale);

        // スキルコントローラーが既に存在する場合は、スキルコントローラーに任せる
        PuckSkillController skillController = GetComponent<PuckSkillController>();
        if (skillController == null)
        {
            // スキルコントローラーがない場合は直接スケールを適用
            transform.localScale = currentBaseScale;
        }

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

    // 現在の成長段階における基本スケールを取得
    public Vector3 GetCurrentBaseScale()
    {
        return currentBaseScale;
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

    // 貫通オブジェクトリストをクリア
    public void ClearPenetratedObjects()
    {
        penetratedObjects.Clear();
    }
}