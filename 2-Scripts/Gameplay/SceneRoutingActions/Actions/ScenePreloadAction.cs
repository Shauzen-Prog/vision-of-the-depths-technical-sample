using UnityEngine;
using Zenject;

/// <summary>
/// Acción invocable desde UnityEvents para pre-cargar escenas usando ISceneRouter.
/// Permite precargar la escena principal y/o escenas aditivas por grupo.
/// </summary>
public class ScenePreloadAction : MonoBehaviour
{
    private enum AdditivesPreloadMode
    {
        None = 0,
        All = 1,
        ByRole = 2
    }
    
    [Header("Referencia lógica de escena")]
    [SerializeField] private SceneReferenceObject _sceneReference;

    [Header("Escena principal")]
    [Tooltip("Si está activo, precarga la escena principal asociada.")]
    [SerializeField] private bool _preloadMainScene = true;

    [Header("Escenas aditivas")]
    [SerializeField] private AdditivesPreloadMode _additivesMode = AdditivesPreloadMode.All;

    [Tooltip("Rol a precargar cuando el modo está en ByRole.")]
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
    /// Dispara la precarga según la configuración actual.
    /// Pensado para ser llamado desde UnityEvents sin parámetros.
    /// </summary>
    public void Preload()
    {
        if (_sceneReference == null)
        {
            Debug.LogWarning("[ScenePreloadAction] No hay SceneReference asignado.", this);
            return;
        }

        // Precarga de escena principal (sin activarla aún).
        if (_preloadMainScene && !string.IsNullOrWhiteSpace(_sceneReference.MainSceneName))
        {
            var request = new SceneRequest
            {
                sceneName = _sceneReference.MainSceneName,
                activateOnLoad = false
            };

            _sceneRouter.PreloadAsync(request);
        }

        // Precarga de escenas aditivas según modo.
        switch (_additivesMode)
        {
            case AdditivesPreloadMode.None:
                return;

            case AdditivesPreloadMode.All:
                foreach (string additiveName in _sceneReference.GetAllAdditiveNames())
                {
                    var request = new SceneRequest
                    {
                        sceneName = additiveName,
                        activateOnLoad = false
                    };
                    _sceneRouter.PreloadAsync(request);
                }
                break;

            case AdditivesPreloadMode.ByRole:
                foreach (string additiveName in _sceneReference.GetAdditiveNamesByRole(_additivesRole))
                {
                    var request = new SceneRequest
                    {
                        sceneName = additiveName,
                        activateOnLoad = false
                    };
                    _sceneRouter.PreloadAsync(request);
                }
                break;
        }
    }
}
