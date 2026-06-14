using System;
using Vintagestory.API.Common;

namespace BathTime;

class ItemTowel : Item
{
    public override void TryMergeStacks(ItemStackMergeOperation op)
    {
        double sinkWetness;
        // Check sink has towel behavior, otherwise pass-thru.
        if (GetCollectibleBehavior<CollectibleBehaviorTowel>(true) is CollectibleBehaviorTowel sinkTowelBehavior)
        {
            // NOTE: these calls to GetWetness will sync wetness on client + server.
            sinkWetness = sinkTowelBehavior.GetWetness(op.SinkSlot);
        }
        else
        {
            base.TryMergeStacks(op);
            return;
        }
        // Need to get wetness before calling base as slots might be null after.
        double sourceWetness;
        // Check source has towel behavior.
        if (op.SourceSlot.Itemstack?.Collectible.GetCollectibleBehavior<CollectibleBehaviorTowel>(true) is CollectibleBehaviorTowel sourceTowelBehavior)
        {
            // NOTE: these calls to GetWetness will sync wetness on client + server.
            sourceWetness = sourceTowelBehavior.GetWetness(op.SourceSlot);
        }
        else
        {
            base.TryMergeStacks(op);
            return;
        }

        // If trying to auto merge, refuse merging a wet towel with a much drier towel.
        if (op.CurrentPriority < EnumMergePriority.DirectMerge && Math.Abs(sourceWetness - sinkWetness) > 0.50)
        {
            op.MovedQuantity = 0;
            op.MovableQuantity = 0;
            op.RequiredPriority = EnumMergePriority.DirectMerge;
            return;
        }
        base.TryMergeStacks(op);

        if (op.MovedQuantity <= 0) return;

        if (api.Side == EnumAppSide.Server)
        {
            double resWetness = 0;
            int itemsInSink = op.SinkSlot.StackSize - op.MovedQuantity;
            int itemsMoved = op.MovedQuantity;

            // Use cubic mean to punish stacking wet towels with dry towels instead of arithmetic mean.
            // Merging wetness 1.0 into 3 wetness 0.0 towels => wetness ~0.625.
            resWetness += itemsMoved * Math.Pow(sourceWetness, 3.0);
            resWetness += itemsInSink * Math.Pow(sinkWetness, 3.0);
            resWetness /= op.SinkSlot.StackSize;
            resWetness = Math.Pow(resWetness, 1 / 3.0);

            sinkTowelBehavior.SetWetness(op.SinkSlot, resWetness);
            op.SinkSlot.MarkDirty();
        }
    }
}