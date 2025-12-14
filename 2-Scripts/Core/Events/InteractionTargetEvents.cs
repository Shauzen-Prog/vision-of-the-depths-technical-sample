/// <summary>
/// Evento global para forzar el target actual de interacción.
/// Permite además indicar si se debe bloquear el raycast (lock) o no.
/// </summary>
public readonly struct ForceInteractionTargetEvent
{
    public IInteractionTarget Target { get; }
    public bool LockTarget { get; }

    public ForceInteractionTargetEvent(IInteractionTarget target, bool lockTarget)
    {
        Target = target;
        LockTarget = lockTarget;
    }
}