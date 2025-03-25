using UnityEngine;
using UnityEditor;
using System.IO;

namespace HockeyEditor
{
    public class HockeyPrefabManager : ScriptableObject
    {
        public const string PrefabPath = "Assets/Resources/Prefabs";
        public const string SettingsPath = "Assets/Settings";
        public const string ManagerAssetName = "HockeyPrefabManagerSettings.asset";
        
        [SerializeField] private GameObject destructiblePrefab;
        [SerializeField] private GameObject explosionEffectPrefab;
        
        private static HockeyPrefabManager instance;
        
        public static HockeyPrefabManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LoadOrCreateSettings();
                }
                return instance;
            }
        }
        
        public GameObject DestructiblePrefab
        {
            get { return destructiblePrefab; }
            set 
            { 
                if (destructiblePrefab != value)
                {
                    destructiblePrefab = value;
                    SaveSettings();
                }
            }
        }
        
        public GameObject ExplosionEffectPrefab
        {
            get { return explosionEffectPrefab; }
            set 
            { 
                if (explosionEffectPrefab != value)
                {
                    explosionEffectPrefab = value;
                    SaveSettings();
                }
            }
        }
        
        private static HockeyPrefabManager LoadOrCreateSettings()
        {
            // 設定ファイルを探す
            var settings = AssetDatabase.LoadAssetAtPath<HockeyPrefabManager>(
                $"{SettingsPath}/{ManagerAssetName}");
                
            if (settings == null)
            {
                // 設定ファイルが存在しない場合は新規作成
                settings = CreateInstance<HockeyPrefabManager>();
                
                // 保存先ディレクトリの作成
                if (!AssetDatabase.IsValidFolder(SettingsPath))
                {
                    Directory.CreateDirectory(SettingsPath);
                    AssetDatabase.Refresh();
                }
                
                // アセットとして保存
                AssetDatabase.CreateAsset(settings, $"{SettingsPath}/{ManagerAssetName}");
                AssetDatabase.SaveAssets();
            }
            
            return settings;
        }
        
        private void SaveSettings()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        public static void EnsurePrefabDirectory()
        {
            if (!AssetDatabase.IsValidFolder(PrefabPath))
            {
                Directory.CreateDirectory(PrefabPath);
                AssetDatabase.Refresh();
            }
        }
        
        [MenuItem("Hockey/Prefab Settings")]
        public static void ShowSettings()
        {
            Selection.activeObject = Instance;
            EditorGUIUtility.PingObject(Instance);
        }
    }
}