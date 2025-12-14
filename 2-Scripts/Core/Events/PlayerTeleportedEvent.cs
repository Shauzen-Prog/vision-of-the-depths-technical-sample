using UnityEngine;

/// <summary>
/// Evento que se dispara cuando el player es teletransportado.
/// Permite a otros sistemas (cámara, look) sincronizarse.
/// </summary>
public readonly struct PlayerTeleportedEvent
{
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }

    public PlayerTeleportedEvent(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }
}
