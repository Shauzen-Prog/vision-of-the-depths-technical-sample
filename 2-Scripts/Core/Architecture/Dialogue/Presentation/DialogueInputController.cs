using UnityEngine;
using Zenject;

/// <summary>
/// Controlador que conecta el input de diálogos con el servicio de diálogos.
/// Mientras haya un diálogo activo, permite avanzar con la acción configurada.
/// </summary>
public class DialogueInputController : MonoBehaviour
{
    private IDialogueService _dialogueService;
    private IDialogueInputPort _inputPort;
    private IDialogueTypingController _typingController;

    [Inject]
    private void Construct(
        IDialogueService dialogueService,
        IDialogueInputPort inputPort,
        IDialogueTypingController typingController)
    {
        _dialogueService = dialogueService;
        _inputPort = inputPort;
        _typingController = typingController;
    }

    private void Update()
    {
        if (_dialogueService == null || _inputPort == null)
            return;

        if (!_dialogueService.IsDialogueActive)
            return;

        if (!_inputPort.IsAdvancePressedThisFrame)
            return;

        // 1) Si el typewriter está escribiendo, lo salteamos.
        if (_typingController != null && _typingController.IsTyping)
        {
            _typingController.SkipTyping();
            return;
        }

        // 2) Si ya terminó de escribir, avanzamos al siguiente diálogo.
        _dialogueService.Advance();
    }
}
