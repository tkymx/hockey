using UnityEngine;
using System;

namespace Hockey.Data
{
    [Serializable]
    public class PlayerData
    {
        public float mass = 2.0f;
        public float collisionForceMultiplier = 2.5f;
        
        [SerializeField] public int[] experienceThresholds = { 0, 100, 300, 600, 1000 };
        public float baseDamage = 100.0f;
        public float levelDamageMultiplier = 0.5f;
        
        public int maxGrowthStage = 3;
        public float[] stageScales = { 0.8f, 1.0f, 1.2f };
        public float[] stageMass = { 1.5f, 2.0f, 2.5f };
        public float[] stageCollisionForce = { 2.0f, 2.5f, 3.0f };
    }
}