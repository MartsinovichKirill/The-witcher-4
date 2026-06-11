using UnityEngine;

namespace WitcherRightVersion.Core
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioFeedbackService : MonoBehaviour
    {
        [SerializeField] private float volume = 0.35f;

        private AudioSource audioSource;
        private AudioSource musicSource;
        private AudioClip uiClip;
        private AudioClip questClip;
        private AudioClip hitClip;
        private AudioClip deathClip;
        private bool lastMusicEnabled;

        public static AudioFeedbackService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            uiClip = CreateTone("ui_click_placeholder", 720f, 0.055f, 0.45f);
            questClip = CreateTone("quest_update_placeholder", 520f, 0.18f, 0.55f);
            hitClip = CreateTone("hit_placeholder", 140f, 0.08f, 0.75f);
            deathClip = CreateTone("death_placeholder", 92f, 0.22f, 0.8f);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.16f;
            musicSource.clip = CreateAmbientLoop();
            ApplyMusicSetting(true);
        }

        private void Update()
        {
            ApplyMusicSetting(false);
        }

        public void PlayUi()
        {
            Play(uiClip);
        }

        public void PlayQuest()
        {
            Play(questClip);
        }

        public void PlayHit()
        {
            Play(hitClip);
        }

        public void PlayDeath()
        {
            Play(deathClip);
        }

        private void Play(AudioClip clip)
        {
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        private static AudioClip CreateTone(string clipName, float frequency, float duration, float gain)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var samples = new float[sampleCount];

            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var normalized = i / (float)sampleCount;
                var envelope = 1f - normalized;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * gain;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void ApplyMusicSetting(bool force)
        {
            if (musicSource == null)
            {
                return;
            }

            var musicEnabled = PlayerPrefs.GetInt(RuntimeSettingsService.MusicKey, 1) == 1;
            if (!force && musicEnabled == lastMusicEnabled)
            {
                return;
            }

            lastMusicEnabled = musicEnabled;
            if (musicEnabled)
            {
                if (!musicSource.isPlaying)
                {
                    musicSource.Play();
                }
            }
            else
            {
                musicSource.Stop();
            }
        }

        private static AudioClip CreateAmbientLoop()
        {
            const int sampleRate = 22050;
            const float duration = 8f;
            var sampleCount = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var slowPulse = 0.72f + Mathf.Sin(t * Mathf.PI * 0.25f) * 0.18f;
                var drone = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.12f
                    + Mathf.Sin(2f * Mathf.PI * 82.5f * t) * 0.055f
                    + Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.025f;
                samples[i] = drone * slowPulse;
            }

            var clip = AudioClip.Create("velemar_ambient_loop", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
