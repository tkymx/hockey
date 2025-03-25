using UnityEngine;
using System;

public class DestructibleObject : MonoBehaviour
{
    [Header("Object Properties")]
    [SerializeField] private int pointValue = 100;
    [SerializeField] private float hitThreshold = 5.0f;
    [SerializeField] private GameObject explosionPrefab;
    
    private bool isDestroyed = false;
    
    // オブジェクト破壊時のイベント
    public event Action<DestructibleObject, int> OnObjectDestroyed;
    
    public void Initialize()
    {
        isDestroyed = false;
    }
    
    public void Hit(float force)
    {
        if (isDestroyed) return;
        
        if (force >= hitThreshold)
        {
            Destroy();
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
        GetComponent<Renderer>().enabled = false;
        
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
            // パックの速度から衝突の力を計算
            float impactForce = collision.relativeVelocity.magnitude;
            Hit(impactForce);
        }
    }
}