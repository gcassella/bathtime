using System;
using System.Text;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace BathTime;

public class TowelConfig : IToiletryConfig
{
    public float ApplicationTimeSec { get; set; } = 3;

    public float CooldownTimeHours { get; set; } = 5.0f;

    public bool ConsumeOnUse { get; set; } = false;
}

public class CollectibleBehaviorTowel(CollectibleObject collObj) : CollectibleBehaviorToiletry<DummyBuff, TowelConfig>(collObj)
{
    public double GetWetness(ItemSlot itemSlot)
    {
        if (api is null) return 0;

        ItemStack? itemstack = itemSlot.Itemstack;
        if (itemstack?.Attributes?[Constants.TOWEL_WETNESS_KEY] is not ITreeAttribute treeAttribute)
        {
            return 0;
        }

        double nowHours = api.World.Calendar.TotalHours;
        double lastUpdatedHours = treeAttribute.GetDouble(Constants.TOWEL_LAST_UPDATED_KEY);
        double deltaHours = nowHours - lastUpdatedHours;
        double deltaWetness = deltaHours / config.CooldownTimeHours;

        double lastWetness = treeAttribute.GetDouble(Constants.TOWEL_WETNESS_KEY);
        double newWetness = Math.Clamp(lastWetness - deltaWetness, 0.0, 1.0);

        treeAttribute.SetDouble(Constants.TOWEL_WETNESS_KEY, newWetness);
        treeAttribute.SetDouble(Constants.TOWEL_LAST_UPDATED_KEY, nowHours);
        itemSlot.MarkDirty();

        return newWetness;
    }

    public void SetWetness(ItemSlot itemSlot, double wetness)
    {
        if (api is null) return;

        ItemStack? itemstack = itemSlot.Itemstack;
        if (itemstack is null) return;

        ITreeAttribute treeAttribute = itemstack.Attributes.GetOrAddTreeAttribute(Constants.TOWEL_WETNESS_KEY);
        treeAttribute.SetDouble(Constants.TOWEL_WETNESS_KEY, wetness);
        treeAttribute.SetDouble(Constants.TOWEL_LAST_UPDATED_KEY, api.World.Calendar.TotalHours);
        itemSlot.MarkDirty();
    }

    public override bool OnHeldAttackStep(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, ref EnumHandling handling)
    {
        return false;
    }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
    }

    protected override bool IsValidTarget(Entity targetEntity)
    {
        return targetEntity.HasBehavior<EntityBehaviorBodyTemperature>();
    }

    protected override bool IsValidToiletry(ItemSlot toiletrySlot)
    {
        return GetWetness(toiletrySlot) < 0.2;
    }

    protected override void OnToiletryApply(Entity fromEntity, Entity targetEntity, DummyBuff rateModifier, ItemSlot toiletrySlot)
    {
        if (api is null) return;
        var bodyTempBehavior = targetEntity.GetBehavior<EntityBehaviorBodyTemperature>();
        if (bodyTempBehavior is null)
        {
            targetEntity.Api.Logger.Debug("Failed to get BodyTemperature system?");
            return;
        }

        double userWetness = bodyTempBehavior.Wetness;
        bodyTempBehavior.Wetness = 0;

        if (api.Side == EnumAppSide.Server)
        {
            // Pop a towel off the slot.
            ItemStack transferStack = toiletrySlot.TakeOut(1);
            ItemSlot transferSlot = new DummySlot(transferStack);
            // Make it nice and wet.
            SetWetness(transferSlot, userWetness);

            // Decide where it should go.
            bool flag = false;
            // If target is a player, try and give them the wet towel.
            if (targetEntity is EntityPlayer)
            {
                if (api.World.PlayerByUid(((EntityPlayer)targetEntity).PlayerUID) is IPlayer toPlayer)
                {
                    flag = toPlayer.InventoryManager.TryGiveItemstack(transferStack);
                }
            }

            // If we couldn't give them the wet towel, try to give it back to the user.
            if (!flag)
            {
                if (api.World.PlayerByUid(((EntityPlayer)fromEntity).PlayerUID) is IPlayer fromPlayer)
                {
                    flag = fromPlayer.InventoryManager.TryGiveItemstack(transferStack);
                }
            }

            // If neither player's inventory can 
            if (!flag)
            {
                api.World.SpawnItemEntity(transferStack, fromEntity.Pos.XYZ);
                transferSlot.Itemstack = null;
            }
        }
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        double wetness = GetWetness(inSlot);
        string wetnessStr = wetness switch
        {
            > 0.5 => Lang.Get("bathtime:towel-item-info-soakingwet", $"{wetness:P0}"),
            > 0.2 => Lang.Get("bathtime:towel-item-info-wet", $"{wetness:P0}"),
            <= 0.01 => Lang.Get("bathtime:towel-item-info-dry"),
            _ => Lang.Get("bathtime:towel-item-info-slightlywet", $"{wetness:P0}"),
        };
        dsc.AppendLine(wetnessStr);
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
    {
        WorldInteraction[] res = [];

        if (IsValidToiletry(inSlot))
        {
            res = res.AddToArray(
                new()
                {
                    ActionLangCode = "bathtime:heldhelp-towel",
                    MouseButton = EnumMouseButton.Right,
                }
            );
        }
        else
        {
            res = res.AddToArray(
                new()
                {
                    MouseButton = EnumMouseButton.None,
                    ActionLangCode = "bathtime:handhelp-towel-wet",
                }
            );
        }

        return res;
    }
}
