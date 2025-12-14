/// <summary>
/// Representa una línea individual de diálogo.
/// Puede contener tags de Text Animator (se procesan en la capa UI).
/// </summary>
public sealed class DialogueLine 
{
    /// <summary>
    /// Nombre visible del hablante. Puede ser vacío para narrador.
    /// </summary>
    public string SpeakerName { get; }

    /// <summary>
    /// Texto de la línea, incluyendo tags de Text Animator.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Indica si la línea corresponde a un narrador (sin nombre).
    /// </summary>
    public bool IsNarratorLine => string.IsNullOrWhiteSpace(SpeakerName);

    public DialogueLine(string speakerName, string text)
    {
        SpeakerName = speakerName ?? string.Empty;
        Text = text ?? string.Empty;
    }
}
