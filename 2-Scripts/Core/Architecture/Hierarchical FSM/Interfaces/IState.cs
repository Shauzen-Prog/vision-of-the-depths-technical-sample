/// <summary>
/// Representa un estado desacoplado, con ciclo de vida OnEnter/OnExit. Puede manejar corutinas internas.
/// </summary
public interface IState<TStateId>
{
    /// <summary>Llamado cuando el estado inicia. Puede arrancar corutinas.</summary>
    void OnEnter();

    /// <summary>Llamado cuando el estado termina. Debe limpiar corutinas.</summary>
    void OnExit();

    /// <summary>Permite inyectar la FSM para cambios de estado.</summary>
    void SetController(IFiniteStateMachine<TStateId> fsm);

    /// <summary>Permite inyectar el EventBus global.</summary>
    void InjectEventBus(IEventBus eventBus);
}
