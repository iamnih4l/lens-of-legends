using System;
using UnityEngine;
using LensOfLegends.Regions;

namespace LensOfLegends.Core
{
    /// <summary>
    /// Data-driven environmental audio manager.
    /// When external licensed AudioClips are assigned, plays them per region.
    /// When external audio assets are not present, synthesizes procedural regional soundscapes
    /// (night crickets, crackling fire, wind, synthesized Chenda-inspired percussion pulse,
    /// footsteps, and camera mechanical shutter clicks) transparently labeled as procedural audio.
    /// </summary>
    public class ProceduralSoundscapeGenerator : MonoBehaviour
    {
        public static ProceduralSoundscapeGenerator Instance { get; private set; }

        [Header("Optional External Licensed Audio Clips")]
        public AudioClip externalAmbientLoop;
        public AudioClip externalPercussionLoop;
        public AudioClip externalShutterClip;

        private AudioSource _ambientSource;
        private AudioSource _percussionSource;
        private AudioSource _sfxSource;

        private AudioClip _shutterClip;
        private AudioClip _footstepClip;
        private string _activeProfileId = "";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.spatialBlend = 0f;
            _ambientSource.volume = 0.35f;

            _percussionSource = gameObject.AddComponent<AudioSource>();
            _percussionSource.loop = true;
            _percussionSource.spatialBlend = 0f;
            _percussionSource.volume = 0.25f;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.spatialBlend = 0f;
            _sfxSource.volume = 0.75f;

            _shutterClip = externalShutterClip != null ? externalShutterClip : GenerateShutterSoundClip();
            _footstepClip = GenerateSoftFootstepClip();
        }

        public void ApplyRegionSoundscape(RegionData region)
        {
            if (region == null) return;
            if (_activeProfileId == region.soundscapeProfileId && _ambientSource.isPlaying)
                return;

            _activeProfileId = region.soundscapeProfileId;
            float masterVol = GameStateManager.Instance != null ? GameStateManager.Instance.SaveData.masterVolume : 0.85f;

            if (externalAmbientLoop != null)
            {
                _ambientSource.clip = externalAmbientLoop;
            }
            else
            {
                _ambientSource.clip = GenerateRegionalAmbienceClip(
                    region.soundscapeProfileId,
                    region.baseWindIntensity,
                    region.nightInsectsIntensity);
            }

            _ambientSource.volume = 0.32f * masterVol;
            _ambientSource.Play();

            if (region.playableState == DestinationPlayableState.PlayableVerticalSlice)
            {
                _percussionSource.clip = externalPercussionLoop != null
                    ? externalPercussionLoop
                    : GenerateSynthesizedPercussionClip(region.percussionRhythmTempoBpm);
                _percussionSource.volume = 0.22f * masterVol;
                _percussionSource.Play();
            }
            else
            {
                _percussionSource.Stop();
            }
        }

        public void SetMasterVolume(float volume01)
        {
            volume01 = Mathf.Clamp01(volume01);
            if (_ambientSource != null) _ambientSource.volume = 0.32f * volume01;
            if (_percussionSource != null) _percussionSource.volume = 0.22f * volume01;
            if (_sfxSource != null) _sfxSource.volume = 0.75f * volume01;
        }

        public void PlayMechanicalShutterClick()
        {
            if (_sfxSource != null && _shutterClip != null)
            {
                _sfxSource.PlayOneShot(_shutterClip, 0.85f);
            }
        }

        public void PlayFootstep()
        {
            if (_sfxSource != null && _footstepClip != null)
            {
                _sfxSource.PlayOneShot(_footstepClip, 0.22f);
            }
        }

        private AudioClip GenerateShutterSoundClip()
        {
            int sampleRate = 22050;
            int sampleCount = sampleRate / 5; // 200ms double-curtain mechanical shutter
            float[] samples = new float[sampleCount];
            System.Random rng = new System.Random(42);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                // First curtain click at t=0.005s, second curtain click at t=0.055s
                float env1 = t < 0.04f ? Mathf.Exp(-t * 140f) : 0f;
                float env2 = t >= 0.048f && t < 0.10f ? Mathf.Exp(-(t - 0.048f) * 120f) : 0f;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float metallic = Mathf.Sin(2f * Mathf.PI * 1450f * t) * 0.5f +
                                 Mathf.Sin(2f * Mathf.PI * 2800f * t) * 0.3f;
                samples[i] = Mathf.Clamp((noise * 0.65f + metallic * 0.35f) * (env1 + env2 * 0.85f), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Shutter_Click", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateSoftFootstepClip()
        {
            int sampleRate = 22050;
            int sampleCount = sampleRate / 8;
            float[] samples = new float[sampleCount];
            System.Random rng = new System.Random(108);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 42f);
                float lowThump = Mathf.Sin(2f * Mathf.PI * 95f * t);
                float earthGrit = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.35f;
                samples[i] = Mathf.Clamp((lowThump * 0.65f + earthGrit) * env * 0.5f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Laterite_Footstep", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateRegionalAmbienceClip(string profileId, float windLevel, float cricketLevel)
        {
            int sampleRate = 22050;
            int seconds = 6;
            int sampleCount = sampleRate * seconds;
            float[] samples = new float[sampleCount];
            System.Random rng = new System.Random(profileId != null ? profileId.GetHashCode() : 777);

            float prevFilteredWind = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float rawNoise = (float)(rng.NextDouble() * 2.0 - 1.0);
                // Low-pass filter for warm wind & fire bed
                prevFilteredWind = prevFilteredWind * 0.96f + rawNoise * 0.04f;
                float windWave = prevFilteredWind * windLevel * (0.7f + 0.3f * Mathf.Sin(t * Mathf.PI / 3f));

                // Cricket chirp chorus (synthesized dual-carrier AM pulse)
                float chirpPulse = Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * 3.5f * t)) *
                                   Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * 28f * t));
                float cricketTone = Mathf.Sin(2f * Mathf.PI * 3850f * t) * chirpPulse * cricketLevel * 0.08f;

                // Subtle fire crackle pops
                float crackle = rng.NextDouble() > 0.9985 ? rawNoise * 0.28f : 0f;

                samples[i] = Mathf.Clamp(windWave + cricketTone + crackle, -0.8f, 0.8f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Ambience_" + profileId, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateSynthesizedPercussionClip(float bpm)
        {
            int sampleRate = 22050;
            float beatDuration = 60f / Mathf.Max(60f, bpm);
            int beats = 8;
            int sampleCount = Mathf.RoundToInt(sampleRate * beatDuration * beats);
            float[] samples = new float[sampleCount];
            System.Random rng = new System.Random(909);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float beatPos = (t / beatDuration) % 1.0f;
                int beatIdx = Mathf.FloorToInt(t / beatDuration) % beats;

                float beatTime = beatPos * beatDuration;
                // Resonant drum skin + bronze cymbal overtone
                float drumEnv = Mathf.Exp(-beatTime * 16f);
                float subHitEnv = (beatIdx % 2 == 1 && beatPos > 0.5f)
                    ? Mathf.Exp(-(beatPos - 0.5f) * beatDuration * 22f)
                    : 0f;

                float pitch = (beatIdx == 0 || beatIdx == 4) ? 145f : 185f;
                float drumBody = Mathf.Sin(2f * Mathf.PI * pitch * beatTime) * 0.6f +
                                 Mathf.Sin(2f * Mathf.PI * (pitch * 1.58f) * beatTime) * 0.25f;
                float cymbal = (beatIdx % 2 == 0)
                    ? ((float)(rng.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-beatTime * 28f) * 0.18f)
                    : 0f;

                samples[i] = Mathf.Clamp((drumBody * (drumEnv + subHitEnv * 0.65f) + cymbal) * 0.45f, -0.8f, 0.8f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Chenda_Rhythm_Placeholder", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
