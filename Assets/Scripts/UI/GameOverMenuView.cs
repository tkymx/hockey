using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameOverMenuView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button restartButton;
    
    public UnityEvent OnRestartRequested = new UnityEvent();
    
    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(HandleRestartButtonClick);
        }
        
        gameObject.SetActive(false);
    }
    
    public void Show(int finalScore, int highScore)
    {
        gameObject.SetActive(true);
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"最終スコア: {finalScore}";
        }
        
        if (highScoreText != null)
        {
            highScoreText.text = $"ハイスコア: {highScore}";
        }
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    private void HandleRestartButtonClick()
    {
        Hide();
        OnRestartRequested.Invoke();
    }
    
    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(HandleRestartButtonClick);
        }
    }
}