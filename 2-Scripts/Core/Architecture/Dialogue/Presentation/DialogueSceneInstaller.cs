using Zenject;
using UnityEngine;

/// <summary>
/// Installer de escena para bindings específicos de diálogos (input, vistas, etc.).
/// </summary>
public class DialogueSceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<DialogueInputAdapter>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.BindInterfacesAndSelfTo<TextAnimatorTypingController>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}
