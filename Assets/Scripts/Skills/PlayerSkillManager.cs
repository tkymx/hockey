using UnityEngine;
using System;
using System.Collections.Generic;
using Hockey.Data;

[Serializable]
public class PlayerSkill
{
    public string skillId;
    public int level;

    public PlayerSkill(string id, int lvl = 1)
    {
        skillId = id;
        level = lvl;
    }
}

public class PlayerSkillManager : MonoBehaviour
{
    // プレイヤーが獲得したスキルのリスト
    [SerializeField] private List<PlayerSkill> acquiredSkills = new List<PlayerSkill>();
    
    // GameConfigRepositoryのみ保持
    private GameConfigRepository configRepository;
    
    // 参照するコンポーネント
    private Player player;
    private Puck puck;
    
    // スキル獲得イベント
    public event Action<SkillType, int> OnSkillAcquired;
    
    public void Initialize(PuckController puckController, GameConfigRepository repository)
    {
        configRepository = repository;
        
        player = GetComponent<Player>();
        puck = puckController.Puck;
        
        if (player == null)
        {
            Debug.LogError("PlayerSkillManagerはPlayerコンポーネントと同じゲームオブジェクトにアタッチしてください");
            return;
        }
        
        if (configRepository == null)
        {
            Debug.LogError("GameConfigRepositoryが設定されていません");
            return;
        }
    }

    private void OnDestroy()
    {
        // イベント購読の解除も削除
        // if (player != null)
        // {
        //     player.OnLevelChanged -= OnPlayerLevelUp;
        // }
    }

    // ランダムなスキルオプションを取得
    public List<SkillData> GetRandomSkillOptions(int count)
    {
        if (configRepository == null || configRepository.Skills == null) 
            return new List<SkillData>();

        List<SkillData> allSkills = configRepository.Skills;
        List<SkillData> validOptions = new List<SkillData>();
        List<SkillData> result = new List<SkillData>();

        // 有効なスキルオプションをフィルタリング
        foreach (SkillData skill in allSkills)
        {
            PlayerSkill playerSkill = acquiredSkills.Find(s => s.skillId == skill.skillId);
            if (playerSkill == null || playerSkill.level < skill.maxLevel)
            {
                validOptions.Add(skill);
            }
        }

        // ランダムにcount個のスキルを選択
        int optionsCount = Mathf.Min(count, validOptions.Count);
        for (int i = 0; i < optionsCount; i++)
        {
            if (validOptions.Count == 0) break;

            int randomIndex = UnityEngine.Random.Range(0, validOptions.Count);
            result.Add(validOptions[randomIndex]);
            validOptions.RemoveAt(randomIndex);
        }

        return result;
    }

    // スキル選択の結果を処理
    public void AcquireSkill(string skillId)
    {
        SkillData selectedSkill = configRepository.GetSkillById(skillId);
        if (selectedSkill == null) return;

        bool isNewSkill = false;
        int newLevel = 1;
        
        // すでに持っているスキルかチェック
        PlayerSkill existingSkill = acquiredSkills.Find(s => s.skillId == skillId);
        if (existingSkill != null)
        {
            // レベルアップ
            if (existingSkill.level < selectedSkill.maxLevel)
            {
                existingSkill.level++;
                newLevel = existingSkill.level;
                ApplySkillEffects(selectedSkill, existingSkill.level);
            }
        }
        else
        {
            // 新規スキル獲得
            isNewSkill = true;
            PlayerSkill newSkill = new PlayerSkill(skillId);
            acquiredSkills.Add(newSkill);
            ApplySkillEffects(selectedSkill, newSkill.level);
        }
        
        // スキル獲得イベントを発行
        OnSkillAcquired?.Invoke(selectedSkill.skillType, newLevel);
        
        // スキル獲得エフェクトを表示
        PlaySkillAcquisitionEffect(selectedSkill.skillType, isNewSkill);
        
        Debug.Log($"スキルを獲得しました: {selectedSkill.skillName} (Lv.{newLevel}) - {(isNewSkill ? "新規" : "レベルアップ")}");
    }
    
    // スキル獲得エフェクトの再生
    private void PlaySkillAcquisitionEffect(SkillType skillType, bool isNewSkill)
    {
        if (puck == null) return;
        
        PuckSkillController skillController = puck.GetComponent<PuckSkillController>();
        if (skillController == null) return;
        
        // 特定のスキルタイプに対応するエフェクトを明示的に再生
        skillController.PlayEffectForSkillType(skillType);
    }
    
