using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Zenject;
using Debug = UnityEngine.Debug;

// ReSharper disable All

public enum Sound
{
    // AGREGAR SOLO DESDE ABAJO O SE ROMPE 
    No_Sound,
    Music_MainTheme,
    Music_Menu,
    SFX_ButtonClick,
    SFX_JumpScare,
    SFX_CamaOpening,
    SFX_ChangeClothes,
    SFX_MetalFootstep,
    SFX_DirtFootstep,
    SFX_LeverClick,
    SFX_LeverRoll,
    SFX_KiubsVoice,
    SFX_NarratorVoice,
    SFX_RamonVoice,
    SFX_CaptainVoice,
    SFX_UiClick,
    SFX_Engine_On,
    SFX_Engine_Off,
    SFX_Engine_Loop,
    SFX_Refill_Loop,
    SFX_Knocking,
    SFX_Refill_empty,
    SFX_TextPopup,
    SFX_Eat,
    Music_SpookyAllucination1,
    SFX_PickupGasCan,
    SFX_DropGasCan,
    SFX_FlashlightOn,
    SFX_FlashlightOff,
    SFX_WeikoVoice,
    SFX_FastFootsteps,
    SFX_Stairs,
    SFX_Wood,
    // ... mas identificadores
}

[Serializable]
public class SoundData
{
    public Sound id;
    public AudioClip clip;
    public bool loop;
    [Range(0f, 1f)] public float volume = 1f;
}

[Serializable]
public class SFXPrefabData
{
    public Sound id;
    public List<AudioSource> prefabs;       // Lista de prefabs configurados visualmente
    public int initialPoolSize = 5;         // Tamaño inicial del pool por prefab
    public bool randomizePitch = false;     // Activa pitch aleatorio
    [Range(0.1f, 3f)] public float minPitch = 0.9f;
    [Range(0.1f, 3f)] public float maxPitch = 1.1f;
}

public class SoundManager : MonoBehaviour, ISoundManager
{
    [Header("Mixer & Groups")]
    public AudioMixer mixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    [Header("Snapshots & Ducking")]
    public AudioMixerSnapshot normalSnapshot;
    public AudioMixerSnapshot hallucinationSnapshot;
    [Range(-80f, 0f)] public float duckVolumeDB = -20f;

    [Header("Occlusion Settings")]
    public bool enableOcclusion = false;
    public Transform listener;
    public float occlusionCheckInterval = 0.2f;
    public string lowPassParam = "LowPassCutoff";
    public float occludedCutoff = 800f;
    public float unoccludedCutoff = 22000f;

    [Header("Volumes (0–1)")]
    [Range(0f,1f)] public float musicVolume = 1f;
    [Range(0f,1f)] public float sfxVolume   = 1f;

    [FormerlySerializedAs("sounds")] 
    [Header("Music Clips")]
    public List<SoundData> music;

    [Header("SFX Prefabs & Pools")]
    public List<SFXPrefabData> sfxPrefabs;

    /// <summary>Evento disparado cuando un clip termina.</summary>
    //public event Action<Sound> OnSoundFinished;

    private Dictionary<Sound, SoundData> soundDict;
    private Dictionary<Sound, List<AudioSource>> soundToPrefabs;
    private Dictionary<AudioSource, Queue<AudioSource>> sfxPools;
    private AudioSource musicSource;

    // Nombres de parametros expuestos en el AudioMixer
    private const string MASTER_VOL_PARAM = "MasterVol";
    private const string MUSIC_VOL_PARAM = "MusicVol";
    private const string SFX_VOL_PARAM = "SFXVol";   
    
    
    // Dependencias (persistencia)
    private IPersistenceService _persistence;
    private PersistedGameState _state;
    //private bool _initialized;
    
    [Inject]
    public void Construct(IPersistenceService persistence)
    {
        _persistence = persistence;
    }
    
    void Awake()
    {
        BuildSoundDictionary();
        InitMixerVolumes();
        InitMusicSource();
        InitSFXPools();
        
       ApplyAllVolumesFromState();
        
        if (enableOcclusion && listener != null)
            StartCoroutine(OcclusionRoutine());
    }
    
