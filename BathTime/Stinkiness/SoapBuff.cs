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
        // Tick the soap buff at half rate if not bathing.
        if (EntityBehaviorStinky.IsBathing(entity))
        {
            base.onGameTick(dt);
        }
        else
        {
            var nowHours = entity.Api.World.Calendar.TotalHours;
            durationHours -= (nowHours - lastUpdated) / 2;
            lastUpdated = nowHours;
            if (durationHours <= 0)
            {
                OnEnd();
            }
        }
    }

    public float stinkRateReduction { get; set; } = 50;
}