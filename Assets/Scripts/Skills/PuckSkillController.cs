using UnityEngine;
using System.Collections.Generic;
using Hockey.Data;

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
    
    // エフェクトマネージャーへの参照
    private ParticleEffectManager effectManager;
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize(Puck puck)
    {
        this.puck = puck;
        originalScale = transform.localScale;
        ResetAllSkills();
        
        // エフェクトマネージャーを取得
        FindEffectManager();
        
        // パックの成長段階変更イベントを購読
        if (this.puck != null)
        {
            this.puck.OnGrowthStageChanged += HandlePuckGrowthStageChanged;
        }
    }
    
    /// <summary>
    /// パックの成長段階が変更された時の処理
    /// </summary>
    private void HandlePuckGrowthStageChanged(int newStage)
    {
        // 成長段階が変更されたらサイズ効果を再適用（スキル効果を維持する）
        ApplySizeEffect();
    }
    
    /// <summary>
    /// エフェクトマネージャーを取得
    /// </summary>
    private void FindEffectManager()
    {
        // 子オブジェクトに ParticleEffectManager があるか確認
        effectManager = GetComponentInChildren<ParticleEffectManager>();
        
        // なければ、親/同じヒエラルキー階層から検索
        if (effectManager == null)
        {
            effectManager = transform.root.GetComponentInChildren<ParticleEffectManager>();
        }
        
        // まだ見つからなければ、シーン内から検索
        if (effectManager == null)
        {
            effectManager = FindObjectOfType<ParticleEffectManager>();
        }
        
        // 見つかったら初期化
        if (effectManager != null)
        {
            effectManager.Initialize();
        }
    }
    
    /// <summary>
    /// パックのサイズ倍率を設定
    /// </summary>
    public void SetSizeMultiplier(float multiplier)
    {
        float prevMultiplier = sizeMultiplier;
        sizeMultiplier = Mathf.Max(1.0f, multiplier);
        ApplySizeEffect();
        
        // 値が増加した場合のみエフェクトを再生
        if (sizeMultiplier > prevMultiplier)
        {
            PlaySkillEffectForType(SkillType.PuckSizeUp);
        }
    }
    
    /// <summary>
    /// パックのダメージ倍率を設定
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        float prevMultiplier = damageMultiplier;
        damageMultiplier = Mathf.Max(1.0f, multiplier);
        
        // 値が増加した場合のみエフェクトを再生
        if (damageMultiplier > prevMultiplier)
        {
            PlaySkillEffectForType(SkillType.PuckDamageUp);
        }
    }
    
    /// <summary>
    /// パックの貫通回数を設定
    /// </summary>
    public void SetPenetrationCount(int count)
    {
        int prevCount = penetrationCount;
        penetrationCount = Mathf.Max(0, count);
        
        // 値が増加した場合のみエフェクトを再生
        if (penetrationCount > prevCount)
        {
            PlaySkillEffectForType(SkillType.PuckPenetration);
        }
    }
    
    /// <summary>
    /// 特定のスキルタイプのエフェクトを再生
    /// </summary>
    public void PlayEffectForSkillType(SkillType skillType)
    {
        PlaySkillEffectForType(skillType);
    }
    
    /// <summary>
    /// スキルエフェクトを再生
    /// </summary>
    private void PlaySkillEffectForType(SkillType skillType)
    {
        // エフェクトマネージャーがなければ検索
        if (effectManager == null)
        {
            FindEffectManager();
        }
        
        // エフェクトを再生
        if (effectManager != null)
        {
            effectManager.PlayEffectForSkillType(skillType, transform.position);
        }
        else
        {
            Debug.LogWarning("ParticleEffectManagerが見つかりません。スキルエフェクトを再生できません。");
        }
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
        // パックの現在のベースサイズを取得し、それにスキル倍率を適用する
        if (puck != null)
        {
            // 注：パックが現在使用している成長段階のサイズを取得
            Vector3 currentGrowthScale = puck.GetCurrentBaseScale();
            
            // 現在の成長段階サイズにスキル効果を掛け合わせる
            transform.localScale = new Vector3(
                currentGrowthScale.x * sizeMultiplier,
                currentGrowthScale.y, // 高さはそのまま
                currentGrowthScale.z * sizeMultiplier
            );
        }
        else
        {
            // パックが見つからない場合は、元のスケールとスキル効果のみを適用
            transform.localScale = new Vector3(
                originalScale.x * sizeMultiplier,
                originalScale.y,
                originalScale.z * sizeMultiplier
            );
        }
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
    
    private void OnDestroy()
    {
        // イベント購読を解除
        if (puck != null)
        {
            puck.OnGrowthStageChanged -= HandlePuckGrowthStageChanged;
        }
    }
}