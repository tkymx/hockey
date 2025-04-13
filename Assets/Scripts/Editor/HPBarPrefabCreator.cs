using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class HPBarPrefabCreator : EditorWindow
{
    [MenuItem("Hockey/Tools/Create HP Bar Prefab")]
    public static void ShowWindow()
    {
        GetWindow<HPBarPrefabCreator>("HPバープレハブ作成");
    }
    
    private string prefabName = "HPBar";
    private Color barColor = new Color(0.8f, 0.2f, 0.2f); // 赤めの色
    private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // 半透明の暗めの色
    private float barWidth = 100f;
    private float barHeight = 10f;
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("HPバープレハブ作成ツール", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(10);
        
        // 基本設定
        EditorGUILayout.LabelField("基本設定", EditorStyles.boldLabel);
        prefabName = EditorGUILayout.TextField("プレハブ名", prefabName);
        barColor = EditorGUILayout.ColorField("バーの色", barColor);
        backgroundColor = EditorGUILayout.ColorField("背景色", backgroundColor);
        barWidth = EditorGUILayout.FloatField("バーの幅", barWidth);
        barHeight = EditorGUILayout.FloatField("バーの高さ", barHeight);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("HPバープレハブを作成"))
        {
            CreateHPBarPrefab();
        }
    }
    
    private void CreateHPBarPrefab()
    {
        // HPバーのルートオブジェクト
        GameObject hpBarRoot = new GameObject(prefabName);
        RectTransform rootRectTransform = hpBarRoot.AddComponent<RectTransform>();
        rootRectTransform.sizeDelta = new Vector2(barWidth, barHeight);
        
        // CanvasGroupコンポーネントの追加（フェード用）
        CanvasGroup canvasGroup = hpBarRoot.AddComponent<CanvasGroup>();
        
        // 背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(hpBarRoot.transform);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundColor;
        
        // HPバー
        GameObject fillBar = new GameObject("FillBar");
        fillBar.transform.SetParent(hpBarRoot.transform);
        RectTransform fillRect = fillBar.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = new Vector2(1, 1); // 1ピクセルの境界
        fillRect.offsetMax = new Vector2(-1, -1); // 1ピクセルの境界
        Image fillImage = fillBar.AddComponent<Image>();
        fillImage.color = barColor;
        
        // Sliderコンポーネントの追加
        Slider slider = hpBarRoot.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.wholeNumbers = false;
        slider.direction = Slider.Direction.LeftToRight;
        
        // HPBarControllerコンポーネントの追加
        HPBarController controller = hpBarRoot.AddComponent<HPBarController>();
        controller.GetType().GetField("healthSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, slider);
        controller.GetType().GetField("canvasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, canvasGroup);
        
        // プレハブとして保存
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }
        
        string prefabPath = "Assets/Prefabs/UI/" + prefabName + ".prefab";
        
        PrefabUtility.SaveAsPrefabAsset(hpBarRoot, prefabPath);
        DestroyImmediate(hpBarRoot);
        
        Debug.Log("HPバープレハブを作成しました: " + prefabPath);
        
        // アセットデータベースの更新
        AssetDatabase.Refresh();
        
        // 作成したプレハブを選択
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }
}