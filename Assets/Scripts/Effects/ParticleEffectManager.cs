using System.Collections.Generic;
using UnityEngine;
using Hockey.Data;

/// <summary>
/// パーティクルエフェクトを管理するクラス
/// </summary>
public class ParticleEffectManager : MonoBehaviour
{
    [Header("スキル獲得エフェクト")]
    [SerializeField] private ParticleSystem sizeUpEffectPrefab;
    [SerializeField] private ParticleSystem damageUpEffectPrefab;
    [SerializeField] private ParticleSystem penetrationEffectPrefab;
    
    private Dictionary<SkillType, ParticleSystem> skillEffects = new Dictionary<SkillType, ParticleSystem>();
    private Transform effectParent;
    
    private void Awake()
    {
        // エフェクト配置用の親オブジェクトを作成
        effectParent = new GameObject("SkillEffects").transform;
        effectParent.SetParent(transform);
        effectParent.localPosition = Vector3.zero;
    }
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize()
    {
        // 既存のエフェクトがあれば削除
        foreach (Transform child in effectParent)
        {
            Destroy(child.gameObject);
        }
        
        skillEffects.Clear();
        
        // 各スキルタイプのエフェクトをインスタンス化
        InstantiateEffects();
    }
    
    /// <summary>
    /// エフェクトのインスタンス化
    /// </summary>
    private void InstantiateEffects()
    {
        // サイズアップ用エフェクト
        if (sizeUpEffectPrefab != null)
        {
            ParticleSystem sizeEffect = Instantiate(sizeUpEffectPrefab, effectParent);
            sizeEffect.gameObject.name = "SizeUpEffect";
            skillEffects[SkillType.PuckSizeUp] = sizeEffect;
        }
        
        // ダメージアップ用エフェクト
        if (damageUpEffectPrefab != null)
        {
            ParticleSystem damageEffect = Instantiate(damageUpEffectPrefab, effectParent);
            damageEffect.gameObject.name = "DamageUpEffect";
            skillEffects[SkillType.PuckDamageUp] = damageEffect;
        }
        
        // 貫通用エフェクト
        if (penetrationEffectPrefab != null)
        {
            ParticleSystem penetrationEffect = Instantiate(penetrationEffectPrefab, effectParent);
            penetrationEffect.gameObject.name = "PenetrationEffect";
            skillEffects[SkillType.PuckPenetration] = penetrationEffect;
        }
    }
    
    /// <summary>
    /// スキルタイプに応じたエフェクトを再生
    /// </summary>
    public void PlayEffectForSkillType(SkillType skillType)
    {
        if (skillEffects.TryGetValue(skillType, out ParticleSystem effect) && effect != null)
        {
            effect.transform.position = transform.position;
            effect.Clear();
            effect.Play();
        }
        else
        {
            Debug.LogWarning($"スキルタイプ {skillType} に対応するエフェクトが見つかりません");
        }
    }
    
    /// <summary>
    /// 位置を指定してエフェクトを再生
    /// </summary>
    public void PlayEffectForSkillType(SkillType skillType, Vector3 position)
    {
        if (skillEffects.TryGetValue(skillType, out ParticleSystem effect) && effect != null)
        {
            effect.transform.position = position;
            effect.Clear();
            effect.Play();
        }
        else
        {
            Debug.LogWarning($"スキルタイプ {skillType} に対応するエフェクトが見つかりません");
        }
    }
}