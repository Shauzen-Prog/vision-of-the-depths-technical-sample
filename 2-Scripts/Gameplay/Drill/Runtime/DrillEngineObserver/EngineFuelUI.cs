using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EngineFuelUI : MonoBehaviour
{
    [SerializeField] private EngineId engineId; // Setealo por Inspector
    [SerializeField] private TMPro.TextMeshProUGUI fuelText;
    [SerializeField] private TMPro.TextMeshProUGUI stateText;

    [Inject]
    public void Construct(IEventBus eventBus)
    {
        eventBus.Subscribe<DrillFuelChangedEvent>(OnFuelChanged);
    }

    private void OnFuelChanged(DrillFuelChangedEvent evt)
    {
        if (evt.Engine != engineId) return; // Solo escucha su motor asignado

        fuelText.text = $"Fuel: {evt.Current:0}/{evt.Max:0}";
        stateText.text = evt.IsOn ? "State: ON" : "State: OFF";
    }
}
