using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parser compatible con Unity usando JsonUtility.
/// Estructura:
/// { "dialogue": [ {"speakerName": "...", "line": "..."}, ... ] }
/// </summary>
public sealed class JsonDialogueParser : IDialogueParser
{
    [Serializable]
    private class DialogueJsonRoot
    {
        public List<DialogueJsonEntry> dialogue;
    }

    [Serializable]
    private class DialogueJsonEntry
    {
        public string speakerName;
        public string line;
    }

    public DialogueConversation ParseFromJson(string dialogueId, string json)
    {
        if (string.IsNullOrWhiteSpace(dialogueId))
            throw new ArgumentException("El ID del dialogo no puede ser nulo o estar vacio.", nameof(dialogueId));

        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("El tenido del JSON no puede ser nulo o estar vacio.", nameof(json));

        DialogueJsonRoot root;

        try
        {
            root = JsonUtility.FromJson<DialogueJsonRoot>(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Fallo al hacer el Parse Dialogue Json en: '{dialogueId}'.", ex);
        }

        if (root == null || root.dialogue == null || root.dialogue.Count == 0)
            throw new InvalidOperationException($"Dialogue '{dialogueId}' no contiene entradas.");

        var lines = new List<DialogueLine>();

        foreach (var entry in root.dialogue)
        {
            var speaker = entry.speakerName ?? string.Empty;
            var text = entry.line ?? string.Empty;
            lines.Add(new DialogueLine(speaker, text));
        }

        return new DialogueConversation(dialogueId, lines);
    }
}
