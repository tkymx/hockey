using UnityEngine;
using UnityEditor;

namespace Hockey.EditorTools
{
    /// <summary>
    /// デバッグウィンドウのベースクラス
    /// 各種デバッグウィンドウが継承する基本機能を提供
    /// </summary>
    public abstract class DebugWindowBase : EditorWindow
    {
        protected Vector2 scrollPosition;
        protected GUIStyle headerStyle;
        protected GUIStyle subHeaderStyle;
        protected GUIStyle normalStyle;
        protected GUIStyle highlightStyle;
        
        protected virtual void OnEnable()
        {
            InitializeStyles();
        }
        
        protected void InitializeStyles()
        {
            // ヘッダースタイル
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 16;
            headerStyle.alignment = TextAnchor.MiddleLeft;
            headerStyle.margin = new RectOffset(5, 5, 10, 5);
            
            // サブヘッダースタイル
            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            subHeaderStyle.fontSize = 14;
            subHeaderStyle.margin = new RectOffset(5, 5, 5, 5);
            
            // 通常テキストスタイル
            normalStyle = new GUIStyle(EditorStyles.label);
            normalStyle.wordWrap = true;
            normalStyle.margin = new RectOffset(10, 5, 2, 2);
            
            // 強調テキストスタイル
            highlightStyle = new GUIStyle(EditorStyles.label);
            highlightStyle.normal.textColor = Color.cyan;
            highlightStyle.margin = new RectOffset(10, 5, 2, 2);
        }
        
        /// <summary>
        /// 各デバッグウィンドウで実装する描画処理
        /// </summary>
        protected abstract void DrawDebugContent();
        
        protected virtual void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            DrawTitle();
            
            EditorGUILayout.Space(5);
            
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;
                DrawDebugContent();
            }
            
            EditorGUILayout.Space(10);
            
            // 更新ボタン
            if (GUILayout.Button("更新", GUILayout.Width(100), GUILayout.Height(30)))
            {
                Repaint();
            }
            
            // エディタの更新
            if (EditorApplication.isPlaying)
            {
                Repaint();
            }
        }
        
        /// <summary>
        /// ウィンドウのタイトルを描画
        /// </summary>
        protected virtual void DrawTitle()
        {
            EditorGUILayout.LabelField("デバッグウィンドウ", headerStyle);
        }
        
        /// <summary>
        /// セクションヘッダーを描画
        /// </summary>
        /// <param name="title">セクションのタイトル</param>
        protected void DrawSectionHeader(string title)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(title, subHeaderStyle);
            EditorGUILayout.Space(3);
        }
        
        /// <summary>
        /// 区切り線を描画
        /// </summary>
        protected void DrawSeparator()
        {
            EditorGUILayout.Space(5);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(5);
        }
        
        /// <summary>
        /// キーと値のペアを描画
        /// </summary>
        /// <param name="key">キー名</param>
        /// <param name="value">値</param>
        /// <param name="highlight">強調表示するかどうか</param>
        protected void DrawKeyValuePair(string key, string value, bool highlight = false)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(key + ":", GUILayout.Width(160));
            EditorGUILayout.LabelField(value, highlight ? highlightStyle : normalStyle);
            EditorGUILayout.EndHorizontal();
        }
    }
}