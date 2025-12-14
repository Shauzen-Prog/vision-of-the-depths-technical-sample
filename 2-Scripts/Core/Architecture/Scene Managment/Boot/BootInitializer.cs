using UnityEngine;
using Zenject;

/// <summary>
/// Punto de entrada de Zenject que dispara el flujo de arranque cuando se resuelven los bindings
/// </summary>
public class BootInitializer : IInitializable
{
    private readonly IBootFlowService _boot;

    /// <summary>
    ///  Inyecta la implementacion del flujo de arranque
    /// </summary>
    public BootInitializer(IBootFlowService boot)
    {
        _boot = boot;
    }
    
    /// <summary>
    /// Ejecuta la decision y el enrutamiento al iniciar
    /// </summary>
    public void Initialize()
    {
        _boot.DecideAndRoute();
    }
}
