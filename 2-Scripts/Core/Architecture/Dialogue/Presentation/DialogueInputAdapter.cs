using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Adaptador entre el Input System y el puerto de input de diálogos.
/// Lee una InputAction y expone un flag de "advance" por frame.
/// </summary>
public sealed class DialogueInputAdapter : MonoBehaviour, IDialogueInputPort
{
    [Header("Input System")]
    [SerializeField] private InputActionReference _advanceAction;

    private InputAction _cachedAction;

    /// <summary>
    /// Indica si la acción de avanzar diálogo se disparó en este frame.
    /// </summary>
    public bool IsAdvancePressedThisFrame
    {
        get
        {
            if (_cachedAction == null)
                return false;

            return _cachedAction.WasPerformedThisFrame();
        }
    }

    private void Awake()
    {
        if (_advanceAction != null)
        {
            _cachedAction = _advanceAction.action;
        }
    }

    private void OnEnable()
    {
        if (_cachedAction != null)
        {
            _cachedAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (_cachedAction != null)
        {
            _cachedAction.Disable();
        }
    }
}
