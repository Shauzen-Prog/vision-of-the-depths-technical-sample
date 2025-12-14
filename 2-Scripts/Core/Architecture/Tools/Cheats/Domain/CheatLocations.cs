using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuración de destinos rápidos para cheats de desarrollo.
/// No guarda referencias a objetos de escena: usa IDs (tag) y SceneReferenceObject.
/// </summary>
[CreateAssetMenu(menuName = "Debug/Cheat Locations")]
public class CheatLocations : ScriptableObject
{
    [Header("Scenes to Load (Additive)")]
    [Tooltip("Escenas que se cargarán aditivamente (en orden).")]
    [SerializeField] private List<SceneReferenceObject> _breakingPointLoadAdditive = new();

    [Header("Scenes to Unload (Additive)")]
    [Tooltip("Escenas aditivas que se descargarán antes de cargar el preset.")]
    [SerializeField] private List<SceneReferenceObject> _breakingPointUnloadAdditive = new();

    [Header("Spawn Point por Tag")]
    [Tooltip("Tag del spawn point dentro de la escena cargada.")]
    [SerializeField] private string _breakingPointSpawnTag = "CheatSpawn_BreakingPoint";

    public IReadOnlyList<SceneReferenceObject> BreakingPointLoadAdditive => _breakingPointLoadAdditive;
    public IReadOnlyList<SceneReferenceObject> BreakingPointUnloadAdditive => _breakingPointUnloadAdditive;
    public string BreakingPointSpawnTag => _breakingPointSpawnTag;
}
