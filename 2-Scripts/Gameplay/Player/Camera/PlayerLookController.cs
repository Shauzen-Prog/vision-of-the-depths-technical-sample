using System;
using UnityEditor;
using UnityEngine;
using Zenject;

/// <summary>
/// Aplica el look procesado (suavizado) al Player (yaw) y a la cámara (pitch).
/// Usa PlayerControlService para respetar bloqueos de look (closeup, diálogo, etc.).
/// Diseñado para "feeling" cinematográfico: sin brusquedad ni twitch FPS.
/// </summary>
public class PlayerLookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _yawRoot;      // Raíz que rota horizontalmente (Player)
    [SerializeField] private Transform _cameraPivot;  // Hijo que rota verticalmente (pitch). Parent de la cámara

    private InputProcessor _processor;
    private InputConfigSO _config;
    private IPlayerControlService _playerControl;

    // Estado acumulado (en grados)
    private float _yaw;
    private float _pitch;
    
    public Transform YawRoot => _yawRoot != null ? _yawRoot : transform;
    
    private IEventBus _eventBus;
    private IDisposable _teleportSub;
    
    [Inject]
    public void Construct(
        InputProcessor processor,
        InputConfigSO config,
        IEventBus eventBus,
        IPlayerControlService playerControl)
    {
        _processor = processor;
        _config = config;
        _eventBus = eventBus;
        _playerControl = playerControl;
    }
    
    private void OnEnable()
    {
        if (_eventBus != null)
        {
            _teleportSub = _eventBus.Subscribe<PlayerTeleportedEvent>(OnPlayerTeleported);
        }
    }

    private void OnDisable()
    {
        _teleportSub?.Dispose();
        _teleportSub = null;
    }
    
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Defaults razonables
        if (_yawRoot == null) _yawRoot = transform;
        if (_cameraPivot == null)
        {
            var cam = Camera.main != null ? Camera.main.transform : null;
            _cameraPivot = cam != null ? cam.parent : transform;
        }

        // Tomar rotación inicial exacta del Player y de la cámara
        Vector3 yawEuler = _yawRoot.rotation.eulerAngles;
        Vector3 pitchEuler = _cameraPivot.localRotation.eulerAngles;

        _yaw = yawEuler.y;
        _pitch = NormalizePitch(pitchEuler.x);

        // Aplicar inmediatamente la rotación inicial para evitar salto visual
        _yawRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void Update()
    {
        // Si el look está bloqueado (closeup, diálogo, pausa, etc.),
        // no se procesa input de cámara este frame.
        if (_playerControl != null && !_playerControl.CanLook)
            return;
        
        // 1) Tomar delta suavizado del processor (ya con sensibilidad / invert si aplica)
        var snap = _processor.GetProcessedSnapshot();
        Vector2 lookDelta = snap.Look;

        // 2) Acumular yaw/pitch (delta -> ángulo absoluto)
        // Nota: se resta en pitch para la convención de mouse (arriba = negativo)
        _yaw   += lookDelta.x;
        _pitch -= lookDelta.y;

        // 3) Clampear pitch con los límites del config
        _pitch = Mathf.Clamp(_pitch, _config.minPitch, _config.maxPitch);

        // 4) Aplicar rotaciones
        _yawRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
    
    /// <summary>
    /// Sincroniza el yaw interno cuando el player fue teletransportado.
    /// </summary>
    private void OnPlayerTeleported(PlayerTeleportedEvent evt)
    {
        // Tomamos el yaw de la rotación destino
        float yaw = evt.Rotation.eulerAngles.y;

        // Actualizamos el estado interno y el YawRoot
        _yaw = yaw;
        _yawRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
    }
    
    /// <summary>
    /// Convierte un ángulo 0..360 a un rango -180..180 para trabajar cómodo con el clamp.
    /// </summary>
    private static float NormalizePitch(float xAngle)
    {
        if (xAngle > 180f) xAngle -= 360f;
        return xAngle;
    }
    
    /// <summary>
    /// Sincroniza el yaw interno con una rotación en mundo.
    /// Esto se usa después de teleports u otros cambios bruscos.
    /// </summary>
    public void SnapYawTo(Quaternion worldRotation)
    {
        Vector3 euler = worldRotation.eulerAngles;
        _yaw = euler.y;
        YawRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
    }

    /// <summary>
    /// Versión conveniente para usar directamente un Transform destino.
    /// </summary>
    public void SnapYawTo(Transform target)
    {
        if (target == null)
            return;

        SnapYawTo(target.rotation);
    }
}
