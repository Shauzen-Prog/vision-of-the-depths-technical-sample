using System;

/// <summary>
/// Implementación por defecto de IDialogueService.
/// Mantiene estado y líneas.
/// No usa MonoBehaviour.
/// </summary>
public class DialogueService : IDialogueService
{
    private readonly DialogueState _state = new DialogueState();
    private DialogueConversation _currentConversation;
    private DialogueContext _currentContext;

    public event Action<DialogueContext> DialogueStarted;
    public event Action<DialogueLine> LineChanged;
    public event Action DialogueEnded;

    public bool IsDialogueActive => _state.IsActive;
    public DialogueLine CurrentLine => _state.CurrentLine;

    public bool CanAdvance
    {
        get
        {
            if (!IsDialogueActive || _currentConversation == null) return false;
            return !_state.IsFinished;
        }
    }

    public void StartDialogue(DialogueConversation conversation, DialogueContext context)
    {
        if (conversation == null) throw new ArgumentNullException(nameof(conversation));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (IsDialogueActive) throw new InvalidOperationException("Ya hay un dialogo activo!.");

        _currentConversation = conversation;
        _currentContext = context;

        _state.Start(conversation);

        DialogueStarted?.Invoke(_currentContext);

        if (_state.CurrentLine != null)
            LineChanged?.Invoke(_state.CurrentLine);
    }

    public void Advance()
    {
        if (!IsDialogueActive || _currentConversation == null)
            return;

        if (!_state.TryAdvance(out var nextLine))
        {
            FinalizeDialogue();
            return;
        }

        LineChanged?.Invoke(nextLine);
    }

    public void Cancel()
    {
        if (!IsDialogueActive)
            return;

        FinalizeDialogue();
    }

    private void FinalizeDialogue()
    {
        // marcar diálogo terminado
        _state.TryAdvance(out _); // Garantiza IsActive = false

        _currentConversation = null;
        _currentContext = null;

        DialogueEnded?.Invoke();
    }
}
