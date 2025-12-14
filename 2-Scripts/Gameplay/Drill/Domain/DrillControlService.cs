using Zenject;

/// <summary>
/// Implementación por defecto de IDrillControlPort.
/// Usa IEventBus para hablar con DrillHFSMController y otros listeners.
/// </summary>
public sealed class DrillControlService : IDrillControlPort
{
    private readonly IEventBus _eventBus;

    /// <summary>
    /// Crea el servicio inyectando el EventBus.
    /// </summary>
    /// <param name="eventBus">Bus de eventos tipados compartido.</param>
    public DrillControlService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <inheritdoc />
    public void PauseDrill()
    {
        _eventBus.Publish(new DrillPauseEvent());
    }

    /// <inheritdoc />
    public void ResumeDrill()
    {
        _eventBus.Publish(new DrillResumeEvent());
    }

    /// <inheritdoc />
    public void SetIdle()
    {
        _eventBus.Publish(new DrillStateChangedEvent(DrillStateId.Idle));
    }
}
