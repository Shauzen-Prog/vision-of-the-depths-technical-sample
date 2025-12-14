using UnityEngine;
using TMPro;
using Zenject;
using Febucci.UI;

/// <summary>
/// Vista de diálogo: se encarga de mostrar el texto y el nombre del hablante
/// usando TextMeshPro y Text Animator. No contiene lógica de gameplay.
/// </summary>
public sealed class DialogueUIView : MonoBehaviour
{
    [Header("Panel Raiz")]
    [SerializeField] private GameObject _rootPanel;
     
    [Header("Nombre del Speaker y texto")]
    [SerializeField] private TextMeshProUGUI _textLabel;
    [SerializeField] private TextMeshProUGUI _speakerLabel;

    [Header("Text Animator (Febucci)")]
    [SerializeField] private TextAnimator_TMP _textAnimator;
    [SerializeField] private TypewriterByCharacter _typewriter;
    
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
        _dialogueService.LineChanged += OnLineChanged;
        _dialogueService.DialogueEnded += OnDialogueEnded;

        if (_rootPanel != null)
        {
            _rootPanel.SetActive(false);
        }
        
        ClearTextAndSpeaker();
    }

    private void OnDisable()
    {
        if (_dialogueService == null)
            return;

        _dialogueService.DialogueStarted -= OnDialogueStarted;
        _dialogueService.LineChanged -= OnLineChanged;
        _dialogueService.DialogueEnded -= OnDialogueEnded;
    }

    /// <summary>
    /// Se llama cuando comienza cualquier diálogo.
    /// Muestra el panel principal.
    /// </summary>
    private void OnDialogueStarted(DialogueContext context)
    {
        if (_rootPanel != null)
        {
            _rootPanel.SetActive(true);
        }

        ClearTextAndSpeaker();
    }

    /// <summary>
    /// Se llama cada vez que cambia la línea actual del diálogo.
    /// Actualiza nombre del hablante y texto animado.
    /// </summary>
    private void OnLineChanged(DialogueLine line)
    {
        if (line == null)
            return;

        // Nombre
        if (_speakerLabel != null)
        {
            if (line.IsNarratorLine)
                _speakerLabel.text = string.Empty;
            else
                _speakerLabel.text = line.SpeakerName;
        }

        // Texto + Typewriter
        if (_typewriter != null)
        {
            _typewriter.ShowText(line.Text);
        }
        else if (_textLabel != null)
        {
            _textLabel.text = line.Text;
        }
    }

    /// <summary>
    /// Se llama cuando el diálogo termina.
    /// Oculta el panel principal.
    /// </summary>
    private void OnDialogueEnded()
    {
        
        if (_rootPanel != null)
        {
            _rootPanel.SetActive(false);
        }

        ClearTextAndSpeaker();
    }
    
    /// <summary>
    /// Limpia texto y nombre del hablante.
    /// </summary>
    private void ClearTextAndSpeaker()
    {
        if (_typewriter != null)
            _typewriter.ShowText(string.Empty);
        else if (_textLabel != null)
            _textLabel.text = string.Empty;

        if (_speakerLabel != null)
            _speakerLabel.text = string.Empty;
    }
}
