using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hockey.Data;
using System.Linq;

public class StageGenerator : MonoBehaviour
{
    [SerializeField] private GameConfigRepository configRepository;
    [SerializeField] private string defaultStageId = "stage_default";
    [SerializeField] private Transform stageParent;

    // Resource paths
    private const string DefaultGroundMaterial = "Materials/DefaultGround";
    private const string DefaultWallMaterial = "Materials/DefaultWall";

    // Current stage data
    private StageData currentStageData;
    private GameObject currentStage;

    private void Awake()
    {
        if (configRepository == null)
        {
            configRepository = FindObjectOfType<GameConfigRepository>();
        }

        if (stageParent == null)
        {
            stageParent = transform;
        }
    }

    public void Initialize()
    {
        if (configRepository != null)
        {
            configRepository.LoadAllConfigs();
        }
        else
        {
            Debug.LogError("GameConfigRepository is not assigned to StageGenerator!");
        }
    }

    // Generate stage by ID
    public GameObject GenerateStageById(string stageId = null)
    {
        // 既存のステージを破棄
        if (currentStage != null)
        {
            Destroy(currentStage);
            currentStage = null;
        }

        // ステージデータの取得
        if (string.IsNullOrEmpty(stageId))
        {
            stageId = defaultStageId;
        }

        currentStageData = configRepository.GetStageById(stageId);
        if (currentStageData == null)
        {
            Debug.LogWarning($"Stage with ID '{stageId}' not found. Using first available stage.");
            currentStageData = configRepository.GetStageByIndex(0);
            if (currentStageData == null)
            {
                Debug.LogError("No stage data available!");
                return null;
            }
        }

        // ステージの生成
        currentStage = new GameObject(currentStageData.stageName);
        currentStage.transform.SetParent(stageParent);
        currentStage.transform.localPosition = Vector3.zero;
        currentStage.transform.localRotation = Quaternion.identity;

        // StageManager コンポーネントを追加
        StageManager stageManager = currentStage.AddComponent<StageManager>();
        stageManager.Initialize();

        // グラウンド（床）の作成
        CreateGround(currentStage);

        // ゾーンの作成
        List<ZoneController> zoneControllers = CreateZones(currentStage);

        // 同心円状オブジェクトの作成
        if (currentStageData.concentricRings != null && currentStageData.concentricRings.Count > 0)
        {
            List<DestructibleObject> concentricObjects = CreateConcentricDestructibles(currentStage);
            
            // 各同心円状オブジェクトを対応するゾーンに割り当て
            AssignConcentricObjectsToZones(concentricObjects, zoneControllers);
        }

        return currentStage;
    }

    // 下位互換性のためのメソッド（名前を使用するオーバーロード）
    public GameObject GenerateStage(string stageName = null)
    {
        // 既存のステージを破棄
        if (currentStage != null)
        {
            Destroy(currentStage);
            currentStage = null;
        }

        // ステージデータの取得
        if (string.IsNullOrEmpty(stageName))
        {
            return GenerateStageById(defaultStageId);
        }

        currentStageData = configRepository.GetStageByName(stageName);
        if (currentStageData == null)
        {
            Debug.LogWarning($"Stage '{stageName}' not found. Using default stage.");
            return GenerateStageById(defaultStageId);
        }

        return GenerateStageById(currentStageData.stageId);
    }

    private void CreateGround(GameObject parent)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(parent.transform);

        // ステージサイズの計算
        float maxWidth = 0f;
        float maxDepth = 0f;
        foreach (var zone in currentStageData.zones)
        {
            maxWidth = Mathf.Max(maxWidth, zone.frontWidth, zone.backWidth);
            maxDepth = Mathf.Max(maxDepth, zone.depth);
        }

        // デフォルト値
        maxWidth = maxWidth > 0 ? maxWidth : 20f;
        maxDepth = maxDepth > 0 ? maxDepth : 20f;

        // グラウンドのサイズを設定（余裕を持たせる）
        ground.transform.localScale = new Vector3(maxWidth * 1.5f, 0.1f, maxDepth * 1.5f);
        ground.transform.position = new Vector3(0, -0.05f, 0);