    /// <summary>
    /// Zenject llama a Initialize cuando TODOS los installers terminaron.
    /// </summary>
    public void Initialize()
    {
        EnsureState();
        ApplyAllVolumesFromState();
        //_initialized = true;
    }
    
    /// <summary>
    /// Garantiza que el estado esté cargado incluso si nos llaman antes de Initialize().
    /// </summary>
    private void EnsureState()
    {
        if (_state == null)
            _state = _persistence != null ? _persistence.Load() : new PersistedGameState();
    }
    
    private PersistedGameState State =>
        _state ?? (_state = _persistence?.Load() ?? new PersistedGameState());
    
    /// <summary>
    /// Aplica al mixer los volúmenes guardados en el estado persistente.
    /// </summary>
    private void ApplyAllVolumesFromState()
    {
        if (!EnsureMixerAssigned(nameof(ApplyAllVolumesFromState))) return;
        mixer.SetFloat(MASTER_VOL_PARAM, LinearToDecibels(_state.MasterVolume));
        mixer.SetFloat(MUSIC_VOL_PARAM,  LinearToDecibels(_state.MusicVolume));
        mixer.SetFloat(SFX_VOL_PARAM,    LinearToDecibels(_state.SfxVolume));
    }
    
    private void BuildSoundDictionary()
    {
        soundDict = new Dictionary<Sound, SoundData>();
        foreach (var sd in music)
            if (sd.clip != null && !soundDict.ContainsKey(sd.id))
                soundDict.Add(sd.id, sd);
    }

    private void InitMixerVolumes()
    {
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        normalSnapshot.TransitionTo(0f);
    }

    private void InitMusicSource()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    private void InitSFXPools()
    {
        soundToPrefabs = new Dictionary<Sound, List<AudioSource>>();
        sfxPools = new Dictionary<AudioSource, Queue<AudioSource>>();

        foreach (var pd in sfxPrefabs)
        {
            soundToPrefabs[pd.id] = pd.prefabs;
            foreach (var prefab in pd.prefabs)
            {
                var queue = new Queue<AudioSource>(pd.initialPoolSize);
                for (int i = 0; i < pd.initialPoolSize; i++)
                {
                    var inst = Instantiate(prefab, transform);
                    inst.outputAudioMixerGroup = sfxGroup;
                    inst.playOnAwake = false;
                    inst.gameObject.SetActive(false);
                    queue.Enqueue(inst);
                }
                sfxPools[prefab] = queue;
            }
        }
    }
    
    // ----------------- Conversión lineal <-> dB -----------------

    /// <summary>
    /// Convierte [0..1] a dB. Usa -80dB como "mute".
    /// </summary>
    private float LinearToDecibels(float linear)
    {
        linear = Mathf.Clamp01(linear);
        const float minDb = -80f;
        if (linear <= 0.0001f) return minDb;
        return Mathf.Log10(linear) * 20f;
    }

    /// <summary>
    /// Convierte dB a lineal [0..1].
    /// </summary>
    private float DecibelsToLinear(float db)
    {
        return Mathf.Clamp01(Mathf.Pow(10f, db / 20f));
    }
    
    // ----------------- ISoundManager -----------------

    /// <summary>
    /// Setea Master (lineal 0..1) y persiste.
    /// </summary>
    public void SetMasterVolume(float linear)
    {
        EnsureState();
        if (!EnsureMixerAssigned(nameof(ApplyAllVolumesFromState))) return;
        
        float db = LinearToDecibels(linear);
        mixer.SetFloat(MASTER_VOL_PARAM, db);

        _state.MasterVolume = Mathf.Clamp01(linear);
        _persistence.Save(_state);
    }

