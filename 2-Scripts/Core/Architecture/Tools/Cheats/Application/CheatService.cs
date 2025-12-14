using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

/// <summary>
/// Servicio de tooling interno para desarrollo y debugging.
/// Permite acelerar la iteración (teleport, carga de escenas, flujos)
/// sin acoplar gameplay ni UI a lógica de debug.
/// No se utiliza en builds de producción.
/// </summary>
public class CheatService : ICheatService
{
    private readonly ISceneRouter _sceneRouter;
    private readonly IPlayerFacadeService _playerFacade;
    private readonly CheatLocations _locations;

    private bool _isRunning;

    public CheatService(ISceneRouter sceneRouter, IPlayerFacadeService playerFacade, CheatLocations locations)
    {
        _sceneRouter = sceneRouter;
        _playerFacade = playerFacade;
        _locations = locations;
    }

    public void GoToBreakingPoint()
    {
        if (_isRunning) return;
        _ = GoToBreakingPointAsync();
    }

    private async Task GoToBreakingPointAsync()
    {
        _isRunning = true;

        try
        {
            // 1) Unload (additive)
            foreach (var sceneRef in _locations.BreakingPointUnloadAdditive)
            {
                if (sceneRef == null) continue;
                await EnsureAdditiveUnloadedAsync(sceneRef.MainSceneName);
            }

            // 2) Load (additive) en orden
            foreach (var sceneRef in _locations.BreakingPointLoadAdditive)
            {
                if (sceneRef == null) continue;
                await LoadAdditiveAsync(sceneRef.MainSceneName);
            }

            // 3) Teleport
            TeleportToSpawnTag(_locations.BreakingPointSpawnTag);
        }
        finally
        {
            _isRunning = false;
        }
    }

    private async Task LoadAdditiveAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        var existing = SceneManager.GetSceneByName(sceneName);
        if (existing.IsValid() && existing.isLoaded)
            return;

        var tcs = new TaskCompletionSource<bool>();

        _sceneRouter.LoadAdditiveAsync(new SceneRequest
        {
            sceneName = sceneName,
            activateOnLoad = true,
            onComplete = () => tcs.TrySetResult(true),
            onProgress = null
        });

        await tcs.Task;
    }

    private async Task EnsureAdditiveUnloadedAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        var scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        _sceneRouter.UnloadAdditiveAsync(sceneName);

        const float timeoutSeconds = 5f;
        float start = Time.realtimeSinceStartup;

        while (true)
        {
            var s = SceneManager.GetSceneByName(sceneName);
            if (!s.IsValid() || !s.isLoaded)
                break;

            if (Time.realtimeSinceStartup - start > timeoutSeconds)
            {
                Debug.LogWarning($"[Cheats] Timeout unloading scene '{sceneName}'.");
                break;
            }

            await Task.Yield();
        }
    }

    private void TeleportToSpawnTag(string spawnTag)
    {
        var player = _playerFacade.Current;
        if (player == null)
            return;

        var spawnGo = GameObject.FindGameObjectWithTag(spawnTag);
        if (spawnGo == null)
        {
            Debug.LogWarning($"[Cheats] Spawn tag not found: '{spawnTag}'.");
            return;
        }

        var t = spawnGo.transform;
        player.TeleportTo(t.position, t.rotation, alignYawOnly: true);
    }
}
