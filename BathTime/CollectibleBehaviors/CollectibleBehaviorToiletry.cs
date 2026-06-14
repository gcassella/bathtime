using System.Diagnostics;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace BathTime;

public interface IToiletryConfig
{
    public float ApplicationTimeSec { get; set; }

    public float CooldownTimeHours { get; set; }

    public bool ConsumeOnUse { get; set; }
}

public class CollectibleBehaviorToiletry<TModifier, TConfig>(CollectibleObject collObj) : CollectibleBehavior(collObj) where TModifier : IStinkyRateModifier where TConfig : IToiletryConfig, new()
{
    protected TConfig config { get; set; } = new();

    private IProgressBar? progressBarRender;

    protected ICoreAPI? api;

    protected EnumHandling startHandling = EnumHandling.Handled;

    protected EnumHandling stepHandling = EnumHandling.Handled;

    protected EnumHandling stopHandling = EnumHandling.Handled;

    protected float secondsUsedToCancel = 0;

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);

        if (properties.AsObject<TConfig>() is not TConfig config)
        {
            throw new UnreachableException("Tried to initialize toiletry config with invalid properties.");
        }
        this.config = config;
    }

    public override void OnLoaded(ICoreAPI api)
    {
        this.api = api;
    }

    protected virtual bool IsValidTarget(Entity targetEntity) { return false; }

    protected virtual bool IsValidToiletry(ItemSlot toiletrySlot) { return true; }

    protected virtual void OnToiletryApply(Entity byEntity, Entity targetEntity, TModifier rateModifier, ItemSlot toiletrySlot) { }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection? entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
        handHandling = EnumHandHandling.PreventDefault;
        handling = startHandling;

        if (api?.Side == EnumAppSide.Client)
        {
            ModSystemProgressBar progressBarSystem = api.ModLoader.GetModSystem<ModSystemProgressBar>();
            progressBarSystem.RemoveProgressbar(progressBarRender);
            progressBarRender = progressBarSystem.AddProgressbar();
        }
    }

    public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection? entitySel, ref EnumHandling handling)
    {
        handling = stepHandling;

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
        handling = stopHandling;

        if (secondsUsed < config.ApplicationTimeSec - 0.3f)
        {
            handling = EnumHandling.PassThrough;
            return;
        }

        Entity targetEntity = byEntity;
        if (entitySel is not null) targetEntity = entitySel.Entity;


        if (IsValidTarget(targetEntity) && IsValidToiletry(slot))
        {
            if (targetEntity.GetBehavior<EntityBehaviorStinky>()?.GetRateModifier<TModifier>() is not TModifier rateModifier) return;
            OnToiletryApply(byEntity, targetEntity, rateModifier, slot);

            if (config.ConsumeOnUse)
            {
                slot.TakeOut(1);
                slot.MarkDirty();
            }
        }
    }
}
