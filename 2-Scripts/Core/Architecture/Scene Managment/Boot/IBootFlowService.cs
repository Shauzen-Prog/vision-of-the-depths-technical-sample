/// <summary>
/// Define el contrato del flujo de arranque del juego.
/// Decide a que escena ir (calibracion/menu) segun el estado del sistema persistente
/// </summary>
public interface IBootFlowService
{
    /// <summary>
    /// Evalua el estado en la persistencia y enruta a la escena correspondiente
    /// </summary>
    void DecideAndRoute();
}
