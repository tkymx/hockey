using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class GameSceneSetupEditor : EditorWindow
{
    private GameObject playerPrefab;
    private GameObject stagePrefab;

    [MenuItem("Hockey/Setup Game Scene")]
    public static void ShowWindow()
    {
        GetWindow<GameSceneSetupEditor>("Game Scene Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Hockey Game Scene Setup", EditorStyles.boldLabel);

        playerPrefab = EditorGUILayout.ObjectField("Player Prefab", playerPrefab, typeof(GameObject), false) as GameObject;
        stagePrefab = EditorGUILayout.ObjectField("Stage Prefab", stagePrefab, typeof(GameObject), false) as GameObject;

        if (GUILayout.Button("Setup Scene"))
        {
            SetupGameScene();
        }
    }

    private void SetupGameScene()
    {
        if (playerPrefab == null || stagePrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign all prefabs first!", "OK");
            return;
        }

        // Create main objects
        GameObject gameManagerObj = new GameObject("GameManager");
        GameObject stageManagerObj = new GameObject("StageManager");
        GameObject playerManagerObj = new GameObject("PlayerManager");
        GameObject mouseInputObj = new GameObject("MouseInputController");

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

        // Setup SerializeField references via SerializedObject
        SerializedObject gameManagerSO = new SerializedObject(gameManager);
        gameManagerSO.FindProperty("stageManager").objectReferenceValue = stageManager;
        gameManagerSO.FindProperty("playerManager").objectReferenceValue = playerManager;
        gameManagerSO.FindProperty("mouseInputController").objectReferenceValue = mouseInput;
        gameManagerSO.ApplyModifiedProperties();

        // Setup prefab references
        SerializedObject stageManagerSO = new SerializedObject(stageManager);
        stageManagerSO.FindProperty("stagePrefab").objectReferenceValue = stagePrefab;
        stageManagerSO.ApplyModifiedProperties();

        SerializedObject playerManagerSO = new SerializedObject(playerManager);
        playerManagerSO.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
        playerManagerSO.ApplyModifiedProperties();

        EditorUtility.DisplayDialog("Success", "Game scene setup completed!", "OK");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}