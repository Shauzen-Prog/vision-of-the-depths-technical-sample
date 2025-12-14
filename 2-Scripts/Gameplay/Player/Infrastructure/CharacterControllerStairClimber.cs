using UnityEngine;

/// <summary>
/// CC "ghost" para predecir subida de escalones. NO mueve al Player real.
/// </summary>
[DisallowMultipleComponent]
public sealed class CharacterControllerStairClimber : MonoBehaviour, IStairClimber
{
   [Header("Probe Shape (match player)")]
    [SerializeField] private float _height = 1.8f;
    [SerializeField] private float _radius = 0.35f;

    [Header("Step Tuning")]
    [SerializeField] private float _stepOffset = 0.58f;
    [SerializeField] private float _slopeLimit = 60f;
    [SerializeField] private float _skinWidth = 0.03f;

    [Header("Layers")]
    [SerializeField] private LayerMask _groundAndStairs = ~0;

    private CharacterController _cc;

    private void Awake()
    {
        // No renombramos el GameObject (evita confusión con otros hijos)
        _cc = GetComponent<CharacterController>();
        if (_cc == null)
        {
            _cc = gameObject.AddComponent<CharacterController>();
        }

        _cc.minMoveDistance = 0f;
        _cc.enableOverlapRecovery = true;
        _cc.slopeLimit = _slopeLimit;
        _cc.stepOffset = _stepOffset;
        _cc.skinWidth = _skinWidth;
        _cc.center = Vector3.up * (_height * 0.5f);
        _cc.height = _height;
        _cc.radius = _radius;

#if UNITY_EDITOR
        Debug.Log($"[CCProbe] Ready. h={_height} r={_radius} step={_stepOffset}", this);
#endif
    }

    public bool TryClimb(Vector3 currentCenter, Vector3 horizontalVelocity, float dt, out Vector3 climbedCenter)
    {
        climbedCenter = currentCenter;

        // Alinear la "base" del CC con el centro del Player
        Transform proxy = transform;
        proxy.position = currentCenter - Vector3.up * (_height * 0.5f - _radius);

        Vector3 attempt = new Vector3(horizontalVelocity.x, -2f, horizontalVelocity.z) * dt;
        _cc.Move(attempt);

        Vector3 ccBase = proxy.position;
        Vector3 newCenter = ccBase + Vector3.up * (_height * 0.5f - _radius);

        Vector2 movedXZ = new Vector2(newCenter.x - currentCenter.x, newCenter.z - currentCenter.z);
        float dy = newCenter.y - currentCenter.y;
        bool advanced = movedXZ.sqrMagnitude > 0.0001f;
        bool climbedOk = dy >= -0.05f && dy <= (_stepOffset + 0.05f);

        if (!advanced || !climbedOk) return false;

        // Validación volumétrica: que la cápsula del Player entre en esa posición
        float half = Mathf.Max(_height * 0.5f - _radius, 0.01f);
        Vector3 bottom = newCenter + Vector3.up * (-half);
        Vector3 top    = newCenter + Vector3.up * ( half);
        bool overlap = Physics.CheckCapsule(bottom, top, _radius - 0.001f, _groundAndStairs, QueryTriggerInteraction.Ignore);
        if (overlap) return false;

        climbedCenter = newCenter;
        return true;
    }

    // Helpers públicos para setear desde el Installer si querés
    public void Configure(float height, float radius, float stepOffset, float slopeLimit, float skinWidth, LayerMask groundAndStairs)
    {
        _height = height;
        _radius = radius;
        _stepOffset = stepOffset;
        _slopeLimit = slopeLimit;
        _skinWidth = skinWidth;
        _groundAndStairs = groundAndStairs;
        if (_cc != null) Awake(); // re-aplica
    }
}
