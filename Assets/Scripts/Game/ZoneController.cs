using System;
using UnityEngine;
using System.Collections.Generic;
using Hockey.Data;

public class ZoneController : MonoBehaviour
{
    private int zoneLevel = 0;
    private float frontWidth;
    private float backWidth;
    private float depth;

    private bool isActive = false;
    private bool isCleared = false;
    private bool isCurrentZone;
    private int destructiblesCount;
    private int remainingDestructibles;
    private List<DestructibleObject> zoneObjects = new List<DestructibleObject>();
    // ステージ上の同心円状オブジェクトのうち、このゾーンに属するもの
    private List<DestructibleObject> concentricsInZone = new List<DestructibleObject>();

    public event Action OnZoneCleared;
    // 破壊可能オブジェクトが破壊された時のイベント（ゾーン情報を含む）
    public event Action<ZoneController, DestructibleObject, int> OnObjectDestroyedInZone;

    public int ZoneLevel { get => zoneLevel; set => zoneLevel = value; }
    public float FrontWidth { get => frontWidth; set => frontWidth = value; }
    public float BackWidth { get => backWidth; set => backWidth = value; }
    public float Depth { get => depth; set => depth = value; }

    // ZoneDataの参照を保持
    private ZoneData zoneData;
    public ZoneData ZoneData { get => zoneData; set => zoneData = value; }

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

        // concentricsInZoneは既にStageCreator.csで設定済み
        // その上で同心円オブジェクトのイベントをリッスンする
        foreach (var obj in concentricsInZone)
        {
            if (obj != null)
            {
                obj.OnObjectDestroyed += HandleObjectDestroyed;
            }
        }

        // ゾーン内の破壊可能オブジェクトを集計
        CountDestructibles();

        // 初期状態ではゾーンは非表示
        if (!isCurrentZone)
        {
            SetZoneVisibility(false);
        }
    }

    // 点がゾーンの台形範囲内にあるかどうかを判定（StageCreator側で使用）
    public bool IsPointInTrapezoid(Vector3 point)
    {
        if (zoneData == null) return false;

        // ゾーンの境界を取得
        float halfDepth = zoneData.depth / 2;
        
        // Z座標がゾーンの範囲内かチェック
        if (Mathf.Abs(point.z) > halfDepth)
            return false;
            
        // Z位置の正規化値（0～1）
        float normalizedZ = Mathf.InverseLerp(-halfDepth, halfDepth, point.z);
        
        // Z位置に応じた幅を計算（線形補間）
        float widthAtZ = Mathf.Lerp(zoneData.frontWidth, zoneData.backWidth, normalizedZ);
        float halfWidthAtZ = widthAtZ / 2;
        
        // X座標が幅の範囲内にあるかチェック
        return point.x >= -halfWidthAtZ && point.x <= halfWidthAtZ;
    }
    
    // 同心円状オブジェクトをゾーンに追加するメソッド（StageCreatorから呼び出される）
    public void AddConcentricObject(DestructibleObject obj)
    {
        if (obj != null && !concentricsInZone.Contains(obj))
        {
            concentricsInZone.Add(obj);
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
        // 同心円オブジェクトの数も加算
        destructiblesCount += concentricsInZone.Count;
        remainingDestructibles = destructiblesCount;

        // 各破壊可能オブジェクトにイベントハンドラーを登録
        foreach (var destructible in destructibles)
        {
            destructible.OnObjectDestroyed += HandleDestructibleDestroyed;
        }

        // 同心円オブジェクトにもハンドラーを登録
        foreach (var concentricObj in concentricsInZone)
        {
            if (concentricObj != null)
            {
                concentricObj.OnObjectDestroyed += HandleDestructibleDestroyed;
            }
        }

        Debug.Log($"Zone {zoneLevel}: {destructiblesCount} destructibles found (including {concentricsInZone.Count} concentric objects)");
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
        // TODO: ゾーンデータからダメージ倍率を取得する
        return 1.0f;
    }

    public float GetScoreMultiplier()
    {
        // TODO: ゾーンデータからスコア倍率を取得する
        return 1.0f;
    }

    // ターゲット可能な破壊可能オブジェクトを取得するメソッド
    public List<DestructibleObject> GetTargetableDestructibles()
    {
        List<DestructibleObject> targetables = new List<DestructibleObject>();
        
        // ゾーン内の破壊可能オブジェクトを追加
        foreach (var obj in zoneObjects)
        {
            if (obj != null && !obj.IsDestroyed())
            {
                targetables.Add(obj);
            }
        }
        
        // 同心円状オブジェクトで、このゾーンに属するものを追加
        foreach (var obj in concentricsInZone)
        {
            if (obj != null && !obj.IsDestroyed())
            {
                targetables.Add(obj);
            }
        }
        
        return targetables;
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

        // 同心円オブジェクトのイベント購読も解除
        foreach (var obj in concentricsInZone)
        {
            if (obj != null)
            {
                obj.OnObjectDestroyed -= HandleObjectDestroyed;
                obj.OnObjectDestroyed -= HandleDestructibleDestroyed;
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
