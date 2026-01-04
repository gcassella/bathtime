using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace BathTime;

public class SoapConfig
{
    public float ApplicationTimeSec { get; set; } = 2;

    public float SoapDurationHours { get; set; } = 0.5f;
}

public class CollectibleBehaviorSoap(CollectibleObject collObj) : CollectibleBehavior(collObj)
{
    public SoapConfig config { get; set; } = new();

    protected IProgressBar? progressBarRender;
    protected ICoreAPI? api;

    protected float secondsUsedToCancel = 0;

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);

        config = properties.AsObject<SoapConfig>();
    }

    public override void OnLoaded(ICoreAPI api)
    {
        this.api = api;
    }

    private bool IsSoapable(Entity entity)
    {
        return entity.HasBehavior<EntityBehaviorStinky>() && EntityBehaviorStinky.IsBathing(entity);
    }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
        base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
        handHandling = EnumHandHandling.PreventDefault;
        handling = EnumHandling.PreventSubsequent;

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

        if (secondsUsed < config.ApplicationTimeSec || byEntity.World.Side != EnumAppSide.Server) return;

        Entity targetEntity = byEntity;
        if (entitySel is not null) targetEntity = entitySel.Entity;

        if (IsSoapable(targetEntity))
        {
            // TODO: replace this logic with a proper buffs system!!!
            targetEntity.SetBoolAttribute(Constants.SOAPY_KEY, true);
            targetEntity.SetDoubleAttribute(Constants.LAST_SOAP_UPDATE_KEY, targetEntity.World.Calendar.TotalHours);
            targetEntity.SetDoubleAttribute(Constants.SOAP_DURATION_KEY, config.SoapDurationHours);

            slot.TakeOut(1);
            slot.MarkDirty();
        }
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        dsc.AppendLine(Lang.Get("bathtime:soap-item-info", $"{config.ApplicationTimeSec:F1} seconds", $"{config.SoapDurationHours:F1} hours"));
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
