using UnityEngine;
using Hockey.Data;

public class GrowthManager : MonoBehaviour
{
    [Header("Growth Settings")]
    private int maxGrowthStage = 5; // デフォルト最大成長段階

    private PlayerManager playerManager;
    private PuckController puckController;
    private GrowthData growthData;

    public void Initialize(PlayerManager playerManagerRef, PuckController puckControllerRef, GameConfigRepository configRepository = null)
    {
        playerManager = playerManagerRef;
        puckController = puckControllerRef;

        // GameConfigRepositoryから設定を読み込む
        if (configRepository != null)
        {
            growthData = configRepository.GrowthConfig;
            if (growthData != null)
            {
                maxGrowthStage = growthData.maxGrowthStage;
                Debug.Log($"GrowthManager: 設定を読み込みました。最大成長段階: {maxGrowthStage}");
            }
        }

        // プレイヤーのレベル変更イベントを購読
        Player player = playerManager.GetPlayer();
        if (player != null)
        {
            player.OnLevelChanged += HandlePlayerLevelChanged;
            
            // 初期レベルに基づいて成長段階を設定
            UpdateGrowthStageBasedOnLevel(player.Level);
        }
    }

    // プレイヤーのレベルが変更された時の処理
    private void HandlePlayerLevelChanged(int newLevel)
    {
        // レベルに応じて成長段階を更新
        UpdateGrowthStageBasedOnLevel(newLevel);
    }
    
    // レベルに応じて成長段階を決定し、PlayerとPuckに適用する
    public void UpdateGrowthStageBasedOnLevel(int level)
    {
        // レベルがそのまま成長段階になるように変更
        int newGrowthStage = Mathf.Min(level, maxGrowthStage);
        
        // Playerの成長段階を更新
        if (playerManager != null)
        {
            Player player = playerManager.GetPlayer();
            if (player != null)
            {
                player.UpdateGrowthStage(newGrowthStage);
                Debug.Log($"プレイヤーの成長段階を{newGrowthStage}に更新しました");
            }
        }
        
        // Puckの成長段階を更新
        if (puckController != null && puckController.Puck != null)
        {
            puckController.Puck.UpdateGrowthStage(newGrowthStage);
            Debug.Log($"パックの成長段階を{newGrowthStage}に更新しました");
        }
    }

    private void OnDestroy()
    {
        // イベントの購読解除
        if (playerManager != null)
        {
            Player player = playerManager.GetPlayer();
            if (player != null)
            {
                player.OnLevelChanged -= HandlePlayerLevelChanged;
            }
        }
    }
}