using System;
using UnityEngine;
using Zenject;

/// <summary>
/// Desactiva movimiento, look e input de interacción mientras un closeup
/// esté activo. Se basa en eventos del EventBus (CloseupStarted/Ended).
/// </summary>
public sealed class CloseupPlayerBlocker : MonoBehaviour
{
    [Header("Componentes a bloquear durante closeup")]
    [SerializeField] private MonoBehaviour _movementComponent;       // PlayerPresenterCC
    [SerializeField] private MonoBehaviour _lookComponent;           // PlayerLookController
    [SerializeField] private MonoBehaviour _interactionInputComponent; // InteractionInputAdapter

    private IEventBus _eventBus;

    [Inject]
    private void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private IDisposable _startedSub;
    private IDisposable _endedSub;

    private void OnEnable()
    {
        if (_eventBus == null)
            return;

        _startedSub = _eventBus.Subscribe<CloseupStartedEvent>(_ => SetControls(false));
        _endedSub   = _eventBus.Subscribe<CloseupEndedEvent>(_   => SetControls(true));
    }

    private void OnDisable()
    {
        _startedSub?.Dispose();
        _endedSub?.Dispose();
        _startedSub = null;
        _endedSub = null;
    }

    private void SetControls(bool enabled)
    {
        if (_movementComponent != null)
            _movementComponent.enabled = enabled;

        if (_lookComponent != null)
            _lookComponent.enabled = enabled;

        if (_interactionInputComponent != null)
            _interactionInputComponent.enabled = enabled;
    }
}
