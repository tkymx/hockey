using UnityEngine;
using System;
using Hockey.Data;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    private float mass;
    private float collisionForceMultiplier;

    [Header("Player Stats")]
    private int[] experienceThresholds;
    private float[] stageAttackPower;  // 成長段階ごとの攻撃力

    [Header("Growth Settings")]
    private int growthStage = 1;
    private int maxGrowthStage;
    private float[] stageScales;
    private float[] stageMass;
    private float[] stageCollisionForce;

    [Header("Skills")]
    [SerializeField] private MissileSkill missileSkill;

    private int level = 1;
    int experiencePoints = 0;
    private int currentLevel = 1;
    private int currentExperience = 0;
    private ZoneController currentZone;

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

    public void Initialize(PlayerData playerData)
    {
        mass = playerData.mass;
        collisionForceMultiplier = playerData.collisionForceMultiplier;
        experienceThresholds = playerData.experienceThresholds;
        
        // 成長段階ごとの攻撃力のみを初期化
        stageAttackPower = playerData.stageAttackPower;
        
        maxGrowthStage = playerData.maxGrowthStage;
        stageScales = playerData.stageScales;
        stageMass = playerData.stageMass;
        stageCollisionForce = playerData.stageCollisionForce;

        previousPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        meshRenderer = GetComponentInChildren<MeshRenderer>();
        playerCollider = GetComponent<Collider>();
        mainCamera = Camera.main;

        SetActiveState(false);

        rb.mass = mass;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.None;

        ApplyGrowthStageSettings(growthStage);

        InitializeSkills();
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
            rb.detectCollisions = active;
        }
    }

    public void SetPosition(Vector3 position)
    {
        Vector3 oldPosition = transform.position;
        position.y = transform.position.y;
        transform.position = position;

        currentVelocity = (position - oldPosition) / Time.deltaTime;
        previousPosition = position;
    }

    public void MoveTo(Vector3 position)
    {
        if (Vector3.Distance(previousPosition, position) < 0.01f)
        {
            return;
        }

        Vector3 oldPosition = transform.position;
        position.y = transform.position.y;
        transform.position = position;

        currentVelocity = (position - oldPosition) / Time.deltaTime;
        previousPosition = position;

        UnityEngine.Debug.Log($"Player moved to: {position}, Velocity: {currentVelocity}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Puck puck = collision.gameObject.GetComponent<Puck>();
        if (puck != null)
        {
            Vector3 collisionForce = currentVelocity * collisionForceMultiplier * mass;
            puck.ApplyForce(collisionForce);
        }
    }

    public int GetBreakableObjectLevel()
    {
        return level;
    }

    public bool GainExperience(int exp)
    {
        bool didLevelUp = false;
        
        if (currentZone != null)
        {
            exp = Mathf.RoundToInt(exp * currentZone.GetScoreMultiplier());
        }
        
        experiencePoints += exp;
        currentExperience = experiencePoints;

        while (level < experienceThresholds.Length && experiencePoints >= experienceThresholds[level])
        {
            level++;
            currentLevel = level;
            didLevelUp = true;
            OnLevelChanged?.Invoke(level);
        }

        return didLevelUp;
    }

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

    private void ApplyGrowthStageSettings(int stage)
    {
        int index = Mathf.Clamp(stage - 1, 0, stageScales.Length - 1);

        transform.localScale = new Vector3(stageScales[index], stageScales[index], stageScales[index]);

        mass = stageMass[index];
        collisionForceMultiplier = stageCollisionForce[index];

        if (rb != null)
        {
            rb.mass = mass;
        }
    }

    public void ResetGrowth()
    {
        if (level != 1)
        {
            level = 1;
            OnLevelChanged?.Invoke(level);
        }

        experiencePoints = 0;

        if (growthStage != 1)
        {
            UpdateGrowthStage(1);
        }
    }

    /// <summary>
    /// ゾーンによるダメージ倍率のみを返す
    /// </summary>
    /// <returns>ゾーンによる倍率（ゾーンがない場合は1.0）</returns>
    internal float GetDamageMultiplier()
    {
        return (currentZone != null) ? currentZone.GetDamageMultiplier() : 1.0f;
    }
    
    /// <summary>
    /// 現在の攻撃力を取得する
    /// </summary>
    /// <returns>攻撃力の値</returns>
    public float GetAttackPower()
    {
        // 成長段階に基づく攻撃力の取得
        int index = Mathf.Clamp(growthStage - 1, 0, stageAttackPower.Length - 1);
        float stageAttack = stageAttackPower[index];
        
        // ゾーン効果
        float zoneMultiplier = GetDamageMultiplier();
        
        // レベルと成長段階のみに基づく攻撃力
        return stageAttack * zoneMultiplier;
    }
    
    /// <summary>
    /// 攻撃力を計算する（スキル倍率適用）
    /// </summary>
    /// <param name="skillMultiplier">スキルによる倍率（デフォルト: 1.0）</param>
    /// <returns>計算された攻撃力</returns>
    public float GetAttackPowerMultiplied(float skillMultiplier = 1.0f)
    {
        // 基本攻撃力を取得
        float baseAttack = GetAttackPower();
        
        // スキル倍率を適用
        return baseAttack * skillMultiplier;
    }
    
    /// <summary>
    /// スキル効果を含む攻撃力を加算方式で取得
    /// </summary>
    /// <param name="additionalPower">スキルによる追加攻撃力</param>
    /// <returns>計算された攻撃力</returns>
    public float GetAttackPowerAdditive(float additionalPower = 0.0f)
    {
        // 基本攻撃力を取得
        float baseAttack = GetAttackPower();
        
        // スキルによる追加攻撃力を加算
        return baseAttack + additionalPower;
    }

    private void InitializeSkills()
    {
        // 非推奨のFindObjectOfTypeを修正
        Puck puck = FindFirstObjectByType<Puck>();
        if (puck == null)
        {
            Debug.LogError("シーン内にPuckが見つかりません。ミサイルスキルが正常に動作しない可能性があります。");
        }
        
        if (missileSkill != null)
        {
            missileSkill.Initialize(this, puck);
        }
        else
        {
            MissileSkill skill = GetComponent<MissileSkill>();
            if (skill == null)
            {
                skill = gameObject.AddComponent<MissileSkill>();
            }
            skill.Initialize(this, puck);
        }
    }

    public void SetCurrentZone(ZoneController zone)
    {
        currentZone = zone;
        
        if (missileSkill != null)
        {
            missileSkill.SetCurrentZone(zone);
        }
        else
        {
            MissileSkill skill = GetComponent<MissileSkill>();
            if (skill != null)
            {
                skill.SetCurrentZone(zone);
            }
        }
    }

    public void ResetPosition()
    {
        if (currentZone != null)
        {
            float z = -(currentZone.Depth / 2f) + 2f;
            transform.position = new Vector3(0f, transform.position.y, z);
        }
        else
        {
            transform.position = new Vector3(0f, transform.position.y, -10f);
        }
    }

    // 現在の速度を取得するメソッド
    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }
    
    // 質量を取得するメソッド
    public float GetMass()
    {
        return mass;
    }
    
    // 衝突力倍率を取得するメソッド
    public float GetCollisionForceMultiplier()
    {
        return collisionForceMultiplier;
    }
}