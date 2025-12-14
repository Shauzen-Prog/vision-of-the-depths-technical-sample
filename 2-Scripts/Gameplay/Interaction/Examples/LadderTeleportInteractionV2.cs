using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

//================================================================================
/// <summary>
/// Ejemplo de interacción de mundo que utiliza el sistema de interacción V2.
/// Demuestra integración con PlayerFacade y feedback desacoplado (EyeBlink),
/// sin acoplar gameplay a controladores concretos.
/// </summary>
//================================================================================

/// <summary>
/// Interacción de teletransporte para escaleras/pasajes usando el sistema
/// de interacción V2. No usa UnityEvents ni depende de controladores
/// específicos: sólo se apoya en IPlayerFacade del InteractionContext.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class LadderTeleportInteractionV2 : MonoBehaviour, IInteractionTarget
{
    [Header("General")]
    [SerializeField] public bool _isEnabled = true;

    [Tooltip("Si está activo, sólo se puede usar una vez.")]
    [SerializeField] private bool _oneShot = false;

    [Header("Destino de teletransporte")]
    [Tooltip("Punto al que se va a teletransportar el jugador.")]
    [SerializeField] private Transform _destination;

    [Tooltip("Si está activo, sólo se copia el yaw (rotación en Y) del destino.")]
    [SerializeField] private bool _alignYawOnly = false;

    [Header("Timing")]
    [Tooltip("Delay antes de aplicar el teleport (segundos). Útil si después querés meter un blink/cut.")]
    [SerializeField] private float _preTeleportDelay = 0.1f;

    [Tooltip("Tiempo después del teleport durante el cual se ignoran nuevas interacciones.")]
    [SerializeField] private float _cooldownAfterTeleport = 0.25f;

    [SerializeField] private bool TpFromNowere;

    private bool _hasInteracted;
    private bool _isTeleporting;
    
    private IEventBus _eventBus;
    private IEyeBlinkService _eyes;
    
    [Inject]
    private void Construct(IEventBus eventBus, IEyeBlinkService eyes)
    {
        _eventBus = eventBus;
        _eyes = eyes;
    }
    
    /// <summary>
    /// Indica si esta interacción está disponible actualmente.
    /// Respeta enable, one-shot y si hay un teleport en curso.
    /// </summary>
    public bool IsEnabled => _isEnabled && !_isTeleporting && (!_oneShot || !_hasInteracted);
    
    /// <summary>
    /// Determina si se puede interactuar con esta escalera según el contexto.
    /// </summary>
    public bool CanInteract(InteractionContext context)
    {
        if (!IsEnabled)
            return false;

        if (_destination == null)
            return false;

        // Se espera que el contexto traiga un PlayerFacade válido. 
        if (context.Player == null)
            return false;

        // Si más adelante querés agregar chequeo de distancia, estado del jugador, etc.,
        // este es el lugar.
        return true;
    }

    /// <summary>
    /// Ejecuta la lógica de teletransporte usando el PlayerFacade del contexto.
    /// </summary>
    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context))
            return;

        if (_isTeleporting)
            return;
        
        _ = TeleportAsync(context.Player);
    }
    
    /// <summary>
    /// Ejecuta el teleport con transición de ojos (close -> teleport -> open).
    /// Evita UnityEvents y mantiene el gameplay desacoplado del Animator.
    /// </summary>
    private async Task TeleportAsync(IPlayerFacade player)
    {
        _isTeleporting = true;

        try
        {
            if (_preTeleportDelay > 0f)
                await Task.Delay(Mathf.RoundToInt(_preTeleportDelay * 1000f));

            // Calculamos rotación objetivo según config (misma lógica que tenías)
            Quaternion rawRotation = _destination.rotation;
            Quaternion targetRotation;

            if (_alignYawOnly)
            {
                Vector3 euler = rawRotation.eulerAngles;
                targetRotation = Quaternion.Euler(0f, euler.y, 0f);
            }
            else
            {
                targetRotation = rawRotation;
            }

            Vector3 targetPosition = _destination.position;

            // Ojos: cerrar -> acción en negro -> abrir
            if (_eyes != null)
            {
                await _eyes.TransitionAsync(async () =>
                {
                    player.TeleportTo(targetPosition, targetRotation, alignYawOnly: false);
                    _eventBus?.Publish(new PlayerTeleportedEvent(targetPosition, targetRotation));
                    await Task.Delay(1);
                });
            }
            else
            {
                // Fallback: si por alguna razón no hay presenter en main scene
                player.TeleportTo(targetPosition, targetRotation, alignYawOnly: false);
                _eventBus?.Publish(new PlayerTeleportedEvent(targetPosition, targetRotation));
            }

            _hasInteracted = true;

            if (_cooldownAfterTeleport > 0f)
                await Task.Delay(Mathf.RoundToInt(_cooldownAfterTeleport * 1000f));
        }
        finally
        {
            _isTeleporting = false;
        }
    }

    /// <summary>
    /// Se asegura de que el collider sea trigger para que no bloquee físicamente al jugador
    /// (el raycast del detector igual lo va a ver).
    /// </summary>
    private void OnValidate()
    {
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }
}
