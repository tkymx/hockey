public class ScoreUseCase : IScoreUseCase
{
    private GameModel gameModel;
    private GameConfig config;

    public ScoreUseCase(GameModel model)
    {
        gameModel = model;
        config = ConfigManager.Instance.GetConfig();
    }

    public void AddScore(int points)
    {
        gameModel.UpdateScore(gameModel.CurrentScore + points);
        UpdateCombo();
    }

    public void UpdateCombo()
    {
        // コンボ更新のロジック
        gameModel.UpdateComboCount(gameModel.ComboCount + 1);
        CalculateComboBonus();
    }

    public void ResetCombo()
    {
        // コンボリセットのロジック
        gameModel.UpdateComboCount(0);
    }

    private void CalculateComboBonus()
    {
        // コンボボーナスの計算ロジック - 設定から閾値とボーナス値を取得
        if (gameModel.ComboCount >= config.comboThreshold)
        {
            int bonusPoints = (gameModel.ComboCount / config.comboThreshold) * config.comboBonus;
            gameModel.UpdateScore(gameModel.CurrentScore + bonusPoints);
        }
    }
}