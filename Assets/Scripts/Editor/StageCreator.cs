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
        
        [Header("Destructible Object Settings")]
        private int destructibleCount = 10;
        private float playerAreaClearance = 10f;
        private Material destructibleMaterial;
        private bool autoAddComponents = true;
        
        [Header("Puck Settings")]
        private GameObject puckPrefab;
        private Vector3 puckSpawnPosition = new Vector3(0, 0.5f, -10f);
        private bool createPuck = true;
        private float stageWidth;
        private float stageLength;

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
            
            // 基本ステージ設定
            EditorGUILayout.LabelField("Basic Stage Settings", EditorStyles.boldLabel);
            stageWidth = EditorGUILayout.FloatField("Stage Width", stageWidth);
            stageLength = EditorGUILayout.FloatField("Stage Length", stageLength);
            stageMaterial = (Material)EditorGUILayout.ObjectField("Stage Material", stageMaterial, typeof(Material), false);
            
            EditorGUILayout.Space();
            
            // 破壊可能オブジェクト設定
            EditorGUILayout.LabelField("Destructible Object Settings", EditorStyles.boldLabel);
            
            // 共通設定から参照
            prefabManager.DestructiblePrefab = 
                (GameObject)EditorGUILayout.ObjectField("Destructible Prefab", 
                    prefabManager.DestructiblePrefab, typeof(GameObject), false);
            
            destructibleCount = EditorGUILayout.IntField("Destructible Count", destructibleCount);
            playerAreaClearance = EditorGUILayout.FloatField("Player Area Clearance", playerAreaClearance);
            destructibleMaterial = (Material)EditorGUILayout.ObjectField("Destructible Material", destructibleMaterial, typeof(Material), false);
            
            prefabManager.ExplosionEffectPrefab = 
                (GameObject)EditorGUILayout.ObjectField("Explosion Effect Prefab", 
                    prefabManager.ExplosionEffectPrefab, typeof(GameObject), false);
                    
            autoAddComponents = EditorGUILayout.Toggle("Auto Add Components", autoAddComponents);
            
            EditorGUILayout.Space();
            
            // パック設定
            EditorGUILayout.LabelField("Puck Settings", EditorStyles.boldLabel);
            createPuck = EditorGUILayout.Toggle("Create Puck", createPuck);
            puckPrefab = (GameObject)EditorGUILayout.ObjectField("Puck Prefab", puckPrefab, typeof(GameObject), false);
            puckSpawnPosition = EditorGUILayout.Vector3Field("Puck Spawn Position", puckSpawnPosition);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create Stage"))
            {
                CreateStage();
            }
            
            // デバッグ機能
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create Test Prefabs"))
            {
                CreateTestPrefabs();
            }
            
            if (GUILayout.Button("Setup Test Scene"))
            {
                SetupTestScene();
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
            
            // パックの配置
            if (createPuck)
            {
                CreatePuck(stageObject);
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
            
            List<Vector3> usedPositions = new List<Vector3>();
            float minDestructibleDistance = 4f;
            
            GameObject destructiblesContainer = new GameObject("DestructibleObjects");
            destructiblesContainer.transform.SetParent(parent.transform);
            
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
                    GameObject destructible;
                    
                    if (prefabManager.DestructiblePrefab != null)
                    {
                        destructible = PrefabUtility.InstantiatePrefab(
                            prefabManager.DestructiblePrefab, 
                            destructiblesContainer.transform) as GameObject;
                    }
                    else
                    {
                        destructible = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        destructible.transform.SetParent(destructiblesContainer.transform);
                        
                        if (destructibleMaterial != null)
                        {
                            destructible.GetComponent<MeshRenderer>().material = destructibleMaterial;
                        }
                    }
                    
                    destructible.name = "Destructible_" + i;
                    destructible.transform.localPosition = position;
                    destructible.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    
                    if (autoAddComponents)
                    {
                        DestructibleObject destructibleComp = destructible.AddComponent<DestructibleObject>();
                        DestructibleObjectView destructibleView = destructible.AddComponent<DestructibleObjectView>();
                        
                        if (prefabManager.ExplosionEffectPrefab != null)
                        {
                            var field = typeof(DestructibleObject).GetField("explosionPrefab", 
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                            if (field != null)
                            {
                                field.SetValue(destructibleComp, prefabManager.ExplosionEffectPrefab);
                            }
                        }
                        
                        destructibleView.Initialize(destructibleComp);
                    }
                    
                    usedPositions.Add(position);
                }
            }
        }
        
        private void CreatePuck(GameObject parent)
        {
            GameObject puckObject;
            
            if (puckPrefab != null)
            {
                puckObject = PrefabUtility.InstantiatePrefab(puckPrefab, parent.transform) as GameObject;
            }
            else
            {
                puckObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                puckObject.name = "Puck";
                puckObject.transform.SetParent(parent.transform);
                puckObject.transform.localScale = new Vector3(1f, 0.2f, 1f);
                
                Renderer renderer = puckObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.black;
                }
            }
            
            puckObject.transform.position = puckSpawnPosition;
            
            if (autoAddComponents)
            {
                Puck puckComp = puckObject.GetComponent<Puck>();
                if (puckComp == null)
                {
                    puckComp = puckObject.AddComponent<Puck>();
                }
                
                PuckView puckView = puckObject.GetComponent<PuckView>();
                if (puckView == null)
                {
                    puckView = puckObject.AddComponent<PuckView>();
                    puckView.Initialize(puckComp);
                }
                
                GameObject puckControllerObj = new GameObject("PuckController");
                puckControllerObj.transform.SetParent(parent.transform);
                
                PuckController puckController = puckControllerObj.AddComponent<PuckController>();
                puckController.Initialize(puckComp, puckView);
            }
        }
        
        private void CreateTestPrefabs()
        {
            // 必要なフォルダの作成
            string prefabPath = "Assets/Resources/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                Directory.CreateDirectory(prefabPath);
            }
            
            // 爆発エフェクトのプレハブ作成
            GameObject explosionEffect = new GameObject("ExplosionEffect");
            ParticleSystem ps = explosionEffect.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.startSize = 0.5f;
            main.startColor = new ParticleSystem.MinMaxGradient(Color.yellow, Color.red);
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            
            var emission = ps.emission;
            emission.enabled = true;
            var burst = new ParticleSystem.Burst(0.0f, 30);
            emission.SetBurst(0, burst);
            
            string explosionPath = prefabPath + "/ExplosionEffect.prefab";
            PrefabUtility.SaveAsPrefabAsset(explosionEffect, explosionPath);
            DestroyImmediate(explosionEffect);
            
            // 破壊可能オブジェクトのプレハブ作成
            GameObject destructibleObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            destructibleObj.name = "DestructibleCube";
            destructibleObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            
            Renderer renderer = destructibleObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.8f, 0.2f, 0.2f);
            }
            
            DestructibleObject destructibleComp = destructibleObj.AddComponent<DestructibleObject>();
            DestructibleObjectView destructibleView = destructibleObj.AddComponent<DestructibleObjectView>();
            
            GameObject loadedExplosion = AssetDatabase.LoadAssetAtPath<GameObject>(explosionPath);
            if (loadedExplosion != null)
            {
                var field = typeof(DestructibleObject).GetField("explosionPrefab", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(destructibleComp, loadedExplosion);
                }
            }
            
            destructibleView.Initialize(destructibleComp);
            
            string destructiblePath = prefabPath + "/DestructibleCube.prefab";
            PrefabUtility.SaveAsPrefabAsset(destructibleObj, destructiblePath);
            DestroyImmediate(destructibleObj);
            
            // パックのプレハブ作成
            GameObject puckObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            puckObj.name = "Puck";
            puckObj.transform.localScale = new Vector3(1f, 0.2f, 1f);
            
            Renderer puckRenderer = puckObj.GetComponent<Renderer>();
            if (puckRenderer != null)
            {
                puckRenderer.material.color = Color.black;
            }
            
            Puck puckComp = puckObj.AddComponent<Puck>();
            PuckView puckView = puckObj.AddComponent<PuckView>();
            puckView.Initialize(puckComp);
            
            string puckPath = prefabPath + "/Puck.prefab";
            PrefabUtility.SaveAsPrefabAsset(puckObj, puckPath);
            DestroyImmediate(puckObj);
            
            EditorUtility.DisplayDialog("Success", "Test prefabs have been created!", "OK");
        }
        
        private void SetupTestScene()
        {
            // まずテスト用のプレハブを作成
            CreateTestPrefabs();
            
            // 新しいシーン作成
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            }
            else
            {
                return;
            }
            
            GameObject stageObject = new GameObject("Stage");
            
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(stageObject.transform);
            ground.transform.localScale = new Vector3(3f, 1, 4f);
            
            CreateWall(stageObject, new Vector3(0, 1, 20), new Vector3(30, 2, 0.5f));
            CreateWall(stageObject, new Vector3(0, 1, -20), new Vector3(30, 2, 0.5f));
            CreateWall(stageObject, new Vector3(15, 1, 0), new Vector3(0.5f, 2, 40));
            CreateWall(stageObject, new Vector3(-15, 1, 0), new Vector3(0.5f, 2, 40));
            
            // プレイヤーの作成と配置
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            player.name = "Player";
            player.transform.position = new Vector3(0, 0.5f, -15);
            player.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            
            Renderer playerRenderer = player.GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                playerRenderer.material.color = new Color(0.2f, 0.6f, 1f);
            }
            
            player.AddComponent<Player>();
            
            // パックの読み込みと配置
            GameObject puckPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Puck.prefab");
            if (puckPrefab != null)
            {
                GameObject puckObj = PrefabUtility.InstantiatePrefab(puckPrefab) as GameObject;
                puckObj.transform.position = new Vector3(0, 0.5f, -10);
            }
            
            // 破壊可能オブジェクトの配置
            GameObject destructiblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/DestructibleCube.prefab");
            if (destructiblePrefab != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    float x = Random.Range(-10f, 10f);
                    float z = Random.Range(-5f, 15f);
                    
                    GameObject obj = PrefabUtility.InstantiatePrefab(destructiblePrefab) as GameObject;
                    obj.transform.position = new Vector3(x, 0.75f, z);
                    obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                }
            }
            
            // マネージャーの作成と設定
            GameObject gameManager = new GameObject("GameManager");
            GameObject stageManager = new GameObject("StageManager");
            GameObject playerManager = new GameObject("PlayerManager");
            GameObject mouseInput = new GameObject("MouseInputController");
            
            GameManager gm = gameManager.AddComponent<GameManager>();
            StageManager sm = stageManager.AddComponent<StageManager>();
            PlayerManager pm = playerManager.AddComponent<PlayerManager>();
            MouseInputController mic = mouseInput.AddComponent<MouseInputController>();
            
            // シーンの保存
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/TestScene.unity");
            
            EditorUtility.DisplayDialog("Success", "Test scene has been set up!", "OK");
        }
    }
}