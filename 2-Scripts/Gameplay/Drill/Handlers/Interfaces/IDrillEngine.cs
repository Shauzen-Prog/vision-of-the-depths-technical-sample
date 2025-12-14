public interface IDrillEngine
{
    float CurrentFuel { get; }
    float MaxFuel { get; }
    
    void SetFuel(float current, float max);
    void SetOn(DrillEngineHFSMController.EngineState state);
    EngineId EngineId { get; }
    DrillEngineHFSMController.EngineState engineState { get; set; }
}
