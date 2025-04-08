using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// パックに適用されるスキル効果を制御するクラス
/// </summary>
public class PuckSkillController : MonoBehaviour
{
    // スキル効果の設定値
    private float sizeMultiplier = 1.0f;
    private float damageMultiplier = 1.0f;
    private int penetrationCount = 0;
    
    // 元のスケールを保持
    private Vector3 originalScale;
    
    // パックへの参照
    private Puck puck;
    
    // 貫通したオブジェクトを追跡
    private List<GameObject> penetratedObjects = new List<GameObject>();
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize(Puck puck)
    {
        this.puck = puck;
        originalScale = transform.localScale;
        ResetAllSkills();
    }
    
    /// <summary>
    /// パックのサイズ倍率を設定
    /// </summary>
    public void SetSizeMultiplier(float multiplier)
    {
        sizeMultiplier = Mathf.Max(1.0f, multiplier);
        ApplySizeEffect();
    }
    
    /// <summary>
    /// パックのダメージ倍率を設定
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(1.0f, multiplier);
    }
    
    /// <summary>
    /// パックの貫通回数を設定
    /// </summary>
    public void SetPenetrationCount(int count)
    {
        penetrationCount = Mathf.Max(0, count);
    }
    
    /// <summary>
    /// サイズ倍率をリセット
    /// </summary>
    public void ResetSizeMultiplier()
    {
        sizeMultiplier = 1.0f;
        ApplySizeEffect();
    }
    
    /// <summary>
    /// ダメージ倍率をリセット
    /// </summary>
    public void ResetDamageMultiplier()
    {
        damageMultiplier = 1.0f;
    }
    
    /// <summary>
    /// 貫通回数をリセット
    /// </summary>
    public void ResetPenetrationCount()
    {
        penetrationCount = 0;
        penetratedObjects.Clear();
    }
    
    /// <summary>
    /// 全てのスキル効果をリセット
    /// </summary>
    public void ResetAllSkills()
    {
        ResetSizeMultiplier();
        ResetDamageMultiplier();
        ResetPenetrationCount();
    }
    
    /// <summary>
    /// サイズ効果を適用
    /// </summary>
    private void ApplySizeEffect()
    {
        transform.localScale = originalScale * sizeMultiplier;
    }
    
    /// <summary>
    /// 破壊可能オブジェクトとの衝突を処理
    /// </summary>
    /// <returns>貫通したかどうか</returns>
    public bool ProcessDestructibleCollision(DestructibleObject destructible)
    {
        // 貫通スキルの処理
        if (penetrationCount > 0)
        {
            // まだ貫通していないオブジェクトの場合
            if (!penetratedObjects.Contains(destructible.gameObject))
            {
                // 貫通オブジェクトのリストに追加
                penetratedObjects.Add(destructible.gameObject);
                
                // 貫通回数を消費
                SetPenetrationCount(penetrationCount - 1);
                
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 現在のダメージ倍率を取得
    /// </summary>
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }
    
    /// <summary>
    /// 現在のサイズ倍率を取得
    /// </summary>
    public float GetSizeMultiplier()
    {
        return sizeMultiplier;
    }
    
    /// <summary>
    /// 現在の貫通回数を取得
    /// </summary>
    public int GetPenetrationCount()
    {
        return penetrationCount;
    }
}