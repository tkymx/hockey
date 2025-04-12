using UnityEngine;
using System.Collections.Generic;
using System;

public class StageController : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private PlayerManager playerManager; // PlayerManagerへの参照を追加
    private List<ZoneController> zoneControllers = new List<ZoneController>();
    private ZoneController currentZone;
    private int currentZoneIndex = 0;
    
    public event Action<int> OnZoneChanged;
    public event Action OnAllZonesCleared;
    
    // 破壊可能オブジェクトのイベントを追加
    public event Action<ZoneController, DestructibleObject, int> OnObjectDestroyedInStage;

    public void Initialize(StageManager stageManager, PlayerManager playerManager = null)
    {
        this.stageManager = stageManager;
        this.playerManager = playerManager;
        InitializeZoneControllers();
        ActivateFirstZone();
    }

    public void InitializeZoneControllers()
    {
        zoneControllers.Clear();
        currentZoneIndex = 0;
        
        GameObject stageInstance = stageManager.GetCurrentStage();
        if (stageInstance != null)
        {
            // ステージインスタンス内のZoneControllerを取得
            ZoneController[] controllers = stageInstance.GetComponentsInChildren<ZoneController>(true);
            foreach (var controller in controllers)
            {
                zoneControllers.Add(controller);
                
                // 各ゾーンコントローラを初期化
                controller.Initialize();
                
                // ゾーンクリアイベントを購読
                controller.OnZoneCleared += () => HandleZoneCleared(controller);
                
                // オブジェクト破壊イベントを購読
                controller.OnObjectDestroyedInZone += HandleObjectDestroyedInZone;
                
                // 初期状態では全てのゾーンを非表示に
                controller.SetZoneVisibility(false);
            }
            
            // ZoneLevelでソート（低いレベルから順に）
            zoneControllers.Sort((a, b) => a.ZoneLevel.CompareTo(b.ZoneLevel));
            
            Debug.Log($"{zoneControllers.Count}個のゾーンを初期化しました。");
        }
    }

    private void ActivateFirstZone()
    {
        if (zoneControllers.Count > 0)
        {
            currentZoneIndex = 0;
            currentZone = zoneControllers[currentZoneIndex];
            currentZone.ActivateZone();
            
            // プレイヤーに現在のゾーンを設定
            if (playerManager != null)
            {
                playerManager.UpdatePlayerZone(currentZone);
            }
            
            OnZoneChanged?.Invoke(currentZoneIndex + 1);
        }
    }

    private void HandleZoneCleared(ZoneController clearedZone)
    {
        // 現在のゾーンがクリアされた場合にのみ処理
        if (clearedZone == currentZone)
        {
            // 現在のゾーンを非アクティブに
            currentZone.DeactivateZone();
            
            // 次のゾーンがあれば進む
            if (currentZoneIndex < zoneControllers.Count - 1)
            {
                currentZoneIndex++;
                currentZone = zoneControllers[currentZoneIndex];
                currentZone.ActivateZone();
                
                // プレイヤーに新しいゾーンを設定
                if (playerManager != null)
                {
                    playerManager.UpdatePlayerZone(currentZone);
                }
                
                OnZoneChanged?.Invoke(currentZoneIndex + 1);
            }
            else
            {
                // 全てのゾーンをクリアした場合
                OnAllZonesCleared?.Invoke();
                Debug.Log("すべてのゾーンをクリアしました！");
            }
        }
    }

    // ゾーン内でオブジェクトが破壊された時の処理
    private void HandleObjectDestroyedInZone(ZoneController zone, DestructibleObject obj, int points)
    {
        // 各ゾーンのスコア倍率を適用
        float scoreMultiplier = zone.GetScoreMultiplier();
        int adjustedPoints = Mathf.RoundToInt(points * scoreMultiplier);
        
        // GameManagerに通知
        OnObjectDestroyedInStage?.Invoke(zone, obj, adjustedPoints);
    }

    public ZoneController GetCurrentZone()
    {
        return currentZone;
    }

    public int GetCurrentZoneIndex()
    {
        return currentZoneIndex;
    }

    public int GetTotalZoneCount()
    {
        return zoneControllers.Count;
    }

    public Vector3 GetCurrentZoneBounds()
    {
        if (currentZone != null)
        {
            return new Vector3(Mathf.Max(currentZone.FrontWidth, currentZone.BackWidth), 0, currentZone.Depth);
        }
        return Vector3.zero;
    }

    private void OnDestroy()
    {
        // イベントの購読解除
        foreach (var zone in zoneControllers)
        {
            if (zone != null)
            {
                zone.OnZoneCleared -= () => HandleZoneCleared(zone);
                zone.OnObjectDestroyedInZone -= HandleObjectDestroyedInZone;
            }
        }
    }
}