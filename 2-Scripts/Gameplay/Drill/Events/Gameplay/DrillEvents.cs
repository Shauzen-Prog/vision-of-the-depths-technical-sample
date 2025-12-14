public struct DrillResumeEvent { }
public struct DrillPauseEvent { }
public struct DrillDestroyedEvent { }
public struct DrillStateChangedEvent
{
    public DrillStateId NewState;
    public DrillStateChangedEvent(DrillStateId state) { NewState = state; }
}