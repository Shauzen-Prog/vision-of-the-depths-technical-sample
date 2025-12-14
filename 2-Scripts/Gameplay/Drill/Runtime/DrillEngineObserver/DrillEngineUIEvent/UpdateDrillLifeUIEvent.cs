public struct UpdateDrillLifeUIEvent
{
    public int CurrentLife;
    public int MaxLife;
    
    public UpdateDrillLifeUIEvent(int currentLife, int maxLife)
    {
        CurrentLife = currentLife;
        MaxLife = maxLife;
    }
}
