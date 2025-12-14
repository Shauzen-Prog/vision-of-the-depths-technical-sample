using Cinemachine;

/// <summary>
/// Implementación simple de IPlayerCameraProvider.
/// Guarda una referencia a la cámara del jugador y la expone al resto del sistema.
/// </summary>
public sealed class PlayerCameraProvider : IPlayerCameraProvider
{
    public CinemachineVirtualCamera PlayerCamera { get; private set; }

    public void SetPlayerCamera(CinemachineVirtualCamera camera)
    {
        PlayerCamera = camera;
    }
}
