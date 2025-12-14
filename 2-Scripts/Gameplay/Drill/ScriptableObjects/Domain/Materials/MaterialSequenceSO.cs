using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MaterialsToDrill/MaterialList/SequenceList")]
public class MaterialSequenceSO : ScriptableObject
{
    public string sequenceName;
    public List<DrillableMaterialSO> materials;
    public bool loopLastMaterial = false;
}
