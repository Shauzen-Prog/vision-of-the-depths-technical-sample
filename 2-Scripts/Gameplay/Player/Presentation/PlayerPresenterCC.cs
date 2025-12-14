using UnityEngine;
using Zenject;

/// <summary>
/// Presenter del Player usando CharacterController para el movimiento y colisiones,
/// manteniendo un feeling cinematográfico (aceleración, desaceleración, fricción, damping).
/// La lógica de inputs sigue viniendo desde Application (PlayerMovementTick).
/// </summary>

[RequireComponent(typeof(CharacterController))]
public class PlayerPresenterCC : MonoBehaviour
{
    [Header("Velocidad / Suavizado")]
    [SerializeField, Range(0.5f, 8f)] private float _walkSpeed = 2.8f;
    [SerializeField, Range(0.01f, 1f)] private float _accelTime = 0.10f;
    [SerializeField, Range(0.01f, 1f)] private float _decelTime = 0.12f;
    [SerializeField, Range(0f, 10f)]  private float _groundFriction = 3.0f;
    [SerializeField, Range(0f, 0.5f)] private float _stopEpsilon = 0.06f;

    [Header("Gravedad / Piso")]
    [SerializeField, Range(0f, 30f)] private float _gravity = 9.81f;
    [SerializeField] private LayerMask _groundMask = ~0;   // incluir Ground y Stairs
    [SerializeField, Range(0.0f, 0.3f)] private float _groundSnap = 0.1f;
    
    [Header("Stairs / Slopes")]
    [Tooltip("Ángulo mínimo del suelo para considerar que estamos en escalera / pendiente pronunciada.")]
    [SerializeField, Range(0f, 60f)]
    private float _stairsAngleThreshold = 20f;

    [Tooltip("Factor de velocidad horizontal cuando estamos sobre una escalera / pendiente.")]
    [SerializeField, Range(0.2f, 1f)]
    private float _stairsSpeedMultiplier = 0.7f;

    [Header("Debug")]
    [SerializeField] private bool _drawGroundNormal = false;

    private CharacterController _cc;
    private PlayerMovementTick _movementTick;

    // Estado interno
    private Vector3 _velXZ;          // velocidad horizontal "propia" (controla el feeling)
    private Vector3 _velXZSmooth;    // estado interno de SmoothDamp
    private float _velY;             // velocidad vertical (gravedad)
    private bool _isGrounded;
    private Vector3 _groundNormal = Vector3.up;
    
    [Inject]
    public void Construct(PlayerMovementTick movementTick)
    {
        _movementTick = movementTick;
    }

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        // Recomendación: seteá estos en el Inspector
        // _cc.stepOffset ≈ 0.55–0.60 (según altura de tus escalones altos)
        // _cc.slopeLimit ≈ 60
        // _cc.skinWidth ≈ 0.03
        // _cc.minMoveDistance = 0f;
        _cc.minMoveDistance = 0f;
    }
    
    private void Update()
    {
        // Si el CharacterController está desactivado (por teleport, cutscene, etc.),
        // se salta el movimiento este frame para evitar errores y comportamientos raros.
        if (_cc == null || !_cc.enabled)
            return;
        
        float dt = Time.deltaTime;

        // 1) Leemos la salida del Application (dirección y magnitud normalizadas)
        var outp = _movementTick.GetOutput();
        Vector3 desiredDir = outp.WorldDirection;     // en world space
        float desiredMag   = outp.Magnitude;          // 0..1
        Vector3 targetVel  = desiredDir * (desiredMag * _walkSpeed);
        bool hasInput      = desiredMag > 0.01f;

        // 2) Suavizado de aceleración / frenado (feeling)
        float smoothTime = hasInput ? _accelTime : _decelTime;
        _velXZ = Vector3.SmoothDamp(_velXZ, targetVel, ref _velXZSmooth, smoothTime, Mathf.Infinity, dt);

        // 3) Fricción extra al soltar input
        if (_isGrounded && !hasInput)
        {
            float k = Mathf.Clamp01(1f - (_groundFriction * dt));
            _velXZ *= k;
            if (_velXZ.magnitude < _stopEpsilon)
            {
                _velXZ = Vector3.zero;
                _velXZSmooth = Vector3.zero;
            }
        }
        
        // 3.b) Si estamos sobre una escalera / pendiente pronunciada, reducimos la velocidad
        if (_isGrounded)
        {
            float angle = Vector3.Angle(_groundNormal, Vector3.up);
            if (angle >= _stairsAngleThreshold)
            {
                _velXZ *= _stairsSpeedMultiplier;
            }
        }

        // 4) Gravedad
        if (_cc.isGrounded)
        {
            _velY = -2f; // pegado suave
        }
        else
        {
            _velY -= _gravity * dt;
        }

        // 5) Move con CharacterController
        Vector3 motion = new Vector3(_velXZ.x, _velY, _velXZ.z) * dt;
        _cc.Move(motion);

        // 6) Grounding + normal (para futuros efectos/cámara)
        UpdateGrounding(dt);

       
    }
    
    /// <summary>
    /// Actualiza estado de grounded y normal del suelo con un ray corto.
    /// </summary>
    private void UpdateGrounding(float dt)
    {
        // Simple chequeo: un raydown desde la base del CC
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float dist = _groundSnap + 0.2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, dist, _groundMask, QueryTriggerInteraction.Ignore))
        {
            _isGrounded = true;
            _groundNormal = Vector3.Lerp(_groundNormal, hit.normal, 0.2f);
        }
        else
        {
            _isGrounded = false;
            _groundNormal = Vector3.up;
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_drawGroundNormal) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, _groundNormal);
    }
#endif
}
