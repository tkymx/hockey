using UnityEngine;
using System.Collections.Generic;

public class StageController : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    private List<ZoneController> zoneControllers = new List<ZoneController>();
    private Player currentPlayer;
    private ZoneController currentZone;

    public void Initialize(Player player, StageManager stageManager)
    {
        this.stageManager = stageManager;
        currentPlayer = player;
        if (currentPlayer != null)
        {
            currentPlayer.OnLevelChanged += HandlePlayerLevelChanged;
        }
        InitializeZoneControllers(currentPlayer?.Level ?? 1);
        UpdateCurrentZone(currentPlayer?.Level ?? 1);
    }

    public void InitializeZoneControllers(int playerLevel)
    {
        zoneControllers.Clear();
        
        GameObject stageInstance = stageManager.GetCurrentStage();
        if (stageInstance != null)
        {
            // ステージインスタンス内のZoneControllerを取得
            ZoneController[] controllers = stageInstance.GetComponentsInChildren<ZoneController>();
            zoneControllers.AddRange(controllers);
            
            // 各ZoneControllerを初期化
            foreach (var controller in zoneControllers)
            {
                controller.Initialize(playerLevel);
            }
            
            // 現在のプレイヤーレベルで状態を更新
            UpdateAllZoneStates(playerLevel);
            
            Debug.Log($"{zoneControllers.Count}個のゾーンを初期化しました。");
        }
    }

    private void HandlePlayerLevelChanged(int newLevel)
    {
        UpdateAllZoneStates(newLevel);
        UpdateCurrentZone(newLevel);
    }

    private void UpdateAllZoneStates(int playerLevel)
    {
        foreach (var zoneController in zoneControllers)
        {
            if (zoneController != null)
            {
                zoneController.UpdateZoneState(playerLevel);
            }
        }
    }

    private void UpdateCurrentZone(int playerLevel)
    {
        // プレイヤーレベルに応じた最適なゾーンを選択
        ZoneController bestZone = null;
        int highestLevel = 0;

        // まず全てのゾーンをnon-currentに設定
        foreach (var zone in zoneControllers)
        {
            zone.IsCurrentZone = false;
        }

        // プレイヤーレベルで到達可能な最高レベルのゾーンを見つける
        foreach (var zone in zoneControllers)
        {
            if (zone.RequiredPlayerLevel <= playerLevel && zone.ZoneLevel > highestLevel)
            {
                highestLevel = zone.ZoneLevel;
                bestZone = zone;
            }
        }

        // 見つかったベストなゾーンを現在のゾーンとして設定
        if (bestZone != null)
        {
            bestZone.IsCurrentZone = true;
            currentZone = bestZone;
        }
        else
        {
            // プレイヤーレベルが全てのゾーンに満たない場合は、最初のゾーンを選択
            if (zoneControllers.Count > 0)
            {
                zoneControllers[0].IsCurrentZone = true;
                currentZone = zoneControllers[0];
            }
        }

        // 全てのゾーンの状態を更新
        UpdateAllZoneStates(playerLevel);
    }

    public ZoneController GetCurrentZone()
    {
        return currentZone;
    }

    public Vector3 GetCurrentZoneBounds()
    {
        if (currentZone != null)
        {
            float radius = currentZone.Radius;
            return new Vector3(radius * 2, 0, radius * 2); // 直径をx,zに設定
        }
        return Vector3.zero;
    }

    private void OnDestroy()
    {
        if (currentPlayer != null)
        {
            currentPlayer.OnLevelChanged -= HandlePlayerLevelChanged;
        }
    }
}