        // マテリアル設定
        if (!string.IsNullOrEmpty(currentStageData.groundMaterialPath))
        {
            Material groundMat = LoadMaterial(currentStageData.groundMaterialPath);
            if (groundMat != null)
            {
                ground.GetComponent<MeshRenderer>().material = groundMat;
            }
            else
            {
                SetDefaultMaterial(ground, DefaultGroundMaterial);
            }
        }
        else
        {
            SetDefaultMaterial(ground, DefaultGroundMaterial);
        }
    }

    private List<ZoneController> CreateZones(GameObject parent)
    {
        List<ZoneController> zoneControllers = new List<ZoneController>();
        
        if (currentStageData.zones == null || currentStageData.zones.Count == 0)
        {
            Debug.LogError("No zones defined in stage data!");
            return zoneControllers;
        }

        GameObject zonesContainer = new GameObject("Zones");
        zonesContainer.transform.SetParent(parent.transform);
        zonesContainer.transform.localPosition = Vector3.zero;

        for (int i = 0; i < currentStageData.zones.Count; i++)
        {
            ZoneData zoneData = currentStageData.zones[i];
            
            GameObject zone = new GameObject($"Zone_{i + 1}");
            zone.transform.SetParent(zonesContainer.transform);
            zone.transform.localPosition = Vector3.zero;

            // ZoneControllerの追加と設定
            ZoneController zoneController = zone.AddComponent<ZoneController>();
            zoneController.ZoneLevel = i;
            zoneController.Width = zoneData.frontWidth;
            zoneController.Depth = zoneData.depth;
            
            zoneControllers.Add(zoneController);

            // ゾーン壁の作成
            if (zoneData.wall.height > 0)
            {
                CreateZoneWall(zone, zoneData);
            }

            // 破壊可能オブジェクトの配置
            CreateZoneDestructibles(zone, zoneData);
        }
        
        return zoneControllers;
    }

    private void CreateZoneWall(GameObject zone, ZoneData zoneData)
    {
        GameObject wallContainer = new GameObject("ZoneWall");
        wallContainer.transform.SetParent(zone.transform);
        wallContainer.transform.localPosition = Vector3.zero;

        // ZoneWallコンポーネントを追加
        ZoneWall zoneWall = wallContainer.AddComponent<ZoneWall>();

        // 台形の壁を作成
        CreateTrapezoidWalls(wallContainer, zoneData);
    }

    private void CreateTrapezoidWalls(GameObject parent, ZoneData zoneData)
    {
        // 台形の壁を作成
        float halfDepth = zoneData.depth / 2;
        float halfFrontWidth = zoneData.frontWidth / 2;
        float halfBackWidth = zoneData.backWidth / 2;

        // 壁のマテリアルを読み込む
        Material wallMaterial = null;
        if (!string.IsNullOrEmpty(zoneData.wall.materialPath))
        {
            wallMaterial = LoadMaterial(zoneData.wall.materialPath);
        }
        
        if (wallMaterial == null)
        {
            wallMaterial = Resources.Load<Material>(DefaultWallMaterial);
        }

        // 前壁（プレイヤー側）
        CreateWallSegment(parent, new Vector3(0, 0, -halfDepth), zoneData.frontWidth, zoneData.wall, wallMaterial, "Front");
        
        // 後壁（奥側）
        CreateWallSegment(parent, new Vector3(0, 0, halfDepth), zoneData.backWidth, zoneData.wall, wallMaterial, "Back");
        
        // 側壁（台形の形になるように角度をつけて配置）
        CreateTrapezoidSideWall(parent, 1, halfFrontWidth, halfBackWidth, halfDepth, zoneData.wall, wallMaterial, "Right"); // 右側壁
        CreateTrapezoidSideWall(parent, -1, halfFrontWidth, halfBackWidth, halfDepth, zoneData.wall, wallMaterial, "Left"); // 左側壁
    }

    private void CreateTrapezoidSideWall(GameObject parent, float side, float halfFrontWidth, float halfBackWidth, float halfDepth, WallData wallData, Material wallMaterial, string name)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = $"Wall_{name}";
        wall.transform.SetParent(parent.transform);
        
        // 壁の中心位置を計算
        // 前後の端点の中間を壁の中心とする
        float frontX = side * halfFrontWidth;
        float backX = side * halfBackWidth;
        float centerX = (frontX + backX) / 2;
        
        // 壁の中心位置
        wall.transform.position = new Vector3(
            centerX,
            wallData.height / 2,
            0
        );
        
        // 側壁の長さを計算（ピタゴラスの定理で対角線の長さを求める）
        float wallLength = Mathf.Sqrt(Mathf.Pow(halfDepth * 2, 2) + Mathf.Pow(backX - frontX, 2));
        
        // 側壁の回転角度を計算
        float angle = Mathf.Atan2(backX - frontX, halfDepth * 2) * Mathf.Rad2Deg;
        
        // 壁を適切な角度に回転
        wall.transform.rotation = Quaternion.Euler(0, angle, 0);
        
        // 壁のスケールを設定
        wall.transform.localScale = new Vector3(
            wallData.thickness,
            wallData.height,
            wallLength
        );
        
        // マテリアルの設定
        if (wallMaterial != null)
        {
            wall.GetComponent<MeshRenderer>().material = wallMaterial;
        }
        
        // BoxColliderをMeshColliderに変更
        Destroy(wall.GetComponent<BoxCollider>());
        MeshCollider meshCollider = wall.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = false;
    }

    private void CreateWallSegment(GameObject parent, Vector3 position, float length, WallData wallData, Material wallMaterial, string name, bool rotated = false)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = $"Wall_{name}";
        wall.transform.SetParent(parent.transform);
        
        // 壁の位置とスケールを設定
        wall.transform.position = new Vector3(
            position.x,
            wallData.height / 2,
            position.z
        );
        
        wall.transform.localScale = new Vector3(
            length,
            wallData.height,
            wallData.thickness
        );

        if (rotated)
        {
            wall.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        // マテリアルの設定
        if (wallMaterial != null)
        {
            wall.GetComponent<MeshRenderer>().material = wallMaterial;
        }

        // ColliderをMeshColliderに変更
        Destroy(wall.GetComponent<BoxCollider>());
        MeshCollider meshCollider = wall.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = false;
    }

    private void CreateZoneDestructibles(GameObject zone, ZoneData zoneData)
    {
        if (zoneData.destructiblePrefabPaths == null || zoneData.destructiblePrefabPaths.Count == 0) return;
        
        // 破壊可能オブジェクトのコンテナを作成
        GameObject destructiblesContainer = new GameObject("Destructibles");
        destructiblesContainer.transform.SetParent(zone.transform);
        destructiblesContainer.transform.localPosition = Vector3.zero;
        
        // プレイヤーエリアのサイズを計算
        float safeAreaDepth = zoneData.playerAreaDepth;
        float playableAreaStartZ = -zoneData.depth / 2 + safeAreaDepth;
        float playableAreaEndZ = zoneData.depth / 2;
        
        // プレハブの数を決定
        int prefabCount = zoneData.destructiblesCount;
        prefabCount = Mathf.Min(prefabCount, 20); // 最大数を制限
        
        // 既に配置した位置を記録して重複を避ける
        List<Vector3> usedPositions = new List<Vector3>();
        
        for (int i = 0; i < prefabCount; i++)
        {
            // ランダムなプレファブを選択
            int prefabIndex = Random.Range(0, zoneData.destructiblePrefabPaths.Count);
            string prefabPath = zoneData.destructiblePrefabPaths[prefabIndex];
            
            if (string.IsNullOrEmpty(prefabPath)) continue;
            
            // プレハブを読み込む
            GameObject prefab = LoadPrefab(prefabPath);
            if (prefab == null) continue;
            
            // ランダムな位置を生成（プレイヤーエリアを避ける）
            Vector3 position = GetRandomPosition(
                playableAreaStartZ,
                playableAreaEndZ,
                usedPositions,
                prefab,
                zoneData
            );
            
            // プレファブをインスタンス化して配置
            GameObject destructible = Instantiate(prefab);
            destructible.name = $"Destructible_{i+1}";
            destructible.transform.SetParent(destructiblesContainer.transform);
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
            destructible.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            
            // DestructibleObjectとDestructibleObjectViewコンポーネントを追加して設定
            SetupDestructibleComponents(destructible);
            
            usedPositions.Add(position);
        }
    }

    private Vector3 GetRandomPosition(float minZ, float maxZ, List<Vector3> usedPositions, GameObject prefab, ZoneData zoneData)
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
        
        // 台形形状内にランダムな位置を生成
        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            // Z位置に応じた幅の制限を計算（台形の形状に合わせる）
            float z = Random.Range(minZ, maxZ);
            
            // z位置の正規化値（0～1）
            float normalizedZ = Mathf.InverseLerp(-zoneData.depth/2, zoneData.depth/2, z);
            
            // Z位置に応じた幅を計算（線形補間）
            float widthAtZ = Mathf.Lerp(zoneData.frontWidth, zoneData.backWidth, normalizedZ) * 0.9f;
            
            // この幅内でX位置を決定
            float x = Random.Range(-widthAtZ/2, widthAtZ/2);
            
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
        float fallbackZ = Random.Range(minZ, maxZ);
        float normalizedFallbackZ = Mathf.InverseLerp(-zoneData.depth/2, zoneData.depth/2, fallbackZ);
        float widthAtFallbackZ = Mathf.Lerp(zoneData.frontWidth, zoneData.backWidth, normalizedFallbackZ) * 0.9f;
        float fallbackX = Random.Range(-widthAtFallbackZ/2, widthAtFallbackZ/2);
        
        return new Vector3(fallbackX, 0, fallbackZ);
    }

    private List<DestructibleObject> CreateConcentricDestructibles(GameObject parent)
    {
        List<DestructibleObject> concentricObjects = new List<DestructibleObject>();
        
        if (currentStageData.concentricRings == null || currentStageData.concentricRings.Count == 0) 
            return concentricObjects;
        
        // 同心円状オブジェクトのコンテナを作成
        GameObject container = new GameObject("ConcentricDestructibles");
        container.transform.SetParent(parent.transform);
        container.transform.localPosition = Vector3.zero;
        
        // 位置の重複を避けるためのリスト
        List<Vector2> usedPositions = new List<Vector2>();
        
        // 各リングに対して処理
        foreach (var ring in currentStageData.concentricRings)
        {
            if (ring.prefabPaths == null || ring.prefabPaths.Count == 0) continue;
            
            // 使用可能なプレハブをロード
            List<GameObject> availablePrefabs = new List<GameObject>();
            foreach (var prefabPath in ring.prefabPaths)
            {
                GameObject prefab = LoadPrefab(prefabPath);
                if (prefab != null)
                {
                    availablePrefabs.Add(prefab);
                }
            }
            
            if (availablePrefabs.Count == 0) continue;
            
            // オブジェクトの数を計算（円周に基づく）
            float avgRadius = (ring.minRadius + ring.maxRadius) / 2f;
            float circumference = 2f * Mathf.PI * avgRadius;
            int objectCount = Mathf.Max(3, Mathf.RoundToInt(circumference * ring.density / 10f));
            
            for (int i = 0; i < objectCount; i++)
            {
                // リングからランダムなプレハブを選択
                GameObject prefab = availablePrefabs[Random.Range(0, availablePrefabs.Count)];
                
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
                
                // プレファブをインスタンス化
                GameObject obj = Instantiate(prefab);
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
                DestructibleObject destructible = SetupDestructibleComponents(obj);
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
    private DestructibleObject SetupDestructibleComponents(GameObject destructible)
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

    // プレハブのロード
    private GameObject LoadPrefab(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        GameObject prefab = null;
        
        // Resourcesパスかどうかをチェック
        if (path.StartsWith("Assets/Resources/") || path.StartsWith("Resources/"))
        {
            string resourcePath = path;
            if (path.StartsWith("Assets/Resources/"))
            {
                resourcePath = path.Substring("Assets/Resources/".Length);
            }
            else if (path.StartsWith("Resources/"))
            {
                resourcePath = path.Substring("Resources/".Length);
            }
            
            // 拡張子を除去
            resourcePath = System.IO.Path.ChangeExtension(resourcePath, null);
            
            prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogWarning($"Failed to load prefab from Resources: {resourcePath}");
            }
        }
        else
        {
            Debug.LogWarning($"Prefab path is not valid Resources path: {path}");
        }
        
        return prefab;
    }

    // マテリアルのロード
    private Material LoadMaterial(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        Material material = null;
        
        // Resourcesパスかどうかをチェック
        if (path.StartsWith("Assets/Resources/") || path.StartsWith("Resources/"))
        {
            string resourcePath = path;
            if (path.StartsWith("Assets/Resources/"))
            {
                resourcePath = path.Substring("Assets/Resources/".Length);
            }
            else if (path.StartsWith("Resources/"))
            {
                resourcePath = path.Substring("Resources/".Length);
            }
            
            // 拡張子を除去
            resourcePath = System.IO.Path.ChangeExtension(resourcePath, null);
            
            material = Resources.Load<Material>(resourcePath);
            if (material == null)
            {
                Debug.LogWarning($"Failed to load material from Resources: {resourcePath}");
            }
        }
        else
        {
            Debug.LogWarning($"Material path is not valid Resources path: {path}");
        }
        
        return material;
    }

    // デフォルトマテリアルの設定
    private void SetDefaultMaterial(GameObject obj, string defaultMaterialPath)
    {
        Material defaultMat = Resources.Load<Material>(defaultMaterialPath);
        if (defaultMat != null)
        {
            obj.GetComponent<MeshRenderer>().material = defaultMat;
        }
    }
}