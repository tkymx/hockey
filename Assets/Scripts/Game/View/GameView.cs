using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameView : MonoBehaviour, IGameView
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private GameObject pausePanel;
    
    private GameViewModel gameViewModel;

    private void Awake()
    {
        // パネルを初期状態で非表示にする
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        
        // ボタンイベントの設定
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (pauseButton != null) pauseButton.onClick.AddListener(() => {
            if (pausePanel != null) pausePanel.SetActive(true);
        });
        if (resumeButton != null) resumeButton.onClick.AddListener(() => {
            if (pausePanel != null) pausePanel.SetActive(false);
        });
    }

    public void Initialize(GameViewModel viewModel)
    {
        gameViewModel = viewModel;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (gameViewModel == null) return;
        
        // スコアの更新
        if (scoreText != null)
        {
            scoreText.text = $"Score: {gameViewModel.score}";
        }
        
        // タイマーの更新
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameViewModel.timer / 60);
            int seconds = Mathf.FloorToInt(gameViewModel.timer % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        
        // コンボの更新
        if (comboText != null)
        {
            comboText.text = $"Combo: {gameViewModel.combo}x";
            comboText.gameObject.SetActive(gameViewModel.combo > 1); // コンボが1より大きい場合のみ表示
        }
        
        // ゲーム状態に応じたUIの切り替え
        switch (gameViewModel.gameState)
        {
            case GameState.Playing:
                if (pausePanel != null) pausePanel.SetActive(false);
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                break;
                
            case GameState.Paused:
                if (pausePanel != null) pausePanel.SetActive(true);
                break;
                
            case GameState.Ended:
                ShowGameOver();
                break;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {gameViewModel.score}";
            }
        }
    }
    
    private void RestartGame()
    {
        // シーンをリロードしてゲームを再開
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}

public interface IGameView
{
    void Initialize(GameViewModel viewModel);
    void UpdateUI();
    void ShowGameOver();
}