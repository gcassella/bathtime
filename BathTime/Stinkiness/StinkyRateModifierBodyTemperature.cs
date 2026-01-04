using System;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BathTime;

public partial class BathtimeConfig : IConfig
{
    public bool stinkyUseBodyTemperature { get; set; } = true;

    public double stinkyBodyTemperatureCoefficient { get; set; } = 0.151;

    public double stinkyBodyTemperatureExponent { get; set; } = 3;

    public float stinkyBodyTemperatureMultiplierMax { get; set; } = 0.75f;
}

public class StinkyRateModifierBodyTemperature : IStinkyRateModifier
{
    public double stinkyPriority => Constants.RATE_MULTIPLIER_MULTIPLICATIVE_PRIORITY;

    private Entity entity;

    public BathtimeConfig config
    {
        get => BathtimeBaseConfig<BathtimeConfig>.LoadStoredConfig(entity.Api);
    }

    public StinkyRateModifierBodyTemperature(Entity entity)
    {
        this.entity = entity;
    }

    public double ModifyRate(double rateMultplier)
    {
        EntityBehaviorBodyTemperature? bodyTempBehavior = entity.GetBehavior<EntityBehaviorBodyTemperature>();
        float bodyTemp = bodyTempBehavior?.CurBodyTemperature ?? 37.0f;
        float bodyTempDelta = bodyTemp - bodyTempBehavior?.NormalBodyTemperature ?? 37.0f;
        float rateFactor = (float)Math.Pow(
            config.stinkyBodyTemperatureCoefficient * (double)bodyTempDelta,
            config.stinkyBodyTemperatureExponent
        );
        rateFactor = GameMath.Clamp(
            rateFactor,
            -config.stinkyBodyTemperatureMultiplierMax,
            config.stinkyBodyTemperatureMultiplierMax
        );
        return rateMultplier * (1 + rateFactor);
    }

    public bool IsActive => config.stinkyUseBodyTemperature && entity.HasBehavior<EntityBehaviorBodyTemperature>();

    public string Identifier => "body_temp_modifier";
}
