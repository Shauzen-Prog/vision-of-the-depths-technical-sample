using UnityEngine;

[CreateAssetMenu(menuName = "MaterialsToDrill/Material/DrillableMaterial")]
public class DrillableMaterialSO : ScriptableObject
{
    [Header("Name for UI")]
    public string nameUI;
    public Color colorUI;
    
    [Header("DrillableMaterial")]
    public TypeOfMaterialToDrill typeOfMaterialToDrill;
    public Texture texture;
    
    [Header("DrillableLife")]
    public float maxLife = 100f;
    public float currentLife;
    
    [Header("Damage Settings")]
    public float baseDamagePerSecond    = 1f;
    public float multiplierValid        = 10f;
    public float multiplierInvalidMode  = 5f;
    public float multiplierInvalidPower = 5f;
    public float multiplierBothInvalid  = 0f; 
    
}
