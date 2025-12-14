using Cinemachine;

/// <summary>
/// Expone la cámara principal del jugador para cualquier sistema que la necesite,
/// evitando referencias cruzadas entre escenas.
/// </summary>
public interface IPlayerCameraProvider
{
    /// <summary>
    /// Cámara virtual principal del jugador (puede ser null si aún no fue inicializada).
    /// </summary>
    CinemachineVirtualCamera PlayerCamera { get; }

    /// <summary>
    /// Permite registrar o cambiar la cámara principal del jugador en runtime.
    /// </summary>
    /// <param name="camera">Cámara virtual a registrar.</param>
    void SetPlayerCamera(CinemachineVirtualCamera camera);
}