    /// <summary>
    /// Setea Música (lineal 0..1) y persiste.
    /// </summary>
    public void SetMusicVolume(float linear)
    {
        EnsureState();
        if (!EnsureMixerAssigned(nameof(ApplyAllVolumesFromState))) return;
        
        float db = LinearToDecibels(linear);
        mixer.SetFloat(MUSIC_VOL_PARAM, db);

        _state.MusicVolume = Mathf.Clamp01(linear);
        _persistence.Save(_state);
    }

    /// <summary>
    /// Setea SFX (lineal 0..1) y persiste.
    /// </summary>
    public void SetSFXVolume(float linear)
    {
        EnsureState();
        if (!EnsureMixerAssigned(nameof(ApplyAllVolumesFromState))) return;
        
        float db = LinearToDecibels(linear);
        mixer.SetFloat(SFX_VOL_PARAM, db);

        _state.SfxVolume = Mathf.Clamp01(linear);
        _persistence.Save(_state);
    }

    /// <summary>
    /// Devuelve valores lineales 0..1 (para sliders).
    /// </summary>
    public SoundVolumes GetSavedVolumes()
    {
        EnsureState();
        return new SoundVolumes(_state.MasterVolume, _state.MusicVolume, _state.SfxVolume);
    }

    /// <summary>
    /// Acceso al mixer (sólo para sistemas; la UI no debería usarlo).
    /// </summary>
    public AudioMixer Mixer => mixer;
    
    // ----------------- Helpers para routeo -----------------

    /// <summary>
    /// Asigna el grupo de música a un AudioSource (para música de menú, etc.).
    /// </summary>
    public void RouteToMusic(AudioSource source)
    {
        if (source != null && musicGroup != null) source.outputAudioMixerGroup = musicGroup;
    }

    /// <summary>
    /// Asigna el grupo de SFX a un AudioSource (para clicks de UI, etc.).
    /// </summary>
    public void RouteToSfx(AudioSource source)
    {
        if (source != null && sfxGroup != null) source.outputAudioMixerGroup = sfxGroup;
    }

    /// <summary>Reproduce música de fondo. Callback se dispara una vez al terminar.</summary>
    public void PlayMusic(Sound id, Action<Sound> onFinish = null)
    {
        if (!soundDict.TryGetValue(id, out var data) || data.clip == null) return;
        
        musicSource.clip = data.clip;
        musicSource.volume = data.volume * musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (!musicSource.isPlaying || musicSource.clip == null) return;
        
        musicSource?.Stop();
    }

    public void SetSnapshot(string state, float transitionTime = 1f)
    {
        switch (state)
        {
            case "Normal": normalSnapshot.TransitionTo(transitionTime); break;
            case "Hallucination": hallucinationSnapshot.TransitionTo(transitionTime); break;
        }
    }

    public void Duck(bool duck)
    {
        float targetDB = duck ? duckVolumeDB : (Mathf.Log10(Mathf.Clamp(musicVolume,0.0001f,1f))*20f);
        mixer.SetFloat("MusicVol", targetDB);
        mixer.SetFloat("SFXVol",   targetDB);
    }

    private IEnumerator OcclusionRoutine()
    {
        while (true)
        {
            bool blocked = Physics.Linecast(musicSource.transform.position, listener.position, out _);
            mixer.SetFloat(lowPassParam, blocked ? occludedCutoff : unoccludedCutoff);
            yield return new WaitForSeconds(occlusionCheckInterval);
        }
    }

    /// <summary>
    /// Reproduce un prefab SFX en la posición, con pooling y callback una vez.
    /// </summary>
    public void PlaySFX(Sound id, Transform location, Action<Sound> onFinish = null)
    {
            // 1. Buscar el prefab y datos correspondientes
        if (!soundToPrefabs.TryGetValue(id, out var prefabs) || prefabs.Count == 0) return;
        var pd = sfxPrefabs.Find(x => x.id == id);
        var prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];

        // 2. Tomar el AudioClip del prefab original (NO del temporal)
        var clip = prefab.clip;

        if (clip == null)
        {
            Debug.LogError($"[SoundManager] No AudioClip assigned for {id} in prefab {prefab.name}.");
            return;
        }
        if (clip.length <= 0.01f)
        {
            Debug.LogError($"[SoundManager] AudioClip {clip.name} for {id} has zero/invalid length.");
            return;
        }

