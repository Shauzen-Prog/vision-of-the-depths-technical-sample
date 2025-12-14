/// <summary>
/// Contrato genérico para cualquier objeto con el que el jugador
/// pueda interactuar mediante el sistema de interacción V2.
/// </summary>
public interface IInteractionTarget 
{
    /// <summary>
    /// Indica si este target está habilitado actualmente.
    /// Si es false, el sistema lo ignora aunque esté en foco.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Determina si el jugador puede interactuar en este momento
    /// según el contexto entregado (estado del jugador, posición, etc.).
    /// </summary>
    /// <param name="context">Contexto actual de interacción.</param>
    /// <returns>True si la interacción está permitida.</returns>
    bool CanInteract(InteractionContext context);

    /// <summary>
    /// Ejecuta la lógica principal de la interacción.
    /// No debería leer input ni hacer raycasts; eso lo maneja la capa de aplicación.
    /// </summary>
    /// <param name="context">Contexto de la interacción (jugador, origen, etc.).</param>
    void Interact(InteractionContext context);
}
