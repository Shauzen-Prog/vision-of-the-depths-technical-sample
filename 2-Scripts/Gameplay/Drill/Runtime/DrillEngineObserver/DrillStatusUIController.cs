using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class DrillStatusUIController : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private TMP_Text lifeText;
    [SerializeField] private TMP_Text maxLifeText;
    [SerializeField] private TMP_Text percentageText; // opcional

    private IEventBus _eventBus;
    private IDisposable _subscription;

    // Último valor mostrado para evitar actualizaciones redundantes
    private float _lastLife = -1;
    private float _lastMax = -1;

    [Inject]
    public void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private void OnEnable()
    {
        _subscription = _eventBus.Subscribe<UpdateDrillLifeUIEvent>(OnDrillLifeChanged);
    }

    private void OnDestroy()
    {
        _subscription?.Dispose();
    }

    /// <summary>
    /// Recibe el evento y actualiza los textos de UI.
    /// </summary>
    private void OnDrillLifeChanged(UpdateDrillLifeUIEvent evt)
    {
        // Solo actualiza si cambió
        if (!Mathf.Approximately(_lastLife, evt.CurrentLife) || !Mathf.Approximately(_lastMax, evt.MaxLife))
        {
            _lastLife = evt.CurrentLife;
            _lastMax = evt.MaxLife;
            if (lifeText != null)
                lifeText.text = $"{evt.CurrentLife:0}";
            if (maxLifeText != null)
                maxLifeText.text = $"/ {evt.MaxLife:0}";
            if (percentageText != null && evt.MaxLife > 0)
                percentageText.text = $"{(evt.CurrentLife / evt.MaxLife * 100f):0}\u0025";
        }
    }
}
