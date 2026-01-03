using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BathTime;

public partial class BathtimeConfig : BathtimeBaseConfig<BathtimeConfig>, IHasConfigName
{
    public double bathingRateModifier { get; set; } = -5.0;

    public double bathingInsideRateModifier { get; set; } = -50.0;

    public bool bathingUseBodyTemperature { get; set; } = true;

    public double bathingWithBoilerMultiplier { get; set; } = 1.4;

    public float bathingMaxHealingPerSecond { get; set; } = 0.05f;
}


public class StinkyRateModifierBath : IStinkyRateModifier
{

    private RoomRegistry roomRegistry;

    private Entity entity;

    private ICachingBlockAccessor? blockAccess;

    private BlockPos blockPos = new(0);

    private BathtimeConfig config
    {
        get => BathtimeConfig.LoadStoredConfig(entity.Api);
    }

    public StinkyRateModifierBath(Entity entity)
    {
        roomRegistry = entity.Api.ModLoader.GetModSystem<RoomRegistry>();
        this.entity = entity;
        blockAccess = entity.Api.World.GetCachingBlockAccessor(false, false);
    }

    ~StinkyRateModifierBath()
    {
        blockAccess?.Dispose();
        blockAccess = null;
    }

    private DateTime lastHealed = DateTime.Now;
    private void applyBathHealing()
    {
        // Only apply heal once per second to avoid spamming logs.
        if ((DateTime.Now - lastHealed).TotalSeconds < 1) return;

        if (entity.GetBehavior<EntityBehaviorStinky>()?.Stinkiness is double stinkiness)
        {
            DamageSource bathHealing = new DamageSource()
            {
                Type = EnumDamageType.Heal,
                SourceEntity = null,
                KnockbackStrength = 0,
            };
            entity.ReceiveDamage(bathHealing, (float)Math.Sqrt(1 - stinkiness) * config.bathingMaxHealingPerSecond);
            lastHealed = DateTime.Now;
        }
    }

    public bool StinkyRateModifierIsActive()
    {
        var inBlock = entity.Api.World.BlockAccessor.GetBlockRaw(
            entity.Pos.AsBlockPos.X,
            entity.Pos.AsBlockPos.InternalY,
            entity.Pos.AsBlockPos.Z
        );
        return (
            entity.FeetInLiquid &&
            inBlock.BlockMaterial == EnumBlockMaterial.Liquid &&
            (
                inBlock.Code.Path.Contains("water")
            )
        );
    }

    public double StinkyModifyRate(double rateMultiplier)
    {
        double accumulator = config.bathingRateModifier;

        Room room = roomRegistry.GetRoomForPosition(entity.Pos.AsBlockPos);
        bool inRoom = room.ExitCount == 0;
        accumulator += inRoom ? config.bathingInsideRateModifier : 0;

        if (
            (entity.GetBehavior<EntityBehaviorBodyTemperature>()?.CurBodyTemperature is float bodyTemp)
            && config.bathingUseBodyTemperature
        )
        {
            accumulator *= Math.Clamp(bodyTemp / 37, 0, 1);
        }

        if (blockAccess is null)
        {
            return accumulator;  // Something bad has happened.
        }
        else if (inRoom)
        {
            blockAccess.Begin();
            blockAccess.WalkBlocks(
                room.Location.Start.AsBlockPos,
                room.Location.End.AsBlockPos,
                (block, x, y, z) =>
                {
                    if (block is BlockBoiler)
                    {
                        blockPos.Set(x, y, z);
                        // Boiler could be in room bounding box but not actually in the room.
                        if (!room.Contains(blockPos))
                        {
                            return;
                        }

                        BlockEntityBoiler beb = entity.Api.World.BlockAccessor.GetBlockEntity<BlockEntityBoiler>(blockPos);
                        if (beb is not null && (beb.IsBurning || beb.IsSmoldering))
                        {
                            accumulator *= config.bathingWithBoilerMultiplier;

                            // Also heal entity if they are bathing in a boiler bath.
                            applyBathHealing();
                        }
                    }
                }
            );
        }

        return accumulator;
    }

    public double stinkyPriority => 1.0;
}
