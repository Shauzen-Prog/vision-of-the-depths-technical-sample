using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Identificadores de estados del drill.
/// </summary>
public enum DrillStateId { 
    Idle, 
    Drilling,
    Tutorial
}

/// <summary>
/// Controlador principal del HFSM del drill. Orquesta cambios de estado, pausa/reanudar y wiring con EventBus.
/// Gameplay-only: no debería contener lógica de feedback.
/// </summary>
public class DrillHFSMController : MonoBehaviour, IDrillStateChanger
{
    [Inject] private IEventBus _eventBus;
    [Inject] private NewMaterialManager _materialManager;
    [Inject] private DrillStateReferences _drillRefs;
    [Inject] private DrillStateConfig _drillStateConfig;
    [Inject] private IDrillRequirementChecker _drillRequirementChecker;
    [Inject] private ISoundManager _soundManager;
    [Inject] private ITaskStateService _taskService;

    private HierarchicalCoroutineStateMachine<DrillStateId> _fsm;

    private DrillStateId _currentState;
    private IPausable _currentPausableState;

    private IDisposable _resumeSub;
    private IDisposable _pauseSub;
    private IDisposable _changeStateSub;

    /// <summary>
    /// Se ejecuta luego de que Zenject inyecta dependencias. Construye el HFSM.
    /// </summary>
    [Inject]
    private void Initialize()
    {
        var states = new Dictionary<DrillStateId, IState<DrillStateId>>
        {
            { DrillStateId.Idle, new IdleStateHFSM(_drillRefs) },

            { DrillStateId.Drilling, new DrillStateHFSM(
                stateChanger: this,
                drillRefs: _drillRefs,
                config: _drillStateConfig,
                eventBus: _eventBus,
                requirementChecker: _drillRequirementChecker,
                materialManager: _materialManager) },

            { DrillStateId.Tutorial, new TutorialHFSMState(
                stateChanger: this,
                eventBus: _eventBus,
                tutorialSequence: _drillStateConfig.tutorialSequenceSO,
                drillRefs: _drillRefs,
                soundManager: _soundManager,
                taskService: _taskService) }
        };

        _fsm = new HierarchicalCoroutineStateMachine<DrillStateId>(states, DrillStateId.Idle, _eventBus);
        _currentState = DrillStateId.Idle;
        _currentPausableState = _fsm.CurrentStateInstance as IPausable;
    }

    private void OnEnable()
    {
        _resumeSub = _eventBus.Subscribe<DrillResumeEvent>(_ => OnDrillResume());
        _pauseSub = _eventBus.Subscribe<DrillPauseEvent>(_ => OnDrillPause());
        _changeStateSub = _eventBus.Subscribe<DrillStateChangedEvent>(OnExternalStateChanged);
    }

    private void OnDisable()
    {
        _resumeSub?.Dispose();
        _pauseSub?.Dispose();
        _changeStateSub?.Dispose();
    }

    private void OnDrillResume()
    {
        if (_currentState == DrillStateId.Drilling)
        {
            _currentPausableState?.Resume();
            return;
        }

        ChangeState(DrillStateId.Drilling);
    }

    private void OnDrillPause()
    {
        _currentPausableState?.Pause();
    }

    private void OnExternalStateChanged(DrillStateChangedEvent evt)
    {
        ChangeState(evt.NewState);
    }

    /// <inheritdoc />
    public void ChangeState(DrillStateId newState)
    {
        if (_fsm == null)
        {
            Debug.LogError($"{nameof(DrillHFSMController)} was not initialized. Check Zenject bindings / initialization order.");
            return;
        }

        if (_currentState == newState)
        {
            return;
        }

        _fsm.ChangeState(newState);
        _currentState = newState;
        _currentPausableState = _fsm.CurrentStateInstance as IPausable;
    }

    /// <inheritdoc />
    public void ChangeToIdle()
    {
        ChangeState(DrillStateId.Idle);
    }
}
