using UnityEngine;

public class PuckCollisionHandler : MonoBehaviour
{
    private PuckPresenter puckPresenter;
    
    public void Initialize(PuckPresenter presenter)
    {
        puckPresenter = presenter;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (puckPresenter != null)
        {
            // 2D衝突情報を3D衝突情報に変換して渡す
            puckPresenter.HandleCollision(Convert2DTo3DCollision(collision));
        }
    }
    
    // Unity 2Dから3Dへの衝突情報変換ヘルパー
    private Collision Convert2DTo3DCollision(Collision2D collision2D)
    {
        // 実際のプロジェクトでは適切な変換ロジックを実装する必要があります
        // 簡易的な実装として、GameObjectのみ設定します
        Collision collision = new Collision();
        collision.gameObject = collision2D.gameObject;
        return collision;
    }
}