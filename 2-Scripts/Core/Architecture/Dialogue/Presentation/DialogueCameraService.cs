using UnityEngine;
using Cinemachine;

/// <summary>
/// Implementación de IDialogueCameraService usando una CinemachineVirtualCamera
/// dedicada a diálogos. Sube la prioridad de esa cámara y ajusta su LookAt.
/// </summary>
public sealed class DialogueCameraService : IDialogueCameraService
{
    private CinemachineVirtualCamera _dialogueCamera;
    private Transform _defaultLookAtTarget;

    private int _basePriority;
    private Transform _baseLookAt;
    private bool _isConfigured;
    
    /// <summary>
    /// Configura la cámara de diálogo y el target por defecto.
    /// Se llama una vez desde un binder en la escena main.
    /// </summary>
    public void ConfigureDialogueCamera(
        CinemachineVirtualCamera dialogueCamera,
        Transform defaultLookAtTarget)
    {
        _dialogueCamera = dialogueCamera;
        _defaultLookAtTarget = defaultLookAtTarget;

        if (_dialogueCamera != null)
        {
            _basePriority = _dialogueCamera.Priority;
            _baseLookAt = _dialogueCamera.LookAt;
            _isConfigured = true;
        }
        else
        {
            _isConfigured = false;
        }
    }

    /// <inheritdoc />
    public void EnterDialogueCamera(Transform focusTarget)
    {
        if (!_isConfigured || _dialogueCamera == null)
        {
            Debug.LogWarning("[DialogueCameraService] Dialogue camera not configured.");
            return;
        }

        // Target de mirada (explícito o default)
        Transform lookAt = focusTarget != null ? focusTarget : _defaultLookAtTarget;
        if (lookAt == null)
        {
            // Si no hay default, se vuelve al LookAt original
            lookAt = _baseLookAt;
        }

        _dialogueCamera.LookAt = lookAt;

        // Sube prioridad para ganar sobre la cámara de gameplay
        _dialogueCamera.Priority = _basePriority + 20;
    }

    /// <inheritdoc />
    public void ExitDialogueCamera()
    {
        if (!_isConfigured || _dialogueCamera == null)
            return;

        _dialogueCamera.Priority = _basePriority;
        _dialogueCamera.LookAt = _baseLookAt;
       
    }
}
