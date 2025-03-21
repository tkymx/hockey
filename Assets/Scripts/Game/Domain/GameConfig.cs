using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    // ゲーム基本設定
    public float gameDuration = 60f;
    public int initialScore = 0;

    // オブジェクト生成設定
    public int initialObjectCount = 10;
    public float spawnInterval = 2.0f;
    public float minObjectSize = 0.8f;
    public float maxObjectSize = 1.5f;

    // オブジェクトタイプ別設定
    public ObjectTypeConfig normalObject;
    public ObjectTypeConfig bonusObject; 
    public ObjectTypeConfig obstacleObject;

    // プレイヤー設定
    public float puckGrowthRate = 0.1f; // 10%成長
    public float puckGrowthChance = 0.3f; // 30%の確率
    public int maxGrowthLevel = 10;
    public float puckInitialSize = 1.0f;
    public float puckInitialSpeed = 5.0f;

    // コンボ設定
    public int comboThreshold = 5;
    public int comboBonus = 10;
}

[Serializable]
public class ObjectTypeConfig
{
    public float durability;
    public int pointValue;
    public Color color;
}