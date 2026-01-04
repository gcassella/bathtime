using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;

namespace BathTime;

public class SoapConfig : IToiletryConfig
{
    public float ApplicationTimeSec { get; set; } = 2;

    public float CooldownTimeHours { get; set; } = 0.5f;

    public float StinkRateReduction { get; set; } = 50f;
}

public class CollectibleBehaviorSoap(CollectibleObject collObj) : CollectibleBehaviorToiletry<SoapBuff, SoapConfig>(collObj)
{
    protected override bool IsValidTarget(Entity targetEntity)
    {
        var stinkBehavior = targetEntity.GetBehavior<EntityBehaviorStinky>();
        var toiletryModifier = stinkBehavior?.GetRateModifier<SoapBuff>();
        if (toiletryModifier is not null) return !toiletryModifier.IsActive && EntityBehaviorStinky.IsBathing(targetEntity);
        else return false;
    }

    protected override void OnToiletryApply(Entity targetEntity, SoapBuff rateModifier)
    {
        rateModifier.stinkRateReduction = config.StinkRateReduction;
        rateModifier.Apply(config.CooldownTimeHours);
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        dsc.AppendLine(Lang.Get("bathtime:soap-item-info", $"{config.ApplicationTimeSec:F1} secs", $"{config.CooldownTimeHours:F1} hours", $"{config.StinkRateReduction:F1}"));
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
    {
        return [
            new()
            {
                ActionLangCode = "bathtime:heldhelp-soap",
                MouseButton = EnumMouseButton.Right,
            }
        ];
    }

}
