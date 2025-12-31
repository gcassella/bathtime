using Vintagestory.API.Common.Entities;

namespace BathTime;

public class StinkyRateMultiplierModifierWater : IStinkyRateModifier
{
    public bool StinkyRateModifierIsActive(Entity entity)
    {
        return true;
    }

    public double StinkyModifyRate(Entity entity, double rateMultiplier)
    {
        return 10;
    }

    public double stinkyPriority => 1.0;
}
