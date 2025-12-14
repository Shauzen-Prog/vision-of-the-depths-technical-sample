using UnityEngine;

/// <summary>
/// Contexto mínimo que se pasa a los interactuables para que puedan
/// tomar decisiones sin depender directamente de componentes concretos.
/// </summary>
public struct InteractionContext
{
    /// <summary>
    /// Facade del jugador. Sirve para teleports, bloqueo de controles, etc.
    /// Se usa la interfaz para facilitar mocks en tests.
    /// </summary>
    public IPlayerFacade Player { get; }

    /// <summary>
    /// Posición en mundo desde donde se originó la interacción
    /// (por ejemplo, la posición de la cámara del jugador).
    /// </summary>
    public Vector3 OriginWorldPosition { get; }

    /// <summary>
    /// Crea un nuevo contexto de interacción con los datos relevantes del jugador.
    /// </summary>
    /// <param name="player">Facade del jugador.</param>
    /// <param name="originWorldPosition">Posición de origen en mundo.</param>
    public InteractionContext(IPlayerFacade player, Vector3 originWorldPosition)
    {
        Player = player;
        OriginWorldPosition = originWorldPosition;
    }
}
