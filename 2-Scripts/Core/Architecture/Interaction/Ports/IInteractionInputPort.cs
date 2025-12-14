/// <summary>
/// Puerto de entrada para leer el comando de interacción del jugador
/// desde la capa de infraestructura (Input System, gamepad, etc.).
/// </summary>
public interface  IInteractionInputPort 
{
    /// <summary>
    /// Indica si el botón de interact se presionó en este frame.
    /// Debe devolver true una sola vez por pulsación.
    /// </summary>
    bool IsInteractPressedThisFrame { get; }
}
