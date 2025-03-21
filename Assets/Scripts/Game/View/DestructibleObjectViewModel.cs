using UnityEngine;

public class DestructibleObjectViewModel
{
    public Vector2 position;
    public float size;
    public DestructibleObjectState state;
    public ObjectType type;
    
    private DestructibleObjectModel objectModel;
    
    public void SetModel(DestructibleObjectModel model)
    {
        objectModel = model;
    }

    public void UpdateDisplayData()
    {
        if (objectModel != null)
        {
            position = objectModel.Position;
            size = objectModel.Size;
            type = objectModel.Type;
            
            // オブジェクトの耐久度に基づいて表示状態を決定
            float durabilityPercent = objectModel.Durability / 100f; // 初期耐久値を100と仮定
            
            if (objectModel.IsDestroyed())
            {
                state = DestructibleObjectState.Destroyed;
            }
            else if (durabilityPercent < 0.5f)
            {
                state = DestructibleObjectState.Damaged;
            }
            else
            {
                state = DestructibleObjectState.Intact;
            }
        }
    }
}

public enum DestructibleObjectState
{
    Intact,
    Damaged,
    Destroyed
}