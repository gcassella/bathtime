using System;
using Vintagestory;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.Common;
using Vintagestory.GameContent;
using Vintagestory.Server;

namespace BathTime;

internal class EntityBehaviorStinky : EntityBehavior
{
    // Rate multiplier for increment of stinkiness. Linearly increases rate at which stinkiness accumulates.
    public double rateMultiplier = 1.0;

    // Number of days required to reach max stinkiness when rateMultiplier is 1.0.
    private double maxStinkinessDays = 2.0;

    // Days from calendar beginning when attributes were last updated.
    private double lastUpdatedDays
    {
        get
        {
            return entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID).GetDouble("last_updated_days");
        }
        set
        {
            entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID).SetDouble("last_updated_days", value);
            entity.WatchedAttributes.MarkPathDirty(Constants.MOD_ID);
        }
    }

    // Value in [0, 1] indicating how stinky the entity is.
    public double Stinkiness
    {
        get
        {
            return entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID).GetDouble(Constants.STINKINESS);
        }
        set
        {
            double clampedValue = Math.Clamp(value, 0.0, 1.0);
            entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID).SetDouble(Constants.STINKINESS, clampedValue);
            entity.WatchedAttributes.MarkPathDirty(Constants.MOD_ID);
        }
    }

    //

    // Update entity Stinkiness. Stinkiness increases as a quadratic tween w.r.t. in-game time. That is, Stinkiness S
    // as a function of 'normalized time' x is S(x)=x(2-x) where S, x in [0, 1]. The normalized time can be inferred
    // from current Stinkiness as x_c = 1 - sqrt(1 - S), allowing the Stinkiness to be updated to S(x_c + d) where
    // d = (TotalDays - lastUpdatedDays) / maxStinkinessDays.
    public override void OnGameTick(float dt)
    {
        // Server handles updating attributes.
        if (entity.Api.Side == EnumAppSide.Server)
        {
            double delta = (entity.World.Calendar.TotalDays - lastUpdatedDays) / maxStinkinessDays;
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

    // Initialization. Ensure bathtime tree and stinkiness attributes exist
    // on entity.
    public override void Initialize(EntityProperties properties, JsonObject attributes)
    {
        ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID);
        if (treeAttribute == null)
        {
            entity.WatchedAttributes.SetAttribute(Constants.MOD_ID, treeAttribute = new TreeAttribute());
            Stinkiness = 0;
            lastUpdatedDays = entity.World.Calendar.TotalDays;
        }
    }

    public EntityBehaviorStinky(Entity entity) : base(entity)
    {
    }
}