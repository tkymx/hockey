using UnityEngine;
using System.Collections.Generic;

public class PuckController : MonoBehaviour
{
    [SerializeField] private Puck puck;
    [SerializeField] private PuckView puckView;
    
    private Vector3 initialPosition;
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
            
    // パックをリセットする
    public void ResetPuck(Vector3 position)
    {
        if (puck == null) return;
        
        // 物理挙動をリセット
        Rigidbody rb = puck.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
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