using UnityEngine;
using Zenject;

public class PlayerRuntimeInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Busca un PlayerLookController en la jerarquía de la escena y lo bindea como único.
        Container.Bind<PlayerLookController>()
            .FromComponentInHierarchy()
            .AsSingle();

        // Asegura que el tick se registre (si aún no lo bindearon en otro installer)
        Container.BindInterfacesAndSelfTo<PlayerMovementTick>().AsSingle();
        
        Container.Bind<PlayerPresenterCC>().FromComponentInHierarchy().AsSingle();
        
        Container.Bind<IStairClimber>()
            .To<CharacterControllerStairClimber>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}
