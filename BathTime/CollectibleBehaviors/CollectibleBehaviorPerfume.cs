using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;

namespace BathTime;

public class PerfumeConfig : IToiletryConfig
{
    public float ApplicationTimeSec { get; set; } = 2;

    public float StinkinessReduction { get; set; } = 0.3f;

    public float CooldownTimeHours { get; set; } = 8;
}

public class CollectibleBehaviorPerfume(CollectibleObject collObj) : CollectibleBehaviorToiletry<PerfumeBuff, PerfumeConfig>(collObj)
{
    protected override bool IsValidTarget(Entity targetEntity)
    {
        var stinkBehavior = targetEntity.GetBehavior<EntityBehaviorStinky>();
        var toiletryModifier = stinkBehavior?.GetRateModifier<PerfumeBuff>();
        if (toiletryModifier is not null) return !toiletryModifier.IsActive;
        else return false;
    }

    protected override void OnToiletryApply(Entity targetEntity, PerfumeBuff rateModifier)
    {
        if (targetEntity.Api.Side == EnumAppSide.Server)
        {
            var stinkBehavior = targetEntity.GetBehavior<EntityBehaviorStinky>();
            if (stinkBehavior is null) return;

            stinkBehavior.Stinkiness -= config.StinkinessReduction;
            rateModifier.Apply(config.CooldownTimeHours);
        }
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        dsc.AppendLine(Lang.Get("bathtime:perfume-item-info", $"{config.ApplicationTimeSec:F1} sec", $"{config.CooldownTimeHours:F1} hours", $"{100 * config.StinkinessReduction:F1}%"));
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
    {
        return [
            new()
            {
                ActionLangCode = "bathtime:heldhelp-perfume",
                MouseButton = EnumMouseButton.Right,
            }
        ];
    }
}