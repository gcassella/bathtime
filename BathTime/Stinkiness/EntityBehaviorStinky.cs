using System;
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
    /// Array of rate modifiers, sorted by modifier priority (ascending). Applied in order.
    /// </summary>
    private IStinkyRateModifier[] rateMultiplierModifiers = [];

    /// <summary>
    /// Register a new modifier that will apply to this instance of the behavior.
    /// </summary>
    /// <param name="newModifier"></param>
    public void RegisterRateMultiplierModifier(IStinkyRateModifier newModifier)
    {
        rateMultiplierModifiers = rateMultiplierModifiers
        .Append(
            newModifier
        ).OrderBy(
            mod =>
            {
                return mod.stinkyPriority;
            }
        ).ToArray();
    }

    /// <summary>
    /// Days from calendar beginning when attributes were last updated.
    /// </summary>
    private double lastUpdatedDays
    {
        get
        {
            ITreeAttribute? treeAttribute = entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID);
            if (treeAttribute is null) return entity.World.Calendar.TotalDays;
            else return treeAttribute.GetDouble("last_updated_days");
        }
        set
        {
            ITreeAttribute? treeAttribute = entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID);
            if (treeAttribute is null) return;
            treeAttribute.SetDouble("last_updated_days", value);
            entity.WatchedAttributes.MarkPathDirty(Constants.MOD_ID);
        }
    }

    /// <summary>
    /// Value in [0, 1] indicating how stinky the entity is.
    /// </summary>
    public double Stinkiness
    {
        get
        {
            ITreeAttribute? treeAttribute = entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID);
            if (treeAttribute is null) return 0;
            else return treeAttribute.GetDouble(Constants.STINKINESS);
        }
        set
        {
            double clampedValue = Math.Clamp(value, 0.0, 1.0);
            ITreeAttribute? treeAttribute = entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID);
            if (treeAttribute is null) return;
            treeAttribute.SetDouble(Constants.STINKINESS, clampedValue);
            entity.WatchedAttributes.MarkPathDirty(Constants.MOD_ID);
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
            foreach (var modifier in rateMultiplierModifiers)
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
        ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID);
        if (treeAttribute == null)
        {
            entity.WatchedAttributes.SetAttribute(Constants.MOD_ID, new TreeAttribute());
            Stinkiness = 0;
            lastUpdatedDays = entity.World.Calendar.TotalDays;
        }
    }

    public EntityBehaviorStinky(Entity entity) : base(entity)
    {
        RegisterRateMultiplierModifier(new StinkyRateModifierBath(entity));
        RegisterRateMultiplierModifier(new StinkyRateModifierBodyTemperature(entity));
        RegisterRateMultiplierModifier(new StinkyRateModifierSoap(entity));
    }
}