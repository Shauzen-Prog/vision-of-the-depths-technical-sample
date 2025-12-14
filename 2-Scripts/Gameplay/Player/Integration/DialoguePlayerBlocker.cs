using UnityEngine;
using Zenject;

public sealed class DialoguePlayerBlocker : MonoBehaviour
{
    [Header("Componentes de control a desactivar durante diálogos")]
    [SerializeField] private MonoBehaviour _movementComponent;
    [SerializeField] private MonoBehaviour _lookComponent;
    [SerializeField] private MonoBehaviour _interactionInputComponent;

    private IDialogueService _dialogueService;

    [Inject]
    private void Construct(IDialogueService dialogueService)
    {
        _dialogueService = dialogueService;
    }

    private void OnEnable()
    {
        if (_dialogueService == null)
            return;

        _dialogueService.DialogueStarted += OnDialogueStarted;
        _dialogueService.DialogueEnded += OnDialogueEnded;
    }

    private void OnDisable()
    {
        if (_dialogueService == null)
            return;

        _dialogueService.DialogueStarted -= OnDialogueStarted;
        _dialogueService.DialogueEnded -= OnDialogueEnded;
    }

    private void OnDialogueStarted(DialogueContext context)
    {
        SetControlsEnabled(false);
    }

    private void OnDialogueEnded()
    {
        SetControlsEnabled(true);
    }

    private void SetControlsEnabled(bool enabled)
    {
        if (_movementComponent != null)
            _movementComponent.enabled = enabled;

        if (_lookComponent != null)
            _lookComponent.enabled = enabled;

        if (_interactionInputComponent != null)
            _interactionInputComponent.enabled = enabled;
    }
}
