using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Hockey.Data;

namespace Hockey.EditorTools
{
    /// <summary>
    /// スキル情報を表示するデバッグウィンドウ
    /// </summary>
    public class SkillDebugWindow : DebugWindowBase
    {
        private PlayerSkillManager skillManager;
        private GameConfigRepository configRepository;
        
        [MenuItem("Hockey/Debug/スキル情報")]
        public static void ShowWindow()
        {
            var window = GetWindow<SkillDebugWindow>("スキル情報");
            window.minSize = new Vector2(400, 300);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("スキル情報");
        }
        
        protected override void DrawTitle()
        {
            EditorGUILayout.LabelField("スキル情報デバッグウィンドウ", headerStyle);
        }
        
        protected override void DrawDebugContent()
        {
            // プレイモード以外では実行不可
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("プレイモードで実行してください", MessageType.Info);
                return;
            }
            
            // GameConfigRepositoryを取得
            if (configRepository == null)
            {
                configRepository = Object.FindFirstObjectByType<GameConfigRepository>();
                if (configRepository == null)
                {
                    EditorGUILayout.HelpBox("GameConfigRepositoryが見つかりません", MessageType.Error);
                    return;
                }
            }
            
            // PlayerSkillManagerを取得
            if (skillManager == null)
            {
                skillManager = Object.FindFirstObjectByType<PlayerSkillManager>();
                if (skillManager == null)
                {
                    EditorGUILayout.HelpBox("PlayerSkillManagerが見つかりません", MessageType.Error);
                    return;
                }
            }
            
            // プレイヤーが獲得したスキル情報を表示
            DrawPlayerSkills();
            
            // 利用可能なスキル情報を表示
            DrawAvailableSkills();
        }
        
        /// <summary>
        /// プレイヤーが獲得したスキル情報を表示
        /// </summary>
        private void DrawPlayerSkills()
        {
            DrawSectionHeader("獲得済みスキル");
            
            List<PlayerSkill> acquiredSkills = skillManager.GetAcquiredSkills();
            
            if (acquiredSkills == null || acquiredSkills.Count == 0)
            {
                EditorGUILayout.LabelField("獲得済みのスキルはありません", normalStyle);
                return;
            }
            
            foreach (var skill in acquiredSkills)
            {
                SkillData skillData = configRepository.GetSkillById(skill.skillId);
                if (skillData == null) continue;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // スキル名とレベル
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(skillData.skillName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Lv.{skill.level}/{skillData.maxLevel}", highlightStyle);
                EditorGUILayout.EndHorizontal();
                
                // スキルタイプ
                DrawKeyValuePair("タイプ", skillData.skillType.ToString());
                
                // 効果値
                float effectValue = skillData.GetEffectValue(skill.level);
                DrawKeyValuePair("効果値", effectValue.ToString(), true);
                
                // スキルの説明
                EditorGUILayout.LabelField("説明:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(skillData.description, normalStyle);
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            DrawSeparator();
        }
        
        /// <summary>
        /// 利用可能なスキル情報を表示
        /// </summary>
        private void DrawAvailableSkills()
        {
            DrawSectionHeader("利用可能なスキル");
            
            if (configRepository.Skills == null || configRepository.Skills.Count == 0)
            {
                EditorGUILayout.LabelField("利用可能なスキルはありません", normalStyle);
                return;
            }
            
            // スキルタイプ別に表示
            foreach (SkillType skillType in System.Enum.GetValues(typeof(SkillType)))
            {
                List<SkillData> skillsOfType = configRepository.GetSkillsByType(skillType);
                if (skillsOfType.Count == 0) continue;
                
                EditorGUILayout.LabelField(skillType.ToString(), subHeaderStyle);
                
                foreach (var skillData in skillsOfType)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // 獲得済みかどうか判定
                    int currentLevel = skillManager.GetSkillLevel(skillData.skillId);
                    bool isAcquired = currentLevel > 0;
                    
                    // スキル名と状態
                    string status = isAcquired 
                        ? $"獲得済み (Lv.{currentLevel}/{skillData.maxLevel})" 
                        : "未獲得";
                    
                    EditorGUILayout.LabelField(skillData.skillName);
                    EditorGUILayout.LabelField(status, isAcquired ? highlightStyle : normalStyle);
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space(5);
            }
        }
    }
}