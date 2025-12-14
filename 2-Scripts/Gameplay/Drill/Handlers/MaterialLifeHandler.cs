using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Administra la vida de un material virtual y publica el evento cuando se destruye.
/// </summary>
public class MaterialLifeHandler : IMaterialLifeHandler
{
    private DrillableMaterialSO _materialData;
    private float _currentLife;
    private IEventBus _eventBus;

    public DrillableMaterialSO MaterialData => _materialData;

    // Devuelve el tipo de material desde el SO
    public TypeOfMaterialToDrill MaterialType => _materialData.typeOfMaterialToDrill;

    public float CurrentLife => _currentLife;
    
    public event Action<IMaterialLifeHandler> OnDestroyed;
    
    private DiegeticBarController _diegeticBarController;
   
    private DrillStateReferences _drillStateReferences;

    public MaterialLifeHandler(DrillableMaterialSO material, DiegeticBarController diegeticBarController, 
        IEventBus eventBus, DrillStateReferences drillStateReferences)
    {
        _materialData = material;
        _currentLife = material.maxLife;
        _diegeticBarController = diegeticBarController;
        _drillStateReferences = drillStateReferences;
        _eventBus = eventBus;
    }

    // <summary>
    /// Aplica daño al material según el status (requerimientos), tick (deltaTime) y multipliers del SO.
    /// </summary>
    public void ApplyDrillDamage(RequirementStatus status, float tick)
    {
        // Elegí el multiplicador adecuado según el status
        float m = status switch
        {
            RequirementStatus.Valid => _materialData.multiplierValid,
            RequirementStatus.InvalidMode => _materialData.multiplierInvalidMode,
            RequirementStatus.InvalidPower => _materialData.multiplierInvalidPower,
            RequirementStatus.BothInvalid => _materialData.multiplierBothInvalid,
            _ => 0f
        };
        
        _currentLife -= _materialData.baseDamagePerSecond * m * tick;
        float norm = Mathf.Clamp01(_currentLife / _materialData.maxLife);

        _diegeticBarController.UpdateProgress(norm);

        if (!(_currentLife <= 0f)) return;

        _currentLife = 0f;
        OnDestroyed?.Invoke(this);
        // Además, podés publicar el evento global:
        _eventBus.Publish(new MaterialDestroyedEvent(_materialData));
        _drillStateReferences?.impulseSource.GenerateImpulse();
        if (_drillStateReferences == null)
        {
            Debug.Log("Drill is null!");    
        }
    }
    
}
