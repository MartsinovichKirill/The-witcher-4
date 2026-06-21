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

            uiClip = CreateClick("ui_click", 880f, 1320f);
            questClip = CreateChime("quest_update");
            hitClip = CreateImpact("hit", 0.7f);
            deathClip = CreateImpact("death", 1.0f);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.32f;
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

        // Warm A-minor pad (A3 C4 E4 + soft A4) that slowly swells, with a gentle
        // pentatonic melody over it, so it reads as actual music instead of a low drone.
        private static AudioClip CreateAmbientLoop()
        {
            const int sampleRate = 22050;
            const float duration = 16f;
            var n = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[n];

            float[] chord = { 220f, 261.63f, 329.63f, 440f };
            float[] chordGain = { 0.16f, 0.12f, 0.10f, 0.05f };
            float[] melody = { 440f, 523.25f, 659.25f, 587.33f, 523.25f, 440f, 392f, 329.63f };

            for (var i = 0; i < n; i++)
            {
                var t = i / (float)sampleRate;
                var swell = 0.72f + 0.28f * Mathf.Sin(t * Mathf.PI * 2f / duration);
                var s = 0f;
                for (var c = 0; c < chord.Length; c++)
                {
                    s += Mathf.Sin(2f * Mathf.PI * chord[c] * t) * chordGain[c] * swell;
                }

                var noteIndex = (int)(t / 2f) % melody.Length;
                var notePhase = (t % 2f) / 2f;
                var noteEnv = Mathf.Sin(notePhase * Mathf.PI);
                s += Mathf.Sin(2f * Mathf.PI * melody[noteIndex] * t) * 0.09f * noteEnv;

                samples[i] = s * 0.62f;
            }

            // Fade the loop seam so it repeats seamlessly without a click.
            var fade = sampleRate / 3;
            for (var k = 0; k < fade; k++)
            {
                var w = k / (float)fade;
                samples[k] *= w;
                samples[n - 1 - k] *= w;
            }

            var clip = AudioClip.Create("velemar_ambient_loop", n, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // Short crisp two-tone UI click.
        private static AudioClip CreateClick(string clipName, float f1, float f2)
        {
            const int sampleRate = 44100;
            var n = Mathf.RoundToInt(sampleRate * 0.06f);
            var samples = new float[n];
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)sampleRate;
                var env = Mathf.Exp(-t * 60f);
                samples[i] = (Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.4f) * env * 0.5f;
            }

            var clip = AudioClip.Create(clipName, n, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // Rising three-note chime for quest updates.
        private static AudioClip CreateChime(string clipName)
        {
            const int sampleRate = 44100;
            var n = Mathf.RoundToInt(sampleRate * 0.5f);
            var samples = new float[n];
            float[] notes = { 523.25f, 659.25f, 783.99f };
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)sampleRate;
                var idx = Mathf.Min(notes.Length - 1, (int)(t / 0.13f));
                var localT = t - idx * 0.13f;
                var env = Mathf.Exp(-localT * 9f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * notes[idx] * t) * env * 0.5f;
            }

            var clip = AudioClip.Create(clipName, n, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // Punchy impact: noise burst + a fast descending thud (combat hit / death).
        private static AudioClip CreateImpact(string clipName, float power)
        {
            const int sampleRate = 44100;
            var n = Mathf.RoundToInt(sampleRate * 0.22f);
            var samples = new float[n];
            var rng = new System.Random(clipName.GetHashCode());
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)sampleRate;
                var env = Mathf.Exp(-t * 26f);
                var noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                var body = Mathf.Sin(2f * Mathf.PI * (110f - t * 180f) * t);
                samples[i] = (noise * 0.5f + body * 0.6f) * env * power * 0.7f;
            }

            var clip = AudioClip.Create(clipName, n, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
