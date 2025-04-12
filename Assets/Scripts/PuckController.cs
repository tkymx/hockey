using UnityEngine;
using Hockey.Data;
using System.Collections.Generic;

public class PuckController : MonoBehaviour
{
    [SerializeField] private Puck puck;
    [SerializeField] private PuckView puckView;
    [SerializeField] private ParticleEffectManager effectManager;
    
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
        
        // エフェクトマネージャーの初期化
        InitializeEffectManager();
    }
    
    // エフェクトマネージャーの初期化
    private void InitializeEffectManager()
    {
        // シリアライズフィールドにアサインされていなければ探索
        if (effectManager == null)
        {
            // 既に子オブジェクトに存在するか確認
            effectManager = GetComponentInChildren<ParticleEffectManager>();
            
            // 存在しなければ新規作成
            if (effectManager == null && puck != null)
            {
                GameObject effectObj = new GameObject("EffectManager");
                effectObj.transform.SetParent(puck.transform);
                effectObj.transform.localPosition = Vector3.zero;
                effectManager = effectObj.AddComponent<ParticleEffectManager>();
            }
        }
        
        // 初期化
        if (effectManager != null)
        {
            effectManager.Initialize();
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