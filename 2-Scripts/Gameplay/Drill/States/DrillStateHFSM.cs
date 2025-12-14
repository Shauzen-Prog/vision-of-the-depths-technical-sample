using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillStateHFSM : IState<DrillStateId>, IPausable
{
    private readonly DrillStateReferences _drillRefs;
    private readonly NewMaterialManager _newMaterialManager;
    private readonly IDrillRequirementChecker _requirementChecker;

    private IFiniteStateMachine<DrillStateId> _fsm;
    private MonoBehaviour _runner;
    private IEventBus _eventBus;

    private DrillableMaterialSO _currentMaterialSO;
    private IMaterialLifeHandler _currentMaterialHandler;
    private Coroutine _drillRoutine;
    private float _currentMaterialLife;

    private int _currentMode;
    private int _currentPower;
    private bool _isPaused;

    private IDisposable _materialSpawnedSub;
    private IDisposable _materialDestroyedSub;
    private IDisposable _modeChangedSub;
    private IDisposable _powerChangedSub;

    private readonly List<IDrillSubState> _subStates = new();
    private readonly List<IPausableSubState> _pausableSubStates = new();

    private NeedleSubState _needleSubState;
    private TextureScrollerSubState _textureScrollerSubState;
    
    private IPausable _currentPausableState;

    public DrillStateHFSM(MonoBehaviour runner, DrillStateReferences refs, DrillStateConfig drillStateConfig,
        IEventBus eventBus, IDrillRequirementChecker requirementChecker, NewMaterialManager newMaterialManager)
    {
        _runner = runner;
        _drillRefs = refs;
        _eventBus = eventBus;
        _requirementChecker = requirementChecker;
        _newMaterialManager = newMaterialManager;

        // Subestados
        _needleSubState = new NeedleSubState(runner, refs, drillStateConfig);
        _textureScrollerSubState = new TextureScrollerSubState(refs.textureScroller, 
            drillStateConfig.materialScrollConfigListSO, newMaterialManager);

        _subStates.Add(_needleSubState);
        _subStates.Add(_textureScrollerSubState);

        if (_needleSubState is IPausableSubState p1) _pausableSubStates.Add(p1);
        if (_textureScrollerSubState is IPausableSubState p2) _pausableSubStates.Add(p2);
    }

    public void SetController(IFiniteStateMachine<DrillStateId> fsm) => _fsm = fsm;

    public void InjectEventBus(IEventBus eventBus)
    {
        _eventBus = eventBus;
        foreach (var s in _subStates)
            s.InjectEventBus(eventBus);
    }

    public void OnEnter()
    {
        _modeChangedSub = _eventBus.Subscribe<DrillModeChangedEvent>(OnModeChanged);
        _powerChangedSub = _eventBus.Subscribe<DrillPowerChangedEvent>(OnPowerChanged);
        _materialSpawnedSub = _eventBus.Subscribe<MaterialSpawnedEvent>(OnMaterialSpawned);
        _materialDestroyedSub = _eventBus.Subscribe<MaterialDestroyedEvent>(OnMaterialDestroyed);
        _eventBus.Subscribe<DrillPauseEvent>(_ => Pause());
        _eventBus.Subscribe<DrillResumeEvent>(_ => Resume());

        foreach (var s in _subStates)
            s.OnEnter();
        
        _drillRefs.modeLight.enabled = true;
        _drillRefs.powerLight.enabled = true;
        _drillRefs.heatLight.enabled = true;
        
        _drillRefs.modeLever.enabled = true;
        _drillRefs.powerLever.enabled = true;
    }

    public void OnExit()
    {
        foreach (var s in _subStates)
            s.OnExit();

        _materialSpawnedSub?.Dispose();
        _materialDestroyedSub?.Dispose();
        _modeChangedSub?.Dispose();
        _powerChangedSub?.Dispose();

        StopDrillRoutine();
    }

    public void Pause()
    {
        _isPaused = true;
        foreach (var s in _pausableSubStates)
            s.Pause();
    }

    public void Resume()
    {
        if (_currentPausableState != null)
            _currentPausableState.Resume();
        else
            Debug.LogWarning("No hay estado actual pausable.");
        
        _isPaused = false;
        foreach (var s in _pausableSubStates)
            s.Resume();
    }

    private void OnModeChanged(DrillModeChangedEvent evt) => _currentMode = evt.Mode;
    private void OnPowerChanged(DrillPowerChangedEvent evt) => _currentPower = evt.Power;

    private void OnMaterialSpawned(MaterialSpawnedEvent evt)
    {
        _currentMaterialSO = evt.Material;
        _currentMaterialHandler = new MaterialLifeHandler(evt.Material, _drillRefs.diegeticBar, _eventBus, _drillRefs);
        _currentMaterialLife = _currentMaterialSO.maxLife;

        _currentMode = _drillRefs.modeLever?.ValorActual ?? 0;
        _currentPower = _drillRefs.powerLever?.ValorActual ?? 0;

        _needleSubState.StopNeedleRise();
        _textureScrollerSubState.OnMaterialSpawned(evt.Material.typeOfMaterialToDrill);

        StartDrillRoutine();
    }

    private void OnMaterialDestroyed(MaterialDestroyedEvent evt)
    {
        var lastType = evt.Material.texture;
        
        StopDrillRoutine();
        
        _currentMaterialSO = null;
        _currentMaterialLife = 0;

        _needleSubState.TriggerNeedleRise();
        _textureScrollerSubState.OnMaterialDestroyed(lastType);
    }

    private void StartDrillRoutine()
    {
        StopDrillRoutine();
        if (_currentMaterialSO == null)
        {
            Debug.LogWarning("StartDrillRoutine: currentMaterialSO is null");
            return;
        }

        _drillRoutine = _runner.StartCoroutine(DrillRoutine());
    }

    private void StopDrillRoutine()
    {
        if (_drillRoutine == null) return;

        _runner.StopCoroutine(_drillRoutine);
        _drillRoutine = null;
        
    }

    private IEnumerator DrillRoutine()
    {
        while (_currentMaterialSO != null && _currentMaterialLife > 0)
        {
            while (_isPaused) yield return null;

            var status = _requirementChecker.CheckRequirementStatus(
                _currentMaterialSO.typeOfMaterialToDrill,
                _currentMode,
                _currentPower
            );
            
            _currentMaterialHandler?.ApplyDrillDamage(status, _drillRefs.tickInterval);
            _needleSubState?.SetStatus(status);
            
            if (_currentMaterialSO != null)
            {
                _textureScrollerSubState?.UpdateScrollSpeed(_currentMaterialSO.typeOfMaterialToDrill);
            }

            yield return new WaitForSeconds(_drillRefs.tickInterval);
        }
    }
}
