using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using Hockey.Data;
using System.Collections.Generic;

public class SkillSelectionPanelCreator : EditorWindow
{
    // UI設定用のパラメータ
    private Color backgroundColor = new Color(0, 0, 0, 0.5f);
    private string titleText = "スキルを選択";
    private int titleFontSize = 36;
    private string closeButtonText = "閉じる";
    private int buttonFontSize = 24;
    private Color closeButtonColor = Color.red;
    private float layoutSpacing = 20f;

    // スキルオプションUI設定
    private float skillOptionWidth = 200;
    private float skillOptionHeight = 300;
    private Color skillOptionBgColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    [MenuItem("Hockey/Create Skill Selection Panel")]
    public static void ShowWindow()
    {
        GetWindow<SkillSelectionPanelCreator>("Skill Panel Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Skill Selection Panel Creator", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        GUILayout.Label("Panel Settings", EditorStyles.boldLabel);

        // UI設定の編集
        backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
        titleText = EditorGUILayout.TextField("Title Text", titleText);
        titleFontSize = EditorGUILayout.IntField("Title Font Size", titleFontSize);
        
        EditorGUILayout.Space();
        GUILayout.Label("Button Settings", EditorStyles.boldLabel);
        
        closeButtonText = EditorGUILayout.TextField("Close Button Text", closeButtonText);
        buttonFontSize = EditorGUILayout.IntField("Button Font Size", buttonFontSize);
        closeButtonColor = EditorGUILayout.ColorField("Close Button Color", closeButtonColor);
        
        EditorGUILayout.Space();
        GUILayout.Label("Layout Settings", EditorStyles.boldLabel);
        
        layoutSpacing = EditorGUILayout.FloatField("Layout Spacing", layoutSpacing);
        
        EditorGUILayout.Space();
        GUILayout.Label("Skill Option Settings", EditorStyles.boldLabel);
        
        skillOptionWidth = EditorGUILayout.FloatField("Skill Option Width", skillOptionWidth);
        skillOptionHeight = EditorGUILayout.FloatField("Skill Option Height", skillOptionHeight);
        skillOptionBgColor = EditorGUILayout.ColorField("Skill Option BG Color", skillOptionBgColor);

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Create Skill Selection Panel"))
        {
            // 先に SkillOptionUI プレハブを作成
            CreateSkillOptionUIPrefab();
            
            // 次に SkillSelectionPanel プレハブを作成
            CreateSkillSelectionPanelPrefab();
        }
    }

    private GameObject CreateSkillOptionUIPrefab()
    {
        // 新しい GameObject を作成
        GameObject skillOptionGO = new GameObject("SkillOptionUI");
        RectTransform rectTransform = skillOptionGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(skillOptionWidth, skillOptionHeight);
        
        // 背景イメージ
        Image bgImage = skillOptionGO.AddComponent<Image>();
        bgImage.color = skillOptionBgColor;
        
        // スキルアイコン
        GameObject iconGO = new GameObject("SkillIcon");
        iconGO.transform.SetParent(skillOptionGO.transform);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.75f);
        iconRect.anchorMax = new Vector2(0.5f, 0.75f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(64, 64);
        Image skillIcon = iconGO.AddComponent<Image>();
        
        // スキル名テキスト
        GameObject nameGO = new GameObject("SkillName");
        nameGO.transform.SetParent(skillOptionGO.transform);
        RectTransform nameRect = nameGO.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.1f, 0.6f);
        nameRect.anchorMax = new Vector2(0.9f, 0.6f);
        nameRect.pivot = new Vector2(0.5f, 0.5f);
        nameRect.sizeDelta = new Vector2(0, 40);
        TextMeshProUGUI nameText = nameGO.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 24;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.text = "スキル名";
        
        // スキル説明テキスト
        GameObject descGO = new GameObject("SkillDescription");
        descGO.transform.SetParent(skillOptionGO.transform);
        RectTransform descRect = descGO.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.1f, 0.3f);
        descRect.anchorMax = new Vector2(0.9f, 0.5f);
        descRect.pivot = new Vector2(0.5f, 0.5f);
        descRect.sizeDelta = new Vector2(0, 0);
        TextMeshProUGUI descText = descGO.AddComponent<TextMeshProUGUI>();
        descText.fontSize = 18;
        descText.alignment = TextAlignmentOptions.Center;
        descText.text = "スキル説明";
        
