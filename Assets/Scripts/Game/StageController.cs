using UnityEngine;
using System.Collections.Generic;

public class StageController : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    private List<ZoneController> zoneControllers = new List<ZoneController>();
    private Player currentPlayer;

    public void Initialize(Player player, StageManager stageManager)
    {
        this.stageManager = stageManager;
        currentPlayer = player;
        if (currentPlayer != null)
        {
            currentPlayer.OnLevelChanged += HandlePlayerLevelChanged;
        }
        InitializeZoneControllers(currentPlayer?.Level ?? 1);
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

    private void OnDestroy()
    {
        if (currentPlayer != null)
        {
            currentPlayer.OnLevelChanged -= HandlePlayerLevelChanged;
        }
    }
}