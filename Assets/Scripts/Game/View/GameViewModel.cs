using UnityEngine;

public class GameViewModel
{
    public GameState gameState;
    public int score;
    public float timer;
    public int combo;
    
    private GameModel gameModel;
    
    public void SetModel(GameModel model)
    {
        gameModel = model;
    }

    public void UpdateDisplayData()
    {
        if (gameModel != null)
        {
            gameState = gameModel.GameState;
            score = gameModel.CurrentScore;
            timer = gameModel.Timer;
            combo = gameModel.ComboCount;
        }
    }
}