using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Maneja la reproducción de SFX y música en el juego.
/// </summary>
public interface ISoundManager 
{
    void PlaySFX(Sound id, Transform location, Action<Sound> onFinish = null);
    AudioSource PlaySFXLoop(Sound id, Transform location);
    void StopSFXLoop(AudioSource src, Sound id);

    void PlayMusic(Sound id, Action<Sound> onFinish = null);

    /// <summary>
    /// Frena la musica que suena actualmente.
    /// </summary>
    void StopMusic();
    
    //Setters
    /// <summary>
    /// Setea volumen Master en formato lineal [0..1]. Debe persistir el valor.
    /// </summary>
    void SetMasterVolume(float linear);
    
    /// <summary>
    /// Setea volumen de Música en formato lineal [0..1]. Debe persistir el valor.
    /// </summary>
    void SetMusicVolume(float linear);
    
    /// <summary>
    /// Setea volumen de SFX en formato lineal [0..1]. Debe persistir el valor.
    /// </summary>
    void SetSFXVolume(float linear);

    /// <summary>
    /// Devuelve los volúmenes actuales (lineal 0..1) para inicializar UI.
    /// </summary>
    SoundVolumes GetSavedVolumes();
    
    /// <summary>
    /// Acceso al AudioMixer subyacente si se requiere a nivel sistema.
    /// La UI no debería usar esto.
    /// </summary>
    AudioMixer Mixer { get; }
    
    void Duck(bool duck);
}
