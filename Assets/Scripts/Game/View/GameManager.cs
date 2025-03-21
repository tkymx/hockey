using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PuckView puckView;
    [SerializeField] private GameView gameView;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private PuckCollisionHandler puckCollisionHandler;
    [SerializeField] private Transform objectSpawnParent;
    [SerializeField] private GameObject destructibleObjectPrefab;

    // Models
    private GameModel gameModel;
    private PuckModel puckModel;

    // UseCases
    private GameUseCase gameUseCase;
    private ScoreUseCase scoreUseCase;
    private PuckUseCase puckUseCase;

    // ViewModels
    private GameViewModel gameViewModel;
    private PuckViewModel puckViewModel;

    // Presenters
    private GamePresenter gamePresenter;
    private PuckPresenter puckPresenter;
    
    // Config
    private GameConfig config;
    
    // Object management
    private DestructibleObjectFactory objectFactory;
    private List<DestructibleObjectPresenter> destructibleObjects = new List<DestructibleObjectPresenter>();
    private float nextSpawnTime = 0f;

    private void Start()
    {
        // 設定のロード
        config = ConfigManager.Instance.GetConfig();
        
        InitializeGame();
    }

    private void Update()
    {
        if (gameModel.GameState == GameState.Playing)
        {
            // ゲームロジックを実行
            gameUseCase.Execute();
            
            // UIを更新
            gamePresenter.UpdateGameState();
            
            // オブジェクトのスポーン処理
            HandleObjectSpawning();
        }
    }

    private void InitializeGame()
    {
        // モデルの初期化
        gameModel = new GameModel();
        puckModel = new PuckModel();
        
        // UseCaseの初期化
        scoreUseCase = new ScoreUseCase(gameModel);
        gameUseCase = new GameUseCase(gameModel);
        puckUseCase = new PuckUseCase(puckModel, scoreUseCase);
        
        // ViewModelの初期化
        gameViewModel = new GameViewModel();
        puckViewModel = new PuckViewModel();
        
        // ViewModelにモデルを設定
        gameViewModel.SetModel(gameModel);
        puckViewModel.SetModel(puckModel);
        
        // Viewの初期化
        gameView.Initialize(gameViewModel);
        puckView.Initialize(puckViewModel);
        
        // Presenterの初期化
        gamePresenter = new GamePresenter(gameUseCase, gameViewModel, gameView);
        puckPresenter = new PuckPresenter(puckUseCase, puckViewModel, puckView);
        
        // 入力とコリジョンハンドラの初期化
        inputHandler.Initialize(puckPresenter);
        if (puckCollisionHandler != null)
        {
            puckCollisionHandler.Initialize(puckPresenter);
        }
        
        // オブジェクトファクトリの初期化
        objectFactory = new DestructibleObjectFactory(destructibleObjectPrefab, objectSpawnParent);
        
        // 初期オブジェクトの生成
        destructibleObjects.AddRange(objectFactory.CreateInitialObjects());
        
        // ゲーム開始
        gamePresenter.HandleGameStart();
    }

    private void HandleObjectSpawning()
    {
        // 設定からスポーン間隔を取得
        if (Time.time >= nextSpawnTime)
        {
            DestructibleObjectPresenter newObject = objectFactory.CreateObject();
            destructibleObjects.Add(newObject);
            nextSpawnTime = Time.time + config.spawnInterval;
        }
    }

    public void HandlePauseButtonClick()
    {
        gamePresenter.HandleGamePause();
    }

    public void HandleResumeButtonClick()
    {
        gamePresenter.HandleGameStart();
    }
}