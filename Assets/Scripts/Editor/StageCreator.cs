using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

namespace HockeyEditor
{
    using static HockeyPrefabManager;

    [ExecuteInEditMode]
    public class StageCreator : EditorWindow
    {
        private HockeyPrefabManager prefabManager;

        [Header("Stage Settings")]
        private Material stageMaterial;

        [SerializeField] private ZoneSettings zoneSettings;
        private bool showZoneSettings = false;
        private Vector2 zoneSettingsScroll;

        [MenuItem("Hockey/Create Stage")]
        public static void ShowWindow()
        {
            GetWindow<StageCreator>("Stage Creator");
        }

        private void OnEnable()
        {
            prefabManager = HockeyPrefabManager.Instance;
        }


        private void OnGUI()
        {
            if (prefabManager == null)
            {
                prefabManager = HockeyPrefabManager.Instance;
            }

            GUILayout.Label("Stage Creator", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Basic Stage Settings", EditorStyles.boldLabel);
            stageMaterial = (Material)EditorGUILayout.ObjectField("Stage Material", stageMaterial, typeof(Material), false);


            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Level Prefabs", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Stage"))
            {
                CreateStage();
            }

            EditorGUILayout.Space(10);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                showZoneSettings = EditorGUILayout.Foldout(showZoneSettings, "Zone Settings", true);
                if (showZoneSettings)
                {
                    DrawZoneSettings();
                }
            }
        }

        private void DrawZoneSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // ZoneSettings ScriptableObjectの参照
            EditorGUI.BeginChangeCheck();
            zoneSettings = (ZoneSettings)EditorGUILayout.ObjectField(
                "Zone Settings Asset",
                zoneSettings,
                typeof(ZoneSettings),
                false
            );

            if (zoneSettings == null)
            {
                if (GUILayout.Button("Create New Zone Settings"))
                {
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Create Zone Settings",
                        "ZoneSettings",
                        "asset",
                        "Choose where to save the Zone Settings asset"
                    );

                    if (!string.IsNullOrEmpty(path))
                    {
                        zoneSettings = CreateInstance<ZoneSettings>();
                        AssetDatabase.CreateAsset(zoneSettings, path);
                        AssetDatabase.SaveAssets();
                    }
                }
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUI.indentLevel++;

            // 共通マテリアル設定
            zoneSettings.defaultWallMaterial = (Material)EditorGUILayout.ObjectField(
                "Zone Wall Material",
                zoneSettings.defaultWallMaterial,
                typeof(Material),
                false
            );

            zoneSettings.defaultFogMaterial = (Material)EditorGUILayout.ObjectField(
                "Zone Fog Material",
                zoneSettings.defaultFogMaterial,
                typeof(Material),
                false
            );

            EditorGUILayout.Space(5);

            // ゾーンごとの設定
            zoneSettingsScroll = EditorGUILayout.BeginScrollView(zoneSettingsScroll);

            for (int i = 0; i < zoneSettings.zones.Length; i++)
            {
                var zone = zoneSettings.zones[i];
                if (zone == null)
                {
                    zone = new ZoneSettings.ZoneData();
                    zoneSettings.zones[i] = zone;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"Zone {i + 1}", EditorStyles.boldLabel);

                    // サイズ設定を追加
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Size Settings", EditorStyles.boldLabel);
                        zone.width = EditorGUILayout.FloatField("Width", zone.width);
                        zone.depth = EditorGUILayout.FloatField("Depth", zone.depth);
                        zone.playerAreaDepth = EditorGUILayout.FloatField("Player Area Depth", zone.playerAreaDepth);
                    }

                    zone.fogColor = EditorGUILayout.ColorField("Zone Color", zone.fogColor);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Wall Settings", EditorStyles.boldLabel, GUILayout.Width(100));
                        zone.wallHeight = EditorGUILayout.Slider("Height", zone.wallHeight, 0f, 10f);
                        zone.wallThickness = EditorGUILayout.Slider("Thickness", zone.wallThickness, 0.1f, 1f);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Fog Settings", EditorStyles.boldLabel, GUILayout.Width(100));
                        zone.enableFog = EditorGUILayout.Toggle("Enable", zone.enableFog);
                        if (zone.enableFog)
                        {
                            zone.fogHeight = EditorGUILayout.Slider("Height", zone.fogHeight, 0f, 10f);
                        }
                    }

                    EditorGUILayout.Space(5);

                    // Destructible Prefabs Settings
                    EditorGUILayout.LabelField("Destructible Prefabs", EditorStyles.boldLabel);

                    if (zone.destructiblePrefabs == null)
                    {
                        zone.destructiblePrefabs = new List<GameObject>();
                    }

                    EditorGUI.indentLevel++;

                    // Prefab List
                    int prefabCount = zone.destructiblePrefabs.Count;
                    int newPrefabCount = EditorGUILayout.IntField("Prefab Count", prefabCount);

                    if (newPrefabCount != prefabCount)
                    {
                        while (zone.destructiblePrefabs.Count < newPrefabCount)
                            zone.destructiblePrefabs.Add(null);
                        while (zone.destructiblePrefabs.Count > newPrefabCount)
                            zone.destructiblePrefabs.RemoveAt(zone.destructiblePrefabs.Count - 1);
                    }

                    for (int j = 0; j < zone.destructiblePrefabs.Count; j++)
                    {
                        zone.destructiblePrefabs[j] = (GameObject)EditorGUILayout.ObjectField(
                            $"Prefab {j + 1}",
                            zone.destructiblePrefabs[j],
                            typeof(GameObject),
                            false
                        );
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(zoneSettings);
                AssetDatabase.SaveAssets();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void CreateStage()
        {
            if (prefabManager == null)
            {
                prefabManager = HockeyPrefabManager.Instance;
            }

            GameObject stageObject = new GameObject("Stage");

            // グラウンド（床）の作成
            CreateGround(stageObject);

            // ゾーンの作成
            CreateZones(stageObject);

            // プレハブを保存
            HockeyPrefabManager.EnsurePrefabDirectory();
            string completePath = HockeyPrefabManager.PrefabPath + "/Stage.prefab";
            PrefabUtility.SaveAsPrefabAsset(stageObject, completePath);

            DestroyImmediate(stageObject);

            EditorUtility.DisplayDialog("Success", "Stage prefab has been created!", "OK");
        }

        private void CreateGround(GameObject parent)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(parent.transform);

            // 最も大きいゾーンのサイズを基準に
            float maxWidth = 0f;
            float maxDepth = 0f;
            if (zoneSettings != null)
            {
                foreach (var zone in zoneSettings.zones)
                {
                    maxWidth = Mathf.Max(maxWidth, zone.width);
                    maxDepth = Mathf.Max(maxDepth, zone.depth);
                }
            }

            // デフォルト値
            maxWidth = maxWidth > 0 ? maxWidth : 20f;
            maxDepth = maxDepth > 0 ? maxDepth : 20f;

            // グラウンドのサイズを設定（余裕を持たせる）
            ground.transform.localScale = new Vector3(maxWidth * 1.5f, 0.1f, maxDepth * 1.5f);
            ground.transform.position = new Vector3(0, -0.05f, 0);

            if (stageMaterial != null)
            {
                ground.GetComponent<MeshRenderer>().material = stageMaterial;
            }
        }

        private void CreateZones(GameObject parent)
        {
            if (zoneSettings == null)
            {
                Debug.LogError("Zone Settings is not assigned!");
                return;
            }

            // 最初のゾーンを作成
            GameObject firstZone = null;
            float firstZoneBackZ = 0;
            
            for (int i = 0; i < zoneSettings.zones.Length; i++)
            {
                var zoneData = zoneSettings.zones[i];
                if (zoneData == null) continue;

                GameObject zone = new GameObject($"Zone_{i + 1}");
                zone.transform.SetParent(parent.transform);

                // ZoneControllerの追加と設定
                ZoneController zoneController = zone.AddComponent<ZoneController>();
                zoneController.ZoneSettings = zoneSettings;
                zoneController.ZoneLevel = i + 1;
                zoneController.Width = zoneData.width;
                zoneController.Depth = zoneData.depth;

                // ゾーン壁の作成
                if (zoneData.wallHeight > 0)
                {
                    CreateZoneWall(zone, i, zoneData);
                }

                // 破壊可能オブジェクトの配置
                CreateZoneDestructibles(zone, i);

                // フォグエフェクトの作成
                if (zoneData.enableFog && zoneSettings.defaultFogMaterial != null)
                {
                    CreateZoneFog(zone, i, zoneData);
                }
            }
        }

        private void CreateZoneWall(GameObject zone, int zoneIndex, ZoneSettings.ZoneData zoneData)
        {
            GameObject wallContainer = new GameObject("ZoneWall");
            wallContainer.transform.SetParent(zone.transform, false);

            // ZoneWallコンポーネントを追加
            ZoneWall zoneWall = wallContainer.AddComponent<ZoneWall>();

            // 四角形の壁を作成
            CreateRectangularWalls(wallContainer, zoneData);
        }

        private void CreateRectangularWalls(GameObject parent, ZoneSettings.ZoneData zoneData)
        {
            // 四辺の壁を作成
            CreateWallSegment(parent, new Vector3(0, 0, zoneData.depth/2), zoneData.width, zoneData, "Front");
            CreateWallSegment(parent, new Vector3(0, 0, -zoneData.depth/2), zoneData.width, zoneData, "Back");
            CreateWallSegment(parent, new Vector3(zoneData.width/2, 0, 0), zoneData.depth, zoneData, "Right", true);
            CreateWallSegment(parent, new Vector3(-zoneData.width/2, 0, 0), zoneData.depth, zoneData, "Left", true);
        }

        private void CreateWallSegment(GameObject parent, Vector3 position, float length, ZoneSettings.ZoneData zoneData, string name, bool rotated = false)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{name}";
            wall.transform.SetParent(parent.transform, false);
            
            // 壁の位置とスケールを設定
            wall.transform.position = new Vector3(
                position.x,
                zoneData.wallHeight / 2,
                position.z
            );
            
            wall.transform.localScale = new Vector3(
                length,
                zoneData.wallHeight,
                zoneData.wallThickness
            );

            if (rotated)
            {
                wall.transform.rotation = Quaternion.Euler(0, 90, 0);
            }

            // マテリアルの設定
            if (zoneSettings.defaultWallMaterial != null)
            {
                wall.GetComponent<MeshRenderer>().material = zoneSettings.defaultWallMaterial;
            }

            // ColliderをMeshColliderに変更
            DestroyImmediate(wall.GetComponent<BoxCollider>());
            MeshCollider meshCollider = wall.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.isTrigger = false;
        }

        private void CreateZoneFog(GameObject zone, int zoneIndex, ZoneSettings.ZoneData zoneData)
        {
            if (!zoneData.enableFog || zoneSettings.defaultFogMaterial == null) return;

            GameObject fog = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fog.name = "ZoneFog";
            fog.transform.SetParent(zone.transform);

            fog.transform.localScale = new Vector3(
                zoneData.width,
                zoneData.fogHeight,
                zoneData.depth
            );
            fog.transform.position = new Vector3(0, zoneData.fogHeight / 2, 0);

            Material fogMaterialInstance = new Material(zoneSettings.defaultFogMaterial);
            fogMaterialInstance.color = zoneData.fogColor;
            fog.GetComponent<MeshRenderer>().material = fogMaterialInstance;
        }

        private void CreateZoneDestructibles(GameObject zone, int zoneIndex)
        {
            if (zoneSettings == null || zoneIndex >= zoneSettings.zones.Length) return;
            
            var zoneData = zoneSettings.zones[zoneIndex];
            if (zoneData == null || zoneData.destructiblePrefabs == null || zoneData.destructiblePrefabs.Count == 0) return;
            
            // 破壊可能オブジェクトのコンテナを作成
            GameObject destructiblesContainer = new GameObject("Destructibles");
            destructiblesContainer.transform.SetParent(zone.transform, false);
            
            // プレイヤーエリアのサイズを計算
            float safeAreaDepth = zoneData.playerAreaDepth;
            float playableAreaStartZ = -zoneData.depth / 2 + safeAreaDepth;
            float playableAreaEndZ = zoneData.depth / 2;
            float playableWidth = zoneData.width * 0.9f; // 壁に近すぎる場所は避ける
            
            // プレファブの数を決定（ゾーンの広さに応じて変化）
            int prefabCount = Mathf.CeilToInt((zoneData.width * zoneData.depth) / 30f);
            prefabCount = Mathf.Min(prefabCount, 20); // 最大数を制限
            
            // 既に配置した位置を記録して重複を避ける
            List<Vector3> usedPositions = new List<Vector3>();
            
            for (int i = 0; i < prefabCount; i++)
            {
                // ランダムなプレファブを選択
                int prefabIndex = UnityEngine.Random.Range(0, zoneData.destructiblePrefabs.Count);
                GameObject prefab = zoneData.destructiblePrefabs[prefabIndex];
                
                if (prefab == null) continue;
                
                // ランダムな位置を生成（プレイヤーエリアを避ける）
                Vector3 position = GetRandomPosition(
                    playableWidth,
                    playableAreaStartZ,
                    playableAreaEndZ,
                    usedPositions,
                    prefab
                );
                
                // プレファブをインスタンス化して配置
                GameObject destructible = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                destructible.name = $"Destructible_{i+1}";
                destructible.transform.SetParent(destructiblesContainer.transform, false);
                destructible.transform.position = position;
                
                // Y位置を調整（プレファブのサイズに基づいて）
                Renderer renderer = destructible.GetComponent<Renderer>();
                if (renderer != null)
                {
                    float yOffset = renderer.bounds.extents.y;
                    destructible.transform.position = new Vector3(
                        position.x,
                        yOffset,
                        position.z
                    );
                }
                
                // ランダムな回転を適用
                destructible.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                
                // DestructibleObjectとDestructibleObjectViewコンポーネントを追加して設定
                SetupDestructibleComponents(destructible, zoneIndex);
                
                usedPositions.Add(position);
            }
        }
        
        private Vector3 GetRandomPosition(float width, float minZ, float maxZ, List<Vector3> usedPositions, GameObject prefab)
        {
            const int MAX_ATTEMPTS = 30;
            const float MIN_DISTANCE = 2.5f; // オブジェクト間の最小距離
            
            // プレファブのサイズを取得
            float prefabRadius = 1.0f; // デフォルト値
            Renderer renderer = prefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                // プレファブの大きさに基づいて間隔を調整
                prefabRadius = Mathf.Max(
                    renderer.bounds.extents.x,
                    renderer.bounds.extents.z
                );
                prefabRadius *= 1.2f; // 少し余裕を持たせる
            }
            
            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                // ランダムな位置を生成
                float x = UnityEngine.Random.Range(-width/2, width/2);
                float z = UnityEngine.Random.Range(minZ, maxZ);
                Vector3 position = new Vector3(x, 0, z);
                
                // 他のオブジェクトとの距離をチェック
                bool validPosition = true;
                foreach (var usedPos in usedPositions)
                {
                    float distance = Vector2.Distance(
                        new Vector2(position.x, position.z),
                        new Vector2(usedPos.x, usedPos.z)
                    );
                    
                    if (distance < MIN_DISTANCE + prefabRadius)
                    {
                        validPosition = false;
                        break;
                    }
                }
                
                if (validPosition)
                {
                    return position;
                }
            }
            
            // 適切な位置が見つからない場合はフォールバック
            return new Vector3(
                UnityEngine.Random.Range(-width/2, width/2),
                0,
                UnityEngine.Random.Range(minZ, maxZ)
            );
        }

        private void SetupDestructibleComponents(GameObject destructible, int zoneIndex)
        {
            // DestructibleObjectコンポーネントの追加と設定
            DestructibleObject objectModel = destructible.GetComponent<DestructibleObject>();
            if (objectModel == null)
            {
                objectModel = destructible.AddComponent<DestructibleObject>();
            }
                        
            // DestructibleObjectViewコンポーネントの追加と設定
            DestructibleObjectView objectView = destructible.GetComponent<DestructibleObjectView>();
            if (objectView == null)
            {
                objectView = destructible.AddComponent<DestructibleObjectView>();
            }
            
            // Viewを初期化
            objectView.Initialize(objectModel);
        }
    }
}