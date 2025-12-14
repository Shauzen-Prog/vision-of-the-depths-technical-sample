using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Administra el ciclo de materiales virtuales, publica eventos para el drill.
/// </summary>
public class MaterialManager : MonoBehaviour
{
    [Header("Secuencias disponibles")]
    [SerializeField] private List<MaterialSequenceSO> sequences;

    [Header("Secuencia seleccionada (índice en la lista)")]
    [SerializeField] private int selectedSequenceIndex;

    [Header("Tiempo de delay entre materiales")]
    [SerializeField] private float travelTimeToDrill = 2f;

    private MaterialSequenceSO _currentSequence;

    private int _currentIndex;
    private bool _isLoopingLast;
    private bool _hasTriggeredLoopCallback;
    private bool _hasStartedSequence;

    private IEventBus _eventBus;
    private IDisposable _destroyedSub;
    private IDisposable _resumeSub;
    private IDisposable _changeSeqSub;
    private IDisposable _modeChangedSub;
    private IDisposable _powerChangedSub;

    private int _currentMode;
    private int _currentPower;

    [Inject] private IDrillRequirementChecker _requirementChecker;

    /// <summary>
    /// Evento disparado cuando una secuencia arranca efectivamente.
    /// </summary>
    public event Action OnSequenceStarted;

    /// <summary>
    /// Evento disparado cuando la secuencia termina sin loop (reservado para futuro).
    /// </summary>
    //public event Action OnSequenceEnded;

    /// <summary>
    /// Evento disparado cuando la secuencia entra en loop y se perfora bien al menos una vez.
    /// </summary>
    public event Action OnSequenceLooping;

    /// <summary>
    /// Secuencia actual activa.
    /// </summary>
    public MaterialSequenceSO CurrentSequence => _currentSequence;

    /// <summary>
    /// Devuelve el primer material de la secuencia actual, o null si no hay.
    /// </summary>
    public DrillableMaterialSO FirstMaterial =>
        _currentSequence != null && _currentSequence.materials.Count > 0
            ? _currentSequence.materials[0]
            : null;

    /// <summary>
    /// Devuelve el material actual según el índice interno.
    /// </summary>
    public DrillableMaterialSO CurrentMaterial =>
        _currentSequence != null &&
        _currentSequence.materials != null &&
        _currentIndex >= 0 &&
        _currentIndex < _currentSequence.materials.Count
            ? _currentSequence.materials[_currentIndex]
            : null;

    /// <summary>
    /// Devuelve el próximo material (o el de loop si corresponde).
    /// </summary>
    public DrillableMaterialSO NextMaterial =>
        (_currentSequence != null &&
         _currentSequence.materials != null &&
         (_currentIndex + 1) < _currentSequence.materials.Count)
            ? _currentSequence.materials[_currentIndex + 1]
            : (_currentSequence?.loopLastMaterial == true && _currentSequence.materials.Count > 0)
                ? _currentSequence.materials[^1]
                : null;


    #region Inyección y setup

    /// <summary>
    /// Inyecta el EventBus.
    /// </summary>
    [Inject]
    public void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private void OnEnable()
    {
        _destroyedSub = _eventBus.Subscribe<MaterialDestroyedEvent>(OnMaterialDestroyed);
        _resumeSub = _eventBus.Subscribe<DrillResumeEvent>(OnDrillResume);
        _changeSeqSub = _eventBus.Subscribe<ChangeMaterialSequenceEvent>(OnChangeSequence);

        _modeChangedSub = _eventBus.Subscribe<DrillModeChangedEvent>(OnDrillModeChanged);
        _powerChangedSub = _eventBus.Subscribe<DrillPowerChangedEvent>(OnDrillPowerChanged);

        if (selectedSequenceIndex < 0 || selectedSequenceIndex >= sequences.Count)
        {
            Debug.LogError("[NewMaterialManager] Índice de secuencia fuera de rango.");
            return;
        }

        // Setea la secuencia inicial pero no la arranca hasta que el drill se "reanuda".
        _currentSequence = sequences[selectedSequenceIndex];

        OnSequenceLooping += PublishLoopEvent;
    }

    private void OnDisable()
    {
        _destroyedSub?.Dispose();
        _resumeSub?.Dispose();
        _changeSeqSub?.Dispose();
        _modeChangedSub?.Dispose();
        _powerChangedSub?.Dispose();

        OnSequenceLooping -= PublishLoopEvent;

        ResetSequence();
    }

    #endregion

    #region Drill state

    /// <summary>
    /// Actualiza el modo del drill (lo setea un controlador de gameplay).
    /// </summary>
    public void SetDrillMode(int mode) => _currentMode = mode;

    /// <summary>
    /// Actualiza la potencia del drill (lo setea un controlador de gameplay).
    /// </summary>
    public void SetDrillPower(int power) => _currentPower = power;

    private void OnDrillModeChanged(DrillModeChangedEvent e) => _currentMode = e.Mode;
    private void OnDrillPowerChanged(DrillPowerChangedEvent e) => _currentPower = e.Power;

    /// <summary>
    /// Devuelve el tipo de material que se debe usar para chequear requisitos.
    /// Si está en loop, valida contra el último material (el que se repite).
    /// </summary>
    private TypeOfMaterialToDrill GetMaterialTypeForRequirement()
    {
        if (_currentSequence == null ||
            _currentSequence.materials == null ||
            _currentSequence.materials.Count == 0)
        {
            return default;
        }

        if (_isLoopingLast)
        {
            var last = _currentSequence.materials[^1];
            return last.typeOfMaterialToDrill;
        }

        var cm = CurrentMaterial ?? FirstMaterial ?? _currentSequence.materials[0];
        return cm.typeOfMaterialToDrill;
    }

    /// <summary>
    /// Chequea requisitos de modo/potencia contra el material correspondiente.
    /// </summary>
    private bool AreLoopRequirementsMet()
    {
        var materialType = GetMaterialTypeForRequirement();
        var status = _requirementChecker.CheckRequirementStatus(materialType, _currentMode, _currentPower);
        return status == RequirementStatus.Valid;
    }

    #endregion

    #region Ciclo de vida de la secuencia

    /// <summary>
    /// Handler del evento de reanudación del drill. Arranca la secuencia seleccionada si aún no inició.
    /// </summary>
    private void OnDrillResume(DrillResumeEvent evt)
    {
        if (_hasStartedSequence)
            return;

        StartCoroutine(DelayedStartSequence());
    }

    /// <summary>
    /// Handle del evento de cambio de secuencia en runtime.
    /// </summary>
    private void OnChangeSequence(ChangeMaterialSequenceEvent evt)
    {
        if (evt.sequence == null)
            return;

        if (ReferenceEquals(evt.sequence, _currentSequence))
            return;

        StartSequence(evt.sequence);
    }

    /// <summary>
    /// Espera un frame antes de iniciar la secuencia (permite que el resto del sistema se inicialice).
    /// </summary>
    private IEnumerator DelayedStartSequence()
    {
        yield return null;

        if (_hasStartedSequence)
            yield break;

        StartSelectedSequence();
    }

    /// <summary>
    /// Inicia la secuencia seleccionada desde el inspector o la ya seteada.
    /// </summary>
    public void StartSelectedSequence()
    {
        if (_currentSequence != null)
        {
            StartSequence(_currentSequence);
            return;
        }

        if (selectedSequenceIndex < 0 || selectedSequenceIndex >= sequences.Count)
        {
            Debug.LogError("[NewMaterialManager] Índice de secuencia fuera de rango.");
            PublishNextMaterialsListForUI(); // UI vacía en caso de error
            return;
        }

        StartSequence(sequences[selectedSequenceIndex]);
    }

    /// <summary>
    /// Inicia una secuencia específica.
    /// </summary>
    public void StartSequence(MaterialSequenceSO sequence)
    {
        StopAllCoroutines();

        _currentSequence = sequence;
        _hasStartedSequence = true;
        _currentIndex = 0;
        _isLoopingLast = false;
        _hasTriggeredLoopCallback = false;

        OnSequenceStarted?.Invoke();

        StartCoroutine(PublishNextMaterialWithDelay(travelTimeToDrill));
    }

    /// <summary>
    /// Resetea estado interno sin modificar la secuencia actual.
    /// </summary>
    public void ResetSequence()
    {
        _hasStartedSequence = false;
        _currentIndex = 0;
        _isLoopingLast = false;
        _hasTriggeredLoopCallback = false;
    }

    /// <summary>
    /// Devuelve el siguiente material según el índice (sin tener en cuenta loop infinito).
    /// </summary>
    public DrillableMaterialSO GetNextMaterial()
    {
        if (_currentSequence == null || _currentSequence.materials.Count == 0)
            return null;

        int nextIndex = _currentIndex + 1;
        if (nextIndex < _currentSequence.materials.Count)
            return _currentSequence.materials[nextIndex];

        if (_currentSequence.loopLastMaterial)
            return _currentSequence.materials[^1];

        return null;
    }

    #endregion

    #region Spawning y avance

