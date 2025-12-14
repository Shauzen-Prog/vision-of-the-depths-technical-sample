using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Valida los requisitos de minado según el material, modo y potencia.
/// Se puede inyectar para testing o variantes de dificultad.
/// </summary>
public interface IDrillRequirementChecker
{
    RequirementStatus CheckRequirementStatus(TypeOfMaterialToDrill material, int mode, int power);
}
