using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Drill/DrillNeedleConfig")]
public class DrillNeedleConfigSO : ScriptableObject
{
    public float needleDropSpeedYellow = 0.05f;
    public float needleDropSpeedRed = 0.1f;
    public float yellowZoneThreshold = 0.70f;
    public float redZoneThreshold = 0.29f;
    public float needlePosition = 1f;
    public float needleRiseSpeed = 0.25f;

    [Header("Instability Needle")] 
    public float needleShakeStrengthMin = 0.03f; //— arriba/verde.
    public float needleShakeStrengthMax = 0.1f; //— abajo/rojo.
    public float needleShakeFrequency = 18f;
}
