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
    // The listener ID for the gameTickListener.
    private long listenerId;

    // The interval for the tick listener to update attributes in ms.
    private int listenerInterval = 3000;

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

    public EntityBehaviorStinky(Entity entity) : base(entity)
    {
    }

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

    private void IncrementStinkiness(double delta)
    {
        double normalizedStartTime = 1 - Math.Sqrt(1 - Stinkiness);
        double normalizedEndTime = Math.Clamp(normalizedStartTime + rateMultiplier * delta, 0, 1);
        Stinkiness = normalizedEndTime * (2 - normalizedEndTime);
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

        listenerId = entity.World.RegisterGameTickListener(SlowTick, listenerInterval);
    }

    private void SlowTick(float dt)
    {
        // Server handles updating attributes.
        if (entity.Api.Side == EnumAppSide.Server)
        {
            double delta = (entity.World.Calendar.TotalDays - lastUpdatedDays) / maxStinkinessDays;
            IncrementStinkiness(delta);
            lastUpdatedDays = entity.World.Calendar.TotalDays;
        }
    }

    // Remove game tick listener when entity despawns.
    public override void OnEntityDespawn(EntityDespawnData despawn)
    {
        base.OnEntityDespawn(despawn);
        entity.World.UnregisterGameTickListener(listenerId);
    }
}