public struct DrillPowerChangedEvent
{
    public int Power;
    public DrillPowerChangedEvent(int power) => Power = power;
}
    
public struct DrillModeChangedEvent
{
    public int Mode;
    public DrillModeChangedEvent(int mode) => Mode = mode;
}