    // 特定のスキルを削除する
    public void RemoveSkill(string skillId)
    {
        // スキルデータを取得
        SkillData skillData = configRepository.GetSkillById(skillId);
        if (skillData == null) return;
        
        // スキルを探して削除
        PlayerSkill skill = acquiredSkills.Find(s => s.skillId == skillId);
        if (skill != null)
        {
            acquiredSkills.Remove(skill);
            
            // スキル効果を削除
            RemoveSkillEffects(skillData);
            
            Debug.Log($"スキルを削除しました: {skillId} ({skillData.skillName})");
        }
    }
    
    // スキル効果を削除
    private void RemoveSkillEffects(SkillData skill)
    {
        if (puck == null) return;
        
        PuckSkillController skillController = puck.GetComponent<PuckSkillController>();
        if (skillController == null) return;
        
        switch (skill.skillType)
        {
            case SkillType.PuckSizeUp:
                skillController.ResetSizeMultiplier();
                break;
            case SkillType.PuckDamageUp:
                skillController.ResetDamageMultiplier();
                break;
            case SkillType.PuckPenetration:
                skillController.ResetPenetrationCount();
                break;
        }
        
        // すべてのスキルを再適用（削除したスキル以外）
        ReapplyAllSkills();
    }
    
    // すべてのスキルを再適用
    private void ReapplyAllSkills()
    {
        // スキルコントローラをリセット
        PuckSkillController skillController = puck.GetComponent<PuckSkillController>();
        if (skillController != null)
        {
            skillController.ResetAllSkills();
        }
        
        // 獲得済みスキルを再適用
        foreach (PlayerSkill playerSkill in acquiredSkills)
        {
            SkillData skillData = configRepository.GetSkillById(playerSkill.skillId);
            if (skillData != null)
            {
                ApplySkillEffects(skillData, playerSkill.level);
            }
        }
    }

    // スキル効果を適用
    private void ApplySkillEffects(SkillData skill, int level)
    {
        if (puck == null) return;

        switch (skill.skillType)
        {
            case SkillType.PuckSizeUp:
                ApplyPuckSizeSkill(skill.GetEffectValue(level));
                break;
            case SkillType.PuckDamageUp:
                ApplyPuckDamageSkill(skill.GetEffectValue(level));
                break;
            case SkillType.PuckPenetration:
                ApplyPuckPenetrationSkill((int)skill.GetEffectValue(level));
                break;
        }
    }

    // パックサイズスキルの適用
    private void ApplyPuckSizeSkill(float sizeMultiplier)
    {
        PuckSkillController skillController = GetOrAddPuckSkillController();
        skillController.SetSizeMultiplier(sizeMultiplier);
    }

    // パック攻撃力スキルの適用
    private void ApplyPuckDamageSkill(float damageMultiplier)
    {
        PuckSkillController skillController = GetOrAddPuckSkillController();
        skillController.SetDamageMultiplier(damageMultiplier);
    }

    // パック貫通スキルの適用
    private void ApplyPuckPenetrationSkill(int penetrationCount)
    {
        PuckSkillController skillController = GetOrAddPuckSkillController();
        skillController.SetPenetrationCount(penetrationCount);
    }

    // PuckSkillControllerを取得または追加
    private PuckSkillController GetOrAddPuckSkillController()
    {
        PuckSkillController controller = puck.GetComponent<PuckSkillController>();
        if (controller == null)
        {
            controller = puck.gameObject.AddComponent<PuckSkillController>();
            controller.Initialize(puck);
        }
        return controller;
    }

    // プレイヤーのスキル情報をリセット
    public void ResetSkills()
    {
        acquiredSkills.Clear();
        
        // パックのスキルコントローラもリセット
        if (puck != null)
        {
            PuckSkillController controller = puck.GetComponent<PuckSkillController>();
            if (controller != null)
            {
                controller.ResetAllSkills();
            }
        }
    }

    // 特定のスキルのレベルを取得
    public int GetSkillLevel(string skillId)
    {
        PlayerSkill skill = acquiredSkills.Find(s => s.skillId == skillId);
        return skill != null ? skill.level : 0;
    }

    // 獲得したスキルのリストを取得
    public List<PlayerSkill> GetAcquiredSkills()
    {
        return acquiredSkills;
    }
}