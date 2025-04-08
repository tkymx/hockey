using UnityEngine;
using System;
using System.Collections.Generic;

namespace Hockey.Data
{
    [Serializable]
    public enum SkillType 
    {
        PuckSizeUp,     // パックのサイズを大きくする
        PuckDamageUp,   // パックの攻撃力を上げる
        PuckPenetration // パックの貫通能力を与える
    }

    [Serializable]
    public class SkillData
    {
        public string skillId;             // スキルの一意識別子
        public string skillName;           // スキル名
        public string description;         // スキルの説明
        public SkillType skillType;        // スキルの種類
        public int maxLevel = 3;           // スキルの最大レベル
        public List<float> effectValues;   // 各レベルでの効果値

        // デフォルトコンストラクタ
        public SkillData() 
        {
            effectValues = new List<float>();
        }

        // パラメータ付きコンストラクタ
        public SkillData(string id, string name, string desc, SkillType type, int max, List<float> values)
        {
            skillId = id;
            skillName = name;
            description = desc;
            skillType = type;
            maxLevel = max;
            effectValues = values;
        }

        // 特定のレベルでの効果値を取得
        public float GetEffectValue(int level)
        {
            int index = Mathf.Clamp(level - 1, 0, effectValues.Count - 1);
            return effectValues[index];
        }
    }
}