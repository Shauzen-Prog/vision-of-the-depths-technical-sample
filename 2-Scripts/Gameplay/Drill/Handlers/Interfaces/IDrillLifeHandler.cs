public interface IDrillLifeHandler 
{
    int CurrentLife { get; }
    int MaxLife { get; }
    void SetLife(int current, int max);
}
