using UnityEngine;

public class PuckView : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem hitParticleEffect;
    [SerializeField] private float minSpeedForTrail = 2.0f;
    [SerializeField] private Color trailColor = Color.blue;
    
    private Puck puckModel;
    
    public void Initialize(Puck puck)
    {
        puckModel = puck;
        
        // TrailRendererがアタッチされていない場合、追加
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
                SetupTrailRenderer();
            }
        }
        
        // デフォルトではトレイルを無効化
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
        
        // パーティクルエフェクトがなければ、空のシステムを作成
        if (hitParticleEffect == null)
        {
            hitParticleEffect = GetComponentInChildren<ParticleSystem>();
        }
    }
    
    private void SetupTrailRenderer()
    {
        if (trailRenderer != null)
        {
            trailRenderer.time = 0.2f;
            trailRenderer.minVertexDistance = 0.1f;
            trailRenderer.startWidth = 0.2f;
            trailRenderer.endWidth = 0.0f;
            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0);
            
            // マテリアルの設定
            Material trailMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
            trailRenderer.material = trailMaterial;
        }
    }
    
    private void Update()
    {
        if (puckModel == null) return;
        
        // パックの速度に応じてトレイルの表示/非表示を切り替え
        if (trailRenderer != null)
        {
            Vector3 velocity = puckModel.GetVelocity();
            if (velocity.magnitude > minSpeedForTrail)
            {
                if (!trailRenderer.enabled)
                {
                    PlayTrailEffect(true);
                }
            }
            else
            {
                if (trailRenderer.enabled)
                {
                    PlayTrailEffect(false);
                }
            }
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
    
    public void PlayTrailEffect(bool enabled)
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = enabled;
            // トレイルを無効化する時は履歴をクリア
            if (!enabled)
            {
                trailRenderer.Clear();
            }
        }
    }
}