using System;
using UnityEngine;
using Zenject;

/// <summary>
/// Detector acoplado a la cámara del jugador.
/// Hace un raycast al centro de la pantalla y comunica al
/// IPlayerInteractionController qué target está actualmente en foco.
/// No ejecuta Interact(); sólo resuelve el foco.
/// 
/// Mientras haya un closeup activo, deja de actualizar el target para
/// no perder la referencia al closeup aunque el raycast no toque el collider.
/// Además escucha ForceInteractionTargetEvent para permitir que otros
/// sistemas (como el closeup) fuercen el target actual.
/// </summary>
public class PlayerInteractionDetectorV2 : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _maxDistance = 4f;
    [SerializeField] private LayerMask _interactionMask = ~0;

    [Tooltip("Desplaza el origen del ray un poco hacia atrás para evitar que nazca dentro de un collider.")]
    [SerializeField, Min(0f)]
    private float _originBackOffset = 0.35f;
    
    private IPlayerInteractionController _interactionController;
    private IEventBus _eventBus;
    
    // --- Visual state ---
    private IInteractionTarget _lastVisualTarget;
    private InteractionVisual _currentVisual;
    
    private bool _lockTarget;
    private System.IDisposable _subForceTarget;
    
#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _drawDebugRay = true;
#endif

    [Inject]
    public void Construct(IPlayerInteractionController interactionController,
        IEventBus eventBus)
    {
        _interactionController = interactionController;
        _eventBus = eventBus;
    }

    private void OnEnable()
    {
        if (_eventBus != null)
        {
            _subForceTarget  = _eventBus.Subscribe<ForceInteractionTargetEvent>(OnForceInteractionTarget);
        }
    }

    private void OnDisable()
    {
        _subForceTarget?.Dispose();
        _subForceTarget = null;
        
        // Asegurarse de apagar visual si el detector se desactiva
        if (_currentVisual == null) return;
        
        _currentVisual.DisableAll();
        _currentVisual = null;
        _lastVisualTarget = null;
    }

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
    }

    private void Update()
    {
        if (_camera == null || _interactionController == null)
            return;

        if (_lockTarget)
            return;

        IInteractionTarget foundTarget = null;

        // Ray al centro de la pantalla
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 origin = ray.origin;
        Vector3 direction = ray.direction;

        // Mover el origen un poco hacia atrás
        float distance = _maxDistance;
        if (_originBackOffset > 0f)
        {
            origin -= direction * _originBackOffset;
            distance += _originBackOffset;
        }

        if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                distance,
                _interactionMask,
                QueryTriggerInteraction.Collide))
        {
            foundTarget = hit.collider.GetComponentInParent<IInteractionTarget>();
        }

#if UNITY_EDITOR
        if (_drawDebugRay)
        {
            Color color = (foundTarget != null) ? Color.green : Color.red;
            Debug.DrawRay(origin, direction * distance, color);
        }
#endif

        _interactionController.SetCurrentTarget(foundTarget);
        HandleVisualForTarget(foundTarget);
    }
    
    private void OnForceInteractionTarget(ForceInteractionTargetEvent evt)
    {
        // Cuando alguien fuerza el target (por ejemplo un closeup que se abre
        // desde un diálogo), lo seteamos directamente sin depender del raycast.
        _lockTarget = evt.LockTarget;
        _interactionController?.SetCurrentTarget(evt.Target);
        HandleVisualForTarget(evt.Target);
    }

    private void LateUpdate()
    {
        // Si hay un visual activo pero el target ya no está habilitado,
        // apagarlo inmediatamente.
        if (_currentVisual != null && _lastVisualTarget != null)
        {
            if (!_lastVisualTarget.IsEnabled)
            {
                _currentVisual.DisableAll();
                _currentVisual = null;
                return;
            }
        }
    }

    /// <summary>
    /// Enciende / apaga el InteractionVisual en función del nuevo target.
    /// </summary>
    /// <param name="newTarget">Nuevo target de interacción.</param>
    private void HandleVisualForTarget(IInteractionTarget newTarget)
    {
        if (newTarget == _lastVisualTarget)
            return;

        // Apagar feedback del target anterior
        if (_currentVisual != null)
        {
            _currentVisual.DisableAll();
            _currentVisual = null;
        }

        _lastVisualTarget = newTarget;

        // Si no hay target, salir
        if (newTarget == null)
            return;
        
        // Si el target NO está habilitado, NO mostrar nada
        if (!newTarget.IsEnabled)
            return;

        if (newTarget is Component component)
        {
            _currentVisual = component.GetComponentInChildren<InteractionVisual>();
            _currentVisual?.ShowAll();
        }
    }
}
