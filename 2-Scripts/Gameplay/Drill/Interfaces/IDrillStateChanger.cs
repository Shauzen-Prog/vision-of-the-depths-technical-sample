/// <summary>
/// Contrato mínimo para permitir que los estados soliciten cambios de estado
/// sin acoplarse al MonoBehaviour concreto.
/// </summary>
public interface IDrillStateChanger
{
    /// <summary>Solicita el cambio de estado del drill.</summary>
    void ChangeState(DrillStateId newState);

    /// <summary>Helper para volver a Idle.</summary>
    void ChangeToIdle();
}
