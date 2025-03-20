using UnityEngine;
using Hockey.Core;

namespace Hockey.Objects
{
    public abstract class DestructibleObject : MonoBehaviour, IDestructible
    {
        [Header("Object Properties")]
        [SerializeField] protected float durability = 100f;
        [SerializeField] protected int pointValue = 100;
        [SerializeField] protected ObjectType materialType;
        [SerializeField] protected Size objectSize;

        [Header("Physics Settings")]
        [SerializeField] protected bool isStatic = true;
        [SerializeField] protected float mass = 1.0f;
        [SerializeField] protected float breakForceThreshold = 10f;

        [Header("Destruction Effects")]
        [SerializeField] protected ParticleSystem destructionParticles;
        [SerializeField] protected GameObject[] debrisPrefabs;
        [SerializeField] protected int debrisCount = 5;

        protected float currentDurability;
        protected bool isDestroyed = false;
        protected Rigidbody rb;

        public float Durability => currentDurability;
        public int PointValue => pointValue;

        protected virtual void Awake()
        {
            currentDurability = durability;
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = mass;
                rb.isKinematic = isStatic;
            }
        }

        public virtual void TakeDamage(float damage)
        {
            if (isDestroyed) return;

            currentDurability -= damage;
            OnDamaged(damage);

            if (currentDurability <= 0)
            {
                OnDestroy();
            }
        }

        protected virtual void OnDamaged(float damage)
        {
            // ダメージ時の視覚効果（ひび割れなど）
            float damageRatio = 1 - (currentDurability / durability);
            UpdateVisualDamage(damageRatio);
        }

        protected virtual void UpdateVisualDamage(float damageRatio)
        {
            // オーバーライドして具体的な視覚効果を実装
        }

        public virtual void OnDestroy()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            // 破壊エフェクトの再生
            PlayDestructionEffects();

            // 破片の生成
            SpawnDebris();

            // オブジェクトの非表示
            gameObject.SetActive(false);
        }

        protected virtual void PlayDestructionEffects()
        {
            if (destructionParticles != null)
            {
                var particles = Instantiate(destructionParticles, transform.position, Quaternion.identity);
                particles.Play();
                Destroy(particles.gameObject, particles.main.duration);
            }
        }

        protected virtual void SpawnDebris()
        {
            if (debrisPrefabs == null || debrisPrefabs.Length == 0) return;

            for (int i = 0; i < debrisCount; i++)
            {
                GameObject debris = Instantiate(
                    debrisPrefabs[Random.Range(0, debrisPrefabs.Length)],
                    transform.position + Random.insideUnitSphere * 0.5f,
                    Random.rotation
                );

                if (debris.TryGetComponent<Rigidbody>(out Rigidbody debrisRb))
                {
                    Vector3 randomDir = Random.onUnitSphere;
                    randomDir.y = Mathf.Abs(randomDir.y); // 上向きに飛ばす
                    debrisRb.AddForce(randomDir * Random.Range(2f, 5f), ForceMode.Impulse);
                }

                Destroy(debris, 2f); // 2秒後に破片を削除
            }
        }
    }

    public class SmallObject : DestructibleObject
    {
        protected override void Awake()
        {
            base.Awake();
            objectSize = Size.Small;
            pointValue = 100;
            durability = 50f;
            breakForceThreshold = 5f;
        }

        public override void TakeDamage(float damage)
        {
            // 小型オブジェクトは一撃で破壊されやすい
            float multipliedDamage = damage * 1.5f;
            base.TakeDamage(multipliedDamage);
        }
    }

    public class MediumObject : DestructibleObject
    {
        protected override void Awake()
        {
            base.Awake();
            objectSize = Size.Medium;
            pointValue = 300;
            durability = 100f;
            breakForceThreshold = 10f;
        }

        protected override void UpdateVisualDamage(float damageRatio)
        {
            base.UpdateVisualDamage(damageRatio);
            // 中型オブジェクト特有のダメージ表現（必要に応じて実装）
        }
    }

    public class LargeObject : DestructibleObject
    {
        [SerializeField] private GameObject[] subObjects; // 分割可能なサブオブジェクト

        protected override void Awake()
        {
            base.Awake();
            objectSize = Size.Large;
            pointValue = 1000;
            durability = 200f;
            breakForceThreshold = 20f;
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);

            // 一定のダメージで部分的に破壊
            if (currentDurability < durability * 0.5f)
            {
                BreakIntoSubObjects();
            }
        }

        private void BreakIntoSubObjects()
        {
            if (subObjects == null || subObjects.Length == 0) return;

            foreach (var subObject in subObjects)
            {
                if (subObject != null && Random.value < 0.3f) // 30%の確率で部分破壊
                {
                    var mediumObj = subObject.GetComponent<MediumObject>();
                    if (mediumObj != null)
                    {
                        mediumObj.TakeDamage(mediumObj.Durability * 0.5f);
                    }
                }
            }
        }
    }

    public class ObjectFactory : MonoBehaviour
    {
        [System.Serializable]
        private class ObjectPrefabs
        {
            public GameObject[] small;
            public GameObject[] medium;
            public GameObject[] large;
        }

        [SerializeField] private ObjectPrefabs rockPrefabs;
        [SerializeField] private ObjectPrefabs woodPrefabs;
        [SerializeField] private ObjectPrefabs plantPrefabs;
        [SerializeField] private ObjectPrefabs buildingPrefabs;

        private static ObjectFactory instance;
        public static ObjectFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ObjectFactory>();
                }
                return instance;
            }
        }

        public DestructibleObject CreateObject(ObjectType type, Size size, Vector3 position)
        {
            GameObject[] prefabs = GetPrefabsByTypeAndSize(type, size);
            if (prefabs == null || prefabs.Length == 0) return null;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject obj = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            return obj.GetComponent<DestructibleObject>();
        }

        private GameObject[] GetPrefabsByTypeAndSize(ObjectType type, Size size)
        {
            ObjectPrefabs prefabs = type switch
            {
                ObjectType.Rock => rockPrefabs,
                ObjectType.Wood => woodPrefabs,
                ObjectType.Plant => plantPrefabs,
                ObjectType.Building => buildingPrefabs,
                _ => null
            };

            if (prefabs == null) return null;

            return size switch
            {
                Size.Small => prefabs.small,
                Size.Medium => prefabs.medium,
                Size.Large => prefabs.large,
                _ => null
            };
        }
    }
}