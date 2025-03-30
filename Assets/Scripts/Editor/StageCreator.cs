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
                        zone.forwardOffset = EditorGUILayout.FloatField("Forward Offset", zone.forwardOffset);
                    }

                    zone.requiredLevel = EditorGUILayout.IntField("Required Level", zone.requiredLevel);
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
                    maxDepth = Mathf.Max(maxDepth, zone.depth + zone.forwardOffset);
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
                
                // 位置を設定
                if (i == 0)
                {
                    // 最初のゾーンは原点中心に配置
                    zone.transform.position = Vector3.zero;
                    firstZone = zone;
                    firstZoneBackZ = -zoneData.depth / 2; // 最初のゾーンのBack位置を記録
                }
                else
                {
                    // 後続ゾーンは最初のゾーンのBack位置に自分のBackが来るように配置
                    float zoneBackZ = -zoneData.depth / 2; // このゾーンのローカル座標でのBack位置
                    float targetZ = firstZoneBackZ - zoneBackZ; // 最初のゾーンのBackに合わせるZ位置
                    zone.transform.position = new Vector3(0, 0, targetZ);
                }

                // ZoneControllerの追加と設定
                ZoneController zoneController = zone.AddComponent<ZoneController>();
                zoneController.ZoneLevel = i + 1;
                zoneController.RequiredPlayerLevel = zoneData.requiredLevel;
                zoneController.Width = zoneData.width;
                zoneController.Depth = zoneData.depth;
                zoneController.ForwardOffset = zoneData.forwardOffset;

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
            zoneWall.RequiredLevel = zoneData.requiredLevel;

            // 四角形の壁を作成
            CreateRectangularWalls(wallContainer, zoneData);

            // ゾーンを前方にオフセット
            zone.transform.position = new Vector3(0, 0, zoneData.forwardOffset);
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
            GameObject destructiblesContainer = new GameObject("Destructibles");
            destructiblesContainer.transform.SetParent(zone.transform, false);

            var zoneData = zoneSettings.zones[zoneIndex];
            float zoneArea = zoneData.width * zoneData.depth;
            int objectCount = Mathf.RoundToInt(zoneArea * 0.05f); // 密度調整

            List<GameObject> prefabList = zoneData.destructiblePrefabs;
            if (prefabList == null || prefabList.Count == 0)
            {
                Debug.LogWarning($"Zone {zoneIndex + 1} has no prefabs assigned!");
                return;
            }

            PlaceZoneDestructibles(destructiblesContainer, zoneData.width, zoneData.depth, objectCount, zoneIndex + 1, prefabList);
        }

        private void PlaceZoneDestructibles(GameObject container, float width, float depth,
            int count, int zoneLevel, List<GameObject> prefabList)
        {
            var zoneData = zoneSettings.zones[zoneLevel - 1];
            float minDistance = 3f;
            float wallThickness = zoneData.wallThickness;

            // 壁の内側の実際の配置可能範囲を計算
            float innerWidth = width - (wallThickness * 2);
            float innerDepth = depth - (wallThickness * 2);
            
            // ForwardOffsetを考慮した実際の配置可能範囲
            float usableDepth = innerDepth - zoneData.forwardOffset;
            float zOffset = -zoneData.forwardOffset / 2; // Back側から配置開始位置をずらす
            
            // 利用可能な面積に基づいてオブジェクト数を調整
            float usableArea = innerWidth * usableDepth;
            int adjustedCount = Mathf.Min(count, Mathf.FloorToInt(usableArea / (minDistance * minDistance)));
            
            List<Vector3> placedPositions = new List<Vector3>();

            for (int i = 0; i < adjustedCount; i++)
            {
                Vector3 position = GetValidPositionInZone(innerWidth, usableDepth, zOffset, wallThickness, minDistance, placedPositions);
                if (position != Vector3.zero)
                {
                    GameObject destructible = CreateDestructibleObject(position, container, prefabList, i, zoneLevel);
                    if (destructible != null)
                    {
                        placedPositions.Add(position);
                    }
                }
            }
        }

        private Vector3 GetValidPositionInZone(float width, float depth, float zOffset, float wallThickness, float minDistance, List<Vector3> placedPositions)
        {
            int maxAttempts = 50;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // 壁の内側かつForwardOffset考慮した範囲に配置
                float x = Random.Range(-width/2, width/2);
                float z = Random.Range(-depth/2, depth/2) + zOffset;

                Vector3 position = new Vector3(x, 1f, z);

                // 他のオブジェクトとの距離チェック
                bool isValid = true;
                foreach (Vector3 placedPos in placedPositions)
                {
                    if (Vector3.Distance(position, placedPos) < minDistance)
                    {
                        isValid = false;
                        break;
                    }
                }

                // 壁からの最小距離を確保
                float wallMargin = 1f;
                if (Mathf.Abs(x) > (width/2 - wallMargin))
                {
                    isValid = false;
                }

                if (isValid)
                {
                    return position;
                }

                attempts++;
            }

            return Vector3.zero;
        }

        private GameObject CreateDestructibleObject(Vector3 position, GameObject parent, List<GameObject> prefabList, int index, int zoneLevel)
        {
            GameObject selectedPrefab = prefabList[Random.Range(0, prefabList.Count)];
            GameObject destructible = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;

            if (destructible != null)
            {
                destructible.name = $"Destructible_Zone{zoneLevel}_{index}";
                destructible.transform.position = position;
                destructible.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                destructible.transform.SetParent(parent.transform, false);

                SetupDestructibleComponents(destructible, zoneLevel);
            }

            return destructible;
        }

        private void SetupDestructibleComponents(GameObject destructible, int zoneLevel)
        {
            DestructibleObject destructibleComp = destructible.GetComponent<DestructibleObject>();
            if (destructibleComp == null)
            {
                destructibleComp = destructible.AddComponent<DestructibleObject>();
            }

            // レベルと耐久力の設定
            var levelField = typeof(DestructibleObject).GetField("requiredLevel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (levelField != null)
            {
                levelField.SetValue(destructibleComp, zoneLevel);

                var hpField = typeof(DestructibleObject).GetField("maxHitPoints",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (hpField != null)
                {
                    float hp = zoneLevel * 100f;
                    hpField.SetValue(destructibleComp, hp);
                }
            }

            // ViewComponentの設定
            DestructibleObjectView destructibleView = destructible.GetComponent<DestructibleObjectView>();
            if (destructibleView == null)
            {
                destructibleView = destructible.AddComponent<DestructibleObjectView>();
            }
            destructibleView.Initialize(destructibleComp);
        }
    }
}