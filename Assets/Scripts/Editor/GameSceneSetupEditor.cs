using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HockeyEditor
{
    [ExecuteInEditMode]
    public class GameSceneSetupEditor : EditorWindow
    {
        private HockeyPrefabManager prefabManager;
        
        [Header("Required Prefabs")]
        private GameObject playerPrefab;
        private GameObject stagePrefab;
        private GameObject puckPrefab;

        [MenuItem("Hockey/Setup Game Scene")]
        public static void ShowWindow()
        {
            GetWindow<GameSceneSetupEditor>("Game Scene Setup");
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

            GUILayout.Label("Hockey Game Scene Setup", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            GUILayout.Label("Required Prefabs", EditorStyles.boldLabel);
            playerPrefab = EditorGUILayout.ObjectField("Player Prefab", playerPrefab, typeof(GameObject), false) as GameObject;
            stagePrefab = EditorGUILayout.ObjectField("Stage Prefab", stagePrefab, typeof(GameObject), false) as GameObject;
            puckPrefab = EditorGUILayout.ObjectField("Puck Prefab", puckPrefab, typeof(GameObject), false) as GameObject;
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Setup Scene"))
            {
                SetupGameScene();
            }
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Prefab Settings"))
            {
                HockeyPrefabManager.ShowSettings();
            }
        }

        private void SetupGameScene()
        {
            if (playerPrefab == null || stagePrefab == null || puckPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign all required prefabs first!", "OK");
                return;
            }

            // Create main objects
            GameObject gameManagerObj = new GameObject("GameManager");
            GameObject stageManagerObj = new GameObject("StageManager");
            GameObject playerManagerObj = new GameObject("PlayerManager");
            GameObject mouseInputObj = new GameObject("MouseInputController");
            GameObject puckControllerObj = new GameObject("PuckController");

            // Setup camera
            GameObject cameraObj = new GameObject("MainCamera");
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 10, -10);
            camera.transform.rotation = Quaternion.Euler(45, 0, 0);

            // Add components
            GameManager gameManager = gameManagerObj.AddComponent<GameManager>();
            StageManager stageManager = stageManagerObj.AddComponent<StageManager>();
            PlayerManager playerManager = playerManagerObj.AddComponent<PlayerManager>();
            MouseInputController mouseInput = mouseInputObj.AddComponent<MouseInputController>();
            PuckController puckController = puckControllerObj.AddComponent<PuckController>();

            // Instantiate puck
            GameObject puckObj = PrefabUtility.InstantiatePrefab(puckPrefab) as GameObject;
            if (puckObj != null)
            {
                puckObj.transform.position = new Vector3(0, 0.5f, -5f);
                Puck puck = puckObj.GetComponent<Puck>();
                PuckView puckView = puckObj.GetComponent<PuckView>();
                
                if (puck != null && puckView != null)
                {
                    puckController.Initialize(puck, puckView);
                }
            }

            // Setup SerializeField references via SerializedObject
            SerializedObject gameManagerSO = new SerializedObject(gameManager);
            gameManagerSO.FindProperty("stageManager").objectReferenceValue = stageManager;
            gameManagerSO.FindProperty("playerManager").objectReferenceValue = playerManager;
            gameManagerSO.FindProperty("mouseInputController").objectReferenceValue = mouseInput;
            gameManagerSO.FindProperty("puckController").objectReferenceValue = puckController;
            
            // Set layer masks
            SerializedProperty puckLayerProperty = gameManagerSO.FindProperty("puckLayer");
            if (puckLayerProperty != null)
            {
                puckLayerProperty.intValue = LayerMask.GetMask("Puck");
            }
            
            SerializedProperty objectLayerProperty = gameManagerSO.FindProperty("objectLayer");
            if (objectLayerProperty != null)
            {
                objectLayerProperty.intValue = LayerMask.GetMask("Destructible");
            }
            
            gameManagerSO.ApplyModifiedProperties();

            // Setup prefab references
            SerializedObject stageManagerSO = new SerializedObject(stageManager);
            stageManagerSO.FindProperty("stagePrefab").objectReferenceValue = stagePrefab;
            
            // デストラクティブルプレハブを共通マネージャーから設定
            if (prefabManager.DestructiblePrefab != null)
            {
                stageManagerSO.FindProperty("destructiblePrefab").objectReferenceValue = 
                    prefabManager.DestructiblePrefab;
            }
            
            stageManagerSO.ApplyModifiedProperties();

            SerializedObject playerManagerSO = new SerializedObject(playerManager);
            playerManagerSO.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
            playerManagerSO.ApplyModifiedProperties();

            EditorUtility.DisplayDialog("Success", "Game scene setup completed!", "OK");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}