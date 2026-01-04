using Vintagestory.API.Common.Entities;

namespace BathTime;

public class Buff
{
    protected Entity entity;

    private int tickInterval;

    private string identifier;

    private long listenerId;

    protected double durationHours { get; set; }

    protected double lastUpdated { get; set; }

    public Buff(Entity entity, string identifier, int tickInterval = 300)
    {
        this.entity = entity;
        this.tickInterval = tickInterval;
        this.identifier = identifier;
    }

    public virtual void Apply(double durationHours)
    {
        this.durationHours = durationHours;
        lastUpdated = entity.Api.World.Calendar.TotalHours;
        listenerId = entity.Api.Event.RegisterGameTickListener(
            onGameTick,
            tickInterval
        );
        entity.Api.Event.OnEntityDespawn += (Entity entity, EntityDespawnData reason) =>
        {
            if (entity == this.entity)
            {
                OnEnd();
            }
        };
        entity.SetBoolAttribute(identifier, true);
    }

    protected virtual void onGameTick(float dt)
    {
        var nowHours = entity.Api.World.Calendar.TotalHours;
        durationHours -= nowHours - lastUpdated;
        lastUpdated = nowHours;
        if (durationHours <= 0)
        {
            OnEnd();
        }
    }

    private void OnEnd()
    {
        durationHours = 0.0f;
        entity.SetBoolAttribute(identifier, false);
        entity.Api.Event.UnregisterGameTickListener(listenerId);
    }

    public static bool ActiveOnEntity(Entity entity, string identifier)
    {
        return entity.GetBoolAttribute(identifier);
    }
}