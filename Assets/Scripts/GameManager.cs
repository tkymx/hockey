using UnityEngine;
using UnityEngine.SceneManagement;

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
        
        StartGame();

        // イベントの登録
        timeManager.OnTimeChanged.AddListener(HandleTimeChanged);
        timeManager.OnTimeUp.AddListener(HandleGameOver);
        gameOverMenuView.OnRestartRequested.AddListener(RestartGame);

        // GrowthManagerの初期化
        growthManager.Initialize(playerManager, puckController);

        // プレイヤーのレベル変更イベントを購読
        if (player != null)
        {
            gameHUDView.UpdateLevel(player.Level);
            player.OnLevelChanged += HandlePlayerLevelChanged;
        }
        
        // ゾーン変更イベントを購読
        stageController.OnZoneChanged += HandleZoneChanged;
        stageController.OnAllZonesCleared += HandleAllZonesCleared;
        
        // オブジェクト破壊イベントを購読
        stageController.OnObjectDestroyedInStage += HandleObjectDestroyedInStage;
    }

    private void StartGame()
    {
        // StageManagerの初期化
        stageManager.Initialize();
        stageManager.LoadStage();
        
        playerManager.Initialize();

        // スコアのリセット
        scoreManager.ResetScore();
        
        // 時間のリセットと開始
        timeManager.ResetTimer();
        timeManager.StartTimer();
        
        // パックの初期化
        InitializePuck();
        
        // プレイヤーを初期位置にリセット
        if (playerManager != null)
        {
            playerManager.ResetPlayer();
        }
        
        // StageControllerの初期化 - PlayerManagerを渡す
        stageController.Initialize(stageManager, playerManager);
        
        // UIを更新
        UpdateGameUI();
        
        // プレイヤーのレベル変更イベントを再購読
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
        if (cameraController != null && stageController != null)
        {
            ZoneController currentZone = stageController.GetCurrentZone();
            if (currentZone != null)
            {
                Vector3 zonePosition = currentZone.transform.position;
                Vector3 zoneBounds = stageController.GetCurrentZoneBounds();
                cameraController.UpdateCameraPosition(zonePosition, zoneBounds);
            }
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
    
    // ゾーン変更時の処理（StageControllerからのイベント受信）
    private void HandleZoneChanged(int newZoneIndex)
    {
        // UIを更新
        UpdateGameUI();
        
        // 新しいゾーンを開始したときの処理
        PlayZoneChangeEffect();
        
        gameHUDView.UpdateScore(scoreManager.GetCurrentScore());
    }
    
    // 全ゾーンクリア時の処理（StageControllerからのイベント受信）
    private void HandleAllZonesCleared()
    {
        gameHUDView.UpdateScore(scoreManager.GetCurrentScore());
        
        // 勝利演出
        PlayVictoryEffect();
        
        // ゲーム終了処理（すべてクリアでゲーム終了）
        HandleGameOver();
    }
    
    private void PlayZoneChangeEffect()
    {
        // ゾーン変更時のエフェクト再生
        int currentZone = stageController.GetCurrentZoneIndex() + 1;
        Debug.Log($"ゾーン {currentZone} に進みました！");
    }
    
    private void PlayVictoryEffect()
    {
        // 勝利時のエフェクト再生
        Debug.Log("すべてのゾーンをクリアしました！");
    }
    
    private void UpdateGameUI()
    {
        // ゾーン情報をUIに表示
        if (gameHUDView != null && stageController != null)
        {
            int currentZone = stageController.GetCurrentZoneIndex() + 1;
            int totalZones = stageController.GetTotalZoneCount();
            gameHUDView.UpdateZone(currentZone, totalZones);
            gameHUDView.UpdateScore(scoreManager.GetCurrentScore());
        }
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

    // 破壊可能オブジェクトが破壊されたときの処理
    private void HandleObjectDestroyedInStage(ZoneController zone, DestructibleObject obj, int points)
    {
        // スコアに加算
        if (scoreManager != null)
        {
            scoreManager.AddPoints(points);
            
            // UIの更新
            if (gameHUDView != null)
            {
                gameHUDView.UpdateScore(scoreManager.GetCurrentScore());
            }
        }
        
        // プレイヤーに経験値を付与する処理を追加
        if (playerManager != null)
        {
            Player player = playerManager.GetPlayer();
            if (player != null)
            {
                bool didLevelUp = player.GainExperience(points);
                if (didLevelUp)
                {
                    Debug.Log($"Player leveled up to {player.Level}!");
                }
            }
        }
        
        Debug.Log($"破壊オブジェクト +{points}ポイント（ゾーン{zone.ZoneLevel}）");
    }

    private void OnDestroy()
    {
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
        
        if (stageController != null)
        {
            stageController.OnZoneChanged -= HandleZoneChanged;
            stageController.OnAllZonesCleared -= HandleAllZonesCleared;
            stageController.OnObjectDestroyedInStage -= HandleObjectDestroyedInStage;
        }
    }
}