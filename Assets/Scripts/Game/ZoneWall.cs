using UnityEngine;

public class ZoneWall : MonoBehaviour
{
    [SerializeField] private bool isActive = true;
    
    // コライダーリファレンス
    private Collider[] wallColliders;

    private void Awake()
    {
        // 子オブジェクトの全てのコライダーを取得
        wallColliders = GetComponentsInChildren<Collider>();
        SetWallState(isActive);
    }

    public void SetWallState(bool active)
    {
        isActive = active;
        
        // コライダーの有効/無効を切り替え
        foreach (var collider in wallColliders)
        {
            if (collider != null)
            {
                collider.enabled = isActive;
            }
        }
        
        // レンダラーの表示/非表示を切り替え
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer != null)
            {
                renderer.enabled = isActive;
            }
        }
    }
}
