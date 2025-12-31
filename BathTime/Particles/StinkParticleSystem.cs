using BathTime;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

public class StinkParticleSystem
{
    readonly ICoreClientAPI capi;
    private static int[] hsvaBaseColor = ColorUtil.RgbToHsvInts(108, 212, 60);
    private static AdvancedParticleProperties stinkParticles = new AdvancedParticleProperties()
    {
        HsvaColor = [
            NatFloat.createUniform(hsvaBaseColor[0], 16f),
            NatFloat.createUniform(hsvaBaseColor[1], 16f),
            NatFloat.createUniform(hsvaBaseColor[2], 16f),
            NatFloat.createUniform(128f, 64f),
        ],
        GravityEffect = NatFloat.createUniform(-0.02f, 0.0f),
        TerrainCollision = true,
        SelfPropelled = false,
        DieOnRainHeightmap = false,
        DieInLiquid = false,
        DieInAir = false,
        SwimOnLiquid = true,
        WindAffectednes = 0.5f,
        ParticleModel = EnumParticleModel.Quad,
        LifeLength = NatFloat.createGauss(0.75f, 0.25f),
        Velocity = [
            NatFloat.createGauss(0f, 0.5f),
            NatFloat.createUniform(0f, 0f),
            NatFloat.createGauss(0f, 0.5f),
        ],
        PosOffset = [
            NatFloat.createUniform(0f, 0.2f),
            NatFloat.createUniform(0f, 0.2f),
            NatFloat.createUniform(0f, 0.2f),
        ],
        Size = NatFloat.createInvexp(0.2f, 0.5f),
        VertexFlags = 64 & VertexFlags.ReflectiveBitMask & VertexFlags.ZOffsetBitMask,
    };

    private static Vec3d stinkPosVerticalOffset = new Vec3d(0.0, 0.5, 0.0);


    public StinkParticleSystem(ICoreClientAPI capi)
    {
        this.capi = capi;
    }

    public void Initialize()
    {
        capi.Event.RegisterAsyncParticleSpawner(AsyncParticleSpawn);
    }

    private bool AsyncParticleSpawn(float dt, IAsyncParticleManager manager)
    {
        // Search for nearby stinky entities
        foreach (var entity in capi.World.GetEntitiesAround(
            capi.World.Player.Entity.Pos.XYZ,
            100.0f,
            100.0f,
            null
        ))
        {
            if (!entity.HasBehavior<EntityBehaviorStinky>())
            {
                continue;
            }

            double entityStinkiness = entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID).GetDouble(Constants.STINKINESS);
            if (entityStinkiness < 0.25)
            {
                continue;
            }

            // Spawn particles on stinky entities.
            EntityPos entityPos = entity.Pos;
            stinkParticles.basePos = entityPos.XYZ + stinkPosVerticalOffset;
            var normalizedStinkinessAboveThreshold = (entityStinkiness - 0.25) / 0.75;
            var quantityMean = normalizedStinkinessAboveThreshold * normalizedStinkinessAboveThreshold;
            stinkParticles.Quantity = NatFloat.createGauss((float)quantityMean, 0.25f);
            manager.Spawn(stinkParticles);
        }

        return true;
    }
}