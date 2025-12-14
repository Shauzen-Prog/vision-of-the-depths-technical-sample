using System;

/// <summary>
/// Implementación por defecto del controlador de interacción del jugador.
/// No conoce nada de Unity (cámara, raycasts, etc.): sólo targets e input.
/// </summary>
public class PlayerInteractionController : IPlayerInteractionController
{
    private readonly IInteractionInputPort _interactInput;
    private readonly IInteractionCancelInputPort _cancelInput;
    private readonly IEventBus _eventBus;
    
    private IInteractionTarget _currentTarget;

    public IInteractionTarget CurrentTarget => _currentTarget;

    public event Action<IInteractionTarget> OnCurrentTargetChanged;
    public event Action<IInteractionTarget> OnInteractExecuted;
    
    
    public PlayerInteractionController(IInteractionInputPort interactInput, 
        IInteractionCancelInputPort cancelInput, IEventBus eventBus)
    {
        _interactInput = interactInput;
        _cancelInput = cancelInput;
        _eventBus = eventBus;
    }

    public void SetCurrentTarget(IInteractionTarget target)
    {
        if (_currentTarget == target)
            return;

        _currentTarget = target;
        OnCurrentTargetChanged?.Invoke(_currentTarget);

        if (_currentTarget != null)
            UnityEngine.Debug.Log($"[Controller] CurrentTarget set to: {_currentTarget}", _currentTarget as UnityEngine.Object);
        else
            UnityEngine.Debug.Log("[Controller] CurrentTarget cleared (null)");
    }

    public void Tick(InteractionContext context)
    {
        HandleInteract(context);
        HandleCancel();
    }

    private void HandleInteract(InteractionContext context)
    {
        if(!_interactInput.IsInteractPressedThisFrame)
            return;

        if (_currentTarget == null)
        {
            UnityEngine.Debug.Log("[Controller] Interact pressed but CurrentTarget is null");
            return;
        }

        if (!_currentTarget.IsEnabled)
        {
            UnityEngine.Debug.Log("[Controller] Interact pressed but target IsEnabled == false");
            return;
        }

        if (!_currentTarget.CanInteract(context))
        {
            UnityEngine.Debug.Log("[Controller] Interact pressed but CanInteract(context) == false");
            return;
        }

        UnityEngine.Debug.Log("[Controller] Executing Interact() on target");
        _currentTarget.Interact(context);
        OnInteractExecuted?.Invoke(_currentTarget);
    }

    private void HandleCancel()
    {
        if (!_cancelInput.WasCancelPressedThisFrame)
            return;

        if (_currentTarget is ICancelableInteraction cancelable && cancelable.CanCancel())
        {
            UnityEngine.Debug.Log("[Controller] Cancel pressed → CancelInteraction()");
            cancelable.Cancel();
            // Avisar al sistema de que este frame el cancel fue consumido
            _eventBus?.Publish(new InteractionCancelConsumedEvent());
        }
    }
}
