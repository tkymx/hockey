using UnityEngine;

public class GameUseCase : IGameUseCase
{
    private GameModel gameModel;
    private GameConfig config;

    public GameUseCase(GameModel model)
    {
        gameModel = model;
        config = ConfigManager.Instance.GetConfig();
    }

    public void Execute()
    {
        // ゲームのロジックを実行
        if (gameModel.GameState == GameState.Playing)
        {
            gameModel.UpdateTimer(gameModel.Timer - Time.deltaTime);
            if (gameModel.Timer <= 0)
            {
                EndGame();
            }
        }
    }

    public void StartGame()
    {
        gameModel.UpdateState(GameState.Playing);
        gameModel.UpdateTimer(config.gameDuration); // 設定から時間を取得
        gameModel.UpdateScore(config.initialScore); // 設定から初期スコアを取得
        gameModel.UpdateComboCount(0); // コンボをリセット
    }

    public void PauseGame()
    {
        gameModel.UpdateState(GameState.Paused);
    }

    public void EndGame()
    {
        gameModel.UpdateState(GameState.Ended);
    }

    private void ValidateGameState()
    {
        // ゲームの状態を検証
        if (gameModel.GameState == GameState.Ended)
        {
            // ゲーム終了時の処理
        }
    }
}