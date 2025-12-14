/// <summary>
/// Interfaz para estados que pueden pausar su lógica.
/// </summary>
public interface IPausable
{
    void Pause();
    void Resume();
}