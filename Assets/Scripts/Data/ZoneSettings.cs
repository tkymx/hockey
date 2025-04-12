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
        public float frontWidth = 15f; // カメラ手前の幅（短い方）
        public float backWidth = 25f;  // カメラ奥の幅（長い方）
        public float depth = 20f;      // 奥行き
        public float playerAreaDepth = 5f; // プレイヤーが操作する手前エリアの深さ
        
        [Header("Wall Settings")]
        [Range(0f, 10f)]
        public float wallHeight = 3f;
        [Range(0.1f, 1f)]
        public float wallThickness = 0.3f;
        public Material wallMaterial;
        
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
                    frontWidth = 15f + (i * 5f),  // カメラ手前の幅（レベルごとに広げる）
                    backWidth = 25f + (i * 10f),  // カメラ奥の幅（レベルごとに広げる）
                    depth = 20f + (i * 10f),
                    playerAreaDepth = 5f,
                    wallHeight = 3f,
                    wallThickness = 0.3f,
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
            
            // 台形の幅が不正な値にならないようにチェック
            zones[i].frontWidth = Mathf.Max(zones[i].frontWidth, 5f); // 最小値を設定
            zones[i].backWidth = Mathf.Max(zones[i].backWidth, zones[i].frontWidth); // 奥側は手前側より必ず広くする
        }
    }
}