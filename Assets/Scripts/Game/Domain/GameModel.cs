using UnityEngine;

public class GameModel
{
    private GameState gameState = GameState.NotStarted;
    private float timer = 0.0f;
    private int currentScore = 0;
    private int comboCount = 0;

    public GameState GameState => gameState;
    public float Timer => timer;
    public int CurrentScore => currentScore;
    public int ComboCount => comboCount;
    
    public void UpdateState(GameState state)
    {
        gameState = state;
    }

    public void UpdateTimer(float time)
    {
        timer = time;
    }

    public void UpdateScore(int score)
    {
        currentScore = score;
    }

    public void UpdateComboCount(int count)
    {
        comboCount = count;
    }
}

public enum GameState
{
    NotStarted,
    Playing,
    Paused,
    Ended
}