using System;

/// <summary>
/// Servicio que orquesta la ejecución de diálogos lineales.
/// No conoce UI, cámaras ni Unity.
/// </summary>
public interface IDialogueService 
{
    event Action<DialogueContext> DialogueStarted;
    event Action<DialogueLine> LineChanged;
    event Action DialogueEnded;

    bool IsDialogueActive { get; }
    DialogueLine CurrentLine { get; }
    bool CanAdvance { get; }

    void StartDialogue(DialogueConversation conversation, DialogueContext context);
    void Advance();
    void Cancel();
}
