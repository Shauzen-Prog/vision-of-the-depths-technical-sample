public interface IMaterialLifeHandler 
{
    TypeOfMaterialToDrill MaterialType { get; }
    float CurrentLife { get; }
    void ApplyDrillDamage(RequirementStatus status, float tick);
}
