using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // SceneManagementの参照を追加

public class GameManager : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private StageManager stageManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private MouseInputController mouseInputController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private GrowthManager growthManager;
    [SerializeField] private StageController stageController;
    
    [Header("UI References")]
    [SerializeField] private GameHUDView gameHUDView;
    [SerializeField] private GameOverMenuView gameOverMenuView;
    
    [Header("Game Elements")]
    [SerializeField] private PuckController puckController;
    [SerializeField] private Transform puckSpawnPoint;
    
    private List<DestructibleObject> destructibleObjects = new List<DestructibleObject>();
    private bool isGameActive = false;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (stageManager == null || playerManager == null || mouseInputController == null || 
            cameraController == null || scoreManager == null || timeManager == null ||
            gameHUDView == null || gameOverMenuView == null || growthManager == null ||
            stageController == null)
        {
            Debug.LogError("Required components are not assigned to GameManager!");
            return;
        }
        
        // プレイヤーの取得
        Player player = playerManager.GetPlayer();
        
        // StageManagerの初期化
        stageManager.Initialize();
        
        // StageControllerの初期化（プレイヤーとStageManagerの参照を渡す）
        stageController.Initialize(player, stageManager);
        
        StartGame();

        // イベントの登録
        timeManager.OnTimeChanged.AddListener(HandleTimeChanged);
        timeManager.OnTimeUp.AddListener(HandleGameOver);
        gameOverMenuView.OnRestartRequested.AddListener(RestartGame);

        // GrowthManagerの初期化
        growthManager.Initialize(playerManager, puckController);

        // プレイヤーのレベル変更イベントを購読（UI更新用）
        if (player != null)
        {
            gameHUDView.UpdateLevel(player.Level);
            player.OnLevelChanged += HandlePlayerLevelChanged;
        }
    }

    private void StartGame()
    {
        stageManager.LoadStage();
        playerManager.Initialize();
        
        // スコアのリセット
        scoreManager.ResetScore();
        
        // 時間のリセットと開始
        timeManager.ResetTimer();
        timeManager.StartTimer();
        
        // 破壊可能オブジェクトを収集
        CollectDestructibleObjects();
        
        // パックの初期化
        InitializePuck();
        
        // プレイヤーを初期位置にリセット
        if (playerManager != null)
        {
            playerManager.ResetPlayer();
        }
        
        // プレイヤーのレベル変更イベントを再購読（UI更新用）
        Player player = playerManager.GetPlayer();
        if (player != null)
        {
            // 既存の購読を解除してから再購読
            player.OnLevelChanged -= HandlePlayerLevelChanged;
            player.OnLevelChanged += HandlePlayerLevelChanged;
            // 現在のレベルでUIを更新
            gameHUDView.UpdateLevel(player.Level);
        }
        
        isGameActive = true;
        gameOverMenuView.Hide();
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
        if (Input.touchCount > 0 || Input.GetMouseButton(0)) // タッチ入力またはマウス入力
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
        DestructibleObject[] objects = FindObjectsByType<DestructibleObject>(FindObjectsSortMode.None);
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
        scoreManager.AddPoints(points);
        gameHUDView.UpdateScore(scoreManager.GetCurrentScore());
    }
    
    private void HandleTimeChanged(float remainingTime)
    {
        gameHUDView.UpdateTime(remainingTime);
    }
    
    private void HandleGameOver()
    {
        isGameActive = false;
        gameOverMenuView.Show(scoreManager.GetCurrentScore(), scoreManager.GetHighScore());
    }
    
    private void RestartGame()
    {
        // シーンを再ロードしてゲームをリセットする
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ゲームの一時停止/再開
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        if (active)
        {
            timeManager.StartTimer();
        }
        else
        {
            timeManager.StopTimer();
        }
    }

    // プレイヤーのレベルが変更された時のUIの更新処理のみ
    private void HandlePlayerLevelChanged(int newLevel)
    {
        if (gameHUDView != null)
        {
            gameHUDView.UpdateLevel(newLevel);
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

        if (timeManager != null)
        {
            timeManager.OnTimeChanged.RemoveListener(HandleTimeChanged);
            timeManager.OnTimeUp.RemoveListener(HandleGameOver);
        }

        if (gameOverMenuView != null)
        {
            gameOverMenuView.OnRestartRequested.RemoveListener(RestartGame);
        }
    }
}