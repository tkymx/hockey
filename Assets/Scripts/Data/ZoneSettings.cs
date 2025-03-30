using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ZoneSettings", menuName = "Hockey/Zone Settings")]
public class ZoneSettings : ScriptableObject
{
    [Serializable]
    public class ZoneData
    {
        [Header("Basic Settings")]
        public float width = 20f;  // 横幅
        public float depth = 20f;  // 奥行き
        public float forwardOffset = 0f;  // 前方へのオフセット
        public int requiredLevel = 1;
        
        [Header("Wall Settings")]
        [Range(0f, 10f)]
        public float wallHeight = 3f;
        [Range(0.1f, 1f)]
        public float wallThickness = 0.3f;
        public Material wallMaterial;
        
        [Header("Fog Settings")]
        public bool enableFog = true;
        [Range(0f, 10f)]
        public float fogHeight = 5f;
        public Color fogColor = new Color(1f, 1f, 1f, 0.5f);
        
        [Header("Destructible Objects")]
        public List<GameObject> destructiblePrefabs; // このゾーンで使用可能な破壊可能オブジェクトのプレファブ
        
        [Header("Gameplay")]
        public float damageMultiplier = 1f; // このゾーンでのダメージ倍率
        public float scoreMultiplier = 1f;  // このゾーンでのスコア倍率
        
        public ZoneData()
        {
            destructiblePrefabs = new List<GameObject>();
        }
    }

    [Header("Global Settings")]
    public Material defaultWallMaterial;
    public Material defaultFogMaterial;
    
    [Header("Zone Data")]
    public ZoneData[] zones = new ZoneData[5];

    private void OnValidate()
    {
        // デフォルト値の設定
        if (zones == null || zones.Length == 0)
        {
            zones = new ZoneData[5];
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i] = new ZoneData
                {
                    width = 20f + (i * 10f),
                    depth = 20f + (i * 10f),
                    forwardOffset = i * 15f,
                    requiredLevel = i + 1,
                    wallHeight = 3f,
                    wallThickness = 0.3f,
                    fogHeight = 5f,
                    fogColor = new Color(
                        UnityEngine.Random.value,
                        UnityEngine.Random.value,
                        UnityEngine.Random.value,
                        0.5f
                    ),
                    damageMultiplier = 1f + (i * 0.2f), // レベルが上がるごとにダメージ倍率が増加
                    scoreMultiplier = 1f + (i * 0.5f)   // レベルが上がるごとにスコア倍率が増加
                };
            }
        }

        // 値の範囲チェックと自動補正
        for (int i = 0; i < zones.Length; i++)
        {
            if (zones[i] == null)
            {
                zones[i] = new ZoneData();
            }

            // レベル要件が適切な順序になるようにする
            zones[i].requiredLevel = Mathf.Max(1, i + 1);
        }
    }
}