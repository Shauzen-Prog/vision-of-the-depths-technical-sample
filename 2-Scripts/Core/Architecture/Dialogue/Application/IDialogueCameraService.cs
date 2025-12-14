using Cinemachine;
using UnityEngine;

/// <summary>
/// Servicio que maneja el modo de cámara durante diálogos.
/// Permite entrar y salir de una cámara de conversación, apuntando a un target.
/// </summary>
public interface IDialogueCameraService
{
    public void ConfigureDialogueCamera(
        CinemachineVirtualCamera dialogueCamera,
        Transform defaultLookAtTarget);
    
    /// <summary>
    /// Activa la cámara de diálogo mirando al target indicado.
    /// Si el target es null, usa un target por defecto.
    /// </summary>
    /// <param name="focusTarget">Transform de la cabeza del NPC o punto de foco.</param>
    void EnterDialogueCamera(Transform focusTarget);

    /// <summary>
    /// Restaura la cámara normal de gameplay.
    /// </summary>
    void ExitDialogueCamera();
}
