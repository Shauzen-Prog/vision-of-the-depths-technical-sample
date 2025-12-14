using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

/// <summary>
/// Adaptador del Input System (Unity 6) que implementa IInputPort.
/// Crea y habilita las acciones en runtime (sin drag & drop).
/// </summary>
public sealed class InputSystemAdapter : IInputPort, IInitializable, ILateDisposable
{
    private InputAction _move;
    private InputAction _look;
    private InputAction _interact;

    // Cache para evitar allocs por frame
    private Vector2 _cachedMove;
    private Vector2 _cachedLook;
    private bool _cachedInteractPressed;
    
    /// <summary>
    /// Inicializa y habilita las acciones con bindings para KBM y Gamepad.
    /// </summary>
    public void Initialize()
    {
        // Move (Vector2) - WASD + Left Stick
        _move = new InputAction(name: "Move", type: InputActionType.Value, expectedControlType: "Vector2");
        _move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _move.AddBinding("<Gamepad>/leftStick");
        
        // Look (Vector2) - Mouse delta + Right Stick
        _look = new InputAction(name: "Look", type: InputActionType.Value, expectedControlType: "Vector2");
        _look.AddBinding("<Mouse>/delta");
        _look.AddBinding("<Gamepad>/rightStick");
        
        // Interact (Button) - E + Button South
        _interact = new InputAction(name: "Interact", type: InputActionType.Button);
        _interact.AddBinding("<Keyboard>/e");
        _interact.AddBinding("<Gamepad>/buttonSouth");

        _move.Enable();
        _look.Enable();
        _interact.Enable();
    }
    
    /// <summary>
    /// Limpia y deshabilita acciones al finalizar el ciclo de vida del contenedor.
    /// </summary>
    public void LateDispose()
    {
        _move?.Disable();     _move?.Dispose();
        _look?.Disable();     _look?.Dispose();
        _interact?.Disable(); _interact?.Dispose();
        _move = null; _look = null; _interact = null;
    }
    
    public PlayerInputSnapshot GetSnapshot()
    {
        _cachedMove = _move.ReadValue<Vector2>();
        _cachedLook = _look.ReadValue<Vector2>();
        _cachedInteractPressed = _interact.WasPressedThisFrame();

        return new PlayerInputSnapshot(_cachedMove, _cachedLook, _cachedInteractPressed);
    }
}
