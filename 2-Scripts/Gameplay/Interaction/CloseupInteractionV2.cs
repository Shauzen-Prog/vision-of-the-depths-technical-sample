using System;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using Zenject;

/// <summary>
/// Interacción de close-up para pantallas, consolas, etc.
/// Implementa IInteractionTarget para el sistema de interacción V2
/// e ICancelableInteraction para poder salir con Escape / B / O.
/// Publica eventos en el EventBus para que otros sistemas (player)
/// bloqueen movimiento y look mientras el closeup está activo.
/// Ahora también respeta el estado del diálogo: durante un diálogo
/// no se puede salir del closeup, pero al terminar sí.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class CloseupInteractionV2 : MonoBehaviour, IInteractionTarget, ICancelableInteraction
{
    [Header("General")]
    [SerializeField] private bool _isEnabled = true;
    
    [Tooltip("Si está activo, se puede salir del closeup presionando de nuevo el botón de interacción (E).")]
    [SerializeField] private bool _toggleWithInteract = true;
    
    [Header("Target de interacción")]
    [Tooltip("Drill + Dialogos -> true. Closeups simples (monitores,etc) -> false")]
    [SerializeField] private bool _lockInteractionTargetWhileActive = true;
    
    [Header("Diálogo")]
    [Tooltip("Si está activo, el closeup bloquea salir mientras haya un diálogo en curso.")]
    [SerializeField] private bool _blockExitWhileDialogue = true;
    
    [Header("Comportamiento post-diálogo")]
    [Tooltip("Si está activo, luego del primer diálogo este closeup pasa a comportarse como un closeup simple (sin lock ni bloqueo por diálogo).")]
    [SerializeField] private bool _convertToSimpleAfterDialogue = false;

    [Header("Cámara de closeup")]
    [SerializeField] private CinemachineVirtualCamera _closeupCamera;
    
    [Tooltip("Aumenta esta prioridad respecto a la base del closeup para sobrepasar al player.")]
    [SerializeField] private int _priorityBoost = 10;

    [Header("Teleport del jugador (opcional)")]
    [SerializeField] private bool _teleportPlayerToPoint = false;
    [SerializeField] private Transform _interactionPoint;

    [Header("Cursor")]
    [SerializeField] private bool _showCursorDuringCloseup = true;

    [Header("Eventos Unity")]
    [SerializeField] private UnityEvent _onEnterCloseup;
    [SerializeField] private UnityEvent _onExitCloseup;

    private IPlayerFacadeService _playerFacadeService;
    private IEventBus _eventBus;
    private MouseCaptureService _mouseCapture;
    private IPlayerCameraProvider _cameraProvider;
    
    //Servicio de control de player (movement/look/pause)
    private IPlayerControlService _playerControl;

    // Estado interno
    private bool _isInCloseup;
    private bool _hasPlayed;
    
    // flag para saber si hay diálogo activo
    private bool _dialogueRunning;
    
    private IDisposable _dialogueStartSub;
    private IDisposable _dialogueEndSub;

    private int _playerBasePriority;
    private int _closeupBasePriority;

    /// <summary>
    /// Indica si la interacción está habilitada actualmente.
    /// </summary>
    public bool IsEnabled => _isEnabled;

    [Inject]
    private void Construct(
        IPlayerFacadeService playerFacadeService,
        IEventBus eventBus,
        MouseCaptureService mouseCapture,
        IPlayerCameraProvider cameraProvider,
        IPlayerControlService playerControl)
    {
        _playerFacadeService = playerFacadeService;
        _eventBus = eventBus;
        _mouseCapture = mouseCapture;
        _cameraProvider = cameraProvider;
        _playerControl = playerControl;
    }

    private void OnEnable()
    {
        // Escuchar inicio / fin de diálogos
        if (_eventBus != null)
        {
            _dialogueStartSub = _eventBus.Subscribe<DialogueStartedEvent>(_ => _dialogueRunning = true);
            _dialogueEndSub   = _eventBus.Subscribe<DialogueEndedEvent>(_ => _dialogueRunning = false);
        }
    }

    private void OnDisable()
    {
        // Liberar subscripciones
        _dialogueStartSub?.Dispose();
        _dialogueEndSub?.Dispose();
        _dialogueStartSub = null;
        _dialogueEndSub = null;

        // Si se desactiva abruptamente, liberar todos los bloqueos que este closeup tenía
        _playerControl?.ReleaseAll(this);
    }
    
    /// <summary>
    /// Habilita el lock del target de interacción mientras este closeup esté activo.
    /// Se usa para closeups que durante un diálogo necesitan que el raycast no cambie de target.
    /// </summary>
    public void EnableInteractionTargetLock()
    {
        _lockInteractionTargetWhileActive = true;
    }

    /// <summary>
    /// Deshabilita el lock del target de interacción.
    /// A partir de la próxima entrada al closeup, el detector volverá a manejar el target por raycast normalmente.
    /// </summary>
    public void DisableInteractionTargetLock()
    {
        _lockInteractionTargetWhileActive = false;
    }
    
    /// <summary>
    /// Habilita el bloqueo de salida por diálogo.
    /// </summary>
    public void EnableBlockExitWhileDialogue()
    {
        _blockExitWhileDialogue = true;
    }

    /// <summary>
    /// Deshabilita el bloqueo de salida por diálogo.
    /// El jugador puede salir del closeup normalmente.
    /// </summary>
    public void DisableBlockExitWhileDialogue()
    {
        _blockExitWhileDialogue = false;
    }
    
    /// <summary>
    /// Determina si el jugador puede interactuar con este closeup
    /// según el contexto actual.
    /// </summary>
    public bool CanInteract(InteractionContext context)
    {
        if (!_isEnabled)
            return false;

        // Si estamos en closeup y hay un diálogo en curso, no permitimos
        // usar E para salir hasta que termine.
        if (_blockExitWhileDialogue && _isInCloseup && _dialogueRunning)
            return false;

        return true;
    }

    /// <summary>
    /// Ejecuta la interacción de closeup:
    /// - Si no está activo, entra al closeup.
    /// - Si ya está activo y el toggle está habilitado, sale del closeup.
    /// </summary>
    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context))
            return;

        if (_isInCloseup)
        {
            if (_toggleWithInteract)
                ExitCloseup();

            return;
        }

        EnterCloseup();
    }

    /// <summary>
    /// Indica si tiene sentido cancelar en este momento.
    /// </summary>
    public bool CanCancel()
    {
        // ESC / B / O tampoco pueden cerrar mientras haya diálogo.
        if (_blockExitWhileDialogue && _isInCloseup && _dialogueRunning)
            return false;

        return _isInCloseup;
    }

    /// <summary>
    /// Cancelación de la interacción (usada por ESC / B / O).
    /// Sale del closeup si está activo.
    /// </summary>
    public void Cancel()
    {
        if (!CanCancel())
            return;

        ExitCloseup();
    }
    
    // ------------------------------
    // LÓGICA PRINCIPAL DE CLOSEUP
    // ------------------------------

    private void EnterCloseup()
    {
        if (_isInCloseup)
            return;

        _isInCloseup = true;

        // Teleport opcional
        var player = _playerFacadeService?.Current;
        if (_teleportPlayerToPoint && _interactionPoint != null && player != null)
        {
            player.TeleportTo(_interactionPoint, alignYawOnly: true);
        }

        SetCameraPriorities(true);

        if (_showCursorDuringCloseup)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        // Bloquear Movement + Look + Pause mientras esté en closeup
        _playerControl?.RequestBlock(
            new[]
            {
                PlayerControlType.Movement,
                PlayerControlType.Look,
                PlayerControlType.Pause
            },
            this);
        
        _eventBus?.Publish(new ForceInteractionTargetEvent(this, _lockInteractionTargetWhileActive));

        _onEnterCloseup?.Invoke();
        _eventBus?.Publish(new CloseupStartedEvent());

        _hasPlayed = true;
    }

    private void ExitCloseup()
    {
        if (!_isInCloseup)
            return;

        _isInCloseup = false;

        SetCameraPriorities(false);

        _onExitCloseup?.Invoke();

        // Volver a capturar mouse
        _mouseCapture?.Capture();

        // Liberar bloqueos de Movement + Look + Pause
        _playerControl?.ReleaseAll(this);
        
        // Limpiar el target de interacción para que el controller
        // no siga apuntando a este closeup cuando ya salimos.
        _eventBus?.Publish(new ForceInteractionTargetEvent(null, false));

        _eventBus?.Publish(new CloseupEndedEvent());
    }
    
    // ------------------------------
    // CÁMARAS
    // ------------------------------

    /// <summary>
    /// Ajusta las prioridades de la cámara del player y la de closeup.
    /// Usa el provider global para obtener la cámara actual del jugador.
    /// </summary>
    /// <param name="isCloseup">True para entrar al closeup, false para salir.</param>
    private void SetCameraPriorities(bool isCloseup)
    {
        var playerCamera = _cameraProvider?.PlayerCamera;

        if (playerCamera == null || _closeupCamera == null)
        {
            Debug.LogWarning(
                $"[Closeup] No camera set. PlayerCam={playerCamera}, CloseupCam={_closeupCamera}",
                this);
            return;
        }

        // Guardar prioridades base una sola vez
        if (!_hasPlayed)
        {
            _playerBasePriority = playerCamera.Priority;
            _closeupBasePriority = _closeupCamera.Priority;
        }

        if (isCloseup)
        {
            _closeupCamera.Priority = _closeupBasePriority + _priorityBoost;
            playerCamera.Priority = _playerBasePriority;
        }
        else
        {
            _closeupCamera.Priority = _closeupBasePriority;
            playerCamera.Priority = _playerBasePriority;
        }
    }
    
    /// <summary>
    /// Notifica al closeup que empezó un diálogo asociado.
    /// Mientras este flag esté activo, no se puede salir del closeup.
    /// Se llama desde DialogueTriggerV2.OnDialogueStart (UnityEvent).
    /// </summary>
    public void NotifyDialogueStarted()
    {
        if (!_blockExitWhileDialogue)
            return;

        _dialogueRunning = true;
    }

    /// <summary>
    /// Notifica al closeup que el diálogo terminó.
    /// Si está configurado para volverse simple, desactiva el lock y el bloqueo por diálogo.
    /// </summary>
    public void NotifyDialogueEnded()
    {
        _dialogueRunning = false;

        if (_convertToSimpleAfterDialogue)
        {
            // A partir de ahora este closeup funciona como uno simple:
            // - no lockea más el target
            // - no bloquea la salida por diálogos (que ya no va a tener)
            _lockInteractionTargetWhileActive = false;
            _blockExitWhileDialogue = false;
        }
    }
    
    // ------------------------------
    // Métodos auxiliares (opcionalmente usados desde UnityEvents)
    // ------------------------------
    
    /// <summary>
    /// Abre el closeup desde un evento externo (por ejemplo, inicio de diálogo).
    /// Se usa desde UnityEvents sin necesidad de pasar InteractionContext.
    /// </summary>
    public void EnterFromDialogue() => EnterCloseup();
    

    /// <summary>
    /// Cierra el closeup desde un evento externo (por ejemplo, fin de diálogo).
    /// </summary>
    public void ExitFromDialogue() => ExitCloseup();
    
}
