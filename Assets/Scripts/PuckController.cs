using UnityEngine;
using Hockey.Data;
using System.Collections.Generic;

[RequireComponent(typeof(Puck))]
[RequireComponent(typeof(PuckView))]
public class PuckController : MonoBehaviour
{
    private Puck puck;
    private PuckView puckView;

    public Puck Puck => puck;

    public void Initialize(GameConfigRepository gameConfigRepository)
    {
        puck = GetComponent<Puck>();
        puckView = GetComponent<PuckView>();
        
        if (puck != null)
        {
            puck.Initialize(gameConfigRepository.PuckConfig);
        }
        
        if (puckView != null)
        {
            puckView.Initialize(puck);
        }
    }
            
    // パックをリセットする
    public void ResetPuck(Vector3 position)
    {
        if (puck == null) return;
        
        // 物理挙動をリセット
        Rigidbody rb = puck.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 位置をリセット
        puck.transform.position = position;
        
        // エフェクトをリセット
        if (puckView != null)
        {
            puckView.PlayTrailEffect(false);
        }
    }
}