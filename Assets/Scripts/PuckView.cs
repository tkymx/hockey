using UnityEngine;

public class PuckView : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem hitParticleEffect;
    
    private Puck puckModel;
    
    public void Initialize(Puck puck)
    {
        puckModel = puck;
        
        // パーティクルエフェクトがなければ、空のシステムを作成
        if (hitParticleEffect == null)
        {
            hitParticleEffect = GetComponentInChildren<ParticleSystem>();
        }
    }
    
    public void PlayHitEffect(Vector3 hitPosition)
    {
        if (hitParticleEffect != null)
        {
            // ヒット位置にパーティクルを移動して再生
            hitParticleEffect.transform.position = hitPosition;
            hitParticleEffect.Play();
        }
    }
}