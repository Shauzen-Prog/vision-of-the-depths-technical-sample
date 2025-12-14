using UnityEngine;

/// <summary>
/// Contrato de lectura de input desacoplado de Unity.
/// Permite mockear en tests y cambiar la fuente (KBM/Gamepad/Replay) sin tocar gameplay.
/// </summary>
public interface IInputPort
{
    /// <summary>
    /// Devuelve un snapshot inmutable de este frame. Sin smoothing.
    /// </summary>
    PlayerInputSnapshot GetSnapshot();
}

/// <summary>
/// DTO de entrada para el Player. Valores normalizados y listos para Application.
/// </summary>
public readonly struct PlayerInputSnapshot
{
    /// <summary>Movimiento en plano: X=lateral, Y=adelante.</summary>
    public Vector2 Move { get; }
    /// <summary>Mirada: X=yaw, Y=pitch. Mouse delta o stick derecho.</summary>
    public Vector2 Look { get; }
    /// <summary>True solo el frame en que se presiona interactuar.</summary>
    public bool InteractPressed { get; }

    public PlayerInputSnapshot(Vector2 move, Vector2 look, bool interactPressed)
    {
        Move = move;
        Look = look;
        InteractPressed = interactPressed;
    }
}

