using UnityEngine;
using Zenject;

/// <summary>
/// Fuerza a Unity a recapturar el cursor inmediatamente,
/// sin esperar a que el usuario haga clic.
/// </summary>
public class MouseCaptureService
{
    public void Capture()
    {
        Cursor.lockState = CursorLockMode.None; // reset "quirk" visual
        Cursor.visible = false;

        // En el siguiente frame, bloqueamos de verdad
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
