using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FSM jerárquica preparada para estados que ejecutan lógica propia (incluyendo coroutines desde sus implementaciones).
/// Orquesta transiciones y notifica cambios de estado.
/// </summary>
public class HierarchicalCoroutineStateMachine<TStateId> : IFiniteStateMachine<TStateId>
{
    private readonly IReadOnlyDictionary<TStateId, IState<TStateId>> _states;
    private readonly IEventBus _eventBus;

    private IState<TStateId> _currentState;
    private TStateId _currentStateId;

    /// <summary>Estado actual (id).</summary>
    public TStateId CurrentStateId => _currentStateId;

    /// <summary>Instancia del estado actual.</summary>
    public IState<TStateId> CurrentStateInstance => _currentState;

    /// <summary>
    /// Construye la FSM con su set de estados e inicializa en el estado indicado.
    /// </summary>
    public HierarchicalCoroutineStateMachine(
        IReadOnlyDictionary<TStateId, IState<TStateId>> states,
        TStateId initialState,
        IEventBus eventBus)
    {
        _states = states ?? throw new ArgumentNullException(nameof(states));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        foreach (var state in _states.Values)
        {
            if (state is IStateMachineAware<TStateId> aware)
            {
                aware.SetController(this);
            }
        }

        ChangeState(initialState);
    }

    /// <summary>
    /// Cambia al estado solicitado, ejecutando hooks de salida y entrada.
    /// Publica un evento de transición para feedback/debugging.
    /// </summary>
    public void ChangeState(TStateId newStateId)
    {
        if (!_states.TryGetValue(newStateId, out var nextState))
        
            throw new KeyNotFoundException($"State '{newStateId}' is not registered in the FSM.");
        }

        var previous = _currentStateId;
        _currentState?.OnExit();

        _currentState = nextState;
        _currentStateId = newStateId;

        _eventBus.Publish(new FsmStateChangedEvent<TStateId>(previous, newStateId));

        _currentState.OnEnter();
    }
}

/// <summary>
/// Permite que un estado reciba una referencia a su FSM sin acoplar todos los estados a ese detalle.
/// </summary>
public interface IStateMachineAware<TStateId>
{
    void SetController(IFiniteStateMachine<TStateId> controller);
}

/// <summary>
/// Evento genérico para observabilidad del FSM.
/// </summary>
public readonly struct FsmStateChangedEvent<TStateId>
{
    public readonly TStateId Previous;
    public readonly TStateId Current;

    public FsmStateChangedEvent(TStateId previous, TStateId current)
    {
        Previous = previous;
        Current = current;
    }
}
