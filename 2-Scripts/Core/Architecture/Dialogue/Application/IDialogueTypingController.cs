/// <summary>
/// Controlador de estado de tipeo del diálogo.
/// Permite saber si el texto se está escribiendo y saltearlo.
/// No conoce detalles de implementación (Text Animator, etc.).
/// </summary>
public interface IDialogueTypingController
{
    /// <summary>
    /// Indica si actualmente el diálogo está escribiendo letras (typewriter en progreso).
    /// </summary>
    bool IsTyping { get; }

    /// <summary>
    /// Fuerza a que el texto actual se complete/termine de escribir inmediatamente.
    /// </summary>
    void SkipTyping();
}
