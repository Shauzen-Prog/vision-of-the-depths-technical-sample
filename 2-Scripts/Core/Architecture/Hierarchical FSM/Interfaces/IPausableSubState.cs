/// <summary>
/// Subestados del taladro que pueden pausarse.
/// </summary>
public interface IPausableSubState
{
   void Pause();
   void Resume();
}
