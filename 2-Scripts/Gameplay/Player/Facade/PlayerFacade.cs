using System.Collections;
using UnityEngine;
using Zenject;

/// <summary>
/// Implementación por defecto de IPlayerFacade.
/// Encapsula el teletransporte del jugador usando un CharacterController,
/// evitando glitches típicos al mover directamente el transform.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class PlayerFacade : MonoBehaviour, IPlayerFacade
{
    // Añadido para corregir la caída post-teleport
    [SerializeField] private LayerMask _groundMask = ~0; // Máscara para el suelo
    [SerializeField, Range(0.01f, 1f)] private float _probeUp = 0.25f; // Distancia desde la que se inicia el raycast hacia abajo
    [SerializeField, Range(0.5f, 5f)] private float _probeDown = 2.0f; // Distancia máxima que buscamos hacia abajo
    [SerializeField] private float _postEnableDownMove = 0.02f;
    
    private CharacterController _characterController;
    private Coroutine _pendingTeleport;
    private IPlayerFacadeService _facadeService;

    /// <summary>
    /// Inicializa referencias internas del PlayerFacade.
    /// </summary>
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
    
    /// <summary>
    /// Registra este facade en el servicio global cuando Zenject
    /// inyecta las dependencias.
    /// </summary>
    [Inject]
    private void Construct(IPlayerFacadeService facadeService)
    {
        _facadeService = facadeService;
        _facadeService.Register(this);
    }

    /// <summary>
    /// Teletransporta al jugador usando un Transform como destino.
    /// </summary>
    /// <param name="target">Transform con la posición/rotación destino.</param>
    /// <param name="alignYawOnly">
    /// Si está en true, solo se respeta el yaw (rotación en Y) del destino.
    /// </param>
    public void TeleportTo(Transform target, bool alignYawOnly = false)
    {
        if (target == null)
            return;

        TeleportTo(target.position, target.rotation, alignYawOnly);
    }

    /// <summary>
    /// Teletransporta al jugador a una posición y rotación dadas,
    /// gestionando correctamente el CharacterController.
    /// </summary>
    /// <param name="position">Posición destino en mundo.</param>
    /// <param name="rotation">Rotación destino en mundo.</param>
    /// <param name="alignYawOnly">
    /// Si está en true, solo se respeta el yaw (rotación en Y) del destino.
    /// </param>
    public void TeleportTo(Vector3 position, Quaternion rotation, bool alignYawOnly = false)
    {
        Quaternion finalRotation = alignYawOnly
            ? BuildYawOnlyRotation(rotation)
            : rotation;

        if (_pendingTeleport != null)
        {
            StopCoroutine(_pendingTeleport);
            _pendingTeleport = null;
        }

        _pendingTeleport = StartCoroutine(TeleportRoutine(position, finalRotation));
    }

    /// <summary>
    /// Corrutina que aplica el teleport deshabilitando el CharacterController
    /// durante un frame para evitar comportamientos extraños.
    /// </summary>
    /// <param name="position">Posición destino.</param>
    /// <param name="rotation">Rotación destino.</param>
    private IEnumerator TeleportRoutine(Vector3 position, Quaternion rotation)
    {
        bool wasEnabled = _characterController.enabled;
        _characterController.enabled = false;
        
        ApplyTransform(position, rotation);
        Physics.SyncTransforms();
        
        SnapToGround(position);

        yield return null;

        _characterController.enabled = true;
        
        if (wasEnabled)
        {
            _characterController.Move(Vector3.down * _postEnableDownMove);
        }
    }
    
    private void SnapToGround(Vector3 basePosition)
    {
        Vector3 origin = basePosition + Vector3.up * _probeUp;
        float radius = Mathf.Max(0.01f, _characterController.radius - 0.02f);
        float distance = _probeUp + _probeDown;

        if (!Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out RaycastHit hit,
                distance,
                _groundMask,
                QueryTriggerInteraction.Ignore))
            return;

        float targetWorldCenterY = hit.point.y + (_characterController.height * 0.5f);
        float snappedY = targetWorldCenterY - _characterController.center.y;

        transform.position = new Vector3(basePosition.x, snappedY, basePosition.z);
        Physics.SyncTransforms();
    }

    /// <summary>
    /// Aplica el teleport de forma inmediata, sin corrutina.
    /// Útil cuando el objeto aún no está activo en jerarquía.
    /// </summary>
    /// <param name="position">Posición destino.</param>
    /// <param name="rotation">Rotación destino.</param>
    private void ApplyTeleportImmediate(Vector3 position, Quaternion rotation)
    {
        bool wasEnabled = _characterController.enabled;
        _characterController.enabled = false;

        ApplyTransform(position, rotation);
        Physics.SyncTransforms();

        _characterController.enabled = wasEnabled;
    }

    /// <summary>
    /// Aplica posición y rotación al transform del jugador.
    /// Separado para facilitar cambios futuros o tests parciales.
    /// </summary>
    /// <param name="position">Posición destino.</param>
    /// <param name="rotation">Rotación destino.</param>
    private void ApplyTransform(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }

    /// <summary>
    /// Construye una rotación que conserva solo el yaw (rotación en Y)
    /// de la rotación destino.
    /// </summary>
    /// <param name="rotation">Rotación original.</param>
    /// <returns>Rotación filtrada con solo yaw.</returns>
    private Quaternion BuildYawOnlyRotation(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        return Quaternion.Euler(0f, euler.y, 0f);
    }
    
    private void OnDestroy()
    {
        _facadeService?.Unregister(this);
    }
}
