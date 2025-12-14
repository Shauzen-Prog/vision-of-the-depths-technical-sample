using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Representa una conversación lineal de diálogo.
/// Contiene una lista ordenada de líneas.
/// </summary>
public sealed class DialogueConversation 
{
    private readonly List<DialogueLine> _lines;

    /// <summary>
    /// ID lógico del diálogo (por ejemplo, nombre del archivo JSON).
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Lista ordenada de líneas.
    /// </summary>
    public IReadOnlyList<DialogueLine> Lines => _lines;

    public DialogueConversation(string id, IEnumerable<DialogueLine> lines)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El ID del dialogo no puede ser null o estar vacio.", nameof(id));

        if (lines == null)
            throw new ArgumentNullException(nameof(lines));

        _lines = lines.ToList();

        if (_lines.Count == 0)
            throw new ArgumentException("El dialogo debe contener al menos una linea.", nameof(lines));

        Id = id;
    }
}
