using UnityEngine;

public class DestructibleObjectUseCase
{
    private DestructibleObjectModel destructibleObjectModel;
    private GameConfig config;

    public DestructibleObjectUseCase(DestructibleObjectModel model)
    {
        destructibleObjectModel = model;
        config = ConfigManager.Instance.GetConfig();
    }

    public DestructibleObjectModel GetModel()
    {
        return destructibleObjectModel;
    }

    public void Move(Vector2 position)
    {
        destructibleObjectModel.Move(position);
    }

    public void TakeDamage(float damage)
    {
        destructibleObjectModel.TakeDamage(damage);
        
        // 破壊されたオブジェクトの処理
        if (destructibleObjectModel.IsDestroyed())
        {
            // オブジェクトが破壊された場合の追加処理
            HandleDestruction();
        }
    }

    public void HandleCollision(Collision collision)
    {
        // 衝突処理のロジック
        PuckModel puck = collision.gameObject.GetComponent<PuckModel>();
        if (puck != null)
        {
            // コマとの衝突ダメージを計算
            float damage = puck.Size * puck.Speed * 0.5f;
            TakeDamage(damage);
        }
    }
    
    private void HandleDestruction()
    {
        // オブジェクト破壊時の特殊効果
        switch (destructibleObjectModel.Type)
        {
            case ObjectType.Bonus:
                // ボーナスオブジェクトの特殊効果
                break;
            case ObjectType.Obstacle:
                // 障害物の特殊効果
                break;
        }
    }
    
    // オブジェクトタイプに基づいた設定値を取得
    public ObjectTypeConfig GetObjectTypeConfig()
    {
        switch (destructibleObjectModel.Type)
        {
            case ObjectType.Bonus:
                return config.bonusObject;
            case ObjectType.Obstacle:
                return config.obstacleObject;
            default:
                return config.normalObject;
        }
    }
}