using UnityEngine;
using System.Collections.Generic;

public class PuckController : MonoBehaviour
{
    [SerializeField] private Puck puck;
    [SerializeField] private PuckView puckView;
    [SerializeField] private float resetDelay = 3.0f;
    
    private Vector3 initialPosition;
    private bool isResetting = false;
    private float resetTimer = 0f;

    public Puck Puck => puck;

    public void Initialize(Puck puckInstance, PuckView puckViewInstance)
    {
        puck = puckInstance;
        puckView = puckViewInstance;
        
        if (puck != null)
        {
            initialPosition = puck.transform.position;
        }
        
        if (puckView != null)
        {
            puckView.Initialize(puck);
        }
    }
    
    private void Update()
    {
        if (puck == null || puckView == null) return;
        
        // 画面外に出たパックのリセットチェック
        CheckBoundaries();
        
        // リセットタイマーの更新
        if (isResetting)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0f)
            {
                ResetPuck(initialPosition);
                isResetting = false;
            }
        }
    }
    
    private void CheckBoundaries()
    {
        if (puck == null) return;
        
        Vector3 puckPosition = puck.transform.position;
        
        // 画面の境界を定義（仮の値）
        float boundaryX = 20.0f;
        float boundaryZ = 15.0f;
        
        // パックが境界を超えたかチェック
        if (Mathf.Abs(puckPosition.x) > boundaryX || Mathf.Abs(puckPosition.z) > boundaryZ)
        {
            if (!isResetting)
            {
                StartResetTimer();
            }
        }
    }
    
    private void StartResetTimer()
    {
        isResetting = true;
        resetTimer = resetDelay;
    }
        
    // パックをリセットする
    public void ResetPuck(Vector3 position)
    {
        if (puck == null) return;
        
        puck.ResetPosition(position); // Resetをnew ResetPositionに更新
        puckView.PlayTrailEffect(false);
    }
    
    // パックと他のゲームオブジェクトの衝突をチェック
    public void CheckCollisions(List<DestructibleObject> gameObjects)
    {
        if (puck == null) return;
        
        foreach (DestructibleObject obj in gameObjects)
        {
            if (obj == null || obj.IsDestroyed()) continue;
            
            // 距離による簡易衝突チェック（最適化のため）
            float distance = Vector3.Distance(puck.transform.position, obj.transform.position);
            float collisionThreshold = 1.0f; // オブジェクトとパックのサイズに応じて調整
            
            if (distance < collisionThreshold)
            {
                // 詳細な衝突チェックはUnityの物理エンジンに任せるため、
                // ここでは特別な処理は行わない
            }
        }
    }
}