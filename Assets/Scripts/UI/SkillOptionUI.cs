using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hockey.Data;
using System;

public class SkillOptionUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private TextMeshProUGUI skillLevelText;
    
    // スキル選択時のイベント
    public event Action<SkillData> OnSkillSelected;
    
    // スキルデータ参照
    private SkillData currentSkill;

    // スキル情報を設定
    public void SetupSkill(SkillData skill)
    {
        currentSkill = skill;
        
        // スキル名の設定
        if (skillNameText != null)
        {
            skillNameText.text = skill.skillName;
        }
        
        // スキル説明の設定
        if (skillDescriptionText != null)
        {
            string effectValueText = GetEffectValueText(skill);
            skillDescriptionText.text = $"{skill.description}\n{effectValueText}";
        }
        
        // スキルレベルの設定
        if (skillLevelText != null)
        {
            skillLevelText.text = $"Lv.1/{skill.maxLevel}";
        }
        
        // スキルアイコンの設定
        if (skillIcon != null)
        {
            SetSkillIcon(skill.skillType);
        }
        
        // ボタンのクリックイベントを設定
        SetupButton();
    }
    
    // ボタンのクリックイベントを設定
    private void SetupButton()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            // 既存のイベントをクリア
            button.onClick.RemoveAllListeners();
            
            // 新しいイベントを追加
            button.onClick.AddListener(OnButtonClick);
        }
    }
    
    // ボタンクリック時の処理
    private void OnButtonClick()
    {
        if (currentSkill != null)
        {
            // スキル選択イベントを発火
            OnSkillSelected?.Invoke(currentSkill);
        }
    }
    
    // スキルタイプに応じたエフェクト値のテキストを取得
    private string GetEffectValueText(SkillData skill)
    {
        if (skill.effectValues == null || skill.effectValues.Count == 0)
            return string.Empty;

        float currentValue = skill.effectValues[0]; // 初期レベルの値を表示

        switch (skill.skillType)
        {
            case SkillType.PuckSizeUp:
                return $"拡大";
            case SkillType.PuckDamageUp:
                return $"攻撃力UP";
            case SkillType.PuckPenetration:
                return $"貫通";
            default:
                return string.Empty;
        }
    }
    
    // スキルタイプに応じたアイコンを設定
    private void SetSkillIcon(SkillType skillType)
    {
        // プロジェクト内のアイコンを取得
        // 実際のプロジェクトではResourcesフォルダからロードするなど
        // 適切な方法でアイコンを取得する必要があります
        Sprite iconSprite = GetSkillIconSprite(skillType);
        if (iconSprite != null)
        {
            skillIcon.sprite = iconSprite;
        }
    }
    
    // スキルタイプに応じたアイコンスプライトを取得
    private Sprite GetSkillIconSprite(SkillType skillType)
    {
        string iconName;
        switch (skillType)
        {
            case SkillType.PuckSizeUp:
                iconName = "icon_puck_size";
                break;
            case SkillType.PuckDamageUp:
                iconName = "icon_puck_damage";
                break;
            case SkillType.PuckPenetration:
                iconName = "icon_puck_penetration";
                break;
            default:
                iconName = "icon_default";
                break;
        }
        
        // Resourcesフォルダからアイコンをロード
        return Resources.Load<Sprite>($"Icons/{iconName}");
    }
}