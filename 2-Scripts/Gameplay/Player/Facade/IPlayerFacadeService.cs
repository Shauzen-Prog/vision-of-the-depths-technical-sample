/// <summary>
/// Servicio global que expone el Player actual a cualquier sistema
/// (closeups, diálogos, etc.), sin acoplarse a escenas específicas.
/// </summary>
public interface IPlayerFacadeService
{
    /// <summary>
    /// Facade actual del jugador o null si todavía no existe.
    /// </summary>
    IPlayerFacade Current { get; }

    /// <summary>
    /// Registra el facade del jugador activo en el servicio.
    /// Normalmente lo llama el propio Player al inicializarse.
    /// </summary>
    /// <param name="facade">Instancia de facade a registrar.</param>
    void Register(IPlayerFacade facade);

    /// <summary>
    /// Desregistra el facade si coincide con el actual.
    /// Útil cuando el player se destruye o se recarga la escena.
    /// </summary>
    /// <param name="facade">Instancia de facade a quitar.</param>
    void Unregister(IPlayerFacade facade);
}
