using System;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BathTime;

public class StinkyRateModifierBodyTemperature(Entity entity) : IStinkyRateModifier
{
    public double stinkyPriority => 0.5;

    private Entity entity = entity;

    public double StinkyModifyRate(double rateMultplier)
    {
        EntityBehaviorBodyTemperature? bodyTempBehavior = entity.GetBehavior<EntityBehaviorBodyTemperature>();
        float bodyTemp = bodyTempBehavior?.CurBodyTemperature ?? 37.0f;
        float bodyTempDelta = bodyTemp - bodyTempBehavior?.NormalBodyTemperature ?? 37.0f;
        float rateFactor = (float)Math.Pow(0.151 * (double)bodyTempDelta, 3);
        rateFactor = GameMath.Clamp(rateFactor, -0.75f, 0.75f);
        return rateMultplier * rateFactor;
    }

    public bool StinkyRateModifierIsActive()
    {
        return entity.HasBehavior<EntityBehaviorBodyTemperature>();
    }
}