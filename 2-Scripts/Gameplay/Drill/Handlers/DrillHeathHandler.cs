using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class DrillHeathHandler : MonoBehaviour, IDrillLifeHandler
{
    [SerializeField] private int maxDrillLife = 500;
    private DrillLifeDisplay _drillLifeDisplay; 
    
    [Inject] private DrillStateConfig _drillStateConfig;
    [Inject] private DrillStateReferences _drillStateReferences;
    private IPersistenceService _persistenceService;
    
    private int _drillLife;
    private IEventBus _eventBus;
    private IDisposable _damageSub;
    
    private IDisposable _saveGameSub;
    
    [Inject]
    public void Construct(IEventBus eventBus, IPersistenceService persistenceService)
    {
        _eventBus = eventBus;
        _persistenceService = persistenceService;
    }

    private void Awake()
    {
        _drillLifeDisplay = _drillStateReferences.drillLifeDisplay;
        if (LoadDrillLife() <= 0)
        {
            _drillLife = (int)_drillStateConfig.drillLifeConfigSO.drillLife;
        }
        else
        {
            _drillLife = (int)LoadDrillLife();
            
            Debug.Log("Drill life After SceneCharge: " + _drillLife);
            _drillLifeDisplay.UpdateLifeDisplay(_drillLife);
        }
        
    }

    private void OnEnable()
    {
        _damageSub = _eventBus.Subscribe<DrillTakeDamageEvent>(OnTakeDamage);
        _saveGameSub = _eventBus.Subscribe<SaveGameEvent>(OnSaveGameEvent);

        SetLife(_drillLife, maxDrillLife);
        
    }

    private void OnDisable()
    {
        _damageSub?.Dispose();
        _saveGameSub?.Dispose();
    }

    private void OnSaveGameEvent(SaveGameEvent evt)
    {
        SaveDrillLife(CurrentLife);
        Debug.Log(CurrentLife);
    }

    private void OnTakeDamage(DrillTakeDamageEvent evt)
    {
        _drillLife = (int)Mathf.Max(0, _drillLife - _drillStateConfig.drillLifeConfigSO.lifeLostPerTick);
        if (_drillLifeDisplay != null)
            _drillLifeDisplay.UpdateLifeDisplay(_drillLife);
        
        _eventBus.Publish(new UpdateDrillLifeUIEvent(_drillLife, maxDrillLife));
        
        // Feedback (sonido, VFX, animación, etc)
        if (_drillLife <= 0)
        {
            // TODO: Notificá que el taladro murió, por evento o llamado directo
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // (Opcional) Para curar el taladro
    public void Heal(int amount)
    {
        _drillLife = Mathf.Min(_drillLife + amount, maxDrillLife);
        if (_drillLifeDisplay != null)
            _drillLifeDisplay.UpdateLifeDisplay(_drillLife);
    }

    public int CurrentLife => _drillLife;
    int IDrillLifeHandler.MaxLife => MaxLife;

    public void SetLife(int current, int maxLife)
    {
        _drillLife = current;
        maxDrillLife = maxLife;
    }

    int IDrillLifeHandler.CurrentLife => CurrentLife;

    public int MaxLife => maxDrillLife;
    
    public void SaveDrillLife(float value)
    {
        var state = _persistenceService.Load();
        state.DrillLife = value;
        _persistenceService.Save(state);
    }

    public float LoadDrillLife()
    {
        return _persistenceService.Load().DrillLife;
    }
    
}
