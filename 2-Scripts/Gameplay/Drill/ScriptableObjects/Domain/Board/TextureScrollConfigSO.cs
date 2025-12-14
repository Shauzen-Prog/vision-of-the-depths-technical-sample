using UnityEngine;

/// <summary>
/// Configura las velocidades de scroll para cada RequirementStatus.
/// </summary>
[CreateAssetMenu(menuName = "Configs/Drill/Texture Scroll Config")]
public class TextureScrollConfigSO : ScriptableObject
{
    public float validSpeed = 1.0f;
    public float invalidModeSpeed = 0.5f;
    public float invalidPowerSpeed = 0.3f;
    public float bothInvalidSpeed = 0.1f;

    /// <summary>
    /// Devuelve la velocidad según RequirementStatus.
    /// </summary>
    public float GetSpeed(RequirementStatus status)
    {
        return status switch
        {
            RequirementStatus.Valid => validSpeed,
            RequirementStatus.InvalidMode => invalidModeSpeed,
            RequirementStatus.InvalidPower => invalidPowerSpeed,
            RequirementStatus.BothInvalid => bothInvalidSpeed,
            _ => validSpeed
        };
    }
}