    /// <summary>
    /// Publica el próximo material con un delay inicial (primer spawn).
    /// </summary>
    private IEnumerator PublishNextMaterialWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PublishNextMaterial();
    }

    /// <summary>
    /// Spawnea el material actual (o el de loop) y actualiza la UI.
    /// No resuelve lógica de loop ni avance de índice.
    /// </summary>
    private void PublishNextMaterial()
    {
        if (_currentSequence == null ||
            _currentSequence.materials == null ||
            _currentSequence.materials.Count == 0)
        {
            PublishNextMaterialsListForUI();
            return;
        }

        DrillableMaterialSO materialToSpawn;

        // Si el índice está dentro de la lista, usa ese material;
        // si se pasó del final, siempre usa el último (modo loop).
        if (_currentIndex < _currentSequence.materials.Count)
        {
            materialToSpawn = _currentSequence.materials[_currentIndex];
        }
        else
        {
            materialToSpawn = _currentSequence.materials[^1];
        }

        _eventBus.Publish(new MaterialSpawnedEvent(materialToSpawn));
        _eventBus.Publish(new UpdateMaterialToDrillUI(true));

        PublishNextMaterialsListForUI();
    }

    /// <summary>
    /// Handler del evento de destrucción de material. 
    /// Si aún no está en loop, sólo avanza. Si ya está en loop, 
    /// chequea requisitos y dispara el evento de loop cuando se perfora bien.
    /// </summary>
    private void OnMaterialDestroyed(MaterialDestroyedEvent evt)
    {
        if (_currentSequence == null ||
            _currentSequence.materials == null ||
            _currentSequence.materials.Count == 0)
            return;

        // Si ya estamos en loop, cada destrucción es del mismo material.
        // El evento de loop se dispara SOLO cuando se cumple requisito por primera vez.
        if (_isLoopingLast && !_hasTriggeredLoopCallback && AreLoopRequirementsMet())
        {
            _hasTriggeredLoopCallback = true;
            OnSequenceLooping?.Invoke();
        }

        // Avance normal / loop visual
        StartCoroutine(NextMaterialWithDelay());
    }

    /// <summary>
    /// Avanza el índice después del delay de viaje al drill.
    /// Si entra en loop, mantiene el índice más allá del final y siempre spawnea el último material.
    /// </summary>
    private IEnumerator NextMaterialWithDelay()
    {
        yield return new WaitForSeconds(travelTimeToDrill);

        if (_currentSequence == null ||
            _currentSequence.materials == null ||
            _currentSequence.materials.Count == 0)
            yield break;

        if (_isLoopingLast)
        {
            // En loop no se avanza índice, solo se repite el último material.
            PublishNextMaterial();
        }
        else
        {
            _currentIndex++;

            // Si se pasó del final y la secuencia está marcada para loop,
            // entra en modo loop del último.
            if (_currentSequence.loopLastMaterial &&
                _currentIndex >= _currentSequence.materials.Count)
            {
                _isLoopingLast = true;
            }

            PublishNextMaterial();
        }
    }

    #endregion

    #region UI: lista de próximos materiales

    /// <summary>
    /// Construye la lista de materiales visibles para la UI.
    /// Siempre devuelve exactamente count elementos.
    /// Si la secuencia está en loop o se pasó del final, rellena con el último.
    /// </summary>
    private List<DrillableMaterialSO> BuildNextMaterials(int count)
    {
        var result = new List<DrillableMaterialSO>();

        if (_currentSequence == null ||
            _currentSequence.materials == null ||
            _currentSequence.materials.Count == 0)
        {
            for (int i = 0; i < count; i++)
                result.Add(null);

            return result;
        }

        // Agrega materiales reales desde el índice actual hacia adelante.
        for (int i = _currentIndex;
             i < _currentSequence.materials.Count && result.Count < count;
             i++)
        {
            // Si el índice se pasó del final, este for no corre.
            result.Add(_currentSequence.materials[i]);
        }

        // Rellena con el último material si faltan slots (modo loop).
        var last = _currentSequence.materials[^1];

        while (result.Count < count)
            result.Add(last);

        return result;
    }

    /// <summary>
    /// Publica hacia la UI los próximos N materiales (puede incluir nulls si no hay secuencia).
    /// </summary>
    private void PublishNextMaterialsListForUI(int maxVisible = 6)
    {
        var list = BuildNextMaterials(maxVisible);
        _eventBus.Publish(new NextMaterialsUpdatedEvent(list));
    }

    #endregion

    #region Eventos auxiliares

    /// <summary>
    /// Puente que publica el evento global cuando la secuencia entra en loop válido.
    /// </summary>
    private void PublishLoopEvent()
    {
        _eventBus?.Publish(new MaterialSequenceLoopedEvent());
    }

    #endregion
}
