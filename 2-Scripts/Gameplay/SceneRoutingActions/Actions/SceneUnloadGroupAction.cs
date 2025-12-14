using UnityEngine;
using Zenject;

/// <summary>
/// Acción invocable desde UnityEvents para descargar un conjunto de escenas
/// asociadas a una SceneReferenceObject.
/// Puede descargar la escena principal (si fue cargada aditiva)
/// y/o las escenas aditivas (todas o filtradas por rol).
/// </summary>
public class SceneUnloadGroupAction : MonoBehaviour
{
   private enum AdditivesUnloadMode
    {
        None = 0,
        All = 1,
        ByRole = 2
    }

    [Header("Referencia lógica de escena")]
    [SerializeField] private SceneReferenceObject _sceneReference;

    [Header("Escena principal")]
    [Tooltip("Si está activo, intenta descargar la escena principal como aditiva.")]
    [SerializeField] private bool _unloadMainScene = true;

    [Header("Escenas aditivas asociadas")]
    [SerializeField] private AdditivesUnloadMode _additivesMode = AdditivesUnloadMode.All;

    [Tooltip("Rol a descargar cuando el modo está en ByRole.")]
    [SerializeField] private AdditiveSceneRole _additivesRole = AdditiveSceneRole.Lights;

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
    /// Ejecuta la descarga según la configuración actual.
    /// Pensado para UnityEvents sin parámetros.
    /// </summary>
    public void Execute()
    {
        if (_sceneReference == null)
        {
            Debug.LogWarning("[SceneUnloadGroupAction] No hay SceneReference asignado.", this);
            return;
        }

        // 1) Escena principal (solo si fue cargada como aditiva).
        if (_unloadMainScene && !string.IsNullOrWhiteSpace(_sceneReference.MainSceneName))
        {
            _sceneRouter.UnloadAdditiveAsync(_sceneReference.MainSceneName);
        }

        // 2) Escenas aditivas asociadas.
        switch (_additivesMode)
        {
            case AdditivesUnloadMode.None:
                return;

            case AdditivesUnloadMode.All:
                foreach (string additiveName in _sceneReference.GetAllAdditiveNames())
                {
                    _sceneRouter.UnloadAdditiveAsync(additiveName);
                }
                break;

            case AdditivesUnloadMode.ByRole:
                foreach (string additiveName in _sceneReference.GetAdditiveNamesByRole(_additivesRole))
                {
                    _sceneRouter.UnloadAdditiveAsync(additiveName);
                }
                break;
        }
    }
}
