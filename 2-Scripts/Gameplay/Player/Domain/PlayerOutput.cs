using UnityEngine;

/// <summary>
/// Datos de salida del Player para ser aplicados por el Presenter.
/// No contiene lógica de Unity ni físicas.
/// </summary>
public readonly struct PlayerOutput
{
    /// </summary>Dirección del movimiento en espacio mundo (usada por el sistema de movimiento).</summary>
    public Vector3 WorldDirection { get; }

    /// <summary>Magnitud del input de movimiento (0 a 1)..</summary>
    public float Magnitude { get; }
    
    /// <summary>True si hay intención de movimiento.</summary>
    public bool IsMoving => Magnitude > 0.01f;

    public PlayerOutput(Vector3 worldDirection, float magnitude)
    {
        WorldDirection = worldDirection;
        Magnitude = magnitude;
    }
}
