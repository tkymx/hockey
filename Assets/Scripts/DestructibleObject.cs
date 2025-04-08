using UnityEngine;
using System;

public class DestructibleObject : MonoBehaviour
{
    [Header("Object Properties")]
    [SerializeField] private float maxHitPoints = 100f;
    [SerializeField] private int pointValue = 100;
    [SerializeField] private GameObject explosionPrefab;
    
    private float currentHitPoints;
    private bool isDestroyed = false;
    
    // オブジェクト破壊時のイベント（破壊されたオブジェクトとスコアポイントを通知）
    public event Action<DestructibleObject, int> OnObjectDestroyed;

    private void Awake()
    {
        // コライダーがない場合は自動で追加
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            // レンダラーのバウンズを取得
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                // バウンディングボックスに基づいてBoxColliderを追加
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.center = renderer.bounds.center - transform.position;
                boxCollider.size = renderer.bounds.size;
                // トリガーに設定
                boxCollider.isTrigger = true;
            }
            else
            {
                // レンダラーがない場合はデフォルトサイズのBoxColliderを追加
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                Debug.LogWarning($"レンダラーが見つからないため、デフォルトサイズのコライダーを追加しました: {gameObject.name}");
            }
        }
        else
        {
            // 既存のコライダーをトリガーに設定
            collider.isTrigger = true;
        }
        
        Initialize();
    }
    
    public void Initialize()
    {
        isDestroyed = false;
        currentHitPoints = maxHitPoints;
        
        // コライダーを有効化
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
            // 確実にトリガーになるようにする
            collider.isTrigger = true;
        }
        
        // レンダラーを有効化
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
        }
    }
    
    // 戻り値をboolに変更して、破壊されたかどうかを返すようにする
    public bool TakeDamage(float amount, GameObject source = null)
    {
        if (isDestroyed || amount <= 0) return false;
        
        currentHitPoints -= amount;
        
        if (currentHitPoints <= 0)
        {
            DestroyObject(source);
            return true; // オブジェクトが破壊されたことを返す
        }
        
        return false;
    }
    
    public bool Hit(float force, Player player)
    {
        if (isDestroyed) return false;

        float damage = force * (player != null ? player.GetDamageMultiplier() : 1.0f);
        return TakeDamage(damage, player ? player.gameObject : null);
    }
    
    private void DestroyObject(GameObject source)
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        
        // 爆発エフェクトを生成
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        // 破壊時のスコアポイントをイベントで通知
        OnObjectDestroyed?.Invoke(this, pointValue);
        
        // オブジェクトを非表示にする
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        // コライダーを無効化
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // 一定時間後にゲームオブジェクトを削除
        Destroy(gameObject, 2.0f);
    }
    
    public bool IsDestroyed()
    {
        return isDestroyed;
    }
    
    public int GetPointValue()
    {
        return pointValue;
    }
    
    public float GetHealthPercentage()
    {
        return currentHitPoints / maxHitPoints;
    }
}