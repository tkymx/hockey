using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Hockey.Data;

namespace Hockey.EditorTools
{
    /// <summary>
    /// ゲームのデバッグ情報を表示する総合ウィンドウ
    /// </summary>
    public class GameDebugWindow : DebugWindowBase
    {
        // 表示モード
        private enum DebugMode
        {
            PlayerSkills,
            PlayerStats,
            GameState,
            SystemInfo
        }
        
        private DebugMode currentMode = DebugMode.PlayerSkills;
        private string[] modeNames = { "スキル", "プレイヤー情報", "ゲーム状態", "システム情報" };
        
        // 参照
        private PlayerSkillManager skillManager;
        private GameConfigRepository configRepository;
        private GameManager gameManager;
        private Player player;
        
        // スキルデバッグ用
        private string selectedSkillId;
        private Vector2 skillListScrollPosition;
        
        [MenuItem("Hockey/デバッグウィンドウ")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameDebugWindow>("ホッケーデバッグ");
            window.minSize = new Vector2(450, 400);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("ホッケーデバッグ");
        }
        
        protected override void DrawTitle()
        {
            EditorGUILayout.LabelField("ホッケーゲーム - デバッグツール", headerStyle);
        }
        
        protected override void DrawDebugContent()
        {
            // プレイモード以外では実行不可
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("プレイモードで実行してください", MessageType.Info);
                return;
            }
            
            // 各種コンポーネントの参照を取得
            FindComponents();
            
            // モード選択タブ
            DrawModeTabs();
            
            // 選択されたモードに応じた情報を表示
            switch (currentMode)
            {
                case DebugMode.PlayerSkills:
                    DrawPlayerSkillsMode();
                    break;
                case DebugMode.PlayerStats:
                    DrawPlayerStatsMode();
                    break;
                case DebugMode.GameState:
                    DrawGameStateMode();
                    break;
                case DebugMode.SystemInfo:
                    DrawSystemInfoMode();
                    break;
            }
        }
        
        /// <summary>
        /// 必要なコンポーネントへの参照を取得
        /// </summary>
        private void FindComponents()
        {
            if (configRepository == null)
                configRepository = Object.FindFirstObjectByType<GameConfigRepository>();
                
            if (skillManager == null)
                skillManager = Object.FindFirstObjectByType<PlayerSkillManager>();
                
            if (gameManager == null)
                gameManager = Object.FindFirstObjectByType<GameManager>();
                
            if (player == null && gameManager != null)
            {
                PlayerManager playerManager = Object.FindFirstObjectByType<PlayerManager>();
                if (playerManager != null)
                    player = playerManager.GetPlayer();
            }
        }
        
