using System;

/// <summary>
/// Contexto asociado al inicio de un diálogo.
/// No contiene referencias a Unity, solo datos.
/// </summary>
public sealed class DialogueContext
{
    public string DialogueId { get; }
    public string PrimarySpeakerName { get; }
    public string TargetName { get; }

    public DialogueContext(string dialogueId, string primarySpeakerName = null, string targetName = null)
    {
        if (string.IsNullOrWhiteSpace(dialogueId))
            throw new ArgumentException("El ID del dialogo no puede ser nulo o estar vacio.", nameof(dialogueId));

        DialogueId = dialogueId;
        PrimarySpeakerName = primarySpeakerName;
        TargetName = targetName;
    }
}
