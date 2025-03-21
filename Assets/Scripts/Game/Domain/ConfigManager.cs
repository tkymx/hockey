using UnityEngine;
using System.IO;

public class ConfigManager
{
    private static ConfigManager instance;
    private GameConfig gameConfig;
    
    private const string CONFIG_FILE_PATH = "GameConfig";

    public static ConfigManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ConfigManager();
                instance.LoadConfig();
            }
            return instance;
        }
    }
    
    private ConfigManager() { }
    
    public GameConfig GetConfig()
    {
        return gameConfig;
    }
    
    private void LoadConfig()
    {
        // Resources内のJSONファイルからロード
        TextAsset configFile = Resources.Load<TextAsset>(CONFIG_FILE_PATH);
        
        if (configFile != null)
        {
            // JSONからGameConfigにデシリアライズ
            gameConfig = JsonUtility.FromJson<GameConfig>(configFile.text);
            Debug.Log("Config loaded successfully");
        }
        else
        {
            // 設定ファイルがない場合はデフォルト値を使用
            gameConfig = CreateDefaultConfig();
            Debug.LogWarning("Config file not found. Using default values.");
        }
    }
    
    private GameConfig CreateDefaultConfig()
    {
        GameConfig defaultConfig = new GameConfig();
        
        // デフォルト値を設定
        defaultConfig.normalObject = new ObjectTypeConfig
        {
            durability = 100f,
            pointValue = 10,
            color = new Color(0.3f, 0.3f, 0.8f)
        };
        
        defaultConfig.bonusObject = new ObjectTypeConfig
        {
            durability = 80f,
            pointValue = 30,
            color = new Color(0.2f, 0.8f, 0.2f)
        };
        
        defaultConfig.obstacleObject = new ObjectTypeConfig
        {
            durability = 150f,
            pointValue = 15,
            color = new Color(0.8f, 0.2f, 0.2f)
        };
        
        return defaultConfig;
    }
    
    // エディタ用：現在の設定をJSONファイルに保存するメソッド
    #if UNITY_EDITOR
    public void SaveConfig()
    {
        if (gameConfig == null)
        {
            gameConfig = CreateDefaultConfig();
        }
        
        string json = JsonUtility.ToJson(gameConfig, true);
        string filePath = Path.Combine(Application.dataPath, "Resources", CONFIG_FILE_PATH + ".json");
        
        // ディレクトリが存在しない場合は作成
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        
        File.WriteAllText(filePath, json);
        Debug.Log("Config saved to: " + filePath);
    }
    #endif
}