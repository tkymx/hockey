using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private StageManager stageManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private MouseInputController mouseInputController;
    [SerializeField] private CameraController cameraController;
    
    [Header("Game Elements")]
    [SerializeField] private PuckController puckController;
    [SerializeField] private Transform puckSpawnPoint;
    
    private List<DestructibleObject> destructibleObjects = new List<DestructibleObject>();
    private bool isGameActive = false;
    private int score = 0;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (stageManager == null || playerManager == null || mouseInputController == null || cameraController == null)
        {
            Debug.LogError("Required components are not assigned to GameManager!");
            return;
        }

        stageManager.Initialize();
        stageManager.LoadStage();
        
        playerManager.Initialize();
        
        // 破壊可能オブジェクトを収集
        CollectDestructibleObjects();
        
        // パックの初期化
        InitializePuck();
        
        isGameActive = true;
    }

    private void Update()
    {
        if (!isGameActive) return;

        // プレイヤーの移動処理
        HandlePlayerMovement();

        // カメラの位置更新
        if (cameraController != null && stageManager != null)
        {
            Vector3 stageCenter = stageManager.GetStageCenter();
            Vector3 stageBounds = stageManager.GetStageBounds();
            cameraController.UpdateCameraPosition(stageCenter, stageBounds);
        }
    }
    
    private void HandlePlayerMovement()
    {
        if (Input.GetMouseButton(0)) // 左クリック中
        {
            Vector3 mousePosition = mouseInputController.GetMouseWorldPosition();
            Player player = playerManager.GetPlayer();
            if (player != null)
            {
                player.MoveTo(mousePosition);
            }
        }
    }
    
    private void CollectDestructibleObjects()
    {
        destructibleObjects.Clear();
        
        // シーン内の全ての破壊可能オブジェクトを取得
        DestructibleObject[] objects = FindObjectsOfType<DestructibleObject>();
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                destructibleObjects.Add(obj);
                
                // 破壊イベントを購読
                obj.OnObjectDestroyed += HandleObjectDestroyed;
            }
        }
        
        Debug.Log($"{destructibleObjects.Count}個の破壊可能オブジェクトを見つけました。");
    }
    
    private void InitializePuck()
    {
        if (puckController == null)
        {
            Debug.LogWarning("PuckControllerがGameManagerに設定されていません。");
            return;
        }

        // PuckControllerが必要なコンポーネントを持っているか確認
        Puck puck = puckController.Puck;
        
        if (puck == null)
        {
            Debug.LogError("PuckControllerにPuckコンポーネントがありません。");
            return;
        }
        
        // パックの初期位置を設定
        Vector3 spawnPosition = (puckSpawnPoint != null) ? 
            puckSpawnPoint.position : 
            new Vector3(0, 0.5f, 0); // デフォルト位置
        
        puckController.ResetPuck(spawnPosition);
    }
    
    // オブジェクトが破壊された時の処理
    private void HandleObjectDestroyed(DestructibleObject obj, int points)
    {
        score += points;
        Debug.Log($"オブジェクトが破壊されました！ 現在のスコア: {score}");
    }
    
    // ステージをリセット
    public void ResetStage()
    {
        // パックをリセット
        if (puckController != null)
        {
            Vector3 spawnPosition = (puckSpawnPoint != null) ? 
                puckSpawnPoint.position : 
                new Vector3(0, 0.5f, 0);
            puckController.ResetPuck(spawnPosition);
        }
        
        // プレイヤーを初期位置にリセット
        if (playerManager != null)
        {
            playerManager.ResetPlayer();
        }
        
        // 破壊されたオブジェクトを再度収集
        CollectDestructibleObjects();
        
        // スコアをリセット
        score = 0;
    }
    
    // ゲームの一時停止/再開
    public void SetGameActive(bool active)
    {
        isGameActive = active;
    }
    
    // 現在のスコアを取得
    public int GetScore()
    {
        return score;
    }
}