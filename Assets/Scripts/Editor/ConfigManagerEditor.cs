using UnityEngine;
using UnityEditor;

public class ConfigManagerEditor : EditorWindow
{
    [MenuItem("Tools/Hockey Game/Create Default Config")]
    public static void CreateDefaultConfig()
    {
        // デフォルト設定をJSONファイルとして保存
        ConfigManager.Instance.SaveConfig();
    }
}