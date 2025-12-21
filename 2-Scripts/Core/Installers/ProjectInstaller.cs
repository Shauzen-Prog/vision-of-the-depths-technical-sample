using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public SoundManager soundManagerPrefab;
    
    [SerializeField] private InteractionUI interactionUIPrefab;
    
    [SerializeField] private CheatLocations _cheatLocations;

    public override void InstallBindings()
    {
        // Persistencia global (hoy memoria; después se puede pasar a JSON)
        Container.Bind<IPersistenceService>().To<MemoryPersistenceService>().AsSingle();
        
        // ISoundManager global (creado desde prefab en ProjectContext)
        Container.Bind<ISoundManager>()
            .To<SoundManager>()
            .FromComponentInNewPrefab(soundManagerPrefab)
            .AsSingle();
        
        // EventBus global
        Container.Bind<IEventBus>()
            .To<EventBus>()
            .AsSingle()
            .NonLazy();
        
        // Otros servicios globales
        Container.Bind<CharacterMoraleManager>().AsSingle();
        Container.Bind<GameStateManager>().AsSingle();
        
        
        Container.BindInterfacesTo<TaskHudCompatProxy>()
            .AsSingle()
            .NonLazy();
        
        // === Display Settings (JSON + cache + service) ===
        
        // Path del JSON (Sin drag and drop)
        Container.Bind<string>()
            .WithId("DisplaySettingsPath")
            .FromMethod(_ => Path.Combine(Application.persistentDataPath, "display_settings.json"))
            .AsCached();
        
        // Repo JSON
        var path = Path.Combine(Application.persistentDataPath, "display_settings.json");
        Container.Bind<IDisplaySettingsRepository>()
            .To<JsonDisplaySettingsRepository>()
            .AsSingle()
            .WithArguments(path);
        
        // Cache in-memory + servicio de dominio
        Container.BindInterfacesAndSelfTo<DisplaySettingsService>().AsSingle();
        
        // Transición por defecto (instantánea). Después podés swappear por un Fade.
        Container.Bind<ISceneTransition>().To<InstantTransition>().AsSingle();
        
        // Router
        Container.Bind<ISceneRouter>().To<SceneRouter>().AsSingle();
        
        // Flujo de arranque y trigger
        Container.Bind<IBootFlowService>().To<BootFlowService>().AsSingle();
        Container.BindInterfacesTo<BootInitializer>().AsSingle();
        
        Container.Bind<IDialogueService>()
            .To<DialogueService>()
            .AsSingle();

        Container.Bind<IDialogueParser>()
            .To<JsonDialogueParser>()
            .AsSingle();
        
        //Controlar drill para frenar o despausar                
        Container
            .Bind<IDrillControlPort>()
            .To<DrillControlService>()
            .AsSingle();
        
        //Controla los servicios del TaskState
        Container.Bind<ITaskStateService>().To<TaskStateService>().AsSingle();
        
        // Sonido de UI Pause
        Container.Bind<SoundOptionsPresenter>()
            .AsTransient();
        
        // Servicio global de PlayerFacade (proxy para cualquier escena)
        Container.Bind<IPlayerFacadeService>()
            .To<PlayerFacadeService>()
            .AsSingle();
        
        // Servicio global de cámara de diálogo
        Container.Bind<IDialogueCameraService>()
            .To<DialogueCameraService>()
            .AsSingle();
        
        // Servicio para capturar el mouse 
        Container.Bind<MouseCaptureService>().AsSingle();
        
        // Cámara del jugador disponible para todas las escenas
        Container.BindInterfacesAndSelfTo<PlayerCameraProvider>()
            .AsSingle();

        // Servicio global de control del jugador (movement/look/pause/interaction)
        Container.Bind<IPlayerControlService>()
            .To<PlayerControlService>()
            .AsSingle();
        
        // Input para cambiar vistas de closeup (A/D + D-Pad Left/Right)
        Container.Bind<ICloseupViewSwitchInputPort>()
            .To<CloseupViewSwitchInputAdapter>()
            .FromComponentInHierarchy()
            .AsSingle();
        
        // Servicio global del closeup del drill
        Container.BindInterfacesAndSelfTo<DrillCloseupControllerService>()
            .AsSingle();
        
        Container.Bind<IInteractionHudService>()
            .To<InteractionUI>()
            .FromComponentInNewPrefab(interactionUIPrefab)
            .AsSingle()
            .NonLazy();
        
        // Eye Blink (global, usable desde escenas aditivas)
        Container.Bind<IEyeBlinkPresenterRegistry>()
            .To<EyeBlinkPresenterRegistry>()
            .AsSingle();

        Container.Bind<IEyeBlinkService>()
            .To<EyeBlinkService>()
            .AsSingle();
            
        Container.Bind<ICheatService>()
            .To<NoopCheatService>()
            .AsSingle();
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Container.Rebind<ICheatService>()
            .To<CheatService>()
            .AsSingle();

        Container.Bind<CheatLocations>()
            .FromInstance(_cheatLocations)
            .AsSingle();
#endif
    }
    
}
