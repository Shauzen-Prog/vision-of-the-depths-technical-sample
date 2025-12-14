/// <summary>
/// Interacciones continuas (closeup, diálogo, etc.) que pueden
/// cancelarse con input (Escape, B/O).
/// </summary>
public interface ICancelableInteraction
{
    /// <summary>
    /// Indica si tiene sentido cancelar en este momento.
    /// </summary>
    bool CanCancel();

    /// <summary>
    /// Ejecuta la lógica de cancelación (salir de closeup, cerrar diálogo, etc.).
    /// </summary>
    void Cancel();
}
