using System;

/// <summary>
/// Modela el estado de ejecución de un diálogo lineal.
/// Maneja índice actual, activo y finalizado.
/// </summary>
public sealed class DialogueState
{
    private DialogueConversation _conversation;
    private int _currentIndex;

    public bool IsActive { get; private set; }
    public bool IsFinished { get; private set; }

    /// <summary>
    /// Línea actual del diálogo o null si no hay diálogo activo.
    /// </summary>
    public DialogueLine CurrentLine
    {
        get
        {
            if (!IsActive || _conversation == null) return null;
            if (_currentIndex < 0 || _currentIndex >= _conversation.Lines.Count) return null;
            return _conversation.Lines[_currentIndex];
        }
    }

    /// <summary>
    /// Inicializa el estado para una nueva conversación.
    /// </summary>
    public void Start(DialogueConversation conversation)
    {
        _conversation = conversation ?? throw new ArgumentNullException(nameof(conversation));
        _currentIndex = 0;
        IsActive = true;
        IsFinished = false;
    }

    /// <summary>
    /// Avanza a la siguiente línea si existe.
    /// </summary>
    public bool TryAdvance(out DialogueLine nextLine)
    {
        nextLine = null;

        if (!IsActive || _conversation == null || IsFinished)
            return false;

        int lastIndex = _conversation.Lines.Count - 1;

        if (_currentIndex >= lastIndex)
        {
            IsFinished = true;
            IsActive = false;
            return false;
        }

        _currentIndex++;
        nextLine = _conversation.Lines[_currentIndex];
        return true;
    }
}
