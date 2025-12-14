using UnityEngine;
using Zenject;

/// <summary>
/// Procesa el input crudo del adaptador: aplica sensibilidad, suavizado e inversión.
/// Esta capa pertenece a Application, no a Infrastructure.
/// </summary>
public class InputProcessor : ITickable
{
    private readonly IInputPort _inputPort;
    private readonly InputConfigSO _config;

    // Estados internos de smoothing
    private Vector2 _smoothedLook;
    private Vector2 _currentVelocity;

    // Último snapshot procesado (para debug o consumo del TickPlayerMovement)
    private PlayerInputSnapshot _processedSnapshot;
    
    [Inject]
    public InputProcessor(IInputPort inputPort, InputConfigSO config)
    {
        _inputPort = inputPort;
        _config = config;
    }
    
    public void Tick()
    {
        var raw = _inputPort.GetSnapshot();
        
        // --- Movimiento: normalizado ---
        Vector2 move = raw.Move.sqrMagnitude > 1f ? raw.Move.normalized : raw.Move;
        
        // --- Mirada: suavizado + sensibilidad + inversión ---
        Vector2 lookTarget = raw.Look * _config.lookSensitivity;
        if (_config.invertY)
            lookTarget.y *= -1f;
        
        // Suavizado cinematográfico
        _smoothedLook = Vector2.SmoothDamp(
            _smoothedLook,
            lookTarget,
            ref _currentVelocity,
            _config.lookSmoothing,
            Mathf.Infinity,
            Time.deltaTime
        );
        
        // Clamping del pitch (solo informativo, se aplicará en la cámara más adelante)
        _smoothedLook.y = Mathf.Clamp(_smoothedLook.y, _config.minPitch, _config.maxPitch);

        _processedSnapshot = new PlayerInputSnapshot(move, _smoothedLook, raw.InteractPressed);
    }
    
    /// <summary>
    /// Devuelve el último snapshot procesado, listo para usar por la lógica del Player.
    /// </summary>
    public PlayerInputSnapshot GetProcessedSnapshot() => _processedSnapshot;
}
