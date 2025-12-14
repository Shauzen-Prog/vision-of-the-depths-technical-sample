using Cinemachine;
using UnityEngine;
using Zenject;

/// <summary>
/// Registra la cámara de diálogo real en el servicio global.
/// Vive en la escena main junto a la CinemachineVirtualCamera.
/// </summary>
public sealed class DialogueCameraBinder : MonoBehaviour
{
    [Header("Dialogue Camera")]
    [SerializeField] private CinemachineVirtualCamera _dialogueCamera;

    [Header("Default Look At (opcional)")]
    [SerializeField] private Transform _defaultLookAtTarget;

    private IDialogueCameraService _dialogueCameraService;

    [Inject]
    private void Construct(IDialogueCameraService dialogueCameraService)
    {
        _dialogueCameraService = dialogueCameraService;
    }

    private void Awake()
    {
        if (_dialogueCamera == null)
            _dialogueCamera = GetComponent<CinemachineVirtualCamera>();

        if (_dialogueCamera == null)
        {
            Debug.LogWarning("[DialogueCameraBinder] No virtual camera assigned.", this);
            return;
        }

        _dialogueCameraService.ConfigureDialogueCamera(_dialogueCamera, _defaultLookAtTarget);
    }
}
