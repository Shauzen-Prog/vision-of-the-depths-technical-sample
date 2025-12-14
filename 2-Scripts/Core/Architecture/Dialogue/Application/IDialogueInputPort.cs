/// <summary>
/// Puerto de entrada para el input de diálogos.
/// Permite abstraer el origen del input (teclado, gamepad, etc.).
/// </summary>
public interface IDialogueInputPort 
{
    /// <summary>
    /// Indica si en este frame se presionó la acción de "avanzar diálogo".
    /// </summary>
    bool IsAdvancePressedThisFrame { get; }
}
