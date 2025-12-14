using Zenject;

/// <summary>
/// Installer de Zenject para el sistema de interacción V2.
/// Bindea el puerto de input y el controlador de interacción.
/// </summary>
public class InteractionInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Puerto de input (lee del Input System vía MonoBehaviour).
        Container.Bind<IInteractionInputPort>()
            .To<InteractionInputAdapter>()
            .FromComponentInHierarchy()
            .AsSingle();

        // Servicio de aplicación principal.
        Container.Bind<IPlayerInteractionController>()
            .To<PlayerInteractionController>()
            .AsSingle();
        
    }
}