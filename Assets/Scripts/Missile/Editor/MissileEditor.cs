using UnityEngine;
using UnityEditor;

public class MissileEditor : EditorWindow
{
    // ミサイルデータの入力フィールド
    private string dataName = "NewMissileData";
    private float firingInterval = 5f;
    private float speed = 10f;
    private float damage = 50f;
    private float lifetime = 3f;
    
    // ミサイルモデルの設定（オプション）
    private GameObject missileModel;
    private bool useCustomModel = false;
    private Color missileColor = Color.red;
    private float missileScale = 0.5f;
    
    // マテリアル設定
    private bool useCustomMaterial = false;
    private Material customMissileMaterial;
    
    // プレハブと ScriptableObject の保存先
    private string prefabPath = "Assets/Resources/Prefabs/Missiles";
    private string dataPath = "Assets/Resources/Data/Missiles";
    
    // 生成されたアセットの名前
    private string prefabName = "Missile";
    private bool createNewScriptableObject = true;
    private MissileData existingData;
    
    // Player設定
    private Player targetPlayer;
    private bool assignToPlayer = false;
    
    [MenuItem("Hockey/Create Missile Assets")]
    public static void ShowWindow()
    {
        GetWindow<MissileEditor>("ミサイル作成");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("ミサイルアセット作成ツール", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // ScriptableObject 設定セクション
        DrawScriptableObjectSettings();
        
        EditorGUILayout.Space();
        
        // モデル設定セクション
        DrawModelSettings();
        
        EditorGUILayout.Space();
        
        // マテリアル設定セクション
        DrawMaterialSettings();
        
        EditorGUILayout.Space();
        
        // プレイヤー設定セクション
        DrawPlayerSettings();
        
        EditorGUILayout.Space();
        
        // パス設定セクション
        DrawPathSettings();
        
        EditorGUILayout.Space();
        
        // 作成ボタン
        if (GUILayout.Button("ミサイルアセット作成"))
        {
            CreateMissileAssets();
        }
    }
    
    private void DrawScriptableObjectSettings()
    {
        EditorGUILayout.LabelField("ミサイルデータ設定", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        createNewScriptableObject = EditorGUILayout.Toggle("新規データ作成", createNewScriptableObject);
        
        if (createNewScriptableObject)
        {
            dataName = EditorGUILayout.TextField("データ名", dataName);
            firingInterval = EditorGUILayout.FloatField("発射間隔(秒)", firingInterval);
            speed = EditorGUILayout.FloatField("移動速度", speed);
            damage = EditorGUILayout.FloatField("ダメージ量", damage);
            lifetime = EditorGUILayout.FloatField("存在時間(秒)", lifetime);
        }
        else
        {
            existingData = (MissileData)EditorGUILayout.ObjectField("既存ミサイルデータ", existingData, typeof(MissileData), false);
        }
    }
    
    private void DrawModelSettings()
    {
        EditorGUILayout.LabelField("ミサイルモデル設定", EditorStyles.boldLabel);
        
        useCustomModel = EditorGUILayout.Toggle("カスタムモデル使用", useCustomModel);
        
        if (useCustomModel)
        {
            missileModel = (GameObject)EditorGUILayout.ObjectField("モデルプレハブ", missileModel, typeof(GameObject), false);
        }
        else
        {
            // プリミティブ球体の設定
            missileColor = EditorGUILayout.ColorField("ミサイル色", missileColor);
            missileScale = EditorGUILayout.Slider("ミサイルサイズ", missileScale, 0.1f, 10.0f);
        }
    }
    
    private void DrawMaterialSettings()
    {
        EditorGUILayout.LabelField("マテリアル設定", EditorStyles.boldLabel);
        
        // ミサイル本体のマテリアル設定
        useCustomMaterial = EditorGUILayout.Toggle("カスタムマテリアル使用", useCustomMaterial);
        if (useCustomMaterial)
        {
            customMissileMaterial = (Material)EditorGUILayout.ObjectField(
                "ミサイルマテリアル", customMissileMaterial, typeof(Material), false);
        }
    }
    
    private void DrawPlayerSettings()
    {
        EditorGUILayout.LabelField("プレイヤー設定", EditorStyles.boldLabel);
        
        assignToPlayer = EditorGUILayout.Toggle("プレイヤーに設定", assignToPlayer);
        
        if (assignToPlayer)
        {
            EditorGUILayout.HelpBox("ミサイルデータをプレイヤーのミサイルスキルに設定します", MessageType.Info);
            targetPlayer = EditorGUILayout.ObjectField("対象プレイヤー", targetPlayer, typeof(Player), true) as Player;
        }
    }
    
    private void DrawPathSettings()
    {
        EditorGUILayout.LabelField("保存設定", EditorStyles.boldLabel);
        
        prefabPath = EditorGUILayout.TextField("プレハブ保存先", prefabPath);
        if (createNewScriptableObject)
        {
            dataPath = EditorGUILayout.TextField("データ保存先", dataPath);
        }
        prefabName = EditorGUILayout.TextField("プレハブ名", prefabName);
    }
    
    private void CreateMissileAssets()
    {
        MissileData missileData;
        
        // ScriptableObjectの作成または使用
        if (createNewScriptableObject)
        {
            missileData = CreateMissileData();
            if (missileData == null) return;
        }
        else
        {
            if (existingData == null)
            {
                EditorUtility.DisplayDialog("エラー", "既存のミサイルデータが指定されていません。", "OK");
                return;
            }
            missileData = existingData;
        }
        
        // ミサイルPrefabの作成
        CreateMissilePrefab(missileData);
        
        // プレイヤーにミサイルデータを設定
        if (assignToPlayer && targetPlayer != null)
        {
            AssignMissileDataToPlayer(missileData, targetPlayer);
        }
    }
    
    private MissileData CreateMissileData()
    {
        // データパスが存在するか確認し、なければ作成
        EnsureDirectoryExists(dataPath);
        
        // ScriptableObjectを作成
        MissileData missileData = ScriptableObject.CreateInstance<MissileData>();
        
        // データを設定
        missileData.firingInterval = firingInterval;
        missileData.speed = speed;
        missileData.damage = damage;
        missileData.lifetime = lifetime;
        
        // アセットとして保存
        string assetPath = $"{dataPath}/{dataName}.asset";
        
        // 同名のアセットがある場合は確認
        if (AssetDatabase.LoadAssetAtPath<MissileData>(assetPath) != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "上書き確認", 
                $"同名のミサイルデータ '{dataName}' が既に存在します。上書きしますか？", 
                "はい", "いいえ");
                
            if (!overwrite)
            {
                // ユーザーが上書きを拒否した場合
                ScriptableObject.DestroyImmediate(missileData);
                return null;
            }
        }
        
        AssetDatabase.CreateAsset(missileData, assetPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"ミサイルデータを作成しました: {assetPath}");
        
        return missileData;
    }
    
    private void CreateMissilePrefab(MissileData missileData)
    {
        // プレハブパスが存在するか確認し、なければ作成
        EnsureDirectoryExists(prefabPath);
        
        // ミサイルオブジェクトの現在のプレハブを保存
        GameObject currentPrefab = missileData.missilePrefab;
        
        // ミサイルのゲームオブジェクトを作成
        GameObject missileObj;
        
        if (useCustomModel && missileModel != null)
        {
            // カスタムモデルを使用
            missileObj = Instantiate(missileModel);
        }
        else
        {
            // プリミティブの球体を作成
            missileObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            // スケールを設定
            missileObj.transform.localScale = Vector3.one * missileScale;
            
            // マテリアルを設定
            var renderer = missileObj.GetComponent<MeshRenderer>();
            if (useCustomMaterial && customMissileMaterial != null)
            {
                // カスタムマテリアルを使用
                renderer.material = customMissileMaterial;
            }
            else
            {
                // デフォルトマテリアルを作成
                Material material = new Material(Shader.Find("Standard"));
                material.color = missileColor;
                renderer.material = material;
            }
        }
        
        // オブジェクト名を設定
        missileObj.name = prefabName;
        
        // ミサイルコンポーネントの追加
        if (missileObj.GetComponent<Missile>() == null)
        {
            missileObj.AddComponent<Missile>();
        }
        
        // 物理設定
        Rigidbody rb = missileObj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = missileObj.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // コライダーが既にある場合は使用し、なければ追加
        Collider collider = missileObj.GetComponent<Collider>();
        if (collider == null)
        {
            SphereCollider sphereCollider = missileObj.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.5f;
            sphereCollider.isTrigger = true; // トリガーに設定
        }
        else
        {
            // 既存のコライダーをトリガーに設定
            collider.isTrigger = true;
        }
        
        // プレハブの保存パス
        string fullPath = $"{prefabPath}/{prefabName}.prefab";
        
        // 既存のプレハブが存在する場合は、同じパスを使用して参照を維持
        if (currentPrefab != null)
        {
            string existingPath = AssetDatabase.GetAssetPath(currentPrefab);
            if (!string.IsNullOrEmpty(existingPath))
            {
                fullPath = existingPath;
            }
        }
        
        #if UNITY_2018_3_OR_NEWER
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(missileObj, fullPath);
        #else
        GameObject prefab = PrefabUtility.CreatePrefab(fullPath, missileObj);
        #endif
        
        // テンポラリオブジェクトを削除
        DestroyImmediate(missileObj);
        
        // ミサイルデータにプレハブを設定
        if (prefab != null)
        {
            // 既存のデータを使用していて、既にプレハブが設定されている場合は
            // 参照を更新する必要はない（上書き保存のため）
            if (createNewScriptableObject || missileData.missilePrefab == null)
            {
                missileData.missilePrefab = prefab;
                EditorUtility.SetDirty(missileData);
                AssetDatabase.SaveAssets();
            }
            
            EditorUtility.DisplayDialog(
                "成功", 
                $"ミサイルアセットが作成されました！\n" +
                $"プレハブ: {fullPath}\n" +
                $"データ: {AssetDatabase.GetAssetPath(missileData)}", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("エラー", "プレハブの作成に失敗しました。", "OK");
        }
    }
    
    private void AssignMissileDataToPlayer(MissileData missileData, Player player)
    {
        // プレイヤーからミサイルスキルコンポーネントを取得
        MissileSkill skill = player.GetComponent<MissileSkill>();
        
        // ミサイルスキルがなければ追加
        if (skill == null)
        {
            skill = player.gameObject.AddComponent<MissileSkill>();
            Debug.Log($"プレイヤー '{player.name}' にミサイルスキルコンポーネントを追加しました");
        }
        
        // SerializedObjectを使用してSerializeFieldにアクセス
        SerializedObject serializedSkill = new SerializedObject(skill);
        SerializedProperty missileDataProperty = serializedSkill.FindProperty("missileData");
        
        if (missileDataProperty != null)
        {
            missileDataProperty.objectReferenceValue = missileData;
            serializedSkill.ApplyModifiedProperties();
            
            Debug.Log($"プレイヤー '{player.name}' のミサイルスキルにミサイルデータを設定しました");
            
            // プレイ中にスキルを設定した場合、スキルを再初期化
            if (Application.isPlaying)
            {
                // シーンからパックを探す
                Puck puck = FindObjectOfType<Puck>();
                if (puck == null)
                {
                    Debug.LogError("シーン内にPuckが見つかりません。ミサイルスキルが正常に動作しない可能性があります。");
                    return;
                }
                
                skill.Initialize(player, puck);
            }
            
            EditorUtility.SetDirty(player);
        }
        else
        {
            Debug.LogError("ミサイルスキルのmissileDataプロパティが見つかりませんでした");
        }
    }
    
    private void EnsureDirectoryExists(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string folderPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = folderPath;
            }
        }
    }
}