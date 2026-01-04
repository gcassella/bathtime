using Vintagestory.API.Common.Entities;

namespace BathTime;

public class SoapBuff : Buff, IStinkyRateModifier
{
    public SoapBuff(Entity entity) : base(entity, Constants.SOAPY_BUFF_KEY, 300)
    {
    }

    public double stinkyPriority => Constants.BATH_MULTIPLIER_ADDITIVE_PRIORITY;

    public double ModifyRate(double rateMultplier)
    {
        return rateMultplier - stinkRateReduction;
    }

    public bool IsActive => EntityBehaviorStinky.IsBathing(entity) && durationHours > 0;

    protected override void onGameTick(float dt)
    {
        // Only tick the soap buff when we're in a bath.
        if (EntityBehaviorStinky.IsBathing(entity))
        {
            base.onGameTick(dt);
        }
        else
        {
            var nowHours = entity.Api.World.Calendar.TotalHours;
            lastUpdated = nowHours;
        }
    }

    public float stinkRateReduction { get; set; } = 50;
}