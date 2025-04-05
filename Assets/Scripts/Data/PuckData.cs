using UnityEngine;
using System;

namespace Hockey.Data
{
    [Serializable]
    public class PuckData
    {
        public float mass = 1.0f;
        public float frictionCoefficient = 0.995f;
        public float airResistance = 0.998f;
        public float minVelocityThreshold = 0.05f;
        
        public int maxGrowthStage = 3;
        public float[] stageScales = { 0.8f, 1.0f, 1.2f };
        public float[] stageMass = { 0.8f, 1.0f, 1.3f };
        public float[] stageMaxSpeed = { 15.0f, 20.0f, 25.0f };
        public float[] stageMaxForce = { 12.0f, 16.0f, 20.0f };
        public float[] stageFriction = { 0.993f, 0.995f, 0.997f };
    }
}