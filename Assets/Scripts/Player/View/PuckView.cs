using UnityEngine;

public class PuckView : MonoBehaviour, IPuckView
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem growthParticles;
    
    private PuckViewModel viewModel;
    private Transform puckTransform;

    private void Awake()
    {
        puckTransform = transform;
    }

    public void Initialize(PuckViewModel viewModel)
    {
        this.viewModel = viewModel;
        UpdatePosition();
        UpdateSize();
    }

    public void UpdatePosition()
    {
        if (viewModel != null)
        {
            puckTransform.position = new Vector3(viewModel.position.x, viewModel.position.y, 0f);
        }
    }

    public void UpdateSize()
    {
        if (viewModel != null)
        {
            puckTransform.localScale = Vector3.one * viewModel.size;
            
            // 成長レベルに応じてトレイルのサイズも調整
            if (trailRenderer != null)
            {
                trailRenderer.startWidth = viewModel.size * 0.5f;
            }
        }
    }

    public void PlayGrowthEffect()
    {
        if (growthParticles != null)
        {
            growthParticles.Play();
        }
        
        // 成長アニメーション
        LeanTween.scale(gameObject, Vector3.one * viewModel.size * 1.2f, 0.2f)
            .setEasePunch()
            .setOnComplete(() => {
                puckTransform.localScale = Vector3.one * viewModel.size;
            });
    }
}