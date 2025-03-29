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

                    zone.radius = EditorGUILayout.FloatField("Radius", zone.radius);
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

            float maxRadius = zoneSettings != null ? zoneSettings.zones[zoneSettings.zones.Length - 1].radius : 10f;
            // Make the ground larger than the maximum zone radius
            float groundSize = maxRadius * 2.5f;
            ground.transform.localScale = new Vector3(groundSize, 0.1f, groundSize);
            ground.transform.position = new Vector3(0, -0.05f, 0); // Position slightly below to avoid z-fighting

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

            for (int i = 0; i < zoneSettings.zones.Length; i++)
            {
                var zoneData = zoneSettings.zones[i];
                if (zoneData == null) continue;

                GameObject zone = new GameObject($"Zone_{i + 1}");
                zone.transform.SetParent(parent.transform);

                // ZoneControllerの追加と設定
                ZoneController zoneController = zone.AddComponent<ZoneController>();
                zoneController.ZoneLevel = i + 1;
                zoneController.RequiredPlayerLevel = zoneData.requiredLevel;
                zoneController.Radius = zoneData.radius;

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
            wallContainer.transform.SetParent(zone.transform);

            // ZoneWallコンポーネントを追加
            ZoneWall zoneWall = wallContainer.AddComponent<ZoneWall>();
            zoneWall.RequiredLevel = zoneData.requiredLevel;

            // 6角形の壁を作成
            int segments = 6;
            float radius = zoneData.radius;
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float currentAngle = i * angleStep;
                float nextAngle = (i + 1) * angleStep;

                // 正確な頂点位置を計算
                Vector3 currentPoint = new Vector3(
                    Mathf.Cos(currentAngle * Mathf.Deg2Rad) * radius,
                    0,
                    Mathf.Sin(currentAngle * Mathf.Deg2Rad) * radius
                );

                Vector3 nextPoint = new Vector3(
                    Mathf.Cos(nextAngle * Mathf.Deg2Rad) * radius,
                    0,
                    Mathf.Sin(nextAngle * Mathf.Deg2Rad) * radius
                );

                // 壁の中心位置（2点の中間）
                Vector3 wallCenter = (currentPoint + nextPoint) * 0.5f;
                wallCenter.y = zoneData.wallHeight / 2;

                // 2点間の正確な距離
                float wallLength = Vector3.Distance(currentPoint, nextPoint);

                // 壁セグメントを作成
                GameObject wallSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallSegment.name = $"WallSegment_{i}";
                wallSegment.transform.SetParent(wallContainer.transform);
                wallSegment.transform.position = wallCenter;

                // 壁の向きを正確に調整（2点を結ぶ方向に）
                Vector3 direction = (nextPoint - currentPoint).normalized;
                wallSegment.transform.rotation = Quaternion.LookRotation(direction);

                // 壁のサイズを設定（長さは頂点間の正確な距離）
                wallSegment.transform.localScale = new Vector3(
                    zoneData.wallThickness,
                    zoneData.wallHeight,
                    wallLength
                );

                // MeshColliderに変換してConvex設定
                DestroyImmediate(wallSegment.GetComponent<BoxCollider>());
                MeshCollider meshCollider = wallSegment.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.isTrigger = false;

                // マテリアルの設定
                if (zoneSettings.defaultWallMaterial != null)
                {
                    wallSegment.GetComponent<MeshRenderer>().material = zoneSettings.defaultWallMaterial;
                }
            }
        }

        private void CreateZoneFog(GameObject zone, int zoneIndex, ZoneSettings.ZoneData zoneData)
        {
            if (!zoneData.enableFog || zoneSettings.defaultFogMaterial == null) return;

            GameObject fog = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fog.name = "ZoneFog";
            fog.transform.SetParent(zone.transform);

            fog.transform.localScale = new Vector3(
                zoneData.radius * 2,
                zoneData.fogHeight,
                zoneData.radius * 2
            );
            fog.transform.position = new Vector3(0, zoneData.fogHeight / 2, 0);

            Material fogMaterialInstance = new Material(zoneSettings.defaultFogMaterial);
            fogMaterialInstance.color = zoneData.fogColor;
            fog.GetComponent<MeshRenderer>().material = fogMaterialInstance;
        }

        private void CreateZoneDestructibles(GameObject zone, int zoneIndex)
        {
            GameObject destructiblesContainer = new GameObject("Destructibles");
            destructiblesContainer.transform.SetParent(zone.transform);

            float innerRadius = zoneIndex > 0 ? zoneSettings.zones[zoneIndex - 1].radius : 0;
            float outerRadius = zoneSettings.zones[zoneIndex].radius;

            // ゾーンの面積に応じてオブジェクト数を調整
            float zoneArea = Mathf.PI * (outerRadius * outerRadius - innerRadius * innerRadius);
            int objectCount = Mathf.RoundToInt(zoneArea * 0.05f); // 密度調整

            List<GameObject> prefabList = zoneSettings.zones[zoneIndex].destructiblePrefabs;
            if (prefabList == null || prefabList.Count == 0)
            {
                Debug.LogWarning($"Zone {zoneIndex + 1} has no prefabs assigned!");
                return;
            }

            PlaceZoneDestructibles(destructiblesContainer, innerRadius, outerRadius, objectCount, zoneIndex + 1, prefabList);
        }

        private void PlaceZoneDestructibles(GameObject container, float innerRadius, float outerRadius,
            int count, int zoneLevel, List<GameObject> prefabList)
        {
            float minDistance = 3f;
            List<Vector3> placedPositions = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                Vector3 position = GetValidPositionInZone(innerRadius, outerRadius, minDistance, placedPositions);
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

        private Vector3 GetValidPositionInZone(float innerRadius, float outerRadius, float minDistance, List<Vector3> placedPositions)
        {
            int maxAttempts = 50;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));

                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * radius,
                    1f,
                    Mathf.Sin(angle) * radius
                );

                bool isValid = true;
                foreach (Vector3 placedPos in placedPositions)
                {
                    if (Vector3.Distance(position, placedPos) < minDistance)
                    {
                        isValid = false;
                        break;
                    }
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
            GameObject destructible = PrefabUtility.InstantiatePrefab(selectedPrefab, parent.transform) as GameObject;

            if (destructible != null)
            {
                destructible.name = $"Destructible_Zone{zoneLevel}_{index}";
                destructible.transform.position = position;
                destructible.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

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