using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hockey.Data
{
    // 同心円状のオブジェクト配置設定
    [Serializable]
    public class ConcentricRingData
    {
        public float minRadius;
        public float maxRadius;
        public int density = 5;               // 配置密度
        [Range(0, 1)]
        public float randomOffset = 0.3f;     // 円からのランダムオフセット
        public List<string> prefabPaths = new List<string>();  // プレハブのパス
    }

    // ゾーンの壁の設定
    [Serializable]
    public class WallData
    {
        public float height = 2.0f;
        public float thickness = 0.2f;
        public string materialPath;
    }

    // 各ゾーンの設定
    [Serializable]
    public class ZoneData
    {
        public float frontWidth = 10f;        // 前方（カメラ側）の幅
        public float backWidth = 10f;         // 後方（奥側）の幅
        public float depth = 10f;             // 奥行き
        public float playerAreaDepth = 2f;    // プレイヤーエリアの奥行き
        public WallData wall = new WallData();
        public List<string> destructiblePrefabPaths = new List<string>();  // 破壊可能オブジェクトのプレハブパス
        public int destructiblesCount = 8;    // 配置数
    }

    // ステージ全体の設定
    [Serializable]
    public class StageData
    {
        public string stageId = "stage_default"; // ステージの一意のID
        public string stageName = "Default Stage";
        public string groundMaterialPath;
        public List<ZoneData> zones = new List<ZoneData>();
        public List<ConcentricRingData> concentricRings = new List<ConcentricRingData>();
    }

    // JSONシリアライズのためのラッパークラス
    [Serializable]
    public class StageCollection
    {
        public List<StageData> stages = new List<StageData>();
    }
}