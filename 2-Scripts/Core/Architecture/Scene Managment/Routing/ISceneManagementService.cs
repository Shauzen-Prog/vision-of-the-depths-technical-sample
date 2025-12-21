using UnityEngine.SceneManagement;

/// <summary>
/// Puerto de infraestructura para cargar/descargar escenas.
/// Permite testear SceneRouter con fakes sin tocar SceneManager.
/// </summary>
public interface ISceneManagementService
{
    /// <summary> Carga sincrónica. </summary>
    void LoadScene(string sceneName);

    /// <summary> Carga asíncrona. </summary>
    ISceneLoadOperation LoadSceneAsync(string sceneName, LoadSceneMode mode);

    /// <summary> Descarga asíncrona. </summary>
    ISceneLoadOperation UnloadSceneAsync(string sceneName);
}
