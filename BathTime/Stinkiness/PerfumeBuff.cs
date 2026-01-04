using Vintagestory.API.Common.Entities;

namespace BathTime;

public class PerfumeBuff : Buff, IStinkyRateModifier
{
    public double stinkyPriority => Constants.RATE_MULTIPLIER_MULTIPLICATIVE_PRIORITY;

    public double ModifyRate(double rateMultplier)
    {
        return rateMultplier * 0.9;
    }

    public bool IsActive => durationHours > 0;

    public PerfumeBuff(Entity entity) : base(entity, Constants.PERFUME_BUFF_KEY, 300)
    {
    }
}