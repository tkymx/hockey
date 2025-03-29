using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float mass = 2.0f;
    [SerializeField] private float collisionForceMultiplier = 2.5f;

    [Header("Player Stats")]
    [SerializeField] private int[] experienceThresholds = { 0, 100, 300, 600, 1000 }; // レベルアップに必要な経験値

    [Header("Growth Settings")]
    [SerializeField] private int growthStage = 1;
    [SerializeField] private int maxGrowthStage = 3;
    [SerializeField] private float[] stageScales = { 0.8f, 1.0f, 1.2f };
    [SerializeField] private float[] stageMass = { 1.5f, 2.0f, 2.5f };
    [SerializeField] private float[] stageCollisionForce = { 2.0f, 2.5f, 3.0f };

    private int level = 1;
    int experiencePoints = 0;

    public int Level => level;
    public int GrowthStage => growthStage;

    public event Action<int> OnLevelChanged;
    public event Action<int> OnGrowthStageChanged;

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
        rb.isKinematic = true; // 常にKinematicモードを有効にして外力の影響を受けないようにする
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.None; // 補間を無効化

        // 成長段階の初期設定を適用
        ApplyGrowthStageSettings(growthStage);
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
            // 物理的な当たり判定も同時に有効/無効化
            rb.detectCollisions = active;
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
        // 過去の座標と変化がなかったら処理をしない
        if (Vector3.Distance(previousPosition, position) < 0.01f)
        {
            return;
        }

        Vector3 oldPosition = transform.position;
        position.y = transform.position.y;
        transform.position = position;

        // 移動による速度を計算
        currentVelocity = (position - oldPosition) / Time.deltaTime;
        previousPosition = position;

        UnityEngine.Debug.Log($"Player moved to: {position}, Velocity: {currentVelocity}");
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

    // レベルに応じて破壊できるオブジェクトの最大レベルを返す
    public int GetBreakableObjectLevel()
    {
        return level;
    }

    // 経験値を獲得してレベルアップをチェック
    public bool GainExperience(int exp)
    {
        bool didLevelUp = false;
        experiencePoints += exp;

        while (level < experienceThresholds.Length && experiencePoints >= experienceThresholds[level])
        {
            level++;
            didLevelUp = true;
            OnLevelChanged?.Invoke(level);
        }

        return didLevelUp;
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
        transform.localScale = new Vector3(stageScales[index], stageScales[index], stageScales[index]);

        // 物理パラメータの更新
        mass = stageMass[index];
        collisionForceMultiplier = stageCollisionForce[index];

        if (rb != null)
        {
            rb.mass = mass;
        }
    }

    // レベルと成長段階をリセットする
    public void ResetGrowth()
    {
        // レベルを1にリセット
        if (level != 1)
        {
            level = 1;
            OnLevelChanged?.Invoke(level);
        }

        // 経験値をリセット
        experiencePoints = 0;

        // 成長段階を1にリセット
        if (growthStage != 1)
        {
            UpdateGrowthStage(1);
        }
    }
}