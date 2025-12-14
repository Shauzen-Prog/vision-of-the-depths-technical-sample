using UnityEngine;

/// <summary>
/// Fachada de alto nivel para manipular la posición y orientación del jugador.
/// Otros sistemas (triggers, diálogos, cinemáticas) la usan en vez de conocer
/// al PlayerPresenter o al CharacterController concreto.
/// </summary>
public interface IPlayerFacade 
{
    /// <summary>
    /// Teletransporta al jugador a una posición y rotación dadas.
    /// alignYawOnly = true mantiene la rotación solo en Y (útil para no forzar pitch/roll).
    /// </summary>
    /// <param name="position">Posición destino en mundo.</param>
    /// <param name="rotation">Rotación destino en mundo.</param>
    /// <param name="alignYawOnly">
    /// Si está en true, solo se usa el eje Y de la rotación destino.
    /// </param>
    void TeleportTo(Vector3 position, Quaternion rotation, bool alignYawOnly = false);

    /// <summary>
    /// Teletransporta al jugador usando directamente un Transform destino.
    /// </summary>
    /// <param name="target">Transform con la posición/rotación destino.</param>
    /// <param name="alignYawOnly">
    /// Si está en true, solo se usa el eje Y de la rotación destino.
    /// </param>
    void TeleportTo(Transform target, bool alignYawOnly = false);
}
