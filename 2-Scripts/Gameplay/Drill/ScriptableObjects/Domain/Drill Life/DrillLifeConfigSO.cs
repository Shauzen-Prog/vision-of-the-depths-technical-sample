using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Drill/DrillLifeConfig")]
public class DrillLifeConfigSO : ScriptableObject
{
    public float drillLife = 100f;
    public float drillMaxLife = 100f;
    public int lifeLostPerTick = 1;
}