        // スキルレベルテキスト
        GameObject levelGO = new GameObject("SkillLevel");
        levelGO.transform.SetParent(skillOptionGO.transform);
        RectTransform levelRect = levelGO.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.5f, 0.15f);
        levelRect.anchorMax = new Vector2(0.5f, 0.15f);
        levelRect.pivot = new Vector2(0.5f, 0.5f);
        levelRect.sizeDelta = new Vector2(100, 30);
        TextMeshProUGUI levelText = levelGO.AddComponent<TextMeshProUGUI>();
        levelText.fontSize = 20;
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.text = "Lv.1/3";
        
        // ボタン（クリック可能にする）
        Button button = skillOptionGO.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
        button.colors = colors;

        // SkillOptionUI コンポーネントを追加
        SkillOptionUI skillOptionUI = skillOptionGO.AddComponent<SkillOptionUI>();
        
        // リフレクションでプライベートフィールドを設定
        SetFieldValue(skillOptionUI, "skillIcon", skillIcon);
        SetFieldValue(skillOptionUI, "skillNameText", nameText);
        SetFieldValue(skillOptionUI, "skillDescriptionText", descText);
        SetFieldValue(skillOptionUI, "skillLevelText", levelText);
        
        // Prefabの保存先ディレクトリを作成
        string directory = "Assets/Prefabs";
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        // Prefab を保存
        string path = "Assets/Prefabs/SkillOptionUI.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(skillOptionGO, path);
        Debug.Log($"Prefab を作成しました: {path}");
        
        // シーン上のオブジェクトを削除
        DestroyImmediate(skillOptionGO);
        
        return prefab;
    }

    private void CreateSkillSelectionPanelPrefab()
    {
        // 新しい GameObject を作成
        GameObject skillSelectionPanelGO = new GameObject("SkillSelectionPanel");
        RectTransform rectTransform = skillSelectionPanelGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // パネルの背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(skillSelectionPanelGO.transform);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundColor;

        // タイトル
        GameObject title = new GameObject("Title");
        title.transform.SetParent(skillSelectionPanelGO.transform);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(300, 50);
        TextMeshProUGUI titleTextComponent = title.AddComponent<TextMeshProUGUI>();
        titleTextComponent.text = titleText;
        titleTextComponent.fontSize = titleFontSize;
        titleTextComponent.alignment = TextAlignmentOptions.Center;

        // スキルオプションコンテナ
        GameObject container = new GameObject("SkillOptionsContainer");
        container.transform.SetParent(skillSelectionPanelGO.transform);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.1f, 0.3f);
        containerRect.anchorMax = new Vector2(0.9f, 0.7f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        HorizontalLayoutGroup layoutGroup = container.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = layoutSpacing;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        
        // サンプルとして SkillOptionUI をいくつか追加
        GameObject skillOptionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SkillOptionUI.prefab");
        for (int i = 0; i < 3; i++)
        {
            GameObject skillOption = PrefabUtility.InstantiatePrefab(skillOptionPrefab) as GameObject;
            skillOption.transform.SetParent(container.transform);
        }

        // Close ボタン
        GameObject closeButton = new GameObject("CloseButton");
        closeButton.transform.SetParent(skillSelectionPanelGO.transform);
        RectTransform closeRect = closeButton.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0.1f);
        closeRect.anchorMax = new Vector2(0.5f, 0.1f);
        closeRect.pivot = new Vector2(0.5f, 0.5f);
        closeRect.sizeDelta = new Vector2(200, 50);
        Button closeBtn = closeButton.AddComponent<Button>();
        Image closeImage = closeButton.AddComponent<Image>();
        closeImage.color = closeButtonColor;
        
        // ボタンテキスト
        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButton.transform);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = closeButtonText;
        closeText.fontSize = buttonFontSize;
        closeText.alignment = TextAlignmentOptions.Center;

        // SkillSelectionPanel コンポーネントを設定
        SkillSelectionPanel skillSelectionPanel = skillSelectionPanelGO.AddComponent<SkillSelectionPanel>();
        
        // リフレクションを使用してプライベートフィールドを設定
        SetFieldValue(skillSelectionPanel, "skillSelectionPanel", skillSelectionPanelGO);
        SetFieldValue(skillSelectionPanel, "skillOptionsContainer", container.transform);
        SetFieldValue(skillSelectionPanel, "closeButton", closeBtn);
        SetFieldValue(skillSelectionPanel, "titleText", titleTextComponent);

        // Prefabの保存先ディレクトリを作成
        string directory = "Assets/Prefabs";
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Prefab を保存
        string path = "Assets/Prefabs/SkillSelectionPanel.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(skillSelectionPanelGO, path);
        Debug.Log($"Prefab を作成しました: {path}");

        // 作成したオブジェクトを削除
        DestroyImmediate(skillSelectionPanelGO);
        
        // 作成したPrefabを選択
        Selection.activeObject = prefab;
        
        EditorUtility.DisplayDialog("Success", "Skill Selection Panel prefab has been created with SkillOptionUI prefabs!", "OK");
    }
    
    // リフレクションでフィールドを設定
    private void SetFieldValue(object obj, string fieldName, object value)
    {
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        FieldInfo field = obj.GetType().GetField(fieldName, flags);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }
}