using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MaterialScrollConfig
{
    public TypeOfMaterialToDrill type;
    public Texture2D texture;
    public Vector2 scrollSpeed = Vector2.right;
    public float shakeStrength = 0.05f;
    public float shakeDuration = 0.4f;
    public float shakeFrequency = 22f;
    // TODO: Agregar VFX, SFX, color, etc si querés
}

/// <summary>
/// Contiene todas las configuraciones de feedback para cada material del drill.
/// Editás la lista desde el inspector, súper escalable.
/// </summary>
[CreateAssetMenu(menuName = "Configs/Drill/TextureScroll/MaterialScrollConfig", fileName = "MaterialScrollFeedbackSO")]
public class MaterialScrollConfigListSO : ScriptableObject
{
    public List<MaterialScrollConfig> configs;
    
    /// <summary>
    /// Busca la config correspondiente al tipo de material.
    /// </summary>
    public MaterialScrollConfig GetConfig(TypeOfMaterialToDrill type)
    {
        var config = configs.FirstOrDefault(c => c.type == type);
    
        if (config == null)
            Debug.LogWarning($"[MaterialScrollConfigListSO] Config no encontrada para: {type}");
    
        return config;
    }
}
