using UnityEngine;
using Zenject;

/// <summary>
/// Acción invocable desde UnityEvents para descargar un grupo de escenas aditivas
/// asociadas a una SceneReferenceObject, filtrando por rol (Lights, Audio, etc.).
/// </summary>
public class SceneUnloadAdditiveGroupAction : MonoBehaviour
{
    [Header("Referencia lógica de escena")]
    [SerializeField] private SceneReferenceObject _sceneReference;

    [Header("Grupo aditivo a descargar")]
    [SerializeField] private AdditiveSceneRole _roleToUnload = AdditiveSceneRole.Lights;

    private ISceneRouter _sceneRouter;

    /// <summary>
    /// Inyección del router de escenas.
    /// </summary>
    [Inject]
    private void Construct(ISceneRouter sceneRouter)
    {
        _sceneRouter = sceneRouter;
    }

    /// <summary>
    /// Ejecuta la descarga de todas las escenas aditivas que coincidan
    /// con el rol configurado. Pensado para UnityEvents sin parámetros.
    /// </summary>
    public void Execute()
    {
        if (_sceneReference == null)
        {
            Debug.LogWarning("[SceneUnloadAdditiveGroupAction] No hay SceneReference asignado.", this);
            return;
        }

        foreach (string additiveName in _sceneReference.GetAdditiveNamesByRole(_roleToUnload))
        {
            _sceneRouter.UnloadAdditiveAsync(additiveName);
        }
    }
}
