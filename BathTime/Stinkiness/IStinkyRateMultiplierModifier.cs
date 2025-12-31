namespace BathTime.EntityBehaviors;

public interface IStinkyRateMultiplierModifier
{
    public bool active { get; }
    public double modifier { get; }
}


