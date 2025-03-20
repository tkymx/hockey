using UnityEngine;
using Hockey.Core;

namespace Hockey.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerPuck : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private float baseSpeed = 15f;
        [SerializeField] private float maxSpeed = 30f;
        [SerializeField] private float mass = 2f;
        [SerializeField] private float drag = 0.2f;
        [SerializeField] private float angularDrag = 0.8f;

        [Header("Growth Settings")]
        [SerializeField] private float[] sizeMultipliers = { 1f, 1.5f, 2f, 3f };
        [SerializeField] private float growthAnimationDuration = 0.5f;

        [Header("Visual Feedback")]
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ParticleSystem collisionParticles;
        
        private Rigidbody rb;
        private int currentGrowthLevel = 0;
        private Vector3 initialScale;
        private bool isDragging = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            initialScale = transform.localScale;
            SetupRigidbody();
        }

        private void SetupRigidbody()
        {
            rb.mass = mass;
            rb.drag = drag;
            rb.angularDrag = angularDrag;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public void Move(Vector2 direction, float forceMagnitude)
        {
            if (rb == null) return;

            // 力の計算と制限
            Vector3 force = new Vector3(direction.x, 0, direction.y) * forceMagnitude * baseSpeed;
            force = Vector3.ClampMagnitude(force, maxSpeed);

            // 力を加える
            rb.AddForce(force, ForceMode.Impulse);

            // 速度の制限
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }

        public void Grow(int newLevel)
        {
            if (newLevel < 0 || newLevel >= sizeMultipliers.Length) return;
            if (newLevel == currentGrowthLevel) return;

            currentGrowthLevel = newLevel;
            float targetSize = sizeMultipliers[newLevel];

            // 成長アニメーションの開始
            StartCoroutine(GrowthAnimation(targetSize));
        }

        private System.Collections.IEnumerator GrowthAnimation(float targetSize)
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = initialScale * targetSize;
            float elapsedTime = 0f;

            while (elapsedTime < growthAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / growthAnimationDuration;
                
                // イージング関数を使用してスムーズな拡大
                t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
            UpdatePhysicsForGrowth(targetSize);
        }

        private void UpdatePhysicsForGrowth(float sizeMultiplier)
        {
            // 質量とスピードを更新
            rb.mass = mass * sizeMultiplier;
            baseSpeed = Mathf.Max(baseSpeed * 0.9f, baseSpeed * 0.5f); // サイズが大きくなるにつれて若干遅く
            
            // トレイルの幅を更新
            if (trailRenderer != null)
            {
                trailRenderer.startWidth *= sizeMultiplier;
                trailRenderer.endWidth *= sizeMultiplier;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > 5f)
            {
                // 衝突エフェクトの再生
                if (collisionParticles != null)
                {
                    collisionParticles.transform.position = collision.contacts[0].point;
                    collisionParticles.Play();
                }

                // 衝突相手が破壊可能オブジェクトかチェック
                IDestructible destructible = collision.gameObject.GetComponent<IDestructible>();
                if (destructible != null)
                {
                    float damage = CalculateImpactDamage(collision);
                    destructible.TakeDamage(damage);
                }
            }
        }

        private float CalculateImpactDamage(Collision collision)
        {
            // 衝突の運動エネルギーに基づいてダメージを計算
            float impactVelocity = collision.relativeVelocity.magnitude;
            float impactEnergy = 0.5f * rb.mass * impactVelocity * impactVelocity;
            
            // 成長レベルに応じてダメージを増加
            return impactEnergy * (1f + currentGrowthLevel * 0.5f);
        }

        public void ResetPuck()
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            currentGrowthLevel = 0;
            transform.localScale = initialScale;
            SetupRigidbody();
        }
    }
}