        // 3. Si el sonido es corto (≤0.3s), usá un AudioSource temporal (no pool)
        var destroyTime = Mathf.Max(clip.length + 0.05f, 0.1f);
        
        if (clip.length <= 0.3f)
        {
            var tempGO = new GameObject("OneShotSFX_" + id);
            SceneManager.MoveGameObjectToScene(tempGO, SceneManager.GetActiveScene());
            var tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.outputAudioMixerGroup = sfxGroup;
            tempSource.volume = sfxVolume;
            tempSource.pitch = pd.randomizePitch ? UnityEngine.Random.Range(pd.minPitch, pd.maxPitch) : 1f;
            tempSource.transform.position = location.position;
            tempSource.spatialBlend = prefab.spatialBlend;

            tempSource.PlayOneShot(clip, tempSource.volume);

            StartCoroutine(DestroyAfterSeconds(tempGO, destroyTime));

            onFinish?.Invoke(id);
            return;
        }

        // 4. Si es un sonido más largo, usá el pool normalmente
        if (!sfxPools.TryGetValue(prefab, out var queue) || queue.Count == 0) return;
        var src = queue.Dequeue();

        src.transform.position = location.position;
        src.volume = sfxVolume;
        src.pitch = pd.randomizePitch ? UnityEngine.Random.Range(pd.minPitch, pd.maxPitch) : 1f;
        src.loop = false;
        src.gameObject.SetActive(true);
        src.Play();

        StartCoroutine(ReturnToPool(prefab, src, clip.length, id, onFinish));
    }
    
    private IEnumerator DestroyAfterSeconds(GameObject go, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Destroy(go);
    }
    
    /// <summary>
    /// Reproduce un SFX en loop y retorna el AudioSource para control manual.
    /// </summary>
    public AudioSource PlaySFXLoop(Sound id, Transform location)
    {
        if (!soundToPrefabs.TryGetValue(id, out var prefabs) || prefabs.Count == 0) return null;
        var pd = sfxPrefabs.Find(x => x.id == id);
        var prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];

        if (!sfxPools.TryGetValue(prefab, out var queue) || queue.Count == 0) return null;
        var src = queue.Dequeue();

        src.transform.position = location.position;
        src.volume = sfxVolume;
        src.pitch = pd.randomizePitch ? UnityEngine.Random.Range(pd.minPitch, pd.maxPitch) : 1f;
        src.loop = true; // Lo pone en modo loop
        src.gameObject.SetActive(true);
        src.Play();

        return src;
    }

    /// <summary>
    /// Detiene un SFX loop y lo retorna al pool.
    /// </summary>
    public void StopSFXLoop(AudioSource src, Sound id)
    {
        if (src == null) return;
        src.Stop();
        src.loop = false;
        src.gameObject.SetActive(false);
        var pd = sfxPrefabs.Find(x => x.id == id);
        if (pd != null)
        {
            var prefab = pd.prefabs[0];
            if (sfxPools.TryGetValue(prefab, out var queue))
            {
                queue.Enqueue(src);
            }
        }
    }

    private IEnumerator ReturnToPool(AudioSource prefab, AudioSource src, float delay, Sound id, Action<Sound> onFinish)
    {
        yield return new WaitForSeconds(delay);
        src.Stop();
        src.gameObject.SetActive(false);
        sfxPools[prefab].Enqueue(src);
        onFinish?.Invoke(id); // Callback manual
    }
    
    /// <summary>
    /// Valida que el mixer esté asignado. Loguea solo en Editor.
    /// </summary>
    private bool EnsureMixerAssigned(string caller)
    {
        if (mixer != null) return true;

#if UNITY_EDITOR
        UnityEngine.Debug.LogError(
            $"[SoundManager] AudioMixer no asignado (caller: {caller}). " +
            $"Asignalo en el prefab del SoundManager del ProjectContext.", this);
#endif

        return false;
    }
}