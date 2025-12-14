using UnityEngine;
using Zenject;

/// <summary>
/// Acción invocable desde UnityEvents para cambiar de escena
/// utilizando ISceneRouter y una SceneReferenceObject.
/// Permite cargar la escena principal como single o aditiva
/// y opcionalmente cargar escenas aditivas asociadas.
/// </summary>
public class SceneChangeAction : MonoBehaviour
{
    private enum AdditivesLoadMode
    {
        None = 0,
        All = 1,
        ByRole = 2
    }

    [Header("Referencia lógica de escena destino")]
    [SerializeField] private SceneReferenceObject _targetSceneReference;

    [Header("Escena principal")]
    [Tooltip("Si está activo, carga la escena principal asociada.")]
    [SerializeField] private bool _loadMainScene = true;

    [Tooltip("Si está activo, la escena principal se carga como aditiva. Si no, se usa GoTo (single).")]
    [SerializeField] private bool _useAdditiveForMainScene = false;

    [Header("Escenas aditivas asociadas")]
    [SerializeField] private AdditivesLoadMode _additivesMode = AdditivesLoadMode.All;

    [Tooltip("Rol a cargar cuando el modo está en ByRole.")]
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
    /// Ejecuta la transición configurada hacia la escena destino.
    /// Pensado para ser llamado desde UnityEvents sin parámetros.
    /// </summary>
    public void Execute()
    {
        if (_targetSceneReference == null)
        {
            Debug.LogWarning("[SceneChangeAction] No hay SceneReference de destino asignado.", this);
            return;
        }

        // 1) Escena principal.
        if (_loadMainScene && !string.IsNullOrWhiteSpace(_targetSceneReference.MainSceneName))
        {
            var request = new SceneRequest
            {
                sceneName = _targetSceneReference.MainSceneName,
                activateOnLoad = true,
                transition = null // Se puede inyectar una transición más adelante.
            };

            if (_useAdditiveForMainScene)
            {
                _sceneRouter.LoadAdditiveAsync(request);
            }
            else
            {
                _sceneRouter.GoToAsync(request);
            }
        }

        // 2) Escenas aditivas asociadas.
        switch (_additivesMode)
        {
            case AdditivesLoadMode.None:
                return;

            case AdditivesLoadMode.All:
                foreach (string additiveName in _targetSceneReference.GetAllAdditiveNames())
                {
                    var request = new SceneRequest
                    {
                        sceneName = additiveName,
                        activateOnLoad = true
                    };
                    _sceneRouter.LoadAdditiveAsync(request);
                }
                break;

            case AdditivesLoadMode.ByRole:
                foreach (string additiveName in _targetSceneReference.GetAdditiveNamesByRole(_additivesRole))
                {
                    var request = new SceneRequest
                    {
                        sceneName = additiveName,
                        activateOnLoad = true
                    };
                    _sceneRouter.LoadAdditiveAsync(request);
                }
                break;
        }
    }
}
