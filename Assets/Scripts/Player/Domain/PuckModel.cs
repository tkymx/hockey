using UnityEngine;

public class PuckModel
{
    private Vector2 position = Vector2.zero;
    private float size = 1.0f;
    private float speed = 5.0f;
    private int growthLevel = 1;

    public Vector2 Position => position;
    public float Size => size;
    public float Speed => speed;
    public int GrowthLevel => growthLevel;

    public void UpdatePosition(Vector2 pos)
    {
        position = pos;
    }

    public void UpdateSize(float newSize)
    {
        size = newSize;
    }

    public void UpdateSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void UpdateGrowthLevel(int level)
    {
        growthLevel = level;
    }
}