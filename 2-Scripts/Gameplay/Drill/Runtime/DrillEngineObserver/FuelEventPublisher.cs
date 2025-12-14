using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum EngineId { DrillEngine, ShipEngine } 

public class FuelEventPublisher : MonoBehaviour
{
    [SerializeField] private DrillEngineHFSMController engine;
    [SerializeField] private EngineId engineId = EngineId.DrillEngine; 

    private float _lastFuel;
    private DrillEngineHFSMController.EngineState _lastState;

    private IEventBus _eventBus;

    [Inject]
    public void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    void Start()
    {
        if (engine == null) engine = GetComponent<DrillEngineHFSMController>();
        _lastFuel = engine.CurrentFuel;
        _lastState = engine.engineState;
        Publish();
    }

    void Update()
    {
        if (!Mathf.Approximately(engine.CurrentFuel, _lastFuel) || engine.engineState != _lastState)
        {
            _lastFuel = engine.CurrentFuel;
            _lastState = engine.engineState;
            Publish();
        }
    }

    private void Publish()
    {
        _eventBus.Publish(new DrillFuelChangedEvent(engineId, _lastFuel, engine.maxFuel, _lastState == DrillEngineHFSMController.EngineState.On));
    }
}

