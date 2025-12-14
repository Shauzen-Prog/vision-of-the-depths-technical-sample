using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrillRequirementChecker : IDrillRequirementChecker
{
    private readonly (TypeOfMaterialToDrill Material, int Mode, int Power)[] _validCombinations;

    /// <summary>
    /// Constructor principal. Permite inyectar configuraciones custom.
    /// </summary>
    public DrillRequirementChecker(
        (TypeOfMaterialToDrill, int, int)[] validCombinations = null)
    {
        _validCombinations = (validCombinations != null && validCombinations.Length > 0)
            ? validCombinations
            : new[]
            {
                (TypeOfMaterialToDrill.Dirt, 1, 1),
                (TypeOfMaterialToDrill.DirtStone, 1, 2),
                (TypeOfMaterialToDrill.StoneDirt, 2, 2),
                (TypeOfMaterialToDrill.MineralDirt, 3, 4),
                (TypeOfMaterialToDrill.Stone, 2, 3),
                (TypeOfMaterialToDrill.StoneMineral, 2, 4),
                (TypeOfMaterialToDrill.MineralStone, 3, 4),
                (TypeOfMaterialToDrill.DirtMineral, 1, 4),
                (TypeOfMaterialToDrill.Mineral, 3, 5)
            };
    }
    
    /// <inheritdoc/>
    public RequirementStatus CheckRequirementStatus(TypeOfMaterialToDrill material, int mode, int power)
    {
        var exists = _validCombinations.Any(r => r.Material == material);

        if (!exists)
        {
            Debug.LogError($"[CheckRequirementStatus] No existe combinación para material: {material}");
            return RequirementStatus.BothInvalid;
        }

        var requirement = _validCombinations.First(r => r.Material == material);

        var modeCorrect = mode == requirement.Mode;
        var powerCorrect = power == requirement.Power;

        if (modeCorrect && powerCorrect) return RequirementStatus.Valid;
        if (!modeCorrect && !powerCorrect) return RequirementStatus.BothInvalid;
        if (!modeCorrect) return RequirementStatus.InvalidMode;
        return RequirementStatus.InvalidPower;
    }
}
