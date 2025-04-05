using UnityEngine;
using Hockey.Data;
using System.Collections.Generic;

public class PuckController : MonoBehaviour
{
    [SerializeField] private Puck puck;
    [SerializeField] private PuckView puckView;
    
    private Vector3 initialPosition;
    public Puck Puck => puck;

    public void Initialize(GameConfigRepository gameConfigRepository)
    {
        if (puck != null)
        {
            initialPosition = puck.transform.position;
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
    }
}