        /// <summary>
        /// デバッグモード選択タブの描画
        /// </summary>
        private void DrawModeTabs()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            for (int i = 0; i < modeNames.Length; i++)
            {
                DebugMode mode = (DebugMode)i;
                bool selected = GUILayout.Toggle(currentMode == mode, modeNames[i], EditorStyles.toolbarButton);
                if (selected && currentMode != mode)
                {
                    currentMode = mode;
                    GUI.FocusControl(null);
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            DrawSeparator();
        }
        
        #region DebugModes
        
        /// <summary>
        /// スキル情報モード
        /// </summary>
        private void DrawPlayerSkillsMode()
        {
            EditorGUILayout.LabelField("スキル情報", subHeaderStyle);
            
            if (skillManager == null || configRepository == null)
            {
                EditorGUILayout.HelpBox("PlayerSkillManagerまたはGameConfigRepositoryが見つかりません", MessageType.Error);
                return;
            }
            
            // スキル操作セクション
            DrawSectionHeader("スキル操作");
            DrawSkillManipulation();
            
            DrawSeparator();
            
            // プレイヤーが獲得したスキル情報を表示
            DrawSectionHeader("獲得済みスキル");
            DrawAcquiredSkills();
            
            DrawSeparator();
            
            // 利用可能なスキル情報を表示
            DrawSectionHeader("利用可能なスキル");
            DrawAvailableSkills();
        }
        
        /// <summary>
        /// プレイヤー情報モード
        /// </summary>
        private void DrawPlayerStatsMode()
        {
            EditorGUILayout.LabelField("プレイヤー情報", subHeaderStyle);
            
            if (player == null)
            {
                EditorGUILayout.HelpBox("プレイヤーが見つかりません", MessageType.Error);
                return;
            }
            
            // ここにプレイヤー情報の表示を追加
            // 例: レベル、経験値、位置情報など
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // プレイヤーの基本情報
            DrawKeyValuePair("名前", player.name);
            DrawKeyValuePair("位置", player.transform.position.ToString("F2"));
            
            // レベルや経験値などのステータスはプレイヤークラスから必要な情報を取得
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// ゲーム状態モード
        /// </summary>
        private void DrawGameStateMode()
        {
            EditorGUILayout.LabelField("ゲーム状態", subHeaderStyle);
            
            if (gameManager == null)
            {
                EditorGUILayout.HelpBox("GameManagerが見つかりません", MessageType.Error);
                return;
            }
            
            // ここにゲーム状態の情報を表示
            // 例: 現在のステージ、スコア、時間など
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // ゲームの基本情報
            DrawKeyValuePair("タイム", Time.time.ToString("F2"));
            DrawKeyValuePair("フレームレート", $"{1.0f / Time.deltaTime:F1} FPS");
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// システム情報モード
        /// </summary>
        private void DrawSystemInfoMode()
        {
            EditorGUILayout.LabelField("システム情報", subHeaderStyle);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // システム情報
            DrawKeyValuePair("Unity バージョン", Application.unityVersion);
            DrawKeyValuePair("プラットフォーム", Application.platform.ToString());
            DrawKeyValuePair("デバイスモデル", SystemInfo.deviceModel);
            DrawKeyValuePair("デバイス名", SystemInfo.deviceName);
            DrawKeyValuePair("プロセッサ", SystemInfo.processorType);
            DrawKeyValuePair("メモリ", $"{SystemInfo.systemMemorySize} MB");
            DrawKeyValuePair("グラフィックス", SystemInfo.graphicsDeviceName);
            DrawKeyValuePair("VRAM", $"{SystemInfo.graphicsMemorySize} MB");
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region SkillDebug
        
        /// <summary>
        /// スキル操作機能を表示
        /// </summary>
        private void DrawSkillManipulation()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // スキル追加セクション
            EditorGUILayout.LabelField("スキル追加", EditorStyles.boldLabel);
            
            // スキル選択ドロップダウン
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("スキル:", GUILayout.Width(80));
            
            // スキルリストから選択
            if (configRepository.Skills != null && configRepository.Skills.Count > 0)
            {
                int selectedIndex = -1;
                List<string> skillNames = new List<string>();
                
                for (int i = 0; i < configRepository.Skills.Count; i++)
                {
                    skillNames.Add(configRepository.Skills[i].skillName);
                    if (configRepository.Skills[i].skillId == selectedSkillId)
                    {
                        selectedIndex = i;
                    }
                }
                
                int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, skillNames.ToArray());
                if (newSelectedIndex != selectedIndex && newSelectedIndex >= 0)
                {
                    selectedSkillId = configRepository.Skills[newSelectedIndex].skillId;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // スキル追加/レベルアップボタン
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("スキル追加/レベルアップ", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(selectedSkillId) && skillManager != null)
                {
                    skillManager.AcquireSkill(selectedSkillId);
                    Debug.Log($"スキルを追加またはレベルアップしました: {selectedSkillId}");
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // スキルリセットボタン
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("全スキルリセット", GUILayout.Height(30)))
            {
                if (skillManager != null)
                {
                    skillManager.ResetSkills();
                    Debug.Log("全てのスキルをリセットしました");
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// プレイヤーが獲得したスキル情報を表示
        /// </summary>
        private void DrawAcquiredSkills()
        {
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
                
                // 個別スキル削除ボタンを追加
                if (GUILayout.Button("削除", GUILayout.Width(60)))
                {
                    // スキル削除機能を実行
                    RemoveSkill(skill.skillId);
                }
                
                EditorGUILayout.EndHorizontal();
                
                // スキルタイプ
                DrawKeyValuePair("タイプ", skillData.skillType.ToString());
                
                // 効果値
                float effectValue = skillData.GetEffectValue(skill.level);
                DrawKeyValuePair("効果値", effectValue.ToString("F2"), true);
                
                // スキルの説明
                EditorGUILayout.LabelField("説明:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(skillData.description, normalStyle);
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        
        /// <summary>
        /// 利用可能なスキル情報を表示
        /// </summary>
        private void DrawAvailableSkills()
        {
            if (configRepository.Skills == null || configRepository.Skills.Count == 0)
            {
                EditorGUILayout.LabelField("利用可能なスキルはありません", normalStyle);
                return;
            }

            using (var scrollView = new EditorGUILayout.ScrollViewScope(skillListScrollPosition))
            {
                skillListScrollPosition = scrollView.scrollPosition;
                
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
        
        /// <summary>
        /// プレイヤーからスキルを削除する
        /// </summary>
        private void RemoveSkill(string skillId)
        {
            // PlayerSkillManagerの削除メソッドを呼び出す
            if (skillManager != null)
            {
                skillManager.RemoveSkill(skillId);
            }
        }
        
        #endregion
    }
}