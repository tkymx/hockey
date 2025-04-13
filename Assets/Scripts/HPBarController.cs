using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 個々のHPゲージを制御するクラス
/// </summary>
public class HPBarController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 2.0f;
    [SerializeField] private float hideDelay = 1.0f;
    [SerializeField] private bool hideWhenFull = true;
    
    private Camera mainCamera;
    private Transform targetTransform;
    private float currentHealth = 1.0f;
    private float targetAlpha = 1.0f;
    private float hideDelayTimer = 0f;
    private bool isInitialized = false;
    
    /// <summary>
    /// HPゲージを初期化
    /// </summary>
    /// <param name="target">追跡対象のTransform（破壊可能オブジェクト）</param>
    public void Initialize(Transform target)
    {
        targetTransform = target;
        mainCamera = Camera.main;
        
        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
        }
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // 初期値は最大HP
        currentHealth = 1.0f;
        UpdateHealthBar(1.0f);
        
        // 満タン時は非表示にするオプションが有効の場合
        if (hideWhenFull)
        {
            canvasGroup.alpha = 0;
        }
        
        isInitialized = true;
    }
    
    private void Update()
    {
        if (!isInitialized || targetTransform == null)
        {
            // ターゲットが破壊された場合は自分自身も破棄
            Destroy(gameObject);
            return;
        }
        
        // ターゲットの位置に追従
        UpdatePosition();
        
        // フェードイン・アウトの処理
        UpdateVisibility();
    }
    
    /// <summary>
    /// HPゲージの値を更新
    /// </summary>
    /// <param name="healthPercentage">体力の割合（0.0f～1.0f）</param>
    public void UpdateHealthBar(float healthPercentage)
    {
        currentHealth = Mathf.Clamp01(healthPercentage);
        
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        
        // 満タン時は非表示、それ以外は表示
        targetAlpha = (hideWhenFull && currentHealth >= 1.0f) ? 0.0f : 1.0f;
        
        // 表示する場合はタイマーをリセット
        if (targetAlpha > 0.0f)
        {
            hideDelayTimer = hideDelay;
        }
    }
    
    private void UpdatePosition()
    {
        if (targetTransform != null && mainCamera != null)
        {
            // ターゲットのワールド座標をスクリーン座標に変換
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetTransform.position + Vector3.up * 1.2f);
            
            // 画面外の場合は表示しない
            if (screenPos.z < 0)
            {
                canvasGroup.alpha = 0;
                return;
            }
            
            // UI要素の位置を更新
            transform.position = screenPos;
        }
    }
    
    private void UpdateVisibility()
    {
        // 満タンで非表示設定の場合
        if (hideWhenFull && currentHealth >= 1.0f)
        {
            targetAlpha = 0.0f;
        }
        else
        {
            if (hideDelayTimer > 0)
            {
                hideDelayTimer -= Time.deltaTime;
            }
            else if (hideDelayTimer <= 0)
            {
                targetAlpha = 0.0f;
            }
        }
        
        // アルファ値を徐々に目標値に近づける
        if (canvasGroup.alpha != targetAlpha)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }
}