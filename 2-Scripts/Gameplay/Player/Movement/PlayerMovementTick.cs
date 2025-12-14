using UnityEngine;
using Zenject;

/// <summary>
/// Tick de movimiento del jugador. Traduce el input procesado en un
/// vector de movimiento en mundo, en base al yaw actual del player.
/// Usa PlayerControlService para respetar bloqueos de movimiento.
/// </summary>
public class PlayerMovementTick : ITickable
{
    private readonly InputProcessor _inputProcessor;
    private readonly PlayerLookController _lookController;
    private readonly IPlayerControlService _playerControl;
    
    // Último resultado (para debug o consumo del Presenter)
    private PlayerOutput _lastOutput;
    
    [Inject]
    public PlayerMovementTick(
        InputProcessor inputProcessor,
        PlayerLookController lookController,
        IPlayerControlService playerControl)
    {
        _inputProcessor = inputProcessor;
        _lookController = lookController;
        _playerControl = playerControl;
    }
    
    public void Tick()
    {
        // Si el movimiento está bloqueado (closeup, diálogo, cutscene, etc.),
        // la salida es siempre cero.
        if (!_playerControl.CanMove)
        {
            _lastOutput = new PlayerOutput(Vector3.zero, 0f);
            return;
        }
        
        // 1. Obtener input procesado
        var snap = _inputProcessor.GetProcessedSnapshot();

        // 2. Vector de movimiento (input local)
        Vector3 localMove = new Vector3(snap.Move.x, 0f, snap.Move.y);
        float magnitude = Mathf.Clamp01(localMove.magnitude);

        if (magnitude > 0.0001f)
        {
            // 3. Rotar según yaw actual del player
            Quaternion yawRot = Quaternion.Euler(0f, _lookController.YawRoot.eulerAngles.y, 0f);

            Vector3 worldDir = yawRot * localMove.normalized;

            _lastOutput = new PlayerOutput(worldDir, magnitude);
        }
        else
        {
            _lastOutput = new PlayerOutput(Vector3.zero, 0f);
        }
    }
    
    /// <summary>Devuelve el último resultado calculado.</summary>
    public PlayerOutput GetOutput() => _lastOutput;
}
