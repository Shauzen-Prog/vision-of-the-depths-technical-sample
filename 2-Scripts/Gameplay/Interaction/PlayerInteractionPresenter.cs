using UnityEngine;
using Zenject;

/// <summary>
/// Bridge de presentación que construye el InteractionContext cada frame
/// y delega en la capa de aplicación la ejecución de la lógica de interacción.
/// </summary>
public class PlayerInteractionPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _camera;

    private IPlayerInteractionController _interactionController;
    private IPlayerFacadeService _playerFacadeService;

    [Inject]
    public void Construct(IPlayerInteractionController interactionController, IPlayerFacadeService playerFacadeService)
    {
        _interactionController = interactionController;
        _playerFacadeService = playerFacadeService;
    }

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
    }

    private void Update()
    {
        // Obtenemos el Player actual registrado globalmente
        var playerFacade = _playerFacadeService.Current;
        if (playerFacade == null)
            return; // Player aún no existe en esta escena

        Vector3 origin = _camera != null ? _camera.transform.position : transform.position;
        var context = new InteractionContext(playerFacade, origin);

        _interactionController.Tick(context);
    }
}
