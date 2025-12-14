using System;
using System.Collections.Generic;

/// <summary>
/// Implementación por defecto del servicio de control del jugador.
/// Maneja un conjunto de bloqueos por tipo de control y por owner.
/// Si al menos un owner bloquea un tipo, ese control queda deshabilitado.
/// </summary>
public sealed class PlayerControlService : IPlayerControlService
{
     private readonly Dictionary<PlayerControlType, HashSet<object>> _blocks =
        new Dictionary<PlayerControlType, HashSet<object>>();

    /// <inheritdoc />
    public bool IsEnabled(PlayerControlType type)
    {
        if (_blocks.TryGetValue(type, out var owners) && owners.Count > 0)
            return false;

        return true;
    }

    public bool CanMove     => IsEnabled(PlayerControlType.Movement);
    public bool CanLook     => IsEnabled(PlayerControlType.Look);
    public bool CanInteract => IsEnabled(PlayerControlType.Interaction);
    public bool CanPause    => IsEnabled(PlayerControlType.Pause);

    /// <summary>
    /// Solicita el bloqueo de uno o varios tipos de control del jugador.
    /// 
    /// Este método es utilizado por sistemas externos (diálogos, closeups,
    /// cinemáticas, UI, etc.) para deshabilitar capacidades específicas del jugador
    /// sin acoplarse a la implementación concreta del movimiento o la cámara.
    /// 
    /// El bloqueo se registra por solicitante, permitiendo que múltiples sistemas
    /// convivan sin interferirse entre sí.
    /// </summary>
    /// <param name="owner">
    /// Objeto que solicita el bloqueo. Se utiliza como identificador para poder
    /// liberar posteriormente los controles de forma segura.
    /// </param>
    /// <param name="type">
    /// Tipos de control que se desean bloquear (movimiento, cámara, interacción, etc.).
    /// </param>
    public void RequestBlock(PlayerControlType type, object owner)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));

        if (!_blocks.TryGetValue(type, out var owners))
        {
            owners = new HashSet<object>();
            _blocks[type] = owners;
        }

        owners.Add(owner);
    }

    /// <inheritdoc />
    public void RequestBlock(IEnumerable<PlayerControlType> types, object owner)
    {
        if (types == null)
            throw new ArgumentNullException(nameof(types));

        foreach (var type in types)
        {
            RequestBlock(type, owner);
        }
    }

    /// <summary>
    /// Libera un bloqueo de control previamente solicitado por un sistema específico.
    /// 
    /// Solo se eliminan los bloqueos asociados al solicitante indicado,
    /// garantizando que otros sistemas que aún requieran el control bloqueado
    /// no se vean afectados.
    /// 
    /// Este enfoque evita errores comunes donde un sistema libera controles
    /// que no le pertenecen.
    /// </summary>
    /// <param name="owner">
    /// Objeto que había solicitado previamente el bloqueo de controles.
    /// </param>
    public void ReleaseBlock(PlayerControlType type, object owner)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));

        if (!_blocks.TryGetValue(type, out var owners))
            return;

        owners.Remove(owner);

        if (owners.Count == 0)
        {
            _blocks.Remove(type);
        }
    }

    /// <summary>
    /// Libera todos los bloqueos de control asociados a un solicitante.
    /// 
    /// Se utiliza típicamente en ciclos de vida como OnDisable o OnDestroy
    /// para asegurar que ningún control quede bloqueado si un sistema
    /// es removido de forma inesperada.
    /// </summary>
    /// <param name="owner">
    /// Objeto del cual se desean liberar todos los bloqueos de control.
    /// </param>
    public void ReleaseAll(object owner)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));

        var types = new List<PlayerControlType>(_blocks.Keys);
        foreach (var type in types)
        {
            if (_blocks.TryGetValue(type, out var owners))
            {
                if (owners.Remove(owner) && owners.Count == 0)
                {
                    _blocks.Remove(type);
                }
            }
        }
    }
}
