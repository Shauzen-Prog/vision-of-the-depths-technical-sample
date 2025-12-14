using System;
using UnityEngine;

/// <summary>
/// Servicio de aplicación que coordina la interacción del jugador
/// con el mundo: foco actual + ejecución de Interact().
/// </summary>
public interface IPlayerInteractionController 
{
    /// <summary>
    /// Target actualmente bajo foco del jugador (puede ser null).
    /// </summary>
    IInteractionTarget CurrentTarget { get; }

    /// <summary>
    /// Evento disparado cuando cambia el target en foco.
    /// Útil para UI (prompts, iconos, etc.).
    /// </summary>
    event Action<IInteractionTarget> OnCurrentTargetChanged;

    /// <summary>
    /// Asigna el target actual detectado por la capa de presentación.
    /// </summary>
    /// <param name="target">Nuevo target o null.</param>
    void SetCurrentTarget(IInteractionTarget target);

    /// <summary>
    /// Avanza la lógica de interacción para este frame:
    /// lee input y dispara Interact() si corresponde.
    /// </summary>
    /// <param name="context">Contexto de interacción actual.</param>
    void Tick(InteractionContext context);
}
