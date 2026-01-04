using Vintagestory.API.Common.Entities;

namespace BathTime;


public class StinkyRateModifierSoap : IStinkyRateModifier
{
    private Entity entity;

    public double stinkyPriority => 1.5;

    public double StinkyModifyRate(double rateMultplier)
    {
        return rateMultplier - 50;
    }

    public bool StinkyRateModifierIsActive()
    {
        bool active = false;
        if (entity.GetBoolAttribute(Constants.SOAPY_KEY))
        {
            var nowHours = entity.Api.World.Calendar.TotalHours;
            // TODO: replace this logic with a proper buffs system!!!
            double soapDurationHours = entity.GetDoubleAttribute(Constants.SOAP_DURATION_KEY);
            entity.Api.Logger.Notification(soapDurationHours.ToString());

            // If bathing, reduce soap duration.
            if (EntityBehaviorStinky.IsBathing(entity))
            {
                soapDurationHours -= nowHours - entity.GetDoubleAttribute(
                    Constants.LAST_SOAP_UPDATE_KEY,
                    defaultValue: soapDurationHours + 1
                );
                active = true;
            }

            if (soapDurationHours <= 0)
            {
                entity.SetBoolAttribute(Constants.SOAPY_KEY, false);
            }

            entity.SetDoubleAttribute(Constants.LAST_SOAP_UPDATE_KEY, nowHours);
            entity.SetDoubleAttribute(Constants.SOAP_DURATION_KEY, soapDurationHours);
        }
        return active;
    }

    public StinkyRateModifierSoap(Entity entity)
    {
        this.entity = entity;
    }
}