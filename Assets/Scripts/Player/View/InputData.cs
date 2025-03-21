using UnityEngine;

public struct InputData
{
    public Vector2 direction;
    public float strength;
    public bool isPressing;

    public InputData(Vector2 dir, float str, bool pressing)
    {
        direction = dir;
        strength = str;
        isPressing = pressing;
    }
}