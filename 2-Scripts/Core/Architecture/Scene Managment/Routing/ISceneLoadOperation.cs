/// <summary>
/// Abstracción testeable de una operación de carga/descarga de escena.
/// Evita depender de AsyncOperation (que no se puede instanciar en tests).
/// </summary>
public interface ISceneLoadOperation
{
    /// <summary> Progreso 0..1 (Unity suele reportar 0..0.9 hasta activación). </summary>
    float Progress { get; }

    /// <summary> Indica si la operación finalizó. </summary>
    bool IsDone { get; }

    /// <summary> Controla la activación de escena (si aplica). </summary>
    bool AllowSceneActivation { get; set; }
}
