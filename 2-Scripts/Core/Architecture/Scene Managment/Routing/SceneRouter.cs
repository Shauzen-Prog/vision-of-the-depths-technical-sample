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
    private readonly IEventBus _eventBus; // para reportes
    
    /// <summary>
    /// Cache de operaciones de preload para escenas ADITIVAS.
    /// La clave es el nombre de la escena.
    /// </summary>
    private readonly Dictionary<string, AsyncOperation> _preloadedAdditiveOps = new();

    public SceneRouter(ISceneTransition defaultTransition = null, IEventBus eventBus = null)
    {
        _defaultTransition = defaultTransition ?? new InstantTransition() as ISceneTransition;
        _eventBus = eventBus;
    }
    
    /// <summary> Loads by name. Can be replaced with Addressables </summary>   
    public void GoTo(string sceneName)
    {
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneRouter] GoTo falló en '{sceneName}': {e}");
            _eventBus?.Publish(new SceneRouterErrorEvent(sceneName, e));
        }
       
    }

    /// <summary>
    /// Carga asíncrona en modo Single con transición.
    /// No usa preload hoy (es para cambios “de bloque” más fuertes).
    /// </summary>
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

            var op = SceneManager.LoadSceneAsync(request.sceneName, LoadSceneMode.Single);
            if (op == null)
                throw new InvalidOperationException($"No se pudo crear load op para '{request.sceneName}'.");

            op.allowSceneActivation = request.activateOnLoad;

            while (!op.isDone)
            {
                // Unity reporta 0..0.9f hasta activación
                float progress = Mathf.Clamp01(op.progress);
                request.onProgress?.Invoke(progress);

                if (!request.activateOnLoad && op.progress >= 0.9f)
                {
                    op.allowSceneActivation = true;
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
            // Siempre dejar la UI desbloqueada por si espera por completar
            request.onComplete?.Invoke();
        }
    }

    /// <summary>
    /// Carga ADITIVA asíncrona.
    /// Si existe un preload pendiente para la escena, reutiliza ese AsyncOperation
    /// en lugar de crear uno nuevo.
    /// </summary>
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

            AsyncOperation op;

            // Intentar reutilizar un preload existente para esta escena
            if (_preloadedAdditiveOps.TryGetValue(request.sceneName, out var cachedOp) &&
                cachedOp != null &&
                !cachedOp.isDone)
            {
                op = cachedOp;
            }
            else
            {
                op = SceneManager.LoadSceneAsync(request.sceneName, LoadSceneMode.Additive);
                if (op == null)
                    throw new InvalidOperationException(
                        $"No se pudo crear load op para '{request.sceneName}'.");
            }

            // Activación controlada según el request
            op.allowSceneActivation = request.activateOnLoad;

            while (!op.isDone)
            {
                float progress = Mathf.Clamp01(op.progress);
                request.onProgress?.Invoke(progress);

                if (!request.activateOnLoad && op.progress >= 0.9f)
                {
                    // Para casos donde se quiera controlar la activación manualmente
                    op.allowSceneActivation = true;
                }

                await Task.Yield();
            }

            // Ya no es un preload pendiente
            if (_preloadedAdditiveOps.ContainsKey(request.sceneName))
            {
                _preloadedAdditiveOps.Remove(request.sceneName);
            }
            
            // -------------------------
            // PARCHE MÍNIMO ANTI-SPIKE
            // -------------------------
            // La activación de la escena (Awake/OnEnable) ya ocurrió
            // en frames anteriores. Este Yield garantiza que la lógica de
            // gameplay asociada (UnityEvents, misiones, animaciones, etc.)
            // no caiga en el MISMO frame que el pico de activación.
            await Task.Yield();

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

    /// <summary>
    /// Descarga una escena aditiva.
    /// También limpia cualquier preload pendiente registrado para ese nombre.
    /// </summary>
    public async void UnloadAdditiveAsync(string sceneName)
    {
         if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneRouter] UnloadAdditiveAsync: sceneName es nulo o vacio.");
            return;
        }

        // Si había un preload cacheado, se descarta.
        if (_preloadedAdditiveOps.ContainsKey(sceneName))
        {
            _preloadedAdditiveOps.Remove(sceneName);
        }

        try
        {
            var op = SceneManager.UnloadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogWarning(
                    $"[SceneRouter] UnloadAdditiveAsync: Escena '{sceneName}' no cargada o no se puede descargar.");
                return;
            }

            while (!op.isDone)
                await Task.Yield();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneRouter] UnloadAdditiveAsync falló para '{sceneName}': {e}");
            _eventBus?.Publish(new SceneRouterErrorEvent(sceneName, e));
        }
    }

    /// <summary>
    /// Preload de escena ADITIVA: la carga hasta ~0.9f sin activarla
    /// y deja el AsyncOperation cacheado para que luego LoadAdditiveAsync lo reutilice.
    /// </summary>
    public async void PreloadAsync(SceneRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.sceneName))
        {
            Debug.LogError("[SceneRouter] SceneRequest invalida.");
            return;
        }

        // Si ya hay un preload en curso para esta escena, no hacer nada.
        if (_preloadedAdditiveOps.TryGetValue(request.sceneName, out var existingOp) &&
            existingOp != null &&
            !existingOp.isDone)
        {
            return;
        }

        try
        {
            var op = SceneManager.LoadSceneAsync(request.sceneName, LoadSceneMode.Additive);
            if (op == null)
                throw new InvalidOperationException(
                    $"No se pudo crear op load para '{request.sceneName}'.");

            // Preload puro: no activamos todavía.
            op.allowSceneActivation = false;

            _preloadedAdditiveOps[request.sceneName] = op;

            while (op.progress < 0.9f)
            {
                request.onProgress?.Invoke(op.progress);
                await Task.Yield();
            }

            // Preload listo para activarse más adelante.
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
