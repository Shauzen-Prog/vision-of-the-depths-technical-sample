/// <summary>
/// Puerto de aplicación para controlar el estado del drill.
/// Publica eventos en el EventBus sin exponer detalles a la UI.
/// </summary>
public interface IDrillControlPort
{
    /// <summary>Pausa el drill manteniendo el estado actual (no cambia a Idle).</summary>
    void PauseDrill();
    
    /// <summary>Reanuda el drill (si es posible) y lo pasa a estado Drilling.</summary>
    void ResumeDrill();
    
    /// <summary>Apaga el drill por completo, mandándolo a Idle.</summary>
    void SetIdle();
}
