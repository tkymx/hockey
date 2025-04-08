using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Hockey.Data
{
    public class GameConfigRepository : MonoBehaviour
    {
        // 各データ
        public PlayerData PlayerConfig { get; private set; }
        public PuckData PuckConfig { get; private set; }
        public GrowthData GrowthConfig { get; private set; }
        public List<SkillData> Skills { get; private set; } // 追加: スキルデータリスト

        private void Awake()
        {
            // 設定データを読み込む
            LoadAllConfigs();
        }

        // すべての設定を読み込む
        public void LoadAllConfigs()
        {
            PlayerConfig = LoadConfig<PlayerData>("player_config.json");
            PuckConfig = LoadConfig<PuckData>("puck_config.json");
            GrowthConfig = LoadConfig<GrowthData>("growth_config.json");
            
            // スキルデータを読み込む
            LoadSkills("skills_config.json");
        }

        // 型指定で設定を読み込む
        private T LoadConfig<T>(string fileName) where T : new()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                T config = JsonUtility.FromJson<T>(jsonContent);
                if (config != null)
                {
                    Debug.Log($"Successfully loaded {fileName}");
                    return config;
                }
                else
                {
                    Debug.LogWarning($"Failed to parse {fileName}, using default settings");
                    return new T();
                }
            }
            else
            {
                Debug.LogWarning($"{fileName} not found, using default settings");
                
                // デフォルト設定を生成して保存
                T defaultConfig = new T();
                SaveConfig(defaultConfig, fileName);
                
                return defaultConfig;
            }
        }

        // スキルデータを読み込む
        private void LoadSkills(string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                SkillList skillList = JsonUtility.FromJson<SkillList>(jsonContent);
                if (skillList != null && skillList.skills != null)
                {
                    Skills = skillList.skills;
                    Debug.Log($"Successfully loaded {fileName}, found {Skills.Count} skills");
                }
                else
                {
                    Debug.LogWarning($"Failed to parse {fileName}, using default skills");
                    Skills = CreateDefaultSkills();
                    SaveSkills(fileName);
                }
            }
            else
            {
                Debug.LogWarning($"{fileName} not found, using default skills");
                Skills = CreateDefaultSkills();
                SaveSkills(fileName);
            }
        }

        // スキルをJSONとして保存
        private void SaveSkills(string fileName)
        {
            string directory = Application.streamingAssetsPath;
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string filePath = Path.Combine(directory, fileName);
            SkillList skillList = new SkillList { skills = Skills };
            string jsonContent = JsonUtility.ToJson(skillList, true);
            File.WriteAllText(filePath, jsonContent);
            
            Debug.Log($"Created default {fileName} with {Skills.Count} skills");
        }

        // 設定をJSONとして保存
        private void SaveConfig<T>(T config, string fileName)
        {
            string directory = Application.streamingAssetsPath;
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string filePath = Path.Combine(directory, fileName);
            string jsonContent = JsonUtility.ToJson(config, true);
            File.WriteAllText(filePath, jsonContent);
            
            Debug.Log($"Created default {fileName}");
        }

        // IDからスキルを取得
        public SkillData GetSkillById(string skillId)
        {
            if (Skills == null) return null;
            return Skills.Find(s => s.skillId == skillId);
        }

        // タイプからスキルリストを取得
        public List<SkillData> GetSkillsByType(SkillType type)
        {
            if (Skills == null) return new List<SkillData>();
            return Skills.FindAll(s => s.skillType == type);
        }

        // デフォルトのスキルリストを作成
        private List<SkillData> CreateDefaultSkills()
        {
            List<SkillData> defaultSkills = new List<SkillData>();

            // パックサイズアップスキル
            defaultSkills.Add(new SkillData(
                "puck_size_up",
                "パックサイズアップ",
                "パックのサイズが大きくなり、当たりやすくなります。",
                SkillType.PuckSizeUp,
                3,
                new List<float> { 1.2f, 1.5f, 2.0f }  // 各レベルでのサイズ倍率
            ));

            // パック攻撃力アップスキル
            defaultSkills.Add(new SkillData(
                "puck_damage_up",
                "パック攻撃力アップ",
                "パックの攻撃力が上昇し、障害物を破壊しやすくなります。",
                SkillType.PuckDamageUp,
                3,
                new List<float> { 1.3f, 1.7f, 2.2f }  // 各レベルでの攻撃力倍率
            ));

            // パック貫通スキル
            defaultSkills.Add(new SkillData(
                "puck_penetration",
                "パック貫通",
                "パックが倒せる敵を貫通して進みます。",
                SkillType.PuckPenetration,
                3,
                new List<float> { 1f, 2f, 3f }  // 各レベルでの貫通可能数
            ));

            return defaultSkills;
        }
    }

    // JSONシリアライズ用のラッパークラス
    [System.Serializable]
    public class SkillList
    {
        public List<SkillData> skills;
    }
}