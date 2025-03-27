using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int _currentScore = 0;
    private int _highScore = 0;

    public delegate void ScoreChangedHandler(int newScore);
    public event ScoreChangedHandler OnScoreChanged;

    public void AddPoints(int points)
    {
        _currentScore += points;
        if (_currentScore > _highScore)
        {
            _highScore = _currentScore;
        }
        OnScoreChanged?.Invoke(_currentScore);
    }

    public int GetCurrentScore()
    {
        return _currentScore;
    }

    public int GetHighScore()
    {
        return _highScore;
    }

    public void ResetScore()
    {
        _currentScore = 0;
        OnScoreChanged?.Invoke(_currentScore);
    }
}