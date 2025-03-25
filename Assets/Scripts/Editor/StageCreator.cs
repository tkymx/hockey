using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class StageCreator : EditorWindow
{
    private float stageWidth = 30f;
    private float stageLength = 40f;
    private Material stageMaterial;
    private GameObject obstacleObject;
    private int obstacleCount = 10;
    private float playerAreaClearance = 10f; // プレイヤーエリアの空きスペース

    [MenuItem("Hockey/Create Stage")]
    public static void ShowWindow()
    {
        GetWindow<StageCreator>("Stage Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Stage Creator", EditorStyles.boldLabel);

        stageWidth = EditorGUILayout.FloatField("Stage Width", stageWidth);
        stageLength = EditorGUILayout.FloatField("Stage Length", stageLength);
        stageMaterial = (Material)EditorGUILayout.ObjectField("Stage Material", stageMaterial, typeof(Material), false);
        obstacleObject = (GameObject)EditorGUILayout.ObjectField("Obstacle Prefab", obstacleObject, typeof(GameObject), false);
        obstacleCount = EditorGUILayout.IntField("Obstacle Count", obstacleCount);
        playerAreaClearance = EditorGUILayout.FloatField("Player Area Clearance", playerAreaClearance);

        if (GUILayout.Button("Create Stage"))
        {
            CreateStage();
        }
    }

    private void CreateStage()
    {
        // Create stage object
        GameObject stageObject = new GameObject("Stage");
        
        // Create ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(stageObject.transform);
        ground.transform.localScale = new Vector3(stageWidth * 0.1f, 1, stageLength * 0.1f);
        
        if (stageMaterial != null)
        {
            ground.GetComponent<MeshRenderer>().material = stageMaterial;
        }

        // Add walls
        CreateWalls(stageObject);

        // Add random obstacles
        if (obstacleObject != null)
        {
            PlaceRandomObstacles(stageObject);
        }

        // Create prefab
        string prefabPath = "Assets/Resources/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            System.IO.Directory.CreateDirectory(prefabPath);
            AssetDatabase.Refresh();
        }

        // Save prefab
        string completePath = prefabPath + "/Stage.prefab";
        PrefabUtility.SaveAsPrefabAsset(stageObject, completePath);
        
        // Clean up
        DestroyImmediate(stageObject);
        
        EditorUtility.DisplayDialog("Success", "Stage prefab has been created!", "OK");
    }

    private void CreateWalls(GameObject parent)
    {
        float wallHeight = 2f;
        float wallThickness = 0.5f;

        // Create walls
        CreateWall(parent, new Vector3(0, wallHeight/2, stageLength/2), new Vector3(stageWidth, wallHeight, wallThickness)); // Top
        CreateWall(parent, new Vector3(0, wallHeight/2, -stageLength/2), new Vector3(stageWidth, wallHeight, wallThickness)); // Bottom
        CreateWall(parent, new Vector3(stageWidth/2, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, stageLength)); // Right
        CreateWall(parent, new Vector3(-stageWidth/2, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, stageLength)); // Left
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

    private void PlaceRandomObstacles(GameObject parent)
    {
        float minX = -stageWidth/2 + 1;
        float maxX = stageWidth/2 - 1;
        float minZ = -stageLength/2 + playerAreaClearance; // プレイヤーエリアを確保
        float maxZ = stageLength/2 - 1;

        List<Vector3> usedPositions = new List<Vector3>();
        float minObstacleDistance = 2f;

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 position = Vector3.zero;
            bool validPosition = false;
            int attempts = 0;
            
            while (!validPosition && attempts < 50)
            {
                position = new Vector3(
                    Random.Range(minX, maxX),
                    obstacleObject.transform.localScale.y / 2,
                    Random.Range(minZ, maxZ)
                );

                validPosition = true;
                foreach (Vector3 usedPos in usedPositions)
                {
                    if (Vector3.Distance(position, usedPos) < minObstacleDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }
                attempts++;
            }

            if (validPosition)
            {
                GameObject obstacle = PrefabUtility.InstantiatePrefab(obstacleObject, parent.transform) as GameObject;
                obstacle.transform.localPosition = position;
                obstacle.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                usedPositions.Add(position);
            }
        }
    }
}