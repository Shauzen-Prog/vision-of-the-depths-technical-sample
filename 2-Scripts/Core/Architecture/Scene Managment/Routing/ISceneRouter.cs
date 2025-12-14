using UnityEngine.SceneManagement;

/// <summary>
//Alto nivel. Capa que usa el juego para cambiar de escena sin saber el "como".
/// </summary>
public interface ISceneRouter
{
    /// <summary> Salto inmediato (sin transición), util para boot o failsafe. </summary>
    void GoTo(string sceneName);

    /// <summary> Carga asincrona con transicion y reporte de progreso. </summary>
    void GoToAsync(SceneRequest request);

    /// <summary> Carga ADITIVA (stack de escenas), p.ej. overlay UI, subniveles. </summary>
    void LoadAdditiveAsync(SceneRequest request);

    /// <summary> Unload aditiva (cuando cerramos un overlay). </summary>
    void UnloadAdditiveAsync(string sceneName);

    /// <summary> Preload (carga asíncrona sin activar), útil para warm-up. </summary>
    void PreloadAsync(SceneRequest request);
}
