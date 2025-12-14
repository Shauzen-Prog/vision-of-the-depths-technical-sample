using System;
using UnityEngine;
using Cinemachine;
using Zenject;

/// <summary>
/// Permite cambiar entre varios puntos de vista de un closeup usando
/// el eje horizontal de movimiento (A/D, stick izquierdo o pad).
/// Usa un "driver" de cámara que interpola suavemente entre los
/// distintos puntos de vista, evitando saltos bruscos. También puede
/// respetar la rotación local de cada punto.
/// </summary>
public class CloseupViewSwitcher : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera _closeupCamera;

    [Tooltip("Puntos de vista disponibles para este closeup (panel, monitor, etc.).")]
    [SerializeField] private Transform[] _viewPoints;

    [Header("Input")]
    [Tooltip("Mínimo valor absoluto del eje horizontal para considerar que hubo input (A/D, stick, flechitas).")]
    [SerializeField] private float _horizontalDeadZone = 0.5f;

    [Tooltip("Tiempo mínimo entre cambios de vista cuando se mantiene el eje inclinado.")]
    [SerializeField] private float _repeatDelay = 0.3f;

    [Header("Smooth Camera")]
    [Tooltip("Velocidad de interpolación hacia el siguiente punto de vista.")]
    [SerializeField] private float _lerpSpeed = 6f;

    [Tooltip("Si está activo, usa la rotación local de cada ViewPoint como destino.")]
    [SerializeField] private bool _useLocalRotation = true;

    [Tooltip("Distancia mínima para considerar que ya se alcanzó la posición destino.")]
    [SerializeField] private float _positionThreshold = 0.001f;

    [Tooltip("Ángulo mínimo para considerar que ya se alcanzó la rotación destino.")]
    [SerializeField] private float _rotationThreshold = 0.1f;

    private InputProcessor _inputProcessor;
    private IEventBus _eventBus;

    private bool _closeupActive;
    private bool _dialogueRunning;

    private int _currentIndex;

    private float _lastSwitchTime;
    private bool _waitingForRelease;

    private IDisposable _closeupStartSub;
    private IDisposable _closeupEndSub;
    private IDisposable _dialogueStartSub;
    private IDisposable _dialogueEndSub;

    // Driver que Cinemachine sigue/observa; se interpola entre los ViewPoints.
    private Transform _driver;
    private Coroutine _lerpRoutine;

    [Inject]
    private void Construct(InputProcessor inputProcessor, IEventBus eventBus)
    {
        _inputProcessor = inputProcessor;
        _eventBus = eventBus;
    }

    private void OnEnable()
    {
        _closeupStartSub = _eventBus.Subscribe<CloseupStartedEvent>(_ => OnCloseupStarted());
        _closeupEndSub   = _eventBus.Subscribe<CloseupEndedEvent>(_ => OnCloseupEnded());
        _dialogueStartSub = _eventBus.Subscribe<DialogueStartedEvent>(_ => _dialogueRunning = true);
        _dialogueEndSub   = _eventBus.Subscribe<DialogueEndedEvent>(_ => _dialogueRunning = false);
    }

    private void OnDisable()
    {
        _closeupStartSub?.Dispose();
        _closeupEndSub?.Dispose();
        _dialogueStartSub?.Dispose();
        _dialogueEndSub?.Dispose();

        _closeupStartSub = null;
        _closeupEndSub = null;
        _dialogueStartSub = null;
        _dialogueEndSub = null;
    }

    private void Start()
    {
        // Si no hay cámara o viewpoints, no hacemos nada.
        if (_closeupCamera == null || _viewPoints == null || _viewPoints.Length == 0)
            return;

        // Crear el driver que Cinemachine va a seguir/observar.
        _driver = new GameObject($"{name}_ViewDriver").transform;
        _driver.position = _viewPoints[_currentIndex].position;
        _driver.rotation = _viewPoints[_currentIndex].rotation;

        _closeupCamera.Follow = _driver;
        _closeupCamera.LookAt = _driver;

        // Asegurar que arrancamos mirando al primer punto.
        ApplyView(_currentIndex, instant: true);
    }

    private void Update()
    {
        // Sólo responde si estamos en closeup y NO hay diálogo activo.
        if (!_closeupActive || _dialogueRunning)
            return;

        if (_viewPoints == null || _viewPoints.Length == 0 || _closeupCamera == null || _driver == null)
            return;

        var snap = _inputProcessor.GetProcessedSnapshot();
        float horizontal = snap.Move.x; // A/D + stick + flechitas si están mapeadas al mismo eje
        float abs = Mathf.Abs(horizontal);

        // Sin input horizontal → resetea la espera de "volver a permitir cambio"
        if (abs < _horizontalDeadZone * 0.5f)
        {
            _waitingForRelease = false;
            return;
        }

        // Dead zone dura
        if (abs < _horizontalDeadZone)
            return;

        // Si todavía no "se liberó" el eje o falta delay, no hacemos nada
        if (_waitingForRelease && (Time.unscaledTime - _lastSwitchTime) < _repeatDelay)
            return;

        int direction = horizontal > 0f ? 1 : -1;
        SwitchView(direction);

        _lastSwitchTime = Time.unscaledTime;
        _waitingForRelease = true;
    }

    /// <summary>
    /// Cambia el índice de vista actual aplicando wrap-around.
    /// </summary>
    private void SwitchView(int direction)
    {
        if (_viewPoints == null || _viewPoints.Length <= 1)
            return;

        _currentIndex += direction;

        if (_currentIndex < 0)
            _currentIndex = _viewPoints.Length - 1;
        else if (_currentIndex >= _viewPoints.Length)
            _currentIndex = 0;

        ApplyView(_currentIndex, instant: false);
    }

    /// <summary>
    /// Aplica la vista dada, iniciando una interpolación suave del driver.
    /// </summary>
    /// <param name="index">Índice del ViewPoint destino.</param>
    /// <param name="instant">Si está en true, salta directo sin lerp.</param>
    private void ApplyView(int index, bool instant)
    {
        if (_viewPoints == null || _viewPoints.Length == 0 || _closeupCamera == null)
            return;

        index = Mathf.Clamp(index, 0, _viewPoints.Length - 1);
        Transform target = _viewPoints[index];

        if (target == null)
            return;

        if (_driver == null)
        {
            _driver = new GameObject($"{name}_ViewDriver").transform;
            _closeupCamera.Follow = _driver;
            _closeupCamera.LookAt = _driver;
        }

        if (instant)
        {
            _driver.position = target.position;
            _driver.rotation = target.rotation;
            return;
        }

        if (_lerpRoutine != null)
            StopCoroutine(_lerpRoutine);

        _lerpRoutine = StartCoroutine(LerpToTarget(target));
    }

    /// <summary>
    /// Interpola la posición y rotación del driver hacia el ViewPoint destino,
    /// produciendo un cambio suave en la cámara.
    /// </summary>
    private System.Collections.IEnumerator LerpToTarget(Transform target)
    {
        while (true)
        {
            float dt = Time.unscaledDeltaTime;
            _driver.position = Vector3.Lerp(_driver.position, target.position, dt * _lerpSpeed);

            if (_useLocalRotation)
            {
                _driver.rotation = Quaternion.Lerp(_driver.rotation, target.rotation, dt * _lerpSpeed);
            }
            else
            {
                _driver.rotation = Quaternion.Slerp(_driver.rotation, target.rotation, dt * _lerpSpeed);
            }

            bool posDone = Vector3.Distance(_driver.position, target.position) <= _positionThreshold;
            bool rotDone = Quaternion.Angle(_driver.rotation, target.rotation) <= _rotationThreshold;

            if (posDone && rotDone)
            {
                _driver.position = target.position;
                _driver.rotation = target.rotation;
                yield break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Callback cuando se entra a un closeup (evento global).
    /// Resetea estado interno y asegura que se muestre la primera vista.
    /// </summary>
    private void OnCloseupStarted()
    {
        _closeupActive = true;
        _waitingForRelease = false;
        _lastSwitchTime = 0f;

        _currentIndex = 0;
        ApplyView(_currentIndex, instant: true);
    }

    /// <summary>
    /// Callback cuando se sale del closeup (evento global).
    /// Limpia flags de input.
    /// </summary>
    private void OnCloseupEnded()
    {
        _closeupActive = false;
        _waitingForRelease = false;
    }
}
