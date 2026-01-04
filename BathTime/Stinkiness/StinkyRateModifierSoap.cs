using Vintagestory.API.Common.Entities;

namespace BathTime;


public class StinkyRateModifierSoap : IStinkyRateModifier
{
    private Entity entity;

    public double stinkyPriority => Constants.BATH_MULTIPLIER_ADDITIVE_PRIORITY;

    private double soapDurationHours;
    private double soapLastUpdated;

    public void ApplySoap(double duration)
    {
        entity.SetBoolAttribute(Constants.SOAPY_KEY, true);
        soapDurationHours = duration;
        soapLastUpdated = entity.Api.World.Calendar.TotalHours;
    }

    public double StinkyModifyRate(double rateMultplier)
    {
        return rateMultplier - 50;
    }

    public bool StinkyRateModifierIsActive()
    {
        var nowHours = entity.Api.World.Calendar.TotalHours;
        bool active = false;

        // If bathing and soapy, active and reduce soap duration.
        if (EntityBehaviorStinky.IsBathing(entity) && soapDurationHours > 0)
        {
            active = true;
            soapDurationHours -= nowHours - soapLastUpdated;
        }

        // Just finished washing off soap.
        if (soapDurationHours <= 0 && active)
        {
            entity.SetBoolAttribute(Constants.SOAPY_KEY, false);
        }

        soapLastUpdated = nowHours;
        return active;
    }

    public string Identifier => "soap_modifier";

    public StinkyRateModifierSoap(Entity entity)
    {
        this.entity = entity;
    }
}