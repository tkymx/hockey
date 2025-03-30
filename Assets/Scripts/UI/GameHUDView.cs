using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUDView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI zoneText;
    
    private void Start()
    {
        if (scoreText == null || timeText == null || levelText == null || zoneText == null)
        {
            Debug.LogError("Required UI elements not assigned to GameHUDView!");
        }
        
        // 初期値を設定
        UpdateScore(0);
        UpdateTime(0);
        UpdateLevel(1);
        UpdateZone(1, 1);
    }
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"スコア: {score}";
        }
    }
    
    public void UpdateTime(float time)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timeText.text = $"時間: {minutes:00}:{seconds:00}";
        }
    }
    
    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"レベル: {level}";
        }
    }
    
    public void UpdateZone(int currentZone, int totalZones)
    {
        if (zoneText != null)
        {
            zoneText.text = $"ゾーン: {currentZone}/{totalZones}";
        }
    }
}