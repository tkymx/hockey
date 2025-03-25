using UnityEngine;
using UnityEditor;

public class PlayerPrefabCreator : EditorWindow
{
    private Material playerMaterial;
    private float playerSize = 1f;
    private float playerHeight = 0.2f;
    private int segments = 32;

    [MenuItem("Hockey/Create Player Prefab")]
    public static void ShowWindow()
    {
        GetWindow<PlayerPrefabCreator>("Player Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Player Prefab Creator", EditorStyles.boldLabel);

        playerMaterial = (Material)EditorGUILayout.ObjectField("Player Material", playerMaterial, typeof(Material), false);
        playerSize = EditorGUILayout.FloatField("Player Size", playerSize);
        playerHeight = EditorGUILayout.FloatField("Player Height", playerHeight);
        segments = EditorGUILayout.IntField("Circle Segments", segments);

        if (GUILayout.Button("Create Player Prefab"))
        {
            CreatePlayerPrefab();
        }
    }

    private void CreatePlayerPrefab()
    {
        // Create player game object
        GameObject playerObject = new GameObject("Player");
        
        // Create cylinder for player representation
        GameObject playerModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        playerModel.transform.SetParent(playerObject.transform);
        playerModel.transform.localScale = new Vector3(playerSize, playerHeight, playerSize);
        playerModel.transform.localPosition = Vector3.zero;
        
        if (playerMaterial != null)
        {
            playerModel.GetComponent<MeshRenderer>().material = playerMaterial;
        }

        // Add required components
        Rigidbody rb = playerObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.linearDamping = 1f;
        rb.angularDamping = 0.5f;

        // Add collider
        playerObject.AddComponent<CapsuleCollider>();

        // Add player script
        playerObject.AddComponent<Player>();

        // Create prefab
        string prefabPath = "Assets/Resources/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            System.IO.Directory.CreateDirectory(prefabPath);
            AssetDatabase.Refresh();
        }

        // Save prefab
        string completePath = prefabPath + "/Player.prefab";
        PrefabUtility.SaveAsPrefabAsset(playerObject, completePath);
        
        // Clean up
        DestroyImmediate(playerObject);
        
        EditorUtility.DisplayDialog("Success", "Player prefab has been created!", "OK");
    }
}