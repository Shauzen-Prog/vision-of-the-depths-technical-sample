/// <summary>
/// Strategy de transicion visual/UX
/// </summary>
public interface ISceneTransition
{
    /// <summary> Prepara transicion (fade out, mostrar loading UI, etc) </summary>
    System.Threading.Tasks.Task BeginAsync();
    
    /// <summary> Cerrar transicion (fade in, esconder loading UI) </summary>
    System.Threading.Tasks.Task EndAsync();
    
}
