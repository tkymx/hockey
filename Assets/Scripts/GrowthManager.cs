using UnityEngine;

public class GrowthManager : MonoBehaviour
{
    [Header("Growth Settings")]
    [SerializeField] private int[] levelThresholdsForGrowth = { 1, 3, 5 }; // レベルに応じた成長段階の閾値

    private PlayerManager playerManager;
    private PuckController puckController;

    public void Initialize(PlayerManager playerManagerRef, PuckController puckControllerRef)
    {
        playerManager = playerManagerRef;
        puckController = puckControllerRef;

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
        // レベルから成長段階を計算
        int newGrowthStage = 1; // デフォルトは段階1
        
        for (int i = 0; i < levelThresholdsForGrowth.Length; i++)
        {
            if (level >= levelThresholdsForGrowth[i])
            {
                newGrowthStage = i + 1;
            }
            else
            {
                break;
            }
        }
        
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