using UnityEngine;

public class DestructibleObjectModel
{
    private float size = 1.0f;
    private float durability = 100.0f;
    private int pointValue = 10;
    private Vector2 position = Vector2.zero;
    private ObjectType type = ObjectType.Normal;

    public float Size => size;
    public float Durability => durability;
    public int PointValue => pointValue;
    public Vector2 Position => position;
    public ObjectType Type => type;

    public DestructibleObjectModel(ObjectType objectType, float objectSize, float objectDurability, int points)
    {
        type = objectType;
        size = objectSize;
        durability = objectDurability;
        pointValue = points;
    }

    public void TakeDamage(float damage)
    {
        durability -= damage;
    }

    public int GetPointValue()
    {
        return pointValue;
    }

    public void Move(Vector2 newPosition)
    {
        position = newPosition;
    }
    
    public bool IsDestroyed()
    {
        return durability <= 0;
    }
}

public enum ObjectType
{
    Normal,
    Bonus,
    Obstacle
}