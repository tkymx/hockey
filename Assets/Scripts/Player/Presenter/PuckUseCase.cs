using UnityEngine;

public class PuckUseCase : IPuckUseCase
{
    private PuckModel puckModel;
    private IScoreUseCase scoreUseCase;
    private GameConfig config;

    public PuckUseCase(PuckModel model, IScoreUseCase score)
    {
        puckModel = model;
        scoreUseCase = score;
        config = ConfigManager.Instance.GetConfig();
        
        // 設定から初期値を設定
        puckModel.UpdateSize(config.puckInitialSize);
        puckModel.UpdateSpeed(config.puckInitialSpeed);
    }

    public void Move(Vector2 direction, float force)
    {
        // コマを移動させるロジック
        Vector2 newPosition = puckModel.Position + direction * force * Time.deltaTime;
        puckModel.UpdatePosition(newPosition);
    }

    public void Grow()
    {
        puckModel.UpdateGrowthLevel(puckModel.GrowthLevel + 1);
        puckModel.UpdateSize(puckModel.Size * (1.0f + config.puckGrowthRate)); // 設定から成長率を取得
        
        // 成長に応じて速度を調整
        float newSpeed = puckModel.Speed * 0.95f; // 成長すると少し遅くなる
        puckModel.UpdateSpeed(newSpeed);
        
        CalculateGrowthProgress();
    }

    public void HandleCollision(Collision collision)
    {
        // 衝突処理のロジック
        if (collision.gameObject.CompareTag("Destructible"))
        {
            DestructibleObjectModel destructible = collision.gameObject.GetComponent<DestructibleObjectModel>();
            if (destructible != null)
            {
                float damage = puckModel.Size * 10; // サイズに比例したダメージ
                destructible.TakeDamage(damage);
                
                if (destructible.IsDestroyed())
                {
                    // オブジェクトが破壊された場合
                    scoreUseCase.AddScore(destructible.GetPointValue());
                    
                    // 一定確率で成長 - 設定から確率を取得
                    if (Random.Range(0f, 1f) < config.puckGrowthChance)
                    {
                        Grow();
                    }
                }
            }
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            // 壁との衝突時のロジック
            scoreUseCase.ResetCombo(); // コンボリセット
        }
    }

    private void CalculateGrowthProgress()
    {
        // 成長進捗の計算ロジック
        if (puckModel.GrowthLevel >= config.maxGrowthLevel)
        {
            // 最大成長レベルに達した場合の処理 - 設定から最大レベルを取得
            puckModel.UpdateGrowthLevel(config.maxGrowthLevel);
        }
    }
}