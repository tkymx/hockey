using UnityEngine;

public class DestructibleObjectView : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private float destroyAnimationDuration = 0.5f;
    [SerializeField] private AudioClip explosionSound;
    
    private DestructibleObject objectModel;
    private Renderer objectRenderer;
    private AudioSource audioSource;
    private Material originalMaterial;
    private Material destroyMaterial;
    
    public void Initialize(DestructibleObject destructibleObject)
    {
        objectModel = destructibleObject;
        objectRenderer = GetComponent<Renderer>();
        
        // UIキャンバス上にHPゲージを生成
        if (HPBarManager.Instance != null)
        {
            HPBarManager.Instance.CreateHPBar(objectModel);
        }

        // AudioSourceがなければ追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // マテリアルを保存
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
            
            // 破壊時のマテリアルを作成（オリジナルのコピーに発光効果を追加）
            destroyMaterial = new Material(originalMaterial);
            destroyMaterial.EnableKeyword("_EMISSION");
            destroyMaterial.SetColor("_EmissionColor", Color.red);
        }
        
        // オブジェクトの破壊イベントをサブスクライブ
        if (objectModel != null)
        {
            objectModel.OnObjectDestroyed += OnObjectDestroyed;
            // HPBarManagerでHP変更イベントをハンドリングするため、こちらでは不要になった
        }
    }
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        if (objectModel != null)
        {
            objectModel.OnObjectDestroyed -= OnObjectDestroyed;
            // HPBarManagerでHP変更イベントをハンドリングするため、こちらでは不要になった
        }
    }
    
    private void OnObjectDestroyed(DestructibleObject obj, int points)
    {
        PlayDestroyAnimation();
        PlayParticleEffect();
        PlayDestroySound();
    }

    public void PlayDestroyAnimation()
    {
        if (objectRenderer == null) return;
        
        // 破壊時のマテリアルに変更
        objectRenderer.material = destroyMaterial;
        
        // オブジェクトをスケールダウンさせて消滅させるアニメーション
        LeanTween.scale(gameObject, Vector3.zero, destroyAnimationDuration)
            .setEase(LeanTweenType.easeInBack);
    }
    
    public void PlayParticleEffect()
    {
        if (explosionEffect != null)
        {
            explosionEffect.Play();
        }
        else
        {
            // エフェクトがアタッチされていない場合は、パーティクルシステムを動的に生成
            GameObject explosionObj = new GameObject("ExplosionEffect");
            explosionObj.transform.position = transform.position;
            
            ParticleSystem ps = explosionObj.AddComponent<ParticleSystem>();
            
            // パーティクルシステムの設定
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 3.0f;
            main.startSize = 0.5f;
            main.startColor = new ParticleSystem.MinMaxGradient(Color.yellow, Color.red);
            
            // パーティクルの形状
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            
            // 爆発効果
            var burst = ps.emission;
            burst.enabled = true;
            var burstCount = new ParticleSystem.Burst(0.0f, 20);
            burst.SetBurst(0, burstCount);
            
            // 一定時間後に削除
            Destroy(explosionObj, 2.0f);
            ps.Play();
        }
    }
    
    public void PlayDestroySound()
    {
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
    }
}