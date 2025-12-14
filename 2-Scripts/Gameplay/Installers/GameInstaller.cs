using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public DrillHeathHandler drillHealthHandler;

    [Header("Engine Drill")]
    [SerializeField] private DrillEngineHFSMController engineDrill;

    [Header("Engine Ship")]
    [SerializeField] private DrillEngineHFSMController engineShip;

    // ReSharper disable Unity.PerformanceAnalysis
    public override void InstallBindings()
    {
        BindSoundManager();
        //BindEventBus();
        BindDrillDependencies();
        BindDrillDataPersistence();
        BindCoreGameplayDependencies(); // ✅ NUEVO
                                        //Valen agregó esto para que la logica no joda con las interacciones
    }

    private void BindSoundManager()
    {
        //Container.Bind<ISoundManager>().To<SoundManager>().FromComponentInNewPrefab(soundManagerPrefab)
        //    .AsSingle().NonLazy();

        Container.Bind<DrillHeathHandler>().FromComponentInHierarchy().AsSingle().NonLazy();

        Container.Bind<BackgroundMusic>().FromComponentInHierarchy().AsSingle();
        Container.Bind<FootstepPlayer>().FromComponentInHierarchy().AsSingle();
        Container.Bind<LeverHandler>().FromComponentInHierarchy().AsSingle();
    }

    private void BindDrillDependencies()
    {
        Container.Bind<DrillHFSMController>().FromComponentInHierarchy().AsSingle();

        Container.Bind<DrillStateReferences>().FromComponentInHierarchy().AsSingle();
        Container.Bind<DrillStateConfig>().FromComponentInHierarchy().AsSingle();
        Container.Bind<NewMaterialManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<TutorialManager>().FromComponentInHierarchy().AsSingle();

        Container.Bind<IDrillRequirementChecker>().To<NewDrillRequirementChecker>().AsSingle();
    }



    private void BindDrillDataPersistence()
    {
        Container.Bind<IDrillLifeHandler>().FromInstance(drillHealthHandler).AsSingle();

        Container.Bind<IDrillEngine>().WithId("EngineDrill").FromInstance(engineDrill).AsCached();
        Container.Bind<IDrillEngine>().WithId("EngineShip").FromInstance(engineShip).AsCached();
            
    }
    
    // ✅ NUEVA SECCIÓN: Gameplay Core
    private void BindCoreGameplayDependencies()
    {
        Container.Bind<InteractionManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<DialogueManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<FirstPersonController>().FromComponentInHierarchy().AsSingle();
    }
    
}
