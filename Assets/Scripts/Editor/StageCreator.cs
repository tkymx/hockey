using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

namespace HockeyEditor
{
    using static HockeyPrefabManager;

    [System.Serializable]
    public class ConcentricRingSettings
    {
        public float minRadius;
        public float maxRadius;
        public List<GameObject> prefabs = new List<GameObject>();
        public int density = 5; // 配置密度
        [Range(0, 1)]
        public float randomOffset = 0.3f; // 円からのランダムオフセット
    }

    [ExecuteInEditMode]
    public class StageCreator : EditorWindow
    {
        private HockeyPrefabManager prefabManager;

        [Header("Stage Settings")]
        private Material stageMaterial;

        [SerializeField] private ZoneSettings zoneSettings;
        private bool showZoneSettings = false;
        private Vector2 zoneSettingsScroll;

        // 同心円状オブジェクト設定
        [SerializeField] private List<ConcentricRingSettings> concentricRings = new List<ConcentricRingSettings>();
        private bool showConcentricSettings = false;
        private Vector2 concentricSettingsScroll;
        private bool createConcentricObjects = false;

        [MenuItem("Hockey/Create Stage")]
        public static void ShowWindow()
        {
            GetWindow<StageCreator>("Stage Creator");
        }

        private void OnEnable()
        {
            prefabManager = HockeyPrefabManager.Instance;
            
            // デフォルトの同心円設定を追加
            if (concentricRings.Count == 0)
            {
                concentricRings.Add(new ConcentricRingSettings { 
                    minRadius = 0, 
                    maxRadius = 5, 
                    density = 5, 
                    randomOffset = 0.2f 
                });
                concentricRings.Add(new ConcentricRingSettings { 
                    minRadius = 5, 
                    maxRadius = 10, 
                    density = 8, 
                    randomOffset = 0.3f 
                });
                concentricRings.Add(new ConcentricRingSettings { 
                    minRadius = 10, 
                    maxRadius = 15, 
                    density = 12, 
                    randomOffset = 0.4f 
                });
            }
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

            // 同心円状オブジェクト設定の表示
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                showConcentricSettings = EditorGUILayout.Foldout(showConcentricSettings, "Concentric Object Settings", true);
                if (showConcentricSettings)
                {
                    DrawConcentricSettings();
                }
            }

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

        private void DrawConcentricSettings()
        {
            EditorGUILayout.BeginVertical();

            // 同心円状オブジェクトを生成するかどうか
            createConcentricObjects = EditorGUILayout.Toggle("Create Concentric Objects", createConcentricObjects);
            
            if (!createConcentricObjects)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            // 同心円の数を制御
            EditorGUI.indentLevel++;
            
            int ringCount = EditorGUILayout.IntField("Ring Count", concentricRings.Count);
            if (ringCount != concentricRings.Count)
            {
                if (ringCount > concentricRings.Count)
                {
                    while (concentricRings.Count < ringCount)
                    {
                        float lastMaxRadius = concentricRings.Count > 0 ? 
                            concentricRings[concentricRings.Count - 1].maxRadius : 0;
                        
                        concentricRings.Add(new ConcentricRingSettings {
                            minRadius = lastMaxRadius,
                            maxRadius = lastMaxRadius + 5,
                            density = 5 + concentricRings.Count * 2,
                            randomOffset = 0.2f + concentricRings.Count * 0.05f
                        });
                    }
                }
                else
                {
                    while (concentricRings.Count > ringCount)
                    {
                        concentricRings.RemoveAt(concentricRings.Count - 1);
                    }
                }
            }

            concentricSettingsScroll = EditorGUILayout.BeginScrollView(concentricSettingsScroll);
            
            // 各同心円リングの設定
            for (int i = 0; i < concentricRings.Count; i++)
            {
                var ring = concentricRings[i];
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Ring {i+1}", EditorStyles.boldLabel);
                
                // 半径の設定
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Radius Range:", GUILayout.Width(100));
                ring.minRadius = EditorGUILayout.FloatField(ring.minRadius, GUILayout.Width(50));
                EditorGUILayout.LabelField("to", GUILayout.Width(20));
                ring.maxRadius = EditorGUILayout.FloatField(ring.maxRadius, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                // 密度とランダムオフセット
                ring.density = EditorGUILayout.IntSlider("Density", ring.density, 1, 20);
                ring.randomOffset = EditorGUILayout.Slider("Random Offset", ring.randomOffset, 0, 1);
                
                // プレハブリスト
                EditorGUILayout.LabelField("Prefabs:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                int prefabCount = EditorGUILayout.IntField("Count", ring.prefabs.Count);
                if (prefabCount != ring.prefabs.Count)
                {
                    while (ring.prefabs.Count < prefabCount)
                        ring.prefabs.Add(null);
                    while (ring.prefabs.Count > prefabCount)
                        ring.prefabs.RemoveAt(prefabCount);
                }
                
                for (int j = 0; j < ring.prefabs.Count; j++)
                {
                    ring.prefabs[j] = (GameObject)EditorGUILayout.ObjectField(
                        $"Prefab {j+1}",
                        ring.prefabs[j],
                        typeof(GameObject),
                        false
                    );
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;
            
            EditorGUILayout.EndVertical();
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
                        zone.frontWidth = EditorGUILayout.FloatField("Front Width (Camera Side)", zone.frontWidth);
                        zone.backWidth = EditorGUILayout.FloatField("Back Width (Far Side)", zone.backWidth);
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
            List<ZoneController> zoneControllers = CreateZones(stageObject);

            // 同心円状オブジェクトの作成
            if (createConcentricObjects)
            {
                List<DestructibleObject> concentricObjects = CreateConcentricDestructibles(stageObject);
                
                // 各同心円状オブジェクトを対応するゾーンに割り当て
                AssignConcentricObjectsToZones(concentricObjects, zoneControllers);
            }

            // プレハブを保存
            HockeyPrefabManager.EnsurePrefabDirectory();
            string completePath = HockeyPrefabManager.PrefabPath + "/Stage.prefab";
            PrefabUtility.SaveAsPrefabAsset(stageObject, completePath);

            DestroyImmediate(stageObject);

            EditorUtility.DisplayDialog("Success", "Stage prefab has been created!", "OK");
        }

        // ゾーンを作成し、ZoneControllerのリストを返す
        private List<ZoneController> CreateZones(GameObject parent)
        {
            List<ZoneController> zoneControllers = new List<ZoneController>();
            
            if (zoneSettings == null)
            {
                Debug.LogError("Zone Settings is not assigned!");
                return zoneControllers;
            }

            for (int i = 0; i < zoneSettings.zones.Length; i++)
            {
                var zoneData = zoneSettings.zones[i];
                if (zoneData == null) continue;

                GameObject zone = new GameObject($"Zone_{i + 1}");
                zone.transform.SetParent(parent.transform);

                // ZoneControllerの追加と設定
                ZoneController zoneController = zone.AddComponent<ZoneController>();
                zoneController.ZoneSettings = zoneSettings;
                zoneController.ZoneLevel = i;
                zoneController.Width = zoneData.frontWidth;
                zoneController.Depth = zoneData.depth;
                
                zoneControllers.Add(zoneController);

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
            
            return zoneControllers;
        }

        // 同心円状オブジェクトを作成し、生成したDestructibleObjectのリストを返す
        private List<DestructibleObject> CreateConcentricDestructibles(GameObject parent)
        {
            List<DestructibleObject> concentricObjects = new List<DestructibleObject>();
            
            if (concentricRings.Count == 0) return concentricObjects;
            
            // 同心円状オブジェクトのコンテナを作成
            GameObject container = new GameObject("ConcentricDestructibles");
            container.transform.SetParent(parent.transform, false);
            
            // 位置の重複を避けるためのリスト
            List<Vector2> usedPositions = new List<Vector2>();
            
            // 各リングに対して処理
            foreach (var ring in concentricRings)
            {
                if (ring.prefabs.Count == 0) continue;
                
                // オブジェクトの数を計算（円周に基づく）
                float avgRadius = (ring.minRadius + ring.maxRadius) / 2f;
                float circumference = 2f * Mathf.PI * avgRadius;
                int objectCount = Mathf.Max(3, Mathf.RoundToInt(circumference * ring.density / 10f));
                
                for (int i = 0; i < objectCount; i++)
                {
                    // リングからランダムなプレハブを選択
                    GameObject prefab = ring.prefabs[Random.Range(0, ring.prefabs.Count)];
                    if (prefab == null) continue;
                    
                    // 円周上の角度（均等に分布）
                    float angle = (i / (float)objectCount) * 360f;
                    // 角度にランダム性を加える
                    angle += Random.Range(-10f, 10f);
                    
                    // 半径にもランダム性を加える
                    float radius = Random.Range(ring.minRadius, ring.maxRadius);
                    
                    // 極座標から直交座標に変換
                    float radian = angle * Mathf.Deg2Rad;
                    float x = Mathf.Cos(radian) * radius;
                    float z = Mathf.Sin(radian) * radius;
                    
                    // 位置にランダムなオフセットを加える
                    float randomOffsetX = Random.Range(-ring.randomOffset, ring.randomOffset) * radius;
                    float randomOffsetZ = Random.Range(-ring.randomOffset, ring.randomOffset) * radius;
                    
                    Vector3 position = new Vector3(x + randomOffsetX, 0, z + randomOffsetZ);
                    
                    // 重複チェック
                    Vector2 pos2D = new Vector2(position.x, position.z);
                    bool tooClose = false;
                    
                    foreach (var usedPos in usedPositions)
                    {
                        if (Vector2.Distance(pos2D, usedPos) < 2f)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    
                    if (tooClose) continue;
                    
                    // プレハブをインスタンス化
                    GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    obj.name = $"Concentric_Destructible_{ring.minRadius:F1}_{angle:F1}";
                    obj.transform.SetParent(container.transform);
                    
                    // 位置と回転を設定
                    obj.transform.position = position;
                    
                    // Y位置を調整（プレファブのサイズに基づいて）
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        float yOffset = renderer.bounds.extents.y;
                        obj.transform.position = new Vector3(
                            position.x,
                            yOffset,
                            position.z
                        );
                    }
                    
                    // ランダムな回転を適用
                    obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    
                    // DestructibleObjectコンポーネントをセットアップ
                    DestructibleObject destructible = SetupDestructibleComponents(obj, -1); // ゾーンレベル-1はステージ全体のオブジェクト
                    concentricObjects.Add(destructible);
                    
                    // 使用済み位置を記録
                    usedPositions.Add(pos2D);
                }
            }
            
            return concentricObjects;
        }

        // 同心円状オブジェクトを各ゾーンに割り当てる
        private void AssignConcentricObjectsToZones(List<DestructibleObject> concentricObjects, List<ZoneController> zoneControllers)
        {
            if (concentricObjects == null || zoneControllers == null || 
                concentricObjects.Count == 0 || zoneControllers.Count == 0)
                return;
            
            foreach (var obj in concentricObjects)
            {
                if (obj == null) continue;
                
                // オブジェクトが属するゾーンを検索
                foreach (var zoneController in zoneControllers)
                {
                    // オブジェクトがゾーンの台形範囲内にあるかチェック
                    if (zoneController.IsPointInTrapezoid(obj.transform.position))
                    {
                        // ゾーンに同心円オブジェクトを追加
                        zoneController.AddConcentricObject(obj);
                        break;
                    }
                }
            }
        }

        // DestructibleObjectコンポーネントを設定し、参照を返す
        public DestructibleObject SetupDestructibleComponents(GameObject destructible, int zoneIndex)
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
            
            return objectModel;
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
                    maxWidth = Mathf.Max(maxWidth, zone.frontWidth, zone.backWidth);
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

        private void CreateZoneWall(GameObject zone, int zoneIndex, ZoneSettings.ZoneData zoneData)
        {
            GameObject wallContainer = new GameObject("ZoneWall");
            wallContainer.transform.SetParent(zone.transform, false);

            // ZoneWallコンポーネントを追加
            ZoneWall zoneWall = wallContainer.AddComponent<ZoneWall>();

            // 台形の壁を作成
            CreateTrapezoidWalls(wallContainer, zoneData);
        }

        private void CreateTrapezoidWalls(GameObject parent, ZoneSettings.ZoneData zoneData)
        {
            // 台形の壁を作成
            float halfDepth = zoneData.depth / 2;
            float halfFrontWidth = zoneData.frontWidth / 2;
            float halfBackWidth = zoneData.backWidth / 2;

            // 前壁（プレイヤー側）
            CreateWallSegment(parent, new Vector3(0, 0, -halfDepth), zoneData.frontWidth, zoneData, "Front");
            
            // 後壁（奥側）
            CreateWallSegment(parent, new Vector3(0, 0, halfDepth), zoneData.backWidth, zoneData, "Back");
            
            // 側壁（台形の形になるように角度をつけて配置）
            CreateTrapezoidSideWall(parent, 1, halfFrontWidth, halfBackWidth, halfDepth, zoneData, "Right"); // 右側壁
            CreateTrapezoidSideWall(parent, -1, halfFrontWidth, halfBackWidth, halfDepth, zoneData, "Left"); // 左側壁
        }

        private void CreateTrapezoidSideWall(GameObject parent, float side, float halfFrontWidth, float halfBackWidth, float halfDepth, ZoneSettings.ZoneData zoneData, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{name}";
            wall.transform.SetParent(parent.transform, false);
            
            // 壁の中心位置を計算
            // 前後の端点の中間を壁の中心とする
            float frontX = side * halfFrontWidth;
            float backX = side * halfBackWidth;
            float centerX = (frontX + backX) / 2;
            
            // 壁の中心位置
            wall.transform.position = new Vector3(
                centerX,
                zoneData.wallHeight / 2,
                0
            );
            
            // 側壁の長さを計算（ピタゴラスの定理で対角線の長さを求める）
            float wallLength = Mathf.Sqrt(Mathf.Pow(zoneData.depth, 2) + Mathf.Pow(backX - frontX, 2));
            
            // 側壁の回転角度を計算
            float angle = Mathf.Atan2(backX - frontX, zoneData.depth) * Mathf.Rad2Deg;
            
            // 壁を適切な角度に回転
            wall.transform.rotation = Quaternion.Euler(0, angle, 0);
            
            // 壁のスケールを設定
            wall.transform.localScale = new Vector3(
                zoneData.wallThickness,
                zoneData.wallHeight,
                wallLength
            );
            
            // マテリアルの設定
            if (zoneSettings.defaultWallMaterial != null)
            {
                wall.GetComponent<MeshRenderer>().material = zoneSettings.defaultWallMaterial;
            }
            
            // BoxColliderをMeshColliderに変更
            DestroyImmediate(wall.GetComponent<BoxCollider>());
            MeshCollider meshCollider = wall.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.isTrigger = false;
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

            // 台形の平均幅を使ってフォグのサイズを決定
            float averageWidth = (zoneData.frontWidth + zoneData.backWidth) / 2f;
            
            fog.transform.localScale = new Vector3(
                averageWidth,
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
                        
            // プレファブの数を決定（ゾーンの広さに応じて変化）
            float averageWidth = (zoneData.frontWidth + zoneData.backWidth) / 2f;
            int prefabCount = Mathf.CeilToInt((averageWidth * zoneData.depth) / 30f);
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
                    playableAreaStartZ,
                    playableAreaEndZ,
                    usedPositions,
                    prefab,
                    zoneIndex
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
        
        private Vector3 GetRandomPosition(float minZ, float maxZ, List<Vector3> usedPositions, GameObject prefab, int zoneIndex)
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
            
            // 台形形状内にランダムな位置を生成するよう更新
            var zoneData = zoneSettings.zones[zoneIndex]; // 基準のゾーン
            if (zoneSettings != null)
            {
                for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
                {
                    // Z位置に応じた幅の制限を計算（台形の形状に合わせる）
                    float z = UnityEngine.Random.Range(minZ, maxZ);
                    
                    // z位置の正規化値（0～1）
                    float normalizedZ = Mathf.InverseLerp(-zoneData.depth/2, zoneData.depth/2, z);
                    
                    // Z位置に応じた幅を計算（線形補間）
                    float widthAtZ = Mathf.Lerp(zoneData.frontWidth, zoneData.backWidth, normalizedZ) * 0.9f;
                    
                    // この幅内でX位置を決定
                    float x = UnityEngine.Random.Range(-widthAtZ/2, widthAtZ/2);
                    
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
            }
            
            // 適切な位置が見つからない場合はフォールバック
            float fallbackZ = UnityEngine.Random.Range(minZ, maxZ);
            float normalizedFallbackZ = Mathf.InverseLerp(-zoneData.depth/2, zoneData.depth/2, fallbackZ);
            float widthAtFallbackZ = Mathf.Lerp(zoneData.backWidth, zoneData.frontWidth, normalizedFallbackZ) * 0.9f;
            float fallbackX = UnityEngine.Random.Range(-widthAtFallbackZ/2, widthAtFallbackZ/2);
            
            return new Vector3(fallbackX, 0, fallbackZ);
        }
    }
}