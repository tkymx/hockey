using UnityEngine;
using System.IO;

namespace Hockey.Data
{
    public class GameConfigRepository : MonoBehaviour
    {
        // 各データ
        public PlayerData PlayerConfig { get; private set; }
        public PuckData PuckConfig { get; private set; }
        public GrowthData GrowthConfig { get; private set; } // 追加: 成長データ

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
            GrowthConfig = LoadConfig<GrowthData>("growth_config.json"); // 追加: 成長設定ファイルの読み込み
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
    }
}