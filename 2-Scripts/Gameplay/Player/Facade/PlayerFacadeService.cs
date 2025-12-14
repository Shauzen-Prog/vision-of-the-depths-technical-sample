/// <summary>
/// Implementación por defecto del servicio global de Player.
/// Guarda una referencia al IPlayerFacade activo y la expone
/// al resto del sistema.
/// </summary>
public class PlayerFacadeService : IPlayerFacadeService
{
    /// <summary>
    /// Facade actual del jugador (puede ser null si todavía no se registró).
    /// </summary>
    public IPlayerFacade Current { get; private set; }

    /// <summary>
    /// Registra el facade del jugador activo.
    /// Si ya había uno registrado, se sobreescribe.
    /// </summary>
    /// <param name="facade">Instancia de IPlayerFacade a registrar.</param>
    public void Register(IPlayerFacade facade)
    {
        Current = facade;
    }

    /// <summary>
    /// Desregistra el facade sólo si coincide con el actual.
    /// </summary>
    /// <param name="facade">Instancia a desregistrar.</param>
    public void Unregister(IPlayerFacade facade)
    {
        if (Current == facade)
        {
            Current = null;
        }
    }
}
