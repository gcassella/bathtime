using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BathTime;

public class StinkyRateModifierBath(Entity entity) : IStinkyRateModifier
{
    
    private RoomRegistry roomRegistry = entity.Api.ModLoader.GetModSystem<RoomRegistry>();
    private Entity entity = entity;

    private ICachingBlockAccessor? blockAccess = entity.Api.World.GetCachingBlockAccessor(false, false);

    private BlockPos blockPos = new(0);

    ~StinkyRateModifierBath()
    {
        blockAccess?.Dispose();
        blockAccess = null;
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
        double accumulator = -25;

        Room room = roomRegistry.GetRoomForPosition(entity.Pos.AsBlockPos);
        bool inRoom = room.ExitCount == 0;
        accumulator += inRoom ? -50 : 0;

        if (entity.GetBehavior<EntityBehaviorBodyTemperature>()?.CurBodyTemperature is float bodyTemp)
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
                            accumulator *= 2;
                        }
                    }
                }
            );
        }

        return accumulator;
    }

    public double stinkyPriority => 1.0;
}
