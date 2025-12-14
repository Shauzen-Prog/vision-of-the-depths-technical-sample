using System;
using System.Collections.Generic;

/// <summary>
/// Implementación de EventBus para eventos tipados, preparada para Zenject y test.
/// 100% desacoplada, no usa static. Sin singleton.
/// Permite escalabilidad futura (prioridades, filtros, logging).
/// </summary>
public class EventBus : IEventBus
{
    // Diccionario por tipo de evento: cada tipo tiene su lista de handlers
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    
    // Para logging/auditoría futuro (ejemplo de extensibilidad)
    public Action<Type, object> OnAnyEventPublished;
    
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
    {
        if (!_subscribers.TryGetValue(typeof(TEvent), out var handlers))
        {
            handlers = new List<Delegate>();
            _subscribers[typeof(TEvent)] = handlers;
        }
        handlers.Add(handler);

        // Retorna un IDisposable para desuscribirse fácilmente
        return new Subscription<TEvent>(this, handler);
    }

    public void Publish<TEvent>(TEvent eventData)
    {
        // Logging futuro
        OnAnyEventPublished?.Invoke(typeof(TEvent), eventData);

        if (!_subscribers.TryGetValue(typeof(TEvent), out var handlers)) 
            return;
        
        // Se clona la lista para evitar modificaciones durante la iteración
        foreach (var handler in handlers.ToArray())
        {
            if (handler is Action<TEvent> action)
            {
                action(eventData);
            }
        }
    }
    
    private void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        if (!_subscribers.TryGetValue(typeof(TEvent), out var handlers)) 
            return;
        
        handlers.Remove(handler);
        if (handlers.Count == 0)
            _subscribers.Remove(typeof(TEvent));
    }
    
    /// <summary>
    /// Clase interna para manejar la desuscripción automática.
    /// </summary>
    private class Subscription<TEvent> : IDisposable
    {
        private readonly EventBus _bus;
        private readonly Action<TEvent> _handler;
        private bool _disposed;

        public Subscription(EventBus bus, Action<TEvent> handler)
        {
            _bus = bus;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _bus.Unsubscribe(_handler);
            _disposed = true;
        }
    }
}
