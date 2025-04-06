using UnityEngine;
using System;

namespace Hockey.Data
{
    [Serializable]
    public class GrowthData
    {
        public int maxGrowthStage = 10;

        // コンストラクタ：デフォルト値を設定
        public GrowthData()
        {
            maxGrowthStage = 10;
        }
    }
}
