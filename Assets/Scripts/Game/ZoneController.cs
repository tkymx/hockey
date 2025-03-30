using System;
using UnityEngine;
using System.Collections.Generic;

public class ZoneController : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private ZoneSettings zoneSettings;
    [SerializeField] private int zoneLevel = 0;
    [SerializeField] private float width;
    [SerializeField] private float depth;

    private bool isActive = false;
    private bool isCleared = false;
    private bool isCurrentZone;
    private int destructiblesCount;
    private int remainingDestructibles;
    private List<DestructibleObject> zoneObjects = new List<DestructibleObject>();

    public event Action OnZoneCleared;
    // 破壊可能オブジェクトが破壊された時のイベント（ゾーン情報を含む）
    public event Action<ZoneController, DestructibleObject, int> OnObjectDestroyedInZone;

    public int ZoneLevel { get => zoneLevel; set => zoneLevel = value; }
    public bool IsCurrentZone { get => isCurrentZone; set => isCurrentZone = value; }
    public float Width { get => width; set => width = value; }
    public float Depth { get => depth; set => depth = value; }
    public int RemainingDestructibles { get => remainingDestructibles; }
    public ZoneSettings ZoneSettings { get => zoneSettings; set => zoneSettings = value; }

    // ZoneDataの参照を保持
    private ZoneSettings.ZoneData zoneData;

    public void Initialize()
    {
        zoneObjects.Clear();
        isCleared = false;

        // ゾーン内の破壊可能オブジェクトを収集
        DestructibleObject[] objects = GetComponentsInChildren<DestructibleObject>(true);
        foreach (var obj in objects)
        {
            zoneObjects.Add(obj);
            obj.OnObjectDestroyed += HandleObjectDestroyed;
        }

        // ZoneSettingsからZoneDataを取得
        if (zoneSettings != null && zoneLevel < zoneSettings.zones.Length)
        {
            zoneData = zoneSettings.zones[zoneLevel];
        }
        else
        {
            Debug.LogWarning($"ZoneSettings for zone level {zoneLevel} is not available.");
        }

        // ゾーン内の破壊可能オブジェクトを集計
        CountDestructibles();

        // 初期状態ではゾーンは非表示
        if (!isCurrentZone)
        {
            SetZoneVisibility(false);
        }
    }

    public void UpdateZoneState()
    {
        // 壁の表示/非表示を制御 - 現在のゾーンの場合は必ず表示
        var wall = GetComponentInChildren<ZoneWall>();
        if (wall != null)
        {
            wall.SetWallState(isCurrentZone);
        }

        // フォグエフェクトの制御 - 現在のゾーンでないなら常に表示
        var fog = transform.Find("ZoneFog")?.gameObject;
        if (fog != null)
        {
            fog.SetActive(!isCurrentZone);
        }
    }

    public void SetZoneVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void CountDestructibles()
    {
        // ゾーン内の破壊可能オブジェクトを取得して数える
        var destructibles = GetComponentsInChildren<DestructibleObject>();
        destructiblesCount = destructibles.Length;
        remainingDestructibles = destructiblesCount;

        // 各破壊可能オブジェクトにイベントハンドラーを登録
        foreach (var destructible in destructibles)
        {
            destructible.OnObjectDestroyed += HandleDestructibleDestroyed;
        }

        Debug.Log($"Zone {zoneLevel}: {destructiblesCount} destructibles found");
    }

    private void HandleDestructibleDestroyed(DestructibleObject obj, int points)
    {
        remainingDestructibles--;

        // 破壊イベントを発火（このゾーン、破壊されたオブジェクト、ポイント）
        OnObjectDestroyedInZone?.Invoke(this, obj, points);

        // すべての破壊可能オブジェクトが破壊されたらイベントを発火
        if (remainingDestructibles <= 0)
        {
            OnZoneCleared?.Invoke();
            Debug.Log($"Zone {zoneLevel} cleared!");
        }
    }

    public void ActivateZone()
    {
        if (isActive) return;

        isActive = true;
        isCurrentZone = true;
        SetZoneVisibility(true);
        UpdateZoneState();
        PlayActivationEffect();
        ResetZoneObjects();
    }

    public void DeactivateZone()
    {
        if (!isActive) return;

        isActive = false;
        isCurrentZone = false;
        UpdateZoneState();
        SetZoneVisibility(false);
    }

    private void PlayActivationEffect()
    {
        // ゾーン開始時のエフェクト再生
        // パーティクルシステムやアニメーション等を実装
    }

    private void ResetZoneObjects()
    {
        isCleared = false;
        foreach (var obj in zoneObjects)
        {
            if (obj != null)
            {
                obj.Initialize();
            }
        }
    }

    private void HandleObjectDestroyed(DestructibleObject destroyedObject, int points)
    {
        // 破壊イベントを発火（このゾーン、破壊されたオブジェクト、ポイント）
        OnObjectDestroyedInZone?.Invoke(this, destroyedObject, points);

        // このオブジェクトの参照をリストから削除
        if (zoneObjects.Contains(destroyedObject))
        {
            zoneObjects.Remove(destroyedObject);
        }

        // すべてのオブジェクトが破壊されたかチェック
        CheckZoneCleared();
    }

    private void CheckZoneCleared()
    {
        if (isCleared) return;

        // アクティブな破壊可能オブジェクトがあるかチェック
        bool allDestroyed = true;
        foreach (var obj in zoneObjects)
        {
            if (obj != null && !obj.IsDestroyed())
            {
                allDestroyed = false;
                break;
            }
        }

        if (allDestroyed && zoneObjects.Count > 0)
        {
            isCleared = true;
            OnZoneCleared?.Invoke();
        }
    }

    public float GetDamageMultiplier()
    {
        return zoneData != null ? zoneData.damageMultiplier : 1.0f;
    }

    public float GetScoreMultiplier()
    {
        return zoneData != null ? zoneData.scoreMultiplier : 1.0f;
    }

    private void OnDestroy()
    {
        // イベント購読を解除
        foreach (var obj in zoneObjects)
        {
            if (obj != null)
            {
                obj.OnObjectDestroyed -= HandleObjectDestroyed;
            }
        }

        // イベントハンドラーの登録解除
        var destructibles = GetComponentsInChildren<DestructibleObject>();
        foreach (var destructible in destructibles)
        {
            if (destructible != null)
            {
                destructible.OnObjectDestroyed -= HandleDestructibleDestroyed;
            }
        }
    }
}
