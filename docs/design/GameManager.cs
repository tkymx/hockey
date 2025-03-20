using UnityEngine;
using UnityEngine.Events;
using Hockey.Core;
using Hockey.Player;
using Hockey.Effects;
using System.Collections.Generic;

namespace Hockey.Management
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private float gameDuration = 180f; // 3分
        [SerializeField] private int targetScoreForWin = 50000;
        [SerializeField] private float timeScaleOnGrowth = 0.5f; // 成長時のスロー演出

        [Header("References")]
        [SerializeField] private PlayerPuck playerPuck;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject pausePanel;

        // イベント
        public UnityEvent onGameStart = new UnityEvent();
        public UnityEvent onGamePause = new UnityEvent();
        public UnityEvent onGameResume = new UnityEvent();
        public UnityEvent onGameOver = new UnityEvent();
        public UnityEvent<int> onScoreChanged = new UnityEvent<int>();
        public UnityEvent<float> onTimeChanged = new UnityEvent<float>();

        private GameState currentState = GameState.MainMenu;
        private float remainingTime;
        private int currentScore;
        private bool isPaused;

        private static GameManager instance;
        public static GameManager Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            remainingTime = gameDuration;
            currentScore = 0;
            isPaused = false;

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        private void Update()
        {
            if (currentState != GameState.Playing || isPaused) return;

            // 残り時間の更新
            remainingTime -= Time.deltaTime;
            onTimeChanged?.Invoke(remainingTime);

            // ゲーム終了条件のチェック
            if (remainingTime <= 0 || currentScore >= targetScoreForWin)
            {
                EndGame();
            }

            // ポーズ入力のチェック
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void StartGame()
        {
            if (currentState == GameState.Playing) return;

            InitializeGame();
            currentState = GameState.Playing;
            Time.timeScale = 1f;

            // プレイヤーの初期化
            if (playerPuck != null && spawnPoint != null)
            {
                playerPuck.transform.position = spawnPoint.position;
                playerPuck.ResetPuck();
            }

            onGameStart?.Invoke();
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing || isPaused) return;

            isPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);

            onGamePause?.Invoke();
        }

        public void ResumeGame()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);

            onGameResume?.Invoke();
        }

        public void TogglePause()
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        public void EndGame()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.GameOver;
            Time.timeScale = 1f;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            // ハイスコアの更新確認
            UpdateHighScore();

            onGameOver?.Invoke();
        }

        public void AddScore(int points)
        {
            if (currentState != GameState.Playing) return;

            int previousScore = currentScore;
            currentScore += points;
            onScoreChanged?.Invoke(currentScore);

            // 成長条件のチェックと演出
            CheckGrowthConditions(previousScore, currentScore);
        }

        private void CheckGrowthConditions(int previousScore, int newScore)
        {
            // 成長閾値の定義
            int[] growthThresholds = { 5000, 15000, 50000 };
            
            for (int i = 0; i < growthThresholds.Length; i++)
            {
                if (previousScore < growthThresholds[i] && newScore >= growthThresholds[i])
                {
                    StartGrowthSequence(i + 1);
                    break;
                }
            }
        }

        private void StartGrowthSequence(int newLevel)
        {
            // 一時的なスロー演出
            StartCoroutine(GrowthSlowMotion());

            // コマの成長
            if (playerPuck != null)
            {
                playerPuck.Grow(newLevel);
            }

            // エフェクト再生
            Vector3 puckPosition = playerPuck != null ? playerPuck.transform.position : Vector3.zero;
            EffectManager.Instance?.PlayEffect(EffectType.Growth, puckPosition);
            SoundManager.Instance?.PlaySound(SoundType.Growth);
        }

        private System.Collections.IEnumerator GrowthSlowMotion()
        {
            Time.timeScale = timeScaleOnGrowth;
            yield return new WaitForSecondsRealtime(1f);
            Time.timeScale = 1f;
        }

        private void UpdateHighScore()
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (currentScore > highScore)
            {
                PlayerPrefs.SetInt("HighScore", currentScore);
                PlayerPrefs.Save();
            }
        }

        // ゲーム状態の取得用プロパティ
        public GameState CurrentState => currentState;
        public float RemainingTime => remainingTime;
        public int CurrentScore => currentScore;
        public bool IsPaused => isPaused;
    }
}