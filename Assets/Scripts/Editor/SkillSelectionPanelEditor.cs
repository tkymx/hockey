using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using Hockey.Data;
using System.Collections.Generic;

[CustomEditor(typeof(SkillSelectionPanel))]
public class SkillSelectionPanelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // SkillSelectionPanel のインスタンスを取得
        SkillSelectionPanel panel = (SkillSelectionPanel)target;

        // デフォルトのインスペクタを描画
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("スキル選択パネル設定", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // Prefab 作成ボタン
        if (GUILayout.Button("スキル選択パネルを生成"))
        {
            CreateSkillSelectionPanelPrefab();
        }

        // 変更があった場合にオブジェクトをマーク
        if (GUI.changed)
        {
            EditorUtility.SetDirty(panel);
        }
    }

    private void CreateSkillSelectionPanelPrefab()
    {
        // 新しい GameObject を作成
        GameObject skillSelectionPanelGO = new GameObject("SkillSelectionPanel");
        RectTransform rectTransform = skillSelectionPanelGO.AddComponent<RectTransform>();
        Canvas canvas = skillSelectionPanelGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        skillSelectionPanelGO.AddComponent<CanvasScaler>();
        skillSelectionPanelGO.AddComponent<GraphicRaycaster>();

        // パネルの背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(skillSelectionPanelGO.transform);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f);

        // タイトル
        GameObject title = new GameObject("Title");
        title.transform.SetParent(skillSelectionPanelGO.transform);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(300, 50);
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "スキルを選択";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;

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
        layoutGroup.spacing = 20;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

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
        closeImage.color = Color.red;
        
        // ボタンテキスト
        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButton.transform);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "閉じる";
        closeText.fontSize = 24;
        closeText.alignment = TextAlignmentOptions.Center;

        // SkillSelectionPanel コンポーネントを設定
        SkillSelectionPanel skillSelectionPanel = skillSelectionPanelGO.AddComponent<SkillSelectionPanel>();
        
        // リフレクションを使用してプライベートフィールドを設定
        SetFieldValue(skillSelectionPanel, "skillSelectionPanel", skillSelectionPanelGO);
        SetFieldValue(skillSelectionPanel, "skillOptionsContainer", container.transform);
        SetFieldValue(skillSelectionPanel, "closeButton", closeBtn);
        SetFieldValue(skillSelectionPanel, "titleText", titleText);

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
