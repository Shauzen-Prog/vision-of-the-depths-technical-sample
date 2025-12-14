public struct DrillFuelChangedEvent
{
    public EngineId Engine;
    public float Current;
    public float Max;
    public bool IsOn;
    
    public DrillFuelChangedEvent(EngineId engine, float current, float max, bool isOn)
    {
        Engine = engine;
        Current = current;
        Max = max;
        IsOn = isOn;
    }
}