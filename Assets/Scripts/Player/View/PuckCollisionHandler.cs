using UnityEngine;

public class PuckCollisionHandler : MonoBehaviour
{
    private PuckPresenter puckPresenter;
    
    public void Initialize(PuckPresenter presenter)
    {
        puckPresenter = presenter;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (puckPresenter != null)
        {
            // 3D衝突情報を直接渡す
            puckPresenter.HandleCollision(collision);
        }
    }
}