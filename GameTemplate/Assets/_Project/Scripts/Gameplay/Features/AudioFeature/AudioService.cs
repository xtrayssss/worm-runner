using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.LoggerFeature;
using _Project.Scripts.Gameplay.Features.SaveFeature;
#if GAMEPUSH_ENABLED
using GamePush;
#endif
using JetBrains.Annotations;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace _Project.Scripts.Gameplay.Features.AudioFeature
{
    [Serializable]
    public sealed class AudioService : MonoBehaviour, IService
    {
        [SerializeField]
        private Transform _musicSourcesContainer;

        [SerializeField]
        private Transform _soundSourcesContainer;

        [SerializeField]
        private AudioServiceSettings _audioSettings;

        [SerializeField]
        private AudioLibrary _audioLibrary;

        private readonly List<SoundSource> _soundSourcePool = new List<SoundSource>();
        private readonly List<MusicSource> _musicSourcePool = new List<MusicSource>();
        private readonly List<SoundSource3D> _soundSource3DPool = new List<SoundSource3D>();

        private MusicSource _primaryMusicSource;
        private MusicSource _secondaryMusicSource;

        private AudioMixerGroup _masterMixerGroup;
        private AudioMixerGroup _musicMixerGroup;
        private AudioMixerGroup _soundEffectsMixerGroup;
        private AudioMixerGroup _specialEffectsMixerGroup;

        [ShowInInspector]
        private Tween _musicFadeTween;

        [ShowInInspector]
        private Sequence _crossfadeTween;

        private readonly Dictionary<AudioClip, List<float>> _activeSoundVolumes =
            new Dictionary<AudioClip, List<float>>();

        private SaveLoadService _saveLoadService;

        private const int MAX_DUPLICATE_AUDIO_CLIPS = 3;
        private const float VOLUME_NORMALIZATION_THRESHOLD = 0.5f;

        private const string MUSIC_VOLUME_PARAM = "MusicVolume";
        private const string SOUND_VOLUME_PARAM = "SoundVolume";

        private void Start() =>
            DontDestroyOnLoad(gameObject);

        [Serializable]
        public class AudioServiceSettings
        {
            [SerializeField]
            private int _soundSourcePoolSize;

            [SerializeField]
            private int _musicSourcePoolSize;

            [field: SerializeField]
            public AudioMixerGroup SoundEffectsMixerGroup { get; private set; }

            [field: SerializeField]
            public AudioMixerGroup MusicMixerGroup { get; private set; }

            [field: SerializeField]
            public AudioMixerGroup MasterMixerGroup { get; private set; }

            [field: SerializeField]
            public AudioMixerGroup SpecialEffectsMixerGroup { get; private set; }

            [SerializeField]
            private int _soundSource3DPoolSize = 10;

            public int MusicSourcePoolSize => _musicSourcePoolSize;
            public int SoundSourcePoolSize => _soundSourcePoolSize;
            public int SoundSource3DPoolSize => _soundSource3DPoolSize;
        }

        [Serializable]
        public sealed class AudioLibrary
        {
            [Serializable]
            public sealed class AudioFile
            {
                [field: SerializeField,
                        ValueDropdown("GetAudioIdDropdown", NumberOfItemsBeforeEnablingSearch = 1),
                        FoldoutGroup("$GetFoldoutName")]
                public string Id { get; private set; }

                [field: SerializeField, FoldoutGroup("$GetFoldoutName")]
                public AudioClip AudioClip { get; private set; }

#if UNITY_EDITOR

                private string GetFoldoutName() =>
                    "Audio File " + Id;

                public static IEnumerable GetAudioIdDropdown()
                {
                    ValueDropdownList<string> dropdownList = new ValueDropdownList<string>();
                    Type audioIdType = typeof(AudioId);

                    PopulateDropdown(audioIdType, dropdownList, "");

                    return dropdownList;
                }

                private static void PopulateDropdown(Type type, ValueDropdownList<string> dropdown, string path)
                {
                    foreach (Type nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
                    {
                        string newPath = string.IsNullOrEmpty(path) ? nestedType.Name : $"{path}/{nestedType.Name}";
                        PopulateDropdown(nestedType, dropdown, newPath);
                    }

                    foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static |
                                                               BindingFlags.FlattenHierarchy))
                    {
                        if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                        {
                            string value = field.GetRawConstantValue() as string;
                            if (!string.IsNullOrEmpty(value))
                            {
                                string fullPath = $"{path}/{value}";
                                dropdown.Add(fullPath, value);
                            }
                        }
                    }
                }
#endif
            }

            [Serializable]
            public sealed class AudioVariationGroup
            {
                [field: SerializeField, ValueDropdown("GetAudioIdDropdown")]
                public string Id { get; private set; }

                [field: SerializeField]
                public List<AudioClip> Variations { get; private set; } = new List<AudioClip>();

                [SerializeField]
                private bool _preventRepetition;

                [SerializeField]
                [HideIf("@!_preventRepetition")]
                private float _minTimeBetweenSounds = 0.1f;

                private int _lastPlayedIndex = -1;
                private float _lastPlayedTime;

                public AudioClip GetRandomClip()
                {
                    if (Variations == null || Variations.Count == 0)
                        return null;

                    int index = GetNextVariationIndex();
                    _lastPlayedIndex = index;
                    _lastPlayedTime = Time.time;

                    return Variations[index];
                }

                private int GetNextVariationIndex()
                {
                    if (Variations.Count == 1)
                        return 0;

                    if (!_preventRepetition)
                        return UnityEngine.Random.Range(0, Variations.Count);

                    if (Time.time - _lastPlayedTime < _minTimeBetweenSounds)
                        return _lastPlayedIndex;

                    int newIndex;
                    do
                    {
                        newIndex = UnityEngine.Random.Range(0, Variations.Count);
                    } while (newIndex == _lastPlayedIndex && Variations.Count > 1);

                    return newIndex;
                }

#if UNITY_EDITOR
                private static IEnumerable GetAudioIdDropdown() =>
                    AudioFile.GetAudioIdDropdown();
#endif
            }

            [Serializable]
            public class SoundRandomizationSettings
            {
                [CanBeNull]
                public Vector2? PitchRange { get; set; }

                [CanBeNull]
                public Vector2? VolumeRange { get; set; }
            }

            [field: SerializeField]
            [field: Searchable]
            [field: ListDrawerSettings(DefaultExpandedState = true)]
            public List<AudioFile> Sounds { get; private set; }

            [field: SerializeField]
            [field: Searchable]
            [field: ListDrawerSettings(DefaultExpandedState = true)]
            public List<AudioFile> Music { get; private set; }

            [field: SerializeField]
            [field: Searchable]
            [field: ListDrawerSettings(DefaultExpandedState = true)]
            public List<AudioVariationGroup> SoundVariations { get; private set; }

            [field: SerializeField]
            [field: Searchable]
            [field: ListDrawerSettings(DefaultExpandedState = true)]
            public List<AudioVariationGroup> MusicVariations { get; private set; }

            public AudioClip GetMusic(string musicId) =>
                Music.First(m => m.Id == musicId).AudioClip;

            public AudioClip GetSound(string soundId) =>
                Sounds.First(s => s.Id == soundId).AudioClip;

            public AudioClip GetRandomSoundVariation(string soundId) =>
                SoundVariations.FirstOrDefault(s => s.Id == soundId)?.GetRandomClip();

            public AudioClip GetRandomMusicVariation(string musicId) =>
                MusicVariations.FirstOrDefault(m => m.Id == musicId)?.GetRandomClip();

#if UNITY_EDITOR
            [Button(ButtonSizes.Large)]
            private void RemoveMissingClips()
            {
                int removedCount = 0;

                removedCount += Sounds.RemoveAll(static s => s.AudioClip == null);

                L.Log($"[AudioService] Removed {removedCount} Sounds with missing AudioClips.");

                int musicRemoved = Music.RemoveAll(static m => m.AudioClip == null);
                removedCount += musicRemoved;

                L.Log($"[AudioService] Removed {musicRemoved} Music entries with missing AudioClips.");

                int soundVarRemoved = SoundVariations.RemoveAll(static sv =>
                    sv.Variations == null || sv.Variations.Count == 0 || sv.Variations.All(static v => v == null));
                removedCount += soundVarRemoved;

                L.Log(
                    $"[AudioService] Removed {soundVarRemoved} SoundVariations with missing or empty AudioClips.");

                int musicVarRemoved = MusicVariations.RemoveAll(static mv =>
                    mv.Variations == null || mv.Variations.Count == 0 || mv.Variations.All(static v => v == null));
                removedCount += musicVarRemoved;

                L.Log(
                    $"[AudioService] Removed {musicVarRemoved} MusicVariations with missing or empty AudioClips.");

                L.Log(removedCount == 0
                    ? "[AudioService] No entries with missing AudioClips found."
                    : $"[AudioService] Total removed entries: {removedCount}. Asset marked as dirty for saving.");
            }
#endif
        }

        public void Initialize(SaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;

            InitializeMixerGroups();
            CreateAudioSourcePools();

#if GAMEPUSH_ENABLED
            GP_Sounds.OnMute += HandleMuteAll;
            GP_Sounds.OnUnmute += HandleUnmuteAll;
            GP_Sounds.OnMuteSFX += HandleMuteSfx;
            GP_Sounds.OnUnmuteSFX += HandleUnmuteSfx;
            GP_Sounds.OnMuteMusic += HandleMuteMusic;
            GP_Sounds.OnUnmuteMusic += HandleUnmuteMusic;

            if (GP_Sounds.IsMuted())
                MuteAll();

            if (GP_Sounds.IsMuted(SoundType.SFX))
            {
                SetSoundVolume(0f);
                _saveLoadService.PlayerSaveData.SfxVolume = 0f;
            }

            if (GP_Sounds.IsMuted(SoundType.Music))
            {
                SetMusicVolume(0f);
                _saveLoadService.PlayerSaveData.MusicVolume = 0f;
            }
#endif

            SetSoundVolume(_saveLoadService.PlayerSaveData.SfxVolume);
            SetMusicVolume(_saveLoadService.PlayerSaveData.MusicVolume);
        }

#if GAMEPUSH_ENABLED
        private void HandleMuteAll() =>
            MuteAll();

        private void HandleUnmuteAll() =>
            UnmuteAll();

        private void HandleMuteSfx() =>
            SetSoundVolume(0f);

        private void HandleUnmuteSfx() =>
            SetSoundVolume(_saveLoadService.PlayerSaveData.SfxVolume);

        private void HandleMuteMusic() =>
            SetMusicVolume(0f);

        private void HandleUnmuteMusic() =>
            SetMusicVolume(_saveLoadService.PlayerSaveData.MusicVolume);
#endif

        public void PlayMusic(string musicId, float volume = 1f, bool shouldLoop = true)
        {
            AudioClip musicTrack = _audioLibrary.GetMusic(musicId);
            if (musicTrack == null)
            {
                L.LogError($"Music track not found: {musicId}");
                return;
            }

            if (_primaryMusicSource == null)
            {
                L.LogError("Primary music source not initialized");
                return;
            }

            if (!_primaryMusicSource.IsFree)
            {
                _primaryMusicSource.Stop();
            }

            _primaryMusicSource.ConfigureAndPlay(musicTrack, volume, shouldLoop);
        }

        public void PlaySpecialSound(string soundId, float volume = 1f)
        {
            AudioClip soundEffect = _audioLibrary.GetSound(soundId);
            if (soundEffect == null)
            {
                L.LogError($"Special sound effect not found: {soundId}");
                return;
            }

            SoundSource source = GetFreeSoundSource();
            if (source != null)
            {
                source.GetAudioSource().outputAudioMixerGroup = _specialEffectsMixerGroup;
                source.ConfigureAndPlay(soundEffect, volume);
            }
        }

        public void PlaySound(string soundId, float volume = 1f)
        {
            AudioClip soundEffect = _audioLibrary.GetSound(soundId);
            if (soundEffect == null)
            {
                L.LogError($"Sound effect not found: {soundId}");
                return;
            }

            PlaySound(soundEffect, volume);
        }

        public void PlaySound(
            AudioClip clip,
            float volume = 1f,
            [CanBeNull] AudioLibrary.SoundRandomizationSettings randomization = null)
        {
            SoundSource source = GetFreeSoundSource();
            if (source != null)
            {
                ApplyRandomization(source.GetAudioSource(), randomization, ref volume);
                float normalizedVolume = CalculateNormalizedVolume(clip, volume);
                source.ConfigureAndPlay(clip, normalizedVolume);
                StartCoroutine(RemoveVolumeFromClip(clip, normalizedVolume));
            }
        }

        public void PlayRandomSoundVariation(string soundId, float volume = 1f, bool isSpecial = false)
        {
            AudioClip soundEffect = _audioLibrary.GetRandomSoundVariation(soundId);
            if (soundEffect == null)
            {
                L.LogError($"No sound variations found for ID: {soundId}");
                return;
            }

            SoundSource source = GetFreeSoundSource();
            if (source != null)
            {
                source.GetAudioSource().outputAudioMixerGroup =
                    isSpecial ? _specialEffectsMixerGroup : _soundEffectsMixerGroup;

                float normalizedVolume = CalculateNormalizedVolume(soundEffect, volume);
                source.ConfigureAndPlay(soundEffect, normalizedVolume);
                StartCoroutine(RemoveVolumeFromClip(soundEffect, normalizedVolume));
            }
        }

        private SoundSource3D GetFreeSoundSource3D()
        {
            SoundSource3D soundSource = _soundSource3DPool.FirstOrDefault(static source => source.IsFree) ??
                                        CreateSoundSource3D();

            soundSource.GetAudioSource().outputAudioMixerGroup = _soundEffectsMixerGroup;

            return soundSource;
        }

        private SoundSource3D CreateSoundSource3D()
        {
            GameObject sourceObject = new GameObject("SoundSource3D");
            SoundSource3D source = sourceObject.AddComponent<SoundSource3D>();
            source.transform.SetParent(_soundSourcesContainer);
            source.Construct(_soundEffectsMixerGroup);
            _soundSource3DPool.Add(source);
            return source;
        }

        public void PlaySound3D(
            string soundId,
            Vector3 position,
            [CanBeNull] AudioLibrary.SoundRandomizationSettings randomization = null,
            float volume = 1f,
            float minDistance = 1f,
            float maxDistance = 50f)
        {
            AudioClip soundEffect = _audioLibrary.GetSound(soundId);
            if (soundEffect == null)
            {
                L.LogError($"Sound effect not found: {soundId}");
                return;
            }

            SoundSource3D source = GetFreeSoundSource3D();
            if (source != null)
            {
                ApplyRandomization(source.GetAudioSource(), randomization, ref volume);
                float normalizedVolume = CalculateNormalizedVolume(soundEffect, volume);
                source.ConfigureAndPlay(soundEffect, normalizedVolume, position, minDistance, maxDistance);
                StartCoroutine(RemoveVolumeFromClip(soundEffect, normalizedVolume));
            }
        }

        public void PlayRandomSoundVariation3D(
            string soundId,
            Vector3 position,
            float volume = 1f,
            float minDistance = 1f,
            float maxDistance = 50f)
        {
            AudioClip soundEffect = _audioLibrary.GetRandomSoundVariation(soundId);
            if (soundEffect == null)
            {
                L.LogError($"No sound variations found for ID: {soundId}");
                return;
            }

            SoundSource3D source = GetFreeSoundSource3D();
            if (source != null)
            {
                float normalizedVolume = CalculateNormalizedVolume(soundEffect, volume);
                source.ConfigureAndPlay(soundEffect, normalizedVolume, position, minDistance, maxDistance);
                StartCoroutine(RemoveVolumeFromClip(soundEffect, normalizedVolume));
            }
        }

        public void PlayRandomMusicVariation(string musicId, float volume = 1f, bool shouldLoop = true)
        {
            AudioClip musicTrack = _audioLibrary.GetRandomMusicVariation(musicId);

            if (musicTrack == null)
            {
                L.LogError($"No music variations found for ID: {musicId}");
                return;
            }

            if (_primaryMusicSource == null)
            {
                L.LogError("Primary music source not initialized");
                return;
            }

            if (!_primaryMusicSource.IsFree)
            {
                _primaryMusicSource.Stop();
            }

            _primaryMusicSource.ConfigureAndPlay(musicTrack, volume, shouldLoop);
        }

        public void SetMusicEnabled(bool isEnabled)
        {
            if (_primaryMusicSource != null)
            {
                _primaryMusicSource.SetMuted(!isEnabled);
            }
        }

        public void SetSoundEnabled(bool isEnabled)
        {
            foreach (SoundSource source in _soundSourcePool)
            {
                source.SetMuted(!isEnabled);
            }
        }

        public void PauseMusic() =>
            _primaryMusicSource?.Pause();

        public void ResumeMusic() =>
            _primaryMusicSource?.Resume();

        public void StopMusic() => _primaryMusicSource?.Stop();

        public void PauseAllAudio()
        {
            _primaryMusicSource?.Pause();
            foreach (SoundSource source in _soundSourcePool.Where(s => !s.IsFree))
            {
                source.Pause();
            }
        }

        public void ResumeAllAudio()
        {
            _primaryMusicSource?.Resume();
            foreach (SoundSource source in _soundSourcePool.Where(s => !s.IsFree))
            {
                source.Resume();
            }
        }

        public void SetMusicVolume(float volume)
        {
            float decibelValue = volume <= 0 ? -80f : Mathf.Log10(volume) * 20f;
            _musicMixerGroup.audioMixer.SetFloat(MUSIC_VOLUME_PARAM, decibelValue);
        }

        public void SetSoundVolume(float volume)
        {
            float decibelValue = volume <= 0 ? -80f : Mathf.Log10(volume) * 20f;
            _soundEffectsMixerGroup.audioMixer.SetFloat(SOUND_VOLUME_PARAM, decibelValue);
        }

        public float GetMusicVolume()
        {
            _musicMixerGroup.audioMixer.GetFloat(MUSIC_VOLUME_PARAM, out var decibelValue);
            return decibelValue <= -80f ? 0f : Mathf.Pow(10f, decibelValue / 20f);
        }

        public float GetSoundVolume()
        {
            _soundEffectsMixerGroup.audioMixer.GetFloat(SOUND_VOLUME_PARAM, out var decibelValue);
            return decibelValue <= -80f ? 0f : Mathf.Pow(10f, decibelValue / 20f);
        }

        public void FadeMusicTo(float targetVolume, float duration)
        {
            StopCrossfade();
            _musicFadeTween.Stop();

            _musicFadeTween = FadeMusicToInternal(
                startVolume: _primaryMusicSource.GetAudioSource().volume,
                targetVolume,
                duration,
                _primaryMusicSource.GetAudioSource());
        }

        public void MuteAll()
        {
            AudioListener.pause = true;
        }

        public void UnmuteAll()
        {
            AudioListener.pause = false;
        }

        private Tween FadeMusicToInternal(
            float startVolume,
            float targetVolume,
            float duration,
            AudioSource audioSource)
        {
            return Tween.AudioVolume(
                target: audioSource,
                startValue: startVolume,
                endValue: targetVolume,
                duration,
                useUnscaledTime: true);
        }

        private void StopCrossfade()
        {
            _crossfadeTween.Stop();

            AudioSource secondaryMusicSource = _secondaryMusicSource.GetAudioSource();
            secondaryMusicSource.volume = 0f;
            _secondaryMusicSource.IsFree = true;
        }

        public void CrossfadeToMusic(string musicId, float duration, float volume = 1f, bool shouldLoop = true)
        {
            _musicFadeTween.Stop();
            StopCrossfade();

            PlayMusic(musicId, volume, shouldLoop);

            AudioClip newMusic = _audioLibrary.GetMusic(musicId);
            if (newMusic == null)
            {
                L.LogError($"Music track not found: {musicId}");
                return;
            }

            _secondaryMusicSource.ConfigureAndPlay(newMusic, 0f, shouldLoop);

            _crossfadeTween = Sequence.Create(useUnscaledTime: true)
                .Group(FadeMusicToInternal(
                    startVolume: _primaryMusicSource.GetAudioSource().volume,
                    targetVolume: 0f,
                    duration,
                    _primaryMusicSource.GetAudioSource()))
                .Group(FadeMusicToInternal(
                    startVolume: 0f,
                    targetVolume: volume,
                    duration,
                    _secondaryMusicSource.GetAudioSource()))
                .ChainCallback(this, static service =>
                {
                    service._primaryMusicSource.Stop();

                    (service._primaryMusicSource, service._secondaryMusicSource) =
                        (service._secondaryMusicSource, service._primaryMusicSource);

                    service._secondaryMusicSource.IsFree = true;
                });
        }

        private void InitializeMixerGroups()
        {
            _masterMixerGroup = _audioSettings.MasterMixerGroup;
            _musicMixerGroup = _audioSettings.MusicMixerGroup;
            _soundEffectsMixerGroup = _audioSettings.SoundEffectsMixerGroup;
            _specialEffectsMixerGroup = _audioSettings.SpecialEffectsMixerGroup;
        }

        private void ApplyRandomization(
            AudioSource audioSource,
            AudioLibrary.SoundRandomizationSettings randomization,
            ref float volume)
        {
            if (randomization == null)
            {
                audioSource.pitch = 1f;
                return;
            }

            audioSource.pitch = randomization.PitchRange.HasValue
                ? UnityEngine.Random.Range(randomization.PitchRange.Value.x, randomization.PitchRange.Value.y)
                : 1f;

            if (randomization.VolumeRange.HasValue)
            {
                volume *= UnityEngine.Random.Range(
                    randomization.VolumeRange.Value.x,
                    randomization.VolumeRange.Value.y);
            }
        }

        private void CreateAudioSourcePools()
        {
            AudioServiceSettings settings = _audioSettings;

            for (int i = 0; i < settings.SoundSourcePoolSize; i++)
            {
                SoundSource source = CreateSoundSource();
                _soundSourcePool.Add(source);
            }

            for (int i = 0; i < settings.SoundSource3DPoolSize; i++)
            {
                SoundSource3D source = CreateSoundSource3D();
                _soundSource3DPool.Add(source);
            }

            for (int i = 0; i < settings.MusicSourcePoolSize; i++)
            {
                MusicSource source = CreateMusicSource();
                _musicSourcePool.Add(source);
            }

            _primaryMusicSource = _musicSourcePool.FirstOrDefault();

            if (_musicSourcePool.Count > 1)
            {
                _secondaryMusicSource = _musicSourcePool[1];
            }
            else
            {
                _secondaryMusicSource = CreateMusicSource();
                _musicSourcePool.Add(_secondaryMusicSource);
            }
        }

        private SoundSource GetFreeSoundSource()
        {
            SoundSource soundSource = _soundSourcePool.FirstOrDefault(source => source.IsFree)
                                      ?? CreateSoundSource();

            soundSource.GetAudioSource().outputAudioMixerGroup = _soundEffectsMixerGroup;

            return soundSource;
        }

        private SoundSource CreateSoundSource()
        {
            GameObject sourceObject = new GameObject("SoundSource");
            SoundSource source = sourceObject.AddComponent<SoundSource>();
            source.transform.SetParent(_soundSourcesContainer);
            source.Construct(_soundEffectsMixerGroup);
            _soundSourcePool.Add(source);
            return source;
        }

        private MusicSource CreateMusicSource()
        {
            GameObject sourceObject = new GameObject("MusicSource");
            sourceObject.transform.SetParent(_musicSourcesContainer);
            MusicSource source = sourceObject.AddComponent<MusicSource>();
            source.Construct(_musicMixerGroup);
            return source;
        }

        private void OnDestroy()
        {
            StopMusic();

            foreach (SoundSource source in _soundSourcePool)
            {
                if (!source.IsFree)
                {
                    source.Stop();
                }
            }

            foreach (SoundSource3D source in _soundSource3DPool)
            {
                if (!source.IsFree)
                {
                    source.Stop();
                }
            }

#if GAMEPUSH_ENABLED
            GP_Sounds.OnMute -= HandleMuteAll;
            GP_Sounds.OnUnmute -= HandleUnmuteAll;
            GP_Sounds.OnMuteSFX -= HandleMuteSfx;
            GP_Sounds.OnUnmuteSFX -= HandleUnmuteSfx;
            GP_Sounds.OnMuteMusic -= HandleMuteMusic;
            GP_Sounds.OnUnmuteMusic -= HandleUnmuteMusic;
#endif
        }

        private IEnumerator RemoveVolumeFromClip(AudioClip clip, float volume)
        {
            if (clip == null)
                yield break;

            const float FADE_OUT_FACTOR = 0.5f;
            yield return new WaitForSeconds(clip.length * FADE_OUT_FACTOR);

            if (_activeSoundVolumes.TryGetValue(clip, out List<float> volumes))
            {
                volumes.Remove(volume);

                if (volumes.Count == 0)
                    _activeSoundVolumes.Remove(clip);
            }
        }

        private float CalculateNormalizedVolume(AudioClip clip, float requestedVolume)
        {
            if (!_activeSoundVolumes.TryGetValue(clip, out List<float> volumes))
            {
                volumes = new List<float>();
                _activeSoundVolumes[clip] = volumes;
            }

            if (volumes.Count >= MAX_DUPLICATE_AUDIO_CLIPS)
                return 0f;

            float minVolume = float.MaxValue;
            float maxVolume = float.MinValue;

            foreach (float volume in volumes)
            {
                minVolume = Mathf.Min(minVolume, volume);
                maxVolume = Mathf.Max(maxVolume, volume);
            }

            float normalizedVolume = requestedVolume;

            if (maxVolume > VOLUME_NORMALIZATION_THRESHOLD)
                normalizedVolume = (minVolume + maxVolume) / (volumes.Count + 2);

            volumes.Add(normalizedVolume);
            return normalizedVolume;
        }
    }
}