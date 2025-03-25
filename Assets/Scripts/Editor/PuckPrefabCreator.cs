using UnityEngine;
using UnityEditor;

public class PuckPrefabCreator : EditorWindow
{
    private Material puckMaterial;
    private float puckRadius = 0.5f;
    private float puckHeight = 0.2f;
    private float mass = 1.0f;
    private float frictionCoefficient = 0.95f;
    private float maxSpeed = 20.0f;
    private Color trailColor = Color.blue;
    private bool createTrailRenderer = true;
    private bool createParticleEffect = true;

    [MenuItem("Hockey/Create Puck Prefab")]
    public static void ShowWindow()
    {
        GetWindow<PuckPrefabCreator>("Puck Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Puck Prefab Creator", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        GUILayout.Label("Visual Settings", EditorStyles.boldLabel);
        puckMaterial = (Material)EditorGUILayout.ObjectField("Puck Material", puckMaterial, typeof(Material), false);
        puckRadius = EditorGUILayout.FloatField("Puck Radius", puckRadius);
        puckHeight = EditorGUILayout.FloatField("Puck Height", puckHeight);

        EditorGUILayout.Space();
        GUILayout.Label("Physics Settings", EditorStyles.boldLabel);
        mass = EditorGUILayout.FloatField("Mass", mass);
        frictionCoefficient = EditorGUILayout.Slider("Friction Coefficient", frictionCoefficient, 0f, 1f);
        maxSpeed = EditorGUILayout.FloatField("Max Speed", maxSpeed);

        EditorGUILayout.Space();
        GUILayout.Label("Effects Settings", EditorStyles.boldLabel);
        createTrailRenderer = EditorGUILayout.Toggle("Create Trail Renderer", createTrailRenderer);
        if (createTrailRenderer)
        {
            EditorGUI.indentLevel++;
            trailColor = EditorGUILayout.ColorField("Trail Color", trailColor);
            EditorGUI.indentLevel--;
        }
        createParticleEffect = EditorGUILayout.Toggle("Create Hit Particle Effect", createParticleEffect);

        EditorGUILayout.Space();
        if (GUILayout.Button("Create Puck Prefab"))
        {
            CreatePuckPrefab();
        }
    }

    private void CreatePuckPrefab()
    {
        // パックのゲームオブジェクトを作成
        GameObject puckObject = new GameObject("Puck");
        
        // シリンダーでパックを表現
        GameObject puckModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        puckModel.name = "PuckModel";
        puckModel.transform.SetParent(puckObject.transform);
        puckModel.transform.localScale = new Vector3(puckRadius * 2, puckHeight, puckRadius * 2);
        puckModel.transform.localPosition = Vector3.zero;
        
        if (puckMaterial != null)
        {
            puckModel.GetComponent<MeshRenderer>().material = puckMaterial;
        }
        else
        {
            // デフォルトのマテリアルを黒に設定
            puckModel.GetComponent<MeshRenderer>().material.color = Color.black;
        }

        // コライダーの設定
        CapsuleCollider capsuleCollider = puckModel.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            DestroyImmediate(capsuleCollider);
        }
        SphereCollider sphereCollider = puckObject.AddComponent<SphereCollider>();
        sphereCollider.radius = puckRadius;
        sphereCollider.material = new PhysicsMaterial
        {
            bounciness = 0.8f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };

        // Rigidbodyの設定
        Rigidbody rb = puckObject.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.1f;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | 
                        RigidbodyConstraints.FreezeRotationX | 
                        RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Puckコンポーネントの追加
        Puck puck = puckObject.AddComponent<Puck>();
        var puckSO = new SerializedObject(puck);
        puckSO.FindProperty("mass").floatValue = mass;
        puckSO.FindProperty("frictionCoefficient").floatValue = frictionCoefficient;
        puckSO.FindProperty("maxSpeed").floatValue = maxSpeed;
        puckSO.ApplyModifiedProperties();

        // トレイルレンダラーの追加（オプション）
        if (createTrailRenderer)
        {
            TrailRenderer trail = puckObject.AddComponent<TrailRenderer>();
            trail.time = 0.2f;
            trail.minVertexDistance = 0.1f;
            trail.startWidth = puckRadius * 0.5f;
            trail.endWidth = 0f;
            trail.startColor = new Color(trailColor.r, trailColor.g, trailColor.b, 1f);
            trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            
            Material trailMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
            trail.material = trailMaterial;
            trail.enabled = false; // デフォルトでは無効
        }

        // ヒットパーティクルエフェクトの追加（オプション）
        if (createParticleEffect)
        {
            GameObject hitEffectObj = new GameObject("HitEffect");
            hitEffectObj.transform.SetParent(puckObject.transform);
            hitEffectObj.transform.localPosition = Vector3.zero;
            
            ParticleSystem ps = hitEffectObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 2f;
            main.startSize = puckRadius * 0.5f;
            main.startColor = new ParticleSystem.MinMaxGradient(Color.white, trailColor);
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = puckRadius * 0.1f;
            
            var emission = ps.emission;
            emission.enabled = true;
            var burst = new ParticleSystem.Burst(0.0f, 15);
            emission.SetBurst(0, burst);
            
            ps.Stop(); // デフォルトでは停止
        }

        // PuckViewコンポーネントの追加
        PuckView puckView = puckObject.AddComponent<PuckView>();
        puckView.Initialize(puck);

        // プレハブを保存
        string prefabPath = "Assets/Resources/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            System.IO.Directory.CreateDirectory(prefabPath);
            AssetDatabase.Refresh();
        }

        string completePath = prefabPath + "/Puck.prefab";
        PrefabUtility.SaveAsPrefabAsset(puckObject, completePath);
        
        // 後処理
        DestroyImmediate(puckObject);
        
        EditorUtility.DisplayDialog("Success", "Puck prefab has been created!", "OK");
    }
}