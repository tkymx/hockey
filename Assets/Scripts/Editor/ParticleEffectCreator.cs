using UnityEngine;
using UnityEditor;
using Hockey.Data;

public class ParticleEffectCreator : EditorWindow
{
    private ParticleSystem template;
    private SkillType targetSkillType = SkillType.PuckSizeUp;
    private Color particleColor = Color.yellow;
    private float duration = 1.0f;
    private float particleSize = 0.2f;
    private int burstCount = 30;
    private float speedMultiplier = 2.0f;
    private Material particleMaterial;
    private string savePath = "Assets/Prefabs/Effects";
    private string prefabName = "SkillEffect";
    
    [MenuItem("Hockey/Tools/Particle Effect Creator")]
    public static void ShowWindow()
    {
        GetWindow<ParticleEffectCreator>("パーティクルエフェクト作成");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("パーティクルエフェクト作成ツール", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("基本設定", EditorStyles.boldLabel);
        
        targetSkillType = (SkillType)EditorGUILayout.EnumPopup("スキルタイプ", targetSkillType);
        template = (ParticleSystem)EditorGUILayout.ObjectField("テンプレート", template, typeof(ParticleSystem), false);
        
        EditorGUILayout.Space(5);
        
        if (template == null)
        {
            // テンプレートがない場合は詳細設定を表示
            particleColor = EditorGUILayout.ColorField("パーティクル色", GetDefaultColorForSkillType(targetSkillType));
            duration = EditorGUILayout.Slider("エフェクト時間", duration, 0.1f, 3.0f);
            particleSize = EditorGUILayout.Slider("パーティクルサイズ", particleSize, 0.05f, 1.0f);
            burstCount = EditorGUILayout.IntSlider("パーティクル数", burstCount, 5, 100);
            speedMultiplier = EditorGUILayout.Slider("速度倍率", speedMultiplier, 0.5f, 5.0f);
            particleMaterial = (Material)EditorGUILayout.ObjectField("マテリアル", particleMaterial, typeof(Material), false);
        }
        
        EditorGUILayout.Space(10);
        GUILayout.Label("保存設定", EditorStyles.boldLabel);
        
        savePath = EditorGUILayout.TextField("保存パス", savePath);
        prefabName = EditorGUILayout.TextField("プレハブ名", GetDefaultNameForSkillType(targetSkillType));
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("エフェクト作成"))
        {
            CreateParticleEffect();
        }
        
        if (GUILayout.Button("プレビュー作成"))
        {
            CreatePreviewParticleEffect();
        }
    }
    
    private void CreateParticleEffect()
    {
        // 保存パスが存在するか確認
        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
        }
        
        GameObject effectObj;
        ParticleSystem particleSystem;
        
        if (template != null)
        {
            // テンプレートからコピー
            effectObj = Instantiate(template.gameObject);
            particleSystem = effectObj.GetComponent<ParticleSystem>();
            
            // スキルタイプに応じて色を変更
            var main = particleSystem.main;
            main.startColor = GetDefaultColorForSkillType(targetSkillType);
        }
        else
        {
            // 新規作成
            effectObj = new GameObject(prefabName);
            particleSystem = effectObj.AddComponent<ParticleSystem>();
            
            // 基本設定
            var main = particleSystem.main;
            main.startColor = particleColor;
            main.startSize = particleSize;
            main.startLifetime = duration;
            main.maxParticles = burstCount * 2;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // エミッション設定
            var emission = particleSystem.emission;
            emission.enabled = false; // 自動再生しない
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, burstCount) });
            
            // シェイプ設定
            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;
            shape.radiusThickness = 0f;
            
            // 速度設定
            var velocity = particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.speedModifier = speedMultiplier;
            
            // サイズ変化設定
            var sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 1f),
                new Keyframe(0.8f, 1f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // カラー変化設定
            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(particleColor, 0.0f), new GradientColorKey(particleColor, 0.7f), new GradientColorKey(particleColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.1f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);
            
            // マテリアル設定
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (particleMaterial != null)
            {
                renderer.material = particleMaterial;
            }
            else
            {
                // デフォルトパーティクルマテリアル
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            }
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }
        
        // プレハブとして保存
        string fullPath = $"{savePath}/{prefabName}.prefab";
        
        // 既存のプレハブを確認
        if (AssetDatabase.LoadAssetAtPath<GameObject>(fullPath) != null)
        {
            if (!EditorUtility.DisplayDialog("上書き確認", 
                $"{fullPath} は既に存在します。上書きしますか？", 
                "上書き", "キャンセル"))
            {
                DestroyImmediate(effectObj);
                return;
            }
        }
        
        // プレハブを作成/更新
        PrefabUtility.SaveAsPrefabAsset(effectObj, fullPath);
        DestroyImmediate(effectObj);
        
        Debug.Log($"パーティクルエフェクトをプレハブとして保存しました: {fullPath}");
        AssetDatabase.Refresh();
        
        // 作成したプレハブを選択
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
    }
    
    private void CreatePreviewParticleEffect()
    {
        GameObject effectObj;
        ParticleSystem particleSystem;
        
        if (template != null)
        {
            // テンプレートからコピー
            effectObj = Instantiate(template.gameObject);
            particleSystem = effectObj.GetComponent<ParticleSystem>();
            
            // スキルタイプに応じて色を変更
            var main = particleSystem.main;
            main.startColor = GetDefaultColorForSkillType(targetSkillType);
        }
        else
        {
            // 新規作成
            effectObj = new GameObject($"Preview_{prefabName}");
            particleSystem = effectObj.AddComponent<ParticleSystem>();
            
            // 基本設定
            var main = particleSystem.main;
            main.startColor = particleColor;
            main.startSize = particleSize;
            main.startLifetime = duration;
            main.maxParticles = burstCount * 2;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // エミッション設定
            var emission = particleSystem.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, burstCount) });
            
            // シェイプ設定
            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;
            shape.radiusThickness = 0f;
            
            // 速度設定
            var velocity = particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.speedModifier = speedMultiplier;
            
            // サイズ変化設定
            var sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 1f),
                new Keyframe(0.8f, 1f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // カラー変化設定
            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(particleColor, 0.0f), new GradientColorKey(particleColor, 0.7f), new GradientColorKey(particleColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.1f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);
            
            // マテリアル設定
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (particleMaterial != null)
            {
                renderer.material = particleMaterial;
            }
            else
            {
                // デフォルトパーティクルマテリアル
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            }
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }
        
        // エディタシーンに配置
        effectObj.transform.position = Vector3.zero;
        particleSystem.Play();
        
        // 選択状態にする
        Selection.activeGameObject = effectObj;
        SceneView.FrameLastActiveSceneView();
        
        Debug.Log("プレビューエフェクトを作成しました。シーンビューで確認できます。");
    }
    
    // スキルタイプごとのデフォルト色を返す
    private Color GetDefaultColorForSkillType(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.PuckSizeUp:
                return new Color(1f, 0.8f, 0.2f); // 黄色
            case SkillType.PuckDamageUp:
                return new Color(1f, 0.2f, 0.2f); // 赤色
            case SkillType.PuckPenetration:
                return new Color(0.2f, 0.4f, 1f); // 青色
            default:
                return Color.white;
        }
    }
    
    // スキルタイプごとのデフォルト名を返す
    private string GetDefaultNameForSkillType(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.PuckSizeUp:
                return "SizeUpEffect";
            case SkillType.PuckDamageUp:
                return "DamageUpEffect";
            case SkillType.PuckPenetration:
                return "PenetrationEffect";
            default:
                return "SkillEffect";
        }
    }
}