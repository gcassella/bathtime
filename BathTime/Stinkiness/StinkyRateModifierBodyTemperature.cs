using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BathTime;

public partial class BathtimeConfig : BathtimeBaseConfig<BathtimeConfig>, IHasConfigName
{
    public bool stinkyUseBodyTemperature { get; set; } = true;

    public double stinkyBodyTemperatureCoefficient { get; set; } = 0.151;

    public double stinkyBodyTemperatureExponent { get; set; } = 3;

    public float stinkyBodyTemperatureMultiplierMax { get; set; } = 0.75f;
}

public class StinkyRateModifierBodyTemperature : IStinkyRateModifier, IHasConfig<BathtimeConfig>
{
    public double stinkyPriority => 0.5;

    private Entity entity;

    private BathtimeConfig _config = new();

    public BathtimeConfig config
    {
        get => _config;
        set => _config = value;
    }

    public StinkyRateModifierBodyTemperature(Entity entity)
    {
        this.entity = entity;

        if (entity.Api.Side == EnumAppSide.Server)
        {
            this.LoadConfig<StinkyRateModifierBodyTemperature, BathtimeConfig>(entity.Api);
            this.ListenConfig<StinkyRateModifierBodyTemperature, BathtimeConfig>(entity.Api);
        }
    }

    public double StinkyModifyRate(double rateMultplier)
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
        return rateMultplier * rateFactor;
    }

    public bool StinkyRateModifierIsActive()
    {
        return config.stinkyUseBodyTemperature && entity.HasBehavior<EntityBehaviorBodyTemperature>();
    }
}