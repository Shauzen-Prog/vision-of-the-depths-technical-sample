using Cinemachine;
using UnityEngine;
using Zenject;

/// <summary>
/// Se encarga de registrar la cámara principal del jugador en el PlayerCameraProvider.
/// De esta forma, otras escenas pueden acceder a ella sin referencias cruzadas.
/// </summary>
public class PlayerCameraBinder : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _playerCamera;

    [Inject]
    private void Construct(IPlayerCameraProvider cameraProvider)
    {
        cameraProvider.SetPlayerCamera(_playerCamera);
    }
}
