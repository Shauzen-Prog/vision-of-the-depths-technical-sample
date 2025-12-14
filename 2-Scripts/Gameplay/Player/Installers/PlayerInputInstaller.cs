using UnityEngine;
using Zenject;

/// <summary>
/// Installer para el input del Player. Bindea IInputPort al adaptador del Input System.
/// </summary>
public sealed class PlayerInputInstaller : MonoInstaller
{
    [Header("Configuración")]
    public InputConfigSO inputConfig;
    
    /// <summary>
    /// Bindings mínimos del input. AsSingle para una única instancia global.
    /// </summary>
    public override void InstallBindings()
    {
        // Un solo AsSingle para todos los contratos (interfaces + self)
        Container.BindInterfacesAndSelfTo<InputSystemAdapter>().AsSingle();
        
        // Config (SO)
        Container.Bind<InputConfigSO>().FromInstance(inputConfig).AsSingle();
        
        // Processor (Application layer)
        Container.BindInterfacesAndSelfTo<InputProcessor>().AsSingle();
        
        Container.Bind<IInteractionCancelInputPort>()
            .FromComponentInHierarchy()
            .AsSingle();
        
        // TODO: EN UN FUTURO TENGO QUE SACAR ESTO
        Container.Bind<PauseMenuController>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}
