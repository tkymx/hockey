using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace Hockey.Data
{
    public class GameConfigRepository : MonoBehaviour
    {
        // 各データ
        public PlayerData PlayerConfig { get; private set; }
        public PuckData PuckConfig { get; private set; }
        public GrowthData GrowthConfig { get; private set; }
        public List<SkillData> Skills { get; private set; } // スキルデータリスト
        public StageCollection StageCollection { get; private set; } // ステージコレクション

        // すべての設定を読み込む
        public void LoadAllConfigs()
        {
            PlayerConfig = LoadConfig<PlayerData>("player_config.json");
            PuckConfig = LoadConfig<PuckData>("puck_config.json");
            GrowthConfig = LoadConfig<GrowthData>("growth_config.json");
            
            // スキルデータを読み込む
            LoadSkills("skills_config.json");
            
            // ステージデータを読み込む
            LoadStages("stages_config.json");
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
            }
        }
        
        // ステージデータを読み込む
        private void LoadStages(string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                StageCollection = JsonUtility.FromJson<StageCollection>(jsonContent);
                if (StageCollection != null && StageCollection.stages != null)
                {
                    Debug.Log($"Successfully loaded {fileName}, found {StageCollection.stages.Count} stages");
                }
                else
                {
                    CreateDefaultStageCollection(fileName);
                }
            }
            else
            {
                CreateDefaultStageCollection(fileName);
            }
        }
        
        // デフォルトのステージコレクションを作成
        private void CreateDefaultStageCollection(string fileName)
        {
            StageCollection = new StageCollection();
            
            // デフォルトのステージを追加
            StageData defaultStage = new StageData
            {
                stageId = "stage_default",
                stageName = "Default Stage",
                zones = new List<ZoneData>
                {
                    new ZoneData
                    {
                        frontWidth = 15f,
                        backWidth = 12f,
                        depth = 10f,
                        playerAreaDepth = 2f
                    }
                }
            };
            
            StageCollection.stages.Add(defaultStage);
            SaveStages(fileName);
            Debug.Log($"Created default {fileName} with 1 stage");
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
        
        // ステージをJSONとして保存
        public void SaveStages(string fileName)
        {
            string directory = Application.streamingAssetsPath;
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string filePath = Path.Combine(directory, fileName);
            string jsonContent = JsonUtility.ToJson(StageCollection, true);
            File.WriteAllText(filePath, jsonContent);
            
            Debug.Log($"Saved {StageCollection.stages.Count} stages to {fileName}");
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
        
        // 名前からステージデータを取得
        public StageData GetStageByName(string stageName)
        {
            if (StageCollection == null || StageCollection.stages == null) return null;
            return StageCollection.stages.Find(s => s.stageName == stageName);
        }
        
        // IDからステージデータを取得
        public StageData GetStageById(string stageId)
        {
            if (StageCollection == null || StageCollection.stages == null) return null;
            return StageCollection.stages.Find(s => s.stageId == stageId);
        }
        
        // インデックスからステージデータを取得
        public StageData GetStageByIndex(int index)
        {
            if (StageCollection == null || StageCollection.stages == null || index < 0 || index >= StageCollection.stages.Count)
                return null;
                
            return StageCollection.stages[index];
        }
    }

    // JSONシリアライズ用のラッパークラス
    [System.Serializable]
    public class SkillList
    {
        public List<SkillData> skills;
    }
}