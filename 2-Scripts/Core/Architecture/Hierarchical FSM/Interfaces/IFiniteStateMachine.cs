/// <summary>
/// Contrato para una FSM genérica desacoplada.
/// No depende de Update. Solo cambia estado bajo demanda.
/// </summary>
public interface IFiniteStateMachine<TStateId>
{
    TStateId CurrentStateId { get; }
    void ChangeState(TStateId newStateId);
}
