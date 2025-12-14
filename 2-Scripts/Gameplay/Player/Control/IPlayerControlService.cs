using System.Collections.Generic;

/// <summary>
/// Servicio centralizado que administra qué sistemas pueden controlar
/// el movimiento, la cámara, la pausa, etc. del jugador.
/// Permite que varios sistemas "pidan" bloquear controles sin pisarse.
/// </summary>
public interface IPlayerControlService 
{
    /// <summary>
    /// Indica si el tipo de control está actualmente habilitado
    /// (es decir, no hay ningún sistema bloqueándolo).
    /// </summary>
    /// <param name="type">Tipo de control a consultar.</param>
    /// <returns>True si el control está habilitado.</returns>
    bool IsEnabled(PlayerControlType type);

    /// <summary>
    /// Solicita bloquear un tipo de control para un owner específico.
    /// Mientras el owner no libere el bloqueo, el control seguirá deshabilitado.
    /// </summary>
    /// <param name="type">Tipo de control a bloquear.</param>
    /// <param name="owner">Objeto que solicita el bloqueo (generalmente this).</param>
    void RequestBlock(PlayerControlType type, object owner);

    /// <summary>
    /// Solicita bloquear varios tipos de control para un owner específico.
    /// </summary>
    /// <param name="types">Tipos de control a bloquear.</param>
    /// <param name="owner">Objeto que solicita el bloqueo.</param>
    void RequestBlock(IEnumerable<PlayerControlType> types, object owner);

    /// <summary>
    /// Libera el bloqueo de un tipo de control para un owner específico.
    /// Si otros owners siguen bloqueando, el control sigue deshabilitado.
    /// </summary>
    /// <param name="type">Tipo de control a liberar.</param>
    /// <param name="owner">Objeto que había solicitado el bloqueo.</param>
    void ReleaseBlock(PlayerControlType type, object owner);

    /// <summary>
    /// Libera todos los bloqueos registrados por un owner determinado.
    /// Útil cuando un sistema se destruye o sale de su estado.
    /// </summary>
    /// <param name="owner">Objeto que había solicitado bloqueos.</param>
    void ReleaseAll(object owner);

    /// <summary>
    /// Atajo conveniente para consultar si el movimiento está habilitado.
    /// </summary>
    bool CanMove { get; }

    /// <summary>
    /// Atajo conveniente para consultar si el look/cámara está habilitado.
    /// </summary>
    bool CanLook { get; }

    /// <summary>
    /// Atajo conveniente para saber si la interacción general está habilitada.
    /// </summary>
    bool CanInteract { get; }

    /// <summary>
    /// Atajo conveniente para saber si se permite abrir el menú de pausa.
    /// </summary>
    bool CanPause { get; }
}
