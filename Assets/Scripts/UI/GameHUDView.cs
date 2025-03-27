using UnityEngine;
using TMPro;

public class GameHUDView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        UpdateScore(0);
        UpdateTime(0);
        UpdateLevel(1);
    }

    internal void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"スコア: {score}";
        }
    }

    internal void UpdateTime(float time)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timeText.text = $"残り時間: {minutes:00}:{seconds:00}";
        }
    }

    internal void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"レベル: {level}";
        }
    }
}