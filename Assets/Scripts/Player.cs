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
    private float baseDamage;
    private float levelDamageMultiplier;

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
        baseDamage = playerData.baseDamage;
        levelDamageMultiplier = playerData.levelDamageMultiplier;
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

    internal float GetDamageMultiplier()
    {
        float levelBonus = (currentLevel - 1) * levelDamageMultiplier;
        float zoneMultiplier = (currentZone != null) ? currentZone.GetDamageMultiplier() : 1.0f;

        return (baseDamage + levelBonus) * zoneMultiplier;
    }

    private void InitializeSkills()
    {
        Puck puck = FindObjectOfType<Puck>();
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
}