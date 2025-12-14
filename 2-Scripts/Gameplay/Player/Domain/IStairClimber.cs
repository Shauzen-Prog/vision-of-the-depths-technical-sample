using UnityEngine;

/// <summary>
/// Esta interfaz permite que cualquier sistema de escalera
/// implemente su propia lógica de escalada sin depender de físicas Unity.
/// <summary>
public interface IStairClimber 
{
    /// <summary>
    /// Intenta subir un escalón a partir de una posición y una velocidad horizontal.
    /// Devuelve true si pudo resolver un step válido, con la posición resultante en world.
    /// </summary>
    bool TryClimb(Vector3 currentCenter, Vector3 horizontalVelocity, float dt, out Vector3 climbedCenter);
}
