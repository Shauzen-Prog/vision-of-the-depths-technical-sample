using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleSubState : IDrillSubState, IPausableSubState
{
    private GameObject _needleGO;
    private readonly DrillStateReferences _needleRefs;
    private readonly DrillStateConfig _config;
    private DrillNeedleConfigSO _needleConfig;
    private readonly DrillLifeConfigSO _lifeConfig;
    private IEventBus _eventBus;

    private Coroutine _updateCoroutine;
    private MonoBehaviour _runner;

    private float _needleValue; // 1 = seguro/arriba, 0 = peligro/abajo
    private float _minAngle = -90f, _maxAngle = 90f;
    
    private bool _isPaused = false;

    private DrillLifeDisplay _lifeDisplay;
    private float _drillLife;
    private float _needlePosition;

    private bool _isRising = false;
    
    private bool _isInRedZone = false;
    private float _redZoneDamageTimer = 0f;

    private RequirementStatus _currentStatus = RequirementStatus.Valid;

    public NeedleSubState(MonoBehaviour runner, DrillStateReferences needleRefs, DrillStateConfig config)
    {
        _runner = runner;
        _needleRefs = needleRefs;
        _config = config;

        _needleGO = needleRefs.needleObject;
        _needleConfig = _config.drillNeedleConfigSO;
        _lifeConfig = _config.drillLifeConfigSO;
    }

    public void OnEnter()
    {
        _lifeDisplay = _needleRefs.drillLifeDisplay;
        _drillLife = _lifeConfig.drillMaxLife;

        _needleValue = 1f; // Resetea la aguja al iniciar
        UpdateLights(_currentStatus);
        _updateCoroutine = _runner.StartCoroutine(UpdateNeedleRoutine());
    }

    public void OnExit()
    {
        if (_updateCoroutine != null)
        {
            _runner.StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
    }

    public void InjectEventBus(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public void SetStatus(RequirementStatus status)
    {
        _currentStatus = status;
        UpdateLights(status);
    }

    private void UpdateLights(RequirementStatus status)
    {
        switch (status)
        {
            case RequirementStatus.Valid:
                _needleRefs.modeLight.color = Color.green;
                _needleRefs.powerLight.color = Color.green;
                break;
            case RequirementStatus.InvalidMode:
                _needleRefs.modeLight.color = Color.yellow;
                _needleRefs.powerLight.color = Color.green;
                break;
            case RequirementStatus.InvalidPower:
                _needleRefs.modeLight.color = Color.green;
                _needleRefs.powerLight.color = Color.yellow;
                break;
            case RequirementStatus.BothInvalid:
                _needleRefs.modeLight.color = Color.red;
                _needleRefs.powerLight.color = Color.red;
                break;
        }
    }
    
    public void TriggerNeedleRise() => _isRising = true;
    
    public void StopNeedleRise() => _isRising = false;
    
    private IEnumerator UpdateNeedleRoutine()
    {
        while (true)
        {
            while (_isPaused) yield return null;
            
            //float shake = 0f;
            
            if (_isRising)
            {
                // Animá la subida suave
                _needleValue = Mathf.MoveTowards(_needleValue, 1f, _needleConfig.needleRiseSpeed * Time.deltaTime);

                // Cuando termina, podés cortar el rising automáticamente
                if (Mathf.Approximately(_needleValue, 1f))
                    _isRising = false;
            }
            else
            {
                // Solo baja según status, nunca sube
                switch (_currentStatus)
                {
                    case RequirementStatus.Valid:
                        //No hace nada
                        break;
                    case RequirementStatus.InvalidMode:
                    case RequirementStatus.InvalidPower:
                        _needleValue =
                            Mathf.Clamp01(_needleValue - _needleConfig.needleDropSpeedYellow * Time.deltaTime);
                        break;
                    case RequirementStatus.BothInvalid:
                        _needleValue =
                            Mathf.Clamp01(_needleValue - _needleConfig.needleDropSpeedRed * Time.deltaTime);
                        break;
                }
            }
            
            if (_needleValue > _needleConfig.yellowZoneThreshold)
                _needleRefs.heatLight.color = Color.green;
            else if (_needleValue > _needleConfig.redZoneThreshold)
                _needleRefs.heatLight.color = Color.yellow;
            else
                _needleRefs.heatLight.color = Color.red;
            

            // ======= Shake basado en needleValue =======
            // El temblor es mínimo en 1, máximo en 0
            float shakeStrength = Mathf.Lerp(
                _needleConfig.needleShakeStrengthMax,
                _needleConfig.needleShakeStrengthMin,
                _needleValue
            );
            // Si needleValue = 1, es min; si es 0, es max
            
            float randomSeed = Time.time * _needleConfig.needleShakeFrequency;
            float shakeValue = (Mathf.PerlinNoise(randomSeed, 0f) - 0.5f) * shakeStrength * 60f;
            
            float angle = Mathf.Lerp(_minAngle, _maxAngle, _needleValue) + shakeValue;
            _needleRefs.needleObject.transform.localRotation = Quaternion.Euler(0, 0, -angle);
            
            
            _isInRedZone = _needleValue <= _needleConfig.redZoneThreshold;
            
            if (_isInRedZone)
            {
                _redZoneDamageTimer += Time.deltaTime;
                if (_redZoneDamageTimer >= 1f)
                {
                    _redZoneDamageTimer = 0f;
                    _eventBus.Publish(new DrillTakeDamageEvent(_lifeConfig.lifeLostPerTick));
                }
            }
            else
            {
                _redZoneDamageTimer = 0f;
            }

            
            yield return null;
        }
    }

    public void Pause() => _isPaused = true;
    public void Resume() => _isPaused = false;
}          
    

