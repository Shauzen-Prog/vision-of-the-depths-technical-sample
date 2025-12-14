using UnityEngine;
using Zenject;

/// <summary>
/// Orquestador de la UI de Opciones/Sonidos. Mantiene la lógica fuera de la View,
/// facilitando tests y reutilización (ej. en el pause menu).
/// </summary>
public class SoundOptionsPresenter 
{
    private readonly ISoundManager _sound;

    [Inject]
    public SoundOptionsPresenter(ISoundManager sound)
    {
        _sound = sound;
    }

    /// <summary>
    /// Obtiene los volúmenes actuales (lineal 0..1) para inicializar sliders.
    /// </summary>
    public SoundVolumes GetCurrentVolumes() => _sound.GetSavedVolumes();

    /// <summary>Handlers llamados por la View.</summary>
    public void SetMaster(float linear) => _sound.SetMasterVolume(linear);
    public void SetMusic(float linear)  => _sound.SetMusicVolume(linear);
    public void SetSfx(float linear)    => _sound.SetSFXVolume(linear);
}
