/// <summary>
/// Evento que indica que hay un nuevo material listo para minar.
/// </summary>
public struct MaterialSpawnedEvent 
{
    public DrillableMaterialSO Material;
    public MaterialSpawnedEvent(DrillableMaterialSO material)
    {
        Material = material;
    }
}  

/// <summary>
/// Evento que indica que el material fue destruido.
/// </summary>
public struct MaterialDestroyedEvent 
{
    public DrillableMaterialSO Material;
    public MaterialDestroyedEvent(DrillableMaterialSO material)
    {
        Material = material;
    }
}
