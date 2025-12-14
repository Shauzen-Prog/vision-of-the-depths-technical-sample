using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Rol lógico de una escena aditiva asociada a una escena principal.
/// Permite distinguir, por ejemplo, luces, audio, navmesh, etc.
/// </summary>
[Serializable]
public enum AdditiveSceneRole
{
    Default = 0,
    Lights = 1,
    Audio = 2,
    NavMesh = 3,
    Vfx = 4
}


/// <summary>
/// Entrada que representa una escena aditiva asociada a una escena principal.
/// Almacena tanto el nombre de la escena (para runtime) como un SceneAsset (editor).
/// </summary>
[Serializable]
public class AdditiveSceneEntry
{
    
#if UNITY_EDITOR
    [Header("Editor only")]
    [Tooltip("Referencia al asset de la escena. Solo se usa en el editor para sincronizar el nombre.")]
    [SerializeField] private SceneAsset _sceneAsset;
#endif
    
    [Header("Runtime")]
    [Tooltip("Nombre de la escena tal como figura en Build Settings.")]
    [SerializeField] private string _sceneName;

    [Tooltip("Rol de esta escena aditiva (Lights, Audio, NavMesh, etc.).")]
    [SerializeField] private AdditiveSceneRole _role = AdditiveSceneRole.Default;

    /// <summary>
    /// Nombre de la escena aditiva. Es el valor usado en runtime.
    /// </summary>
    public string SceneName => _sceneName;

    /// <summary>
    /// Rol lógico asociado a esta escena aditiva.
    /// </summary>
    public AdditiveSceneRole Role => _role;

#if UNITY_EDITOR
    /// <summary>
    /// Sincroniza el nombre de la escena con el asset asignado en el editor.
    /// Se debe llamar desde OnValidate del ScriptableObject contenedor.
    /// </summary>
    public void SyncFromAsset()
    {
        if (_sceneAsset != null)
        {
            _sceneName = _sceneAsset.name;
        }
    }
#endif
}


[CreateAssetMenu(
    fileName = "SceneReference", 
    menuName = "Game/Scene Reference",
    order = 0)]
public class SceneReferenceObject : ScriptableObject
{
  #if UNITY_EDITOR
    [Header("Editor only")]
    [Tooltip("Escena principal en el editor. El nombre se sincroniza a _mainSceneName.")]
    [SerializeField] private SceneAsset _mainSceneAsset;
#endif

    [Header("Runtime")]
    [Tooltip("Nombre de la escena principal tal como figura en Build Settings.")]
    [SerializeField] private string _mainSceneName;

    [Tooltip("Escenas aditivas asociadas a esta locación (Lights, Audio, NavMesh, etc.).")]
    [SerializeField] private List<AdditiveSceneEntry> _additiveScenes = new List<AdditiveSceneEntry>();

    /// <summary>
    /// Nombre de la escena principal que se usa en el ISceneRouter.
    /// </summary>
    public string MainSceneName => _mainSceneName;

    /// <summary>
    /// Devuelve los nombres de todas las escenas aditivas asociadas.
    /// </summary>
    public IEnumerable<string> GetAllAdditiveNames()
    {
        foreach (var entry in _additiveScenes)
        {
            if (string.IsNullOrWhiteSpace(entry.SceneName))
                continue;

            yield return entry.SceneName;
        }
    }

    /// <summary>
    /// Devuelve los nombres de las escenas aditivas que coincidan con el rol indicado.
    /// Por ejemplo, todas las marcadas como Lights.
    /// </summary>
    public IEnumerable<string> GetAdditiveNamesByRole(AdditiveSceneRole role)
    {
        foreach (var entry in _additiveScenes)
        {
            if (string.IsNullOrWhiteSpace(entry.SceneName))
                continue;

            if (entry.Role == role)
                yield return entry.SceneName;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Sincroniza nombre principal desde el asset.
        if (_mainSceneAsset != null)
        {
            _mainSceneName = _mainSceneAsset.name;
        }

        // Sincroniza nombres de escenas aditivas desde sus assets.
        if (_additiveScenes == null)
            return;

        foreach (var entry in _additiveScenes)
        {
            entry?.SyncFromAsset();
        }
    }
#endif
}
