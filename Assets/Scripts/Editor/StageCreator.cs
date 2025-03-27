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
        private float stageWidth = 30f;
        private float stageLength = 40f;
        private Material stageMaterial;
        
        [Header("Destructible Object Settings")]
        private GameObject destructiblePrefab;
        private int destructibleCount = 10;
        private float playerAreaClearance = 10f;
        private Material destructibleMaterial;
        private bool autoAddComponents = true;
        private bool placingDestructible = false;
        private Vector3 lastPlacedPosition;
        [SerializeField] private List<GameObject> level1Prefabs; // 段階1のプレファブリスト
        [SerializeField] private List<GameObject> level2Prefabs; // 段階2のプレファブリスト
        [SerializeField] private List<GameObject> level3Prefabs; // 段階3のプレファブリスト

        [MenuItem("Hockey/Create Stage")]
        public static void ShowWindow()
        {
            GetWindow<StageCreator>("Stage Creator");
        }

        private void OnEnable()
        {
            prefabManager = HockeyPrefabManager.Instance;
            SceneView.duringSceneGui += OnSceneGUI;
            LoadPrefabSettings();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SavePrefabSettings();
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
            stageWidth = EditorGUILayout.FloatField("Stage Width", stageWidth);
            stageLength = EditorGUILayout.FloatField("Stage Length", stageLength);
            stageMaterial = (Material)EditorGUILayout.ObjectField("Stage Material", stageMaterial, typeof(Material), false);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Destructible Object Settings", EditorStyles.boldLabel);
            
            destructiblePrefab = (GameObject)EditorGUILayout.ObjectField(
                "Destructible Prefab", 
                destructiblePrefab ?? prefabManager.DestructiblePrefab, 
                typeof(GameObject), 
                false
            );

            destructibleCount = EditorGUILayout.IntField("Auto Place Count", destructibleCount);
            playerAreaClearance = EditorGUILayout.FloatField("Player Area Clearance", playerAreaClearance);
            destructibleMaterial = (Material)EditorGUILayout.ObjectField("Destructible Material", destructibleMaterial, typeof(Material), false);
            autoAddComponents = EditorGUILayout.Toggle("Auto Add Components", autoAddComponents);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(placingDestructible ? "Stop Placing" : "Start Placing Destructibles"))
            {
                placingDestructible = !placingDestructible;
                if (placingDestructible)
                {
                    EditorUtility.DisplayDialog("Placement Mode", 
                        "Click in the Scene view to place destructible objects.\nPress Esc to exit placement mode.", "OK");
                }
            }
            if (GUILayout.Button("Auto Place Destructibles"))
            {
                AutoPlaceDestructibles();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Level Prefabs", EditorStyles.boldLabel);
        
            // レベル1のプレハブリスト
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Level 1 Prefabs (Easiest)", EditorStyles.boldLabel);
            DrawPrefabList(level1Prefabs);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // レベル2のプレハブリスト
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Level 2 Prefabs (Medium)", EditorStyles.boldLabel);
            DrawPrefabList(level2Prefabs);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // レベル3のプレハブリスト
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Level 3 Prefabs (Hardest)", EditorStyles.boldLabel);
            DrawPrefabList(level3Prefabs);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create Stage"))
            {
                CreateStage();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!placingDestructible) return;

            // Escキーでプレースメントモードを終了
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                placingDestructible = false;
                Repaint();
                return;
            }

            // マウスクリックでDestructibleオブジェクトを配置
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 position = hit.point + Vector3.up * 0.5f; // 地面から少し上に配置
                    PlaceDestructibleObject(position);
                    Event.current.Use();
                }
            }

            // シーンビューの再描画を要求
            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.Repaint();
            }
        }

        private void PlaceDestructibleObject(Vector3 position)
        {
            GameObject destructible;
            if (destructiblePrefab != null)
            {
                destructible = PrefabUtility.InstantiatePrefab(destructiblePrefab) as GameObject;
            }
            else
            {
                destructible = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            if (destructible != null)
            {
                destructible.transform.position = position;
                destructible.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                if (autoAddComponents)
                {
                    DestructibleObject destructibleComponent = destructible.GetComponent<DestructibleObject>();
                    if (destructibleComponent == null)
                    {
                        destructibleComponent = destructible.AddComponent<DestructibleObject>();
                    }
                }

                Undo.RegisterCreatedObjectUndo(destructible, "Place Destructible Object");
            }

            lastPlacedPosition = position;
        }

        private void AutoPlaceDestructibles()
        {
            float minX = -stageWidth/2 + 3;
            float maxX = stageWidth/2 - 3;
            float minZ = -stageLength/2 + playerAreaClearance;
            float maxZ = stageLength/2 - 3;
            
            List<Vector3> usedPositions = new List<Vector3>();
            float minDestructibleDistance = 4f;
            
            for (int i = 0; i < destructibleCount; i++)
            {
                Vector3 position = Vector3.zero;
                bool validPosition = false;
                int attempts = 0;
                
                while (!validPosition && attempts < 50)
                {
                    position = new Vector3(
                        Random.Range(minX, maxX),
                        1f,
                        Random.Range(minZ, maxZ)
                    );
                    
                    validPosition = true;
                    foreach (Vector3 usedPos in usedPositions)
                    {
                        if (Vector3.Distance(position, usedPos) < minDestructibleDistance)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                    attempts++;
                }
                
                if (validPosition)
                {
                    PlaceDestructibleObject(position);
                    usedPositions.Add(position);
                }
            }
        }

        private void CreateStage()
        {
            if (prefabManager == null)
            {
                prefabManager = HockeyPrefabManager.Instance;
            }

            GameObject stageObject = new GameObject("Stage");
            
            // グラウンド（床）の作成
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(stageObject.transform);
            ground.transform.localScale = new Vector3(stageWidth * 0.1f, 1, stageLength * 0.1f);
            
            if (stageMaterial != null)
            {
                ground.GetComponent<MeshRenderer>().material = stageMaterial;
            }
            
            // 壁の追加
            CreateWalls(stageObject);
            
            // 破壊可能オブジェクトの追加
            if (prefabManager.DestructiblePrefab != null || autoAddComponents)
            {
                PlaceDestructibleObjects(stageObject);
            }
            
            // プレハブを保存
            HockeyPrefabManager.EnsurePrefabDirectory();
            string completePath = HockeyPrefabManager.PrefabPath + "/Stage.prefab";
            PrefabUtility.SaveAsPrefabAsset(stageObject, completePath);
            
            DestroyImmediate(stageObject);
            
            EditorUtility.DisplayDialog("Success", "Stage prefab has been created!", "OK");
        }
        
        private void CreateWalls(GameObject parent)
        {
            float wallHeight = 2f;
            float wallThickness = 0.5f;
            
            CreateWall(parent, new Vector3(0, wallHeight/2, stageLength/2), new Vector3(stageWidth, wallHeight, wallThickness));
            CreateWall(parent, new Vector3(0, wallHeight/2, -stageLength/2), new Vector3(stageWidth, wallHeight, wallThickness));
            CreateWall(parent, new Vector3(stageWidth/2, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, stageLength));
            CreateWall(parent, new Vector3(-stageWidth/2, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, stageLength));
        }

        private void CreateWall(GameObject parent, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(parent.transform);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            
            if (stageMaterial != null)
            {
                wall.GetComponent<MeshRenderer>().material = stageMaterial;
            }
        }
        
        private void PlaceDestructibleObjects(GameObject parent)
        {
            float minX = -stageWidth/2 + 3;
            float maxX = stageWidth/2 - 3;
            float minZ = -stageLength/2 + playerAreaClearance;
            float maxZ = stageLength/2 - 3;
            
            // ステージの奥行きを3つの領域に分割
            float zoneDepth = (maxZ - minZ) / 3f;
            float zone1MaxZ = minZ + zoneDepth;
            float zone2MaxZ = zone1MaxZ + zoneDepth;
            
            List<Vector3> usedPositions = new List<Vector3>();
            float minDestructibleDistance = 4f;
            
            GameObject destructiblesContainer = new GameObject("DestructibleObjects");
            destructiblesContainer.transform.SetParent(parent.transform);
            
            for (int i = 0; i < destructibleCount; i++)
            {
                Vector3 position = Vector3.zero;
                bool validPosition = false;
                int attempts = 0;
                
                // オブジェクトの段階を決定
                int level;
                float z;
                
                while (!validPosition && attempts < 50)
                {
                    // Z位置に基づいて段階を決定
                    z = Random.Range(minZ, maxZ);
                    if (z <= zone1MaxZ)
                    {
                        level = 1;
                    }
                    else if (z <= zone2MaxZ)
                    {
                        level = 2;
                    }
                    else
                    {
                        level = 3;
                    }
                    
                    position = new Vector3(
                        Random.Range(minX, maxX),
                        1f,
                        z
                    );
                    
                    validPosition = true;
                    foreach (Vector3 usedPos in usedPositions)
                    {
                        if (Vector3.Distance(position, usedPos) < minDestructibleDistance)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                    attempts++;
                }
                
                if (validPosition)
                {
                    GameObject destructible = null;
                    List<GameObject> prefabList = GetPrefabListForLevel(position.z, zone1MaxZ, zone2MaxZ, maxZ);
                    
                    if (prefabList != null && prefabList.Count > 0)
                    {
                        // ランダムにプレファブを選択
                        GameObject selectedPrefab = prefabList[Random.Range(0, prefabList.Count)];
                        destructible = PrefabUtility.InstantiatePrefab(selectedPrefab, destructiblesContainer.transform) as GameObject;
                    }
                    else
                    {
                        destructible = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        destructible.transform.SetParent(destructiblesContainer.transform);
                    }
                    
                    if (destructible != null)
                    {
                        destructible.name = $"Destructible_Level{GetLevelForPosition(position.z, zone1MaxZ, zone2MaxZ, maxZ)}_{i}";
                        destructible.transform.localPosition = position;
                        destructible.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        
                        if (autoAddComponents)
                        {
                            DestructibleObject destructibleComp = destructible.GetComponent<DestructibleObject>();
                            if (destructibleComp == null)
                            {
                                destructibleComp = destructible.AddComponent<DestructibleObject>();
                            }
                            
                            // レベルに応じた設定
                            var levelField = typeof(DestructibleObject).GetField("requiredLevel", 
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                            if (levelField != null)
                            {
                                int objLevel = GetLevelForPosition(position.z, zone1MaxZ, zone2MaxZ, maxZ);
                                levelField.SetValue(destructibleComp, objLevel);
                                
                                // レベルに応じて耐久力を設定
                                var hpField = typeof(DestructibleObject).GetField("maxHitPoints",
                                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                                if (hpField != null)
                                {
                                    float hp = objLevel * 100f; // レベルに応じて耐久力を増加
                                    hpField.SetValue(destructibleComp, hp);
                                }
                            }
                            
                            DestructibleObjectView destructibleView = destructible.GetComponent<DestructibleObjectView>();
                            if (destructibleView == null)
                            {
                                destructibleView = destructible.AddComponent<DestructibleObjectView>();
                            }
                            destructibleView.Initialize(destructibleComp);
                        }
                        
                        usedPositions.Add(position);
                    }
                }
            }
        }
        
        private List<GameObject> GetPrefabListForLevel(float z, float zone1MaxZ, float zone2MaxZ, float maxZ)
        {
            int level = GetLevelForPosition(z, zone1MaxZ, zone2MaxZ, maxZ);
            switch (level)
            {
                case 1:
                    return level1Prefabs;
                case 2:
                    return level2Prefabs;
                case 3:
                    return level3Prefabs;
                default:
                    return level1Prefabs;
            }
        }

        private int GetLevelForPosition(float z, float zone1MaxZ, float zone2MaxZ, float maxZ)
        {
            if (z <= zone1MaxZ)
            {
                return 1;
            }
            else if (z <= zone2MaxZ)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        private void DrawPrefabList(List<GameObject> prefabList)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Prefab", GUILayout.Width(100)))
            {
                prefabList.Add(null);
            }
            if (GUILayout.Button("Clear All", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Clear Prefabs", 
                    "Are you sure you want to clear all prefabs from this list?", 
                    "Yes", "No"))
                {
                    prefabList.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel++;
            
            for (int i = 0; i < prefabList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                prefabList[i] = (GameObject)EditorGUILayout.ObjectField(
                    $"Prefab {i + 1}", 
                    prefabList[i], 
                    typeof(GameObject), 
                    false
                );
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    prefabList.RemoveAt(i);
                    i--;
                    continue;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUI.indentLevel--;
        }

        private void SavePrefabSettings()
        {
            string settings = JsonUtility.ToJson(new PrefabSettings
            {
                level1Prefabs = level1Prefabs,
                level2Prefabs = level2Prefabs,
                level3Prefabs = level3Prefabs
            });
            
            EditorPrefs.SetString("StageCreator_PrefabSettings", settings);
        }

        private void LoadPrefabSettings()
        {
            string settings = EditorPrefs.GetString("StageCreator_PrefabSettings", "");
            if (!string.IsNullOrEmpty(settings))
            {
                PrefabSettings prefabSettings = JsonUtility.FromJson<PrefabSettings>(settings);
                if (prefabSettings != null)
                {
                    level1Prefabs = prefabSettings.level1Prefabs ?? new List<GameObject>();
                    level2Prefabs = prefabSettings.level2Prefabs ?? new List<GameObject>();
                    level3Prefabs = prefabSettings.level3Prefabs ?? new List<GameObject>();
                }
            }
        }
    }

    [System.Serializable]
    public class PrefabSettings
    {
        public List<GameObject> level1Prefabs;
        public List<GameObject> level2Prefabs;
        public List<GameObject> level3Prefabs;
    }
}