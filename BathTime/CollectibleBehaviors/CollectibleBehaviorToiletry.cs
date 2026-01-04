using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace BathTime;

public interface IToiletryConfig
{
    public float ApplicationTimeSec { get; set; }

    public float CooldownTimeHours { get; set; }
}

public class CollectibleBehaviorToiletry<TModifier, TConfig>(CollectibleObject collObj) : CollectibleBehavior(collObj) where TModifier : IStinkyRateModifier where TConfig : IToiletryConfig, new()
{
    protected TConfig config { get; set; } = new();

    private IProgressBar? progressBarRender;
    private ICoreAPI? api;

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);

        config = properties.AsObject<TConfig>();
    }

    public override void OnLoaded(ICoreAPI api)
    {
        this.api = api;
    }

    protected virtual bool IsValidTarget(Entity targetEntity) { return false; }

    protected virtual void OnToiletryApply(Entity targetEntity, TModifier rateModifier) { }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
        handHandling = EnumHandHandling.PreventDefault;
        handling = EnumHandling.Handled;

        if (api?.Side == EnumAppSide.Client)
        {
            ModSystemProgressBar progressBarSystem = api.ModLoader.GetModSystem<ModSystemProgressBar>();
            progressBarSystem.RemoveProgressbar(progressBarRender);
            progressBarRender = progressBarSystem.AddProgressbar();
        }
    }

    public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
    {
        base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel, ref handling);
        handling = EnumHandling.Handled;

        float progress = secondsUsed / config.ApplicationTimeSec;
        if (progressBarRender is not null)
        {
            progressBarRender.Progress = progress;
        }
        return progress < 1;
    }

    public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason, ref EnumHandling handled)
    {
        api?.ModLoader.GetModSystem<ModSystemProgressBar>()?.RemoveProgressbar(progressBarRender);
        return base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason, ref handled);
    }

    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
    {
        api?.ModLoader.GetModSystem<ModSystemProgressBar>()?.RemoveProgressbar(progressBarRender);

        handling = EnumHandling.Handled;

        Entity targetEntity = byEntity;
        if (entitySel is not null) targetEntity = entitySel.Entity;

        if (IsValidTarget(targetEntity))
        {
            if (targetEntity.GetBehavior<EntityBehaviorStinky>()?.GetRateModifier<TModifier>() is not TModifier rateModifier) return;

            OnToiletryApply(targetEntity, rateModifier);
            slot.TakeOut(1);
            slot.MarkDirty();
        }
    }
}
