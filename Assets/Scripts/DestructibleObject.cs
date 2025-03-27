using UnityEngine;
using System;

public class DestructibleObject : MonoBehaviour
{
    [Header("Object Properties")]
    [SerializeField] private int pointValue = 100;
    [SerializeField] private int requiredLevel = 1; // このオブジェクトを破壊するために必要なプレイヤーレベル
    [SerializeField] private GameObject explosionPrefab;
    
    private bool isDestroyed = false;
    
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
            }
            else
            {
                // レンダラーがない場合はデフォルトサイズのBoxColliderを追加
                gameObject.AddComponent<BoxCollider>();
                Debug.LogWarning($"レンダラーが見つからないため、デフォルトサイズのコライダーを追加しました: {gameObject.name}");
            }
        }
    }
    
    public void Initialize()
    {
        isDestroyed = false;
        
        // コライダーを有効化
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        
        // レンダラーを有効化
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
        }
    }
    
    public void Hit(float force, Player player)
    {
        if (isDestroyed) return;

        // プレイヤーのレベルが要求レベル以上の場合のみ破壊可能
        if (player != null && player.GetBreakableObjectLevel() >= requiredLevel)
        {
            Destroy();
            // 経験値として破壊難易度 * 基本ポイントを付与
            bool didLevelUp = player.GainExperience(pointValue * requiredLevel);
            if (didLevelUp)
            {
                // レベルアップ時の処理（エフェクトなど）
                Debug.Log($"Player leveled up to {player.Level}!");
            }
        }
    }
    
    public void Destroy()
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        
        // 爆発エフェクトを生成
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        // イベント発火
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
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Puck>() != null)
        {
            // パックの所有者（最後に触ったプレイヤー）を取得
            Puck puck = collision.gameObject.GetComponent<Puck>();
            Player player = puck.GetLastHitPlayer();
            
            float impactForce = collision.relativeVelocity.magnitude;
            Hit(impactForce, player);
        }
    }

    // 要求レベル取得用のプロパティ
    public int RequiredLevel => requiredLevel;
}