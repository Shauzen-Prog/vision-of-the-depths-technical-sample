using System;
using System.Threading.Tasks;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Default SceneManager-based router.
/// </summary>
public class SceneRouter : ISceneRouter
{
    private readonly ISceneTransition _defaultTransition;
    private readonly IEventBus _eventBus;
    private readonly ISceneManagementService _sceneManagement;

    /// <summary>
    /// Cache de operaciones de preload para escenas ADITIVAS.
    /// La clave es el nombre de la escena.
    /// </summary>
    private readonly Dictionary<string, ISceneLoadOperation> _preloadedAdditiveOps = new();

    public SceneRouter(
        ISceneTransition defaultTransition = null,
        IEventBus eventBus = null,
        ISceneManagementService sceneManagement = null)
    {
        _defaultTransition = defaultTransition ?? new InstantTransition() as ISceneTransition;
        _eventBus = eventBus;
        _sceneManagement = sceneManagement ?? new UnitySceneManagementService();
    }

    public void GoTo(string sceneName)
    {
        try
        {
            _sceneManagement.LoadScene(sceneName);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneRouter] GoTo falló en '{sceneName}': {e}");
            _eventBus?.Publish(new SceneRouterErrorEvent(sceneName, e));
        }
    }

    public async void GoToAsync(SceneRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.sceneName))
        {
            Debug.LogError("[SceneRouter] SceneRequest invalida.");
            return;
        }

        var transition = request.transition ?? _defaultTransition;

        try
        {
            await (transition.BeginAsync() ?? Task.CompletedTask);

            var op = _sceneManagement.LoadSceneAsync(request.sceneName, LoadSceneMode.Single);
            if (op == null)
                throw new InvalidOperationException($"No se pudo crear load op para '{request.sceneName}'.");

            op.AllowSceneActivation = request.activateOnLoad;

            while (!op.IsDone)
            {
                float progress = Mathf.Clamp01(op.Progress);
                request.onProgress?.Invoke(progress);

                if (!request.activateOnLoad && op.Progress >= 0.9f)
                {
                    op.AllowSceneActivation = true;
                }

                await Task.Yield();
            }

            request.onProgress?.Invoke(1f);
            request.onComplete?.Invoke();

            await (transition.EndAsync() ?? Task.CompletedTask);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneRouter] GoToAsync falló para '{request.sceneName}': {e}");
            _eventBus?.Publish(new SceneRouterErrorEvent(request.sceneName, e));
            request.onComplete?.Invoke();
        }
    }

    public async void LoadAdditiveAsync(SceneRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.sceneName))
        {
            Debug.LogError("[SceneRouter] SceneRequest invalida.");
            return;
        }

        var transition = request.transition ?? _defaultTransition;

        try
        {
            await (transition.BeginAsync() ?? Task.CompletedTask);

            ISceneLoadOperation op;

            if (_preloadedAdditiveOps.TryGetValue(request.sceneName, out var cachedOp) &&
                cachedOp != null &&
                !cachedOp.IsDone)
            {
                op = cachedOp;
            }
            else
            {
                op = _sceneManagement.LoadSceneAsync(request.sceneName, LoadSceneMode.Additive);
                if (op == null)
                    throw new InvalidOperationException($"No se pudo crear load op para '{request.sceneName}'.");
            }

            op.AllowSceneActivation = request.activateOnLoad;

            while (!op.IsDone)
            {
                float progress = Mathf.Clamp01(op.Progress);
                request.onProgress?.Invoke(progress);

                if (!request.activateOnLoad && op.Progress >= 0.9f)
                {
                    op.AllowSceneActivation = true;
                }

                await Task.Yield();
            }

            if (_preloadedAdditiveOps.ContainsKey(request.sceneName))
                _preloadedAdditiveOps.Remove(request.sceneName);

            await Task.Yield(); // tu parche anti-spike

            request.onProgress?.Invoke(1f);
            request.onComplete?.Invoke();

            await (transition.EndAsync() ?? Task.CompletedTask);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneRouter] LoadAdditiveAsync falló para '{request.sceneName}': {e}");
            _eventBus?.Publish(new SceneRouterErrorEvent(request.sceneName, e));
            request.onComplete?.Invoke();
        }
    }

    public async void UnloadAdditiveAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneRouter] UnloadAdditiveAsync: sceneName es nulo o vacio.");
            return;
        }

        if (_preloadedAdditiveOps.ContainsKey(sceneName))
            _preloadedAdditiveOps.Remove(sceneName);

        try
        {
            var op = _sceneManagement.UnloadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogWarning($"[SceneRouter] UnloadAdditiveAsync: Escena '{sceneName}' no cargada o no se puede descargar.");
                return;
            }

            while (!op.IsDone)
                await Task.Yield();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneRouter] UnloadAdditiveAsync falló para '{sceneName}': {e}");
            _eventBus?.Publish(new SceneRouterErrorEvent(sceneName, e));
        }
    }

    public async void PreloadAsync(SceneRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.sceneName))
        {
            Debug.LogError("[SceneRouter] SceneRequest invalida.");
            return;
        }

        if (_preloadedAdditiveOps.TryGetValue(request.sceneName, out var existingOp) &&
            existingOp != null &&
            !existingOp.IsDone)
        {
            return;
        }

        try
        {
            var op = _sceneManagement.LoadSceneAsync(request.sceneName, LoadSceneMode.Additive);
            if (op == null)
                throw new InvalidOperationException($"No se pudo crear op load para '{request.sceneName}'.");

            op.AllowSceneActivation = false;
            _preloadedAdditiveOps[request.sceneName] = op;

            while (op.Progress < 0.9f)
            {
                request.onProgress?.Invoke(op.Progress);
                await Task.Yield();
            }

            request.onProgress?.Invoke(0.9f);
            request.onComplete?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneRouter] PreloadAsync falló para '{request.sceneName}': {e}");
            _eventBus?.Publish(new SceneRouterErrorEvent(request.sceneName, e));
            request.onComplete?.Invoke();
        }
    }
}
