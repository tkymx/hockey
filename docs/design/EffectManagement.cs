using UnityEngine;
using System.Collections.Generic;
using Hockey.Core;
using UnityEngine.Pool;

namespace Hockey.Effects
{
    public class EffectManager : MonoBehaviour, IEffectPlayer
    {
        [System.Serializable]
        private class EffectPreset
        {
            public EffectType type;
            public ParticleSystem prefab;
            public int poolSize = 10;
        }

        [Header("Effect Presets")]
        [SerializeField] private EffectPreset[] effectPresets;

        [Header("Settings")]
        [SerializeField] private bool useObjectPooling = true;
        [SerializeField] private Transform effectsParent;

        private Dictionary<EffectType, ObjectPool<ParticleSystem>> effectPools;
        private Dictionary<EffectType, ParticleSystem> effectPrefabs;

        private static EffectManager instance;
        public static EffectManager Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                InitializeEffectPools();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeEffectPools()
        {
            effectPools = new Dictionary<EffectType, ObjectPool<ParticleSystem>>();
            effectPrefabs = new Dictionary<EffectType, ParticleSystem>();

            foreach (var preset in effectPresets)
            {
                effectPrefabs[preset.type] = preset.prefab;
                
                if (useObjectPooling)
                {
                    effectPools[preset.type] = new ObjectPool<ParticleSystem>(
                        createFunc: () => CreateEffect(preset.type),
                        actionOnGet: (effect) => OnEffectGet(effect),
                        actionOnRelease: (effect) => OnEffectRelease(effect),
                        actionOnDestroy: (effect) => Destroy(effect.gameObject),
                        defaultCapacity: preset.poolSize
                    );
                }
            }
        }

        private ParticleSystem CreateEffect(EffectType type)
        {
            if (effectPrefabs.TryGetValue(type, out ParticleSystem prefab))
            {
                ParticleSystem effect = Instantiate(prefab, effectsParent);
                effect.gameObject.SetActive(false);
                return effect;
            }
            return null;
        }

        private void OnEffectGet(ParticleSystem effect)
        {
            effect.gameObject.SetActive(true);
        }

        private void OnEffectRelease(ParticleSystem effect)
        {
            effect.Stop();
            effect.gameObject.SetActive(false);
        }

        #region IEffectPlayer Implementation
        public void PlayEffect(EffectType type, Vector3 position, float scale = 1.0f)
        {
            if (!effectPrefabs.ContainsKey(type)) return;

            ParticleSystem effect;
            if (useObjectPooling && effectPools.TryGetValue(type, out var pool))
            {
                effect = pool.Get();
            }
            else
            {
                effect = CreateEffect(type);
            }

            if (effect != null)
            {
                effect.transform.position = position;
                effect.transform.localScale = Vector3.one * scale;
                effect.Play();

                if (useObjectPooling)
                {
                    StartCoroutine(ReleaseEffectAfterPlay(effect, effectPools[type]));
                }
                else
                {
                    Destroy(effect.gameObject, effect.main.duration);
                }
            }
        }

        public void StopEffect(EffectType type)
        {
            // 必要に応じて実装
        }

        public void SetEffectScale(EffectType type, float scale)
        {
            // 必要に応じて実装
        }
        #endregion

        private System.Collections.IEnumerator ReleaseEffectAfterPlay(ParticleSystem effect, ObjectPool<ParticleSystem> pool)
        {
            yield return new WaitForSeconds(effect.main.duration);
            if (pool != null && effect != null)
            {
                pool.Release(effect);
            }
        }
    }

    public class SoundManager : MonoBehaviour, ISoundPlayer
    {
        [System.Serializable]
        private class SoundPreset
        {
            public SoundType type;
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0f, 1f)] public float spatialBlend = 0f;
            public bool loop = false;
        }

        [Header("Sound Presets")]
        [SerializeField] private SoundPreset[] soundPresets;
        [SerializeField] private int poolSize = 5;

        [Header("Settings")]
        [SerializeField] private AudioSource musicSource;
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float maxDistance = 20f;

        private Dictionary<SoundType, SoundPreset> presetMap;
        private ObjectPool<AudioSource> sourcePool;
        private List<AudioSource> activeSources;

        private static SoundManager instance;
        public static SoundManager Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                InitializeSoundSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSoundSystem()
        {
            presetMap = new Dictionary<SoundType, SoundPreset>();
            foreach (var preset in soundPresets)
            {
                presetMap[preset.type] = preset;
            }

            sourcePool = new ObjectPool<AudioSource>(
                createFunc: CreateAudioSource,
                actionOnGet: OnSourceGet,
                actionOnRelease: OnSourceRelease,
                actionOnDestroy: (source) => Destroy(source.gameObject),
                defaultCapacity: poolSize
            );

            activeSources = new List<AudioSource>();
        }

        private AudioSource CreateAudioSource()
        {
            GameObject obj = new GameObject("AudioSource");
            obj.transform.SetParent(transform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.maxDistance = maxDistance;
            return source;
        }

        private void OnSourceGet(AudioSource source)
        {
            source.gameObject.SetActive(true);
            activeSources.Add(source);
        }

        private void OnSourceRelease(AudioSource source)
        {
            source.Stop();
            source.gameObject.SetActive(false);
            activeSources.Remove(source);
        }

        #region ISoundPlayer Implementation
        public void PlaySound(SoundType type, Vector3 position = default)
        {
            if (!presetMap.TryGetValue(type, out SoundPreset preset)) return;

            AudioClip clip = preset.clips[Random.Range(0, preset.clips.Length)];
            AudioSource source = sourcePool.Get();

            source.transform.position = position;
            source.clip = clip;
            source.volume = preset.volume * masterVolume;
            source.spatialBlend = preset.spatialBlend;
            source.loop = preset.loop;
            source.Play();

            if (!preset.loop)
            {
                StartCoroutine(ReleaseSourceAfterPlay(source));
            }
        }

        public void StopSound(SoundType type)
        {
            foreach (var source in activeSources.ToArray())
            {
                if (source.clip != null && presetMap.TryGetValue(type, out SoundPreset preset))
                {
                    if (System.Array.Exists(preset.clips, clip => clip == source.clip))
                    {
                        sourcePool.Release(source);
                    }
                }
            }
        }

        public void SetVolume(SoundType type, float volume)
        {
            if (presetMap.TryGetValue(type, out SoundPreset preset))
            {
                preset.volume = Mathf.Clamp01(volume);
                foreach (var source in activeSources)
                {
                    if (source.clip != null && System.Array.Exists(preset.clips, clip => clip == source.clip))
                    {
                        source.volume = preset.volume * masterVolume;
                    }
                }
            }
        }
        #endregion

        private System.Collections.IEnumerator ReleaseSourceAfterPlay(AudioSource source)
        {
            yield return new WaitForSeconds(source.clip.length);
            if (source != null)
            {
                sourcePool.Release(source);
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            foreach (var source in activeSources)
            {
                if (source.clip != null && presetMap.TryGetValue(GetSoundType(source.clip), out SoundPreset preset))
                {
                    source.volume = preset.volume * masterVolume;
                }
            }
        }

        private SoundType GetSoundType(AudioClip clip)
        {
            foreach (var pair in presetMap)
            {
                if (System.Array.Exists(pair.Value.clips, c => c == clip))
                {
                    return pair.Key;
                }
            }
            return default;
        }
    }
}