using System;

/// <summary>
/// Evento global que indica que un diálogo comenzó.
/// Se publica desde los triggers de diálogo cuando arrancan una conversación.
/// </summary>
public readonly struct DialogueStartedEvent { }

/// <summary>
/// Evento global que indica que un diálogo terminó.
/// Se publica cuando el servicio de diálogos notifica el fin de la conversación.
/// </summary>
public readonly struct DialogueEndedEvent { }