using UnityEngine;

public class DestructibleObjectView : MonoBehaviour, IDestructibleObjectView
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite intactSprite;
    [SerializeField] private Sprite damagedSprite;
    
    private DestructibleObjectViewModel viewModel;
    private ParticleSystem destroyParticles;

    private void Awake()
    {
        destroyParticles = GetComponentInChildren<ParticleSystem>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void Initialize(DestructibleObjectViewModel viewModel)
    {
        this.viewModel = viewModel;
        UpdateVisualState(DestructibleObjectState.Intact);
        
        // オブジェクトのスケールを設定
        if (viewModel.size != 0)
        {
            transform.localScale = Vector3.one * viewModel.size;
        }
    }

    public void UpdateVisualState(DestructibleObjectState state)
    {
        switch (state)
        {
            case DestructibleObjectState.Intact:
                spriteRenderer.sprite = intactSprite;
                break;
                
            case DestructibleObjectState.Damaged:
                spriteRenderer.sprite = damagedSprite;
                break;
                
            case DestructibleObjectState.Destroyed:
                PlayDestroyAnimation();
                break;
        }
    }

    public void PlayDestroyAnimation()
    {
        if (destroyParticles != null)
        {
            destroyParticles.Play();
        }
        
        // アニメーション後にオブジェクトを非アクティブにする
        spriteRenderer.enabled = false;
        Invoke("DisableObject", 1.0f);
    }
    
    public void SetObjectColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
    
    private void DisableObject()
    {
        gameObject.SetActive(false);
    }
}