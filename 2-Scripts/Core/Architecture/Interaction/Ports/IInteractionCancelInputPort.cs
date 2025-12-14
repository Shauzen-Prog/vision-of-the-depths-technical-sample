/// <summary>
/// Puerto de entrada para cancelar la interacción actual (Escape, B/O, etc.).
/// </summary>
public interface IInteractionCancelInputPort
{
    /// <summary>
    /// True sólo en el frame en que se presionó el input de cancelar.
    /// </summary>
    bool WasCancelPressedThisFrame { get; }
}
