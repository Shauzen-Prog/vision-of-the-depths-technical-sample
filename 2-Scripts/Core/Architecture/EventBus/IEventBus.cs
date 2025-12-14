using System;

/// <summary>
/// Interfaz base para un bus de eventos desacoplado y tipado.
/// Permite suscripción, desuscripción y publicación de eventos con payload.
/// Pensada para ser inyectada y mockeada.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Suscribe un handler a un evento de tipo TEvent.
    /// </summary>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler);

    /// <summary>
    /// Publica un evento de tipo TEvent a todos los suscriptores.
    /// </summary>
    void Publish<TEvent>(TEvent eventData);
}
