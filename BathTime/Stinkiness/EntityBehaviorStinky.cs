using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace BathTime;

public partial class BathtimeConfig : IConfig
{
    public double maxStinkinessDays { get; set; } = 2.0;
}

internal class EntityBehaviorStinky : EntityBehavior
{
    private BathtimeConfig config
    {
        get => BathtimeBaseConfig<BathtimeConfig>.LoadStoredConfig(entity.Api);
    }

    /// <summary>
    /// Static method for determining if entity is bathing.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static bool IsBathing(Entity entity)
    {
        var inBlock = entity.Api.World.BlockAccessor.GetBlockRaw(
            entity.Pos.AsBlockPos.X,
            entity.Pos.AsBlockPos.InternalY,
            entity.Pos.AsBlockPos.Z
        );
        return (
            entity.FeetInLiquid &&
            inBlock.BlockMaterial == EnumBlockMaterial.Liquid &&
            (
                inBlock.Code.Path.Contains("water")
            )
        );
    }

    /// <summary>
    /// Rate multiplier for increment of stinkiness. Linearly multiplies rate at which normalized time advances.
    /// </summary>
    private double rateMultiplier = 1.0;

    /// <summary>
    /// Dictionary of rate modifiers, keyed by identifiers.
    /// </summary>
    private Dictionary<Type, IStinkyRateModifier> rateModifiers = new();

    /// <summary>
    /// Register a new modifier that will apply to this instance of the behavior.
    /// </summary>
    /// <param name="newModifier"></param>
    public void RegisterRateModifier(IStinkyRateModifier newModifier)
    {
        rateModifiers.Add(newModifier.GetType(), newModifier);
    }

    /// <summary>
    /// Get a registered modifier by type. If none exists, returns null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IStinkyRateModifier? GetRateModifier<T>() where T : IStinkyRateModifier
    {
        Type modifierType = typeof(T);
        if (rateModifiers.ContainsKey(modifierType)) return rateModifiers[modifierType];
        else return null;
    }

    /// <summary>
    /// Days from calendar beginning when attributes were last updated.
    /// </summary>
    private double lastUpdatedDays
    {
        get
        {
            return entity.GetDoubleAttribute(Constants.LAST_STINKINESS_UPDATE_KEY);
        }
        set
        {
            entity.SetDoubleAttribute(Constants.LAST_STINKINESS_UPDATE_KEY, value);
        }
    }

    /// <summary>
    /// Value in [0, 1] indicating how stinky the entity is.
    /// </summary>
    public double Stinkiness
    {
        get
        {
            return entity.GetDoubleAttribute(Constants.STINKINESS_KEY);
        }
        set
        {
            double clampedValue = Math.Clamp(value, 0.0, 1.0);
            entity.SetDoubleAttribute(Constants.STINKINESS_KEY, clampedValue);
        }
    }

    //

    /// <summary>
    /// Update entity Stinkiness. Stinkiness increases as a quadratic tween w.r.t. in-game time. That is, Stinkiness S
    /// as a function of 'normalized time' x is S(x)=x(2-x) where S, x in [0, 1]. The normalized time can be inferred
    /// from current Stinkiness as x_c = 1 - sqrt(1 - S), allowing the Stinkiness to be updated to S(x_c + d) where
    /// d = (TotalDays - lastUpdatedDays) / maxStinkinessDays.
    /// </summary>
    /// <param name="dt">Unused.</param>
    public override void OnGameTick(float dt)
    {
        // Server handles updating attributes.
        if (entity.Api.Side == EnumAppSide.Server)
        {
            rateMultiplier = 1.0;
            foreach (var modifier in rateModifiers.Values.OrderBy(mod => mod.stinkyPriority))
            {
                if (modifier.StinkyRateModifierIsActive())
                {
                    rateMultiplier = modifier.StinkyModifyRate(rateMultiplier);
                }
            }
            double delta = (entity.World.Calendar.TotalDays - lastUpdatedDays) / config.maxStinkinessDays;
            double normalizedStartTime = 1 - Math.Sqrt(1 - Stinkiness);
            // For large deltas, normalizedEndTime can exceed 1 and must be clamped.
            double normalizedEndTime = Math.Clamp(normalizedStartTime + rateMultiplier * delta, 0, 1);
            Stinkiness = normalizedEndTime * (2 - normalizedEndTime);
            lastUpdatedDays = entity.World.Calendar.TotalDays;
        }
    }

    public override string PropertyName()
    {
        return Constants.MOD_ID + ".stinky";
    }

    /// <summary>
    /// Initialization. Ensure bathtime tree and stinkiness attributes exists on entity.
    /// </summary>
    /// <param name="properties"></param>
    /// <param name="attributes"></param>
    public override void Initialize(EntityProperties properties, JsonObject attributes)
    {
        Stinkiness = 0;
        lastUpdatedDays = entity.World.Calendar.TotalDays;
    }

    public EntityBehaviorStinky(Entity entity) : base(entity)
    {
        RegisterRateModifier(new StinkyRateModifierBath(entity));
        RegisterRateModifier(new StinkyRateModifierBodyTemperature(entity));
        RegisterRateModifier(new StinkyRateModifierSoap(entity));
    }
}