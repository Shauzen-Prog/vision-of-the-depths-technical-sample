using System;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

/// <summary>
/// Trigger de diálogo compatible con Interaction V2.
/// Implementa IInteractionTarget para que el raycast del jugador lo detecte
/// y dispara diálogos usando IDialogueService + IDialogueParser.
/// No maneja UI, cámaras ni input; sólo gameplay.
/// Además coordina con PlayerControlService para bloquear movimiento/look/pause
/// mientras el diálogo está activo.
/// </summary>
[DisallowMultipleComponent]
public sealed class DialogueTriggerV2 : MonoBehaviour, IInteractionTarget
{
    [Header("Dialogue Data")] 
    [SerializeField] private TextAsset _dialogueJson;
    [SerializeField] private bool _playOnlyOnce = true;
    
    [SerializeField] private bool _allowWithoutDialogue = false;
    
    [Header("Camera")]
    [Tooltip("Si está activo, usa la cámara de diálogo (IDialogueCameraService).")]
    [SerializeField] private bool _useDialogueCamera = true;
    
    [Header("Interaction Visual (feedback opcional)")] 
    [SerializeField] private InteractionVisual _visual;

    [Header("Cámara (opcional)")]
    [Tooltip("Punto de foco cuando se usa una cámara especial para diálogos.")]
    [SerializeField] private Transform _cameraFocusTarget;
    
    [Header("Legacy Bridge (opcional)")]
    [Tooltip("Si hay un DialogueTrigger legacy en el mismo objeto, se usan sus UnityEvents en lugar de los de V2.")]
    [SerializeField] private DialogueTrigger _legacyTrigger;

    [Header("Events V2")] 
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    private IDialogueService _dialogueService;
    private IDialogueParser _dialogueParser;
    private IDialogueCameraService _dialogueCameraService;
    private IEventBus _eventBus;
    private IPlayerControlService _playerControl;

    private bool _hasPlayed;
    private bool _isDialogueRunning;
    private bool _isSubscribedToEnd;

    /// <summary>
    /// Indica si este target está habilitado actualmente.
    /// Si es false, el sistema lo ignora aunque esté en foco.
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            if (_dialogueJson ==null && !_allowWithoutDialogue)
                return false;

            if (_playOnlyOnce && _hasPlayed)
                return false;

            return true;
        }
    }

    [Inject]
    private void Construct(
        IDialogueService dialogueService,
        IDialogueParser dialogueParser,
        IDialogueCameraService dialogueCameraService,
        IEventBus eventBus,
        IPlayerControlService playerControl)
    {
        _dialogueService = dialogueService;
        _dialogueParser = dialogueParser;
        _dialogueCameraService = dialogueCameraService;
        _eventBus = eventBus;
        _playerControl = playerControl;
    }
    
    /// <summary>
    /// Determina si el jugador puede interactuar en este momento según el contexto.
    /// Respeta IsEnabled y evita re-entrar mientras un diálogo ya está corriendo.
    /// </summary>
    public bool CanInteract(InteractionContext context)
    {
        if (!IsEnabled)
            return false;

        if (_isDialogueRunning)
            return false;

        // Si más adelante hay condiciones extra (distancia, estado del player, flags),
        // se agregan acá usando el contexto.
        return true;
    }

    /// <summary>
    /// Interacción estándar vía sistema de interacción (E, botón de interact).
    /// </summary>
    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context))
            return;

        StartDialogueInternal();
    }

    /// <summary>
    /// Inicia el diálogo sin pasar por el sistema de interacción.
    /// Útil para triggers automáticos (OnTriggerEnter, cutscenes, etc.).
    /// Respeta IsEnabled / playOnlyOnce y evita re-entrar.
    /// </summary>
    public void StartDialogue()
    {
        if (!IsEnabled)
            return;

        if (_isDialogueRunning)
            return;

        StartDialogueInternal();
    }

    /// <summary>
    /// Lógica compartida para arrancar el diálogo tanto desde Interact()
    /// como desde StartDialogue().
    /// </summary>
    private void StartDialogueInternal()
    {
         if (_dialogueService == null || _dialogueParser == null)
         {
             Debug.LogError("[DialogueTriggerV2] Falta el servicio de dialogo o el parser. Verificar bindings de Zenject", this);
             return;
         }

         if (_dialogueJson == null)
         {
             if (_allowWithoutDialogue)
             {
                 Debug.Log("[DialogueTriggerV2] No tiene JSON pero _allowWithoutDialogue está activo. Ejecutando solo eventos.", this);
                 InvokeDialogueStartEvents();
                 InvokeDialogueEndEvents();
             }
             else
             {
                 Debug.LogWarning("[DialogueTriggerV2] Dialogo JSON no asignado.", this);
             }
             return;
         }

         string dialogueId = _dialogueJson.name;
         string json = _dialogueJson.text;

         DialogueConversation conversation;

         try
         {
             conversation = _dialogueParser.ParseFromJson(dialogueId, json);
         }
         catch (Exception ex)
         {
             Debug.LogError($"[DialogueTriggerV2] Falló en hacer el parser '{dialogueId}': {ex.Message}", this);
             return;
         }

         var dialogueContext = new DialogueContext(
             dialogueId: dialogueId,
             primarySpeakerName: null,
             targetName: null
         );

         if (!_isSubscribedToEnd)
         {
             _dialogueService.DialogueEnded += OnDialogueEnded;
             _isSubscribedToEnd = true;
         }

         _isDialogueRunning = true;
         _hasPlayed = true;

         // Gameplay primero (UnityEvents)
         InvokeDialogueStartEvents();

         // Bloquear movimiento / look / pausa mientras dure el diálogo
         _playerControl?.RequestBlock(
             new[]
             {
                 PlayerControlType.Movement,
                 PlayerControlType.Look,
                 PlayerControlType.Pause
             },
             this);

         // Avisar globalmente que arrancó un diálogo (closeup, switchers, etc.)
         _eventBus?.Publish(new DialogueStartedEvent());

         Debug.Log($"[DialogueTriggerV2] StartDialogue() para '{dialogueId}'");
         _dialogueService.StartDialogue(conversation, dialogueContext);

         // Feedback de cámara (opcional)
         if (_useDialogueCamera && _dialogueCameraService != null)
         {
             Debug.Log("[DialogueTriggerV2] EnterDialogueCamera()");
             _dialogueCameraService.EnterDialogueCamera(_cameraFocusTarget);
         }
    }

    private void OnDisable()
    {
        if (_isSubscribedToEnd && _dialogueService != null)
        {
            _dialogueService.DialogueEnded -= OnDialogueEnded;
            _isSubscribedToEnd = false;
        }

        // Si el trigger se desactiva en medio de un diálogo, liberamos cualquier bloqueo
        _playerControl?.ReleaseAll(this);
    }

    /// <summary>
    /// Se llama cuando el servicio notifica que el diálogo terminó.
    /// Limpia estado local, cámara y dispara el UnityEvent de fin.
    /// </summary>
    private void OnDialogueEnded()
    {
        _isDialogueRunning = false;

        if (_isSubscribedToEnd && _dialogueService != null)
        {
            _dialogueService.DialogueEnded -= OnDialogueEnded;
            _isSubscribedToEnd = false;
        }

        if (_useDialogueCamera && _dialogueCameraService != null)
        {
            _dialogueCameraService.ExitDialogueCamera();
        }

        // Desbloqueo de movement/look/pause para este owner
        _playerControl?.ReleaseAll(this);

        // Avisar globalmente que terminó el diálogo
        _eventBus?.Publish(new DialogueEndedEvent());

        InvokeDialogueEndEvents();
    }
    
    /// <summary>
    /// Callback pensado para el drill.  
    /// Se llama desde OnDialogueEnd (UnityEvent) y le avisa al controlador del closeup del drill que vuelva a modo simple.
    /// </summary>
    public void HandleDrillDialogueEnded(IDrillCloseupController drill)
    {
        if (drill == null)
        {
            Debug.LogWarning("[DialogueTriggerV2] Drill controller is null in HandleDrillDialogueEnded.");
            return;
        }

        drill.ExitDialogueMode();
    }

    /// <summary>
    /// Invoca los eventos de inicio de diálogo (V2 o legacy si lo tenés conectado).
    /// </summary>
    private void InvokeDialogueStartEvents()
    {
        onDialogueStart?.Invoke();
    }

    /// <summary>
    /// Invoca los eventos de fin de diálogo (V2).
    /// </summary>
    private void InvokeDialogueEndEvents()
    {
        onDialogueEnd?.Invoke();
    }
}
