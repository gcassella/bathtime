using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BathTime;

public partial class BathtimeClientConfig : IConfig
{
    public int maxFlies { get; set; } = 400;

    public double spawnChanceFlies { get; set; } = 0.01;

    public float spawnCountMeanFlies { get; set; } = 8;

    public float spawnCountVarianceFlies { get; set; } = 16;
}

public class StinkParticleSystem
{
    readonly ICoreClientAPI capi;

    private BathtimeClientConfig config
    {
        get => BathtimeBaseConfig<BathtimeClientConfig>.LoadStoredConfig(capi);
    }

    private AdvancedParticleProperties stinkParticles = new AdvancedParticleProperties()
    {
        HsvaColor = [
            NatFloat.createUniform(Constants.hsvaStinkBaseColor[0], 16f),
            NatFloat.createUniform(Constants.hsvaStinkBaseColor[1], 16f),
            NatFloat.createUniform(Constants.hsvaStinkBaseColor[2], 16f),
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

    private NatFloat flySpawnCount = new(8, 16, EnumDistribution.GAUSSIAN);

    private NatFloat flySwarmCohesion = new(0.26f, 0.18f, EnumDistribution.UNIFORM);

    private NatFloat[] flySwarmPos = [
        new(0.0f, 2.5f, EnumDistribution.UNIFORM),
        new(0, 0, EnumDistribution.UNIFORM),
        new(0.0f, 2.5f, EnumDistribution.UNIFORM),
    ];

    private NatFloat flyShouldSpawn = new(0.5f, 0.5f, EnumDistribution.UNIFORM);

    private Vec3d stinkPosVerticalOffset = new Vec3d(0.0, 0.5, 0.0);

    public StinkParticleSystem(ICoreClientAPI capi)
    {
        this.capi = capi;
    }

    private EntityParticleSystem? _entityParticleSystem;
    public void Initialize()
    {
        flySpawnCount.avg = config.spawnCountMeanFlies;
        flySpawnCount.var = config.spawnCountVarianceFlies;

        capi.Event.RegisterAsyncParticleSpawner(AsyncParticleSpawn);
        _entityParticleSystem = capi.ModLoader.GetModSystem<EntityParticleSystem>();
        if (_entityParticleSystem == null)
        {
            capi.Logger.Error(Constants.LOGGING_PREFIX + "Couldn't find entity particle system.");
            return;
        }
        _entityParticleSystem.OnSimTick += OnSimTickGnats;
    }

    private bool AsyncParticleSpawn(float dt, IAsyncParticleManager manager)
    {
        // Search for nearby stinky entities
        foreach (var entity in capi.World.GetEntitiesAround(
            capi.World.Player.Entity.Pos.XYZ,
            100.0f,
            100.0f,
            entity =>
            {
                return (
                    entity.HasBehavior<EntityBehaviorStinky>()
                    && entity.GetBehavior<EntityBehaviorStinky>()?.Stinkiness > 0.25
                );
            }
        ))
        {
            var stinkiness = entity.GetBehavior<EntityBehaviorStinky>()?.Stinkiness;
            if (stinkiness is null)
            {
                continue;
            }
            // Spawn particles on stinky entities.
            EntityPos entityPos = entity.Pos;
            stinkParticles.basePos = entityPos.XYZ + stinkPosVerticalOffset;
            var normalizedStinkinessAboveThreshold = (stinkiness - 0.25) / 0.75;
            var quantityMean = normalizedStinkinessAboveThreshold * normalizedStinkinessAboveThreshold;
            stinkParticles.Quantity = NatFloat.createGauss((float)quantityMean, 0.25f);
            manager.Spawn(stinkParticles);
        }

        return true;
    }

    private BlockPos gnatSpawnPos = new(0);
    private void OnSimTickGnats(float dt)
    {
        if (capi == null) return;

        var entityParticleSystem = capi.ModLoader.GetModSystem<EntityParticleSystem>();
        if (entityParticleSystem is null) return;

        var roomRegistry = capi.ModLoader.GetModSystem<RoomRegistry>();
        if (roomRegistry is null) return;

        try
        {
            // Search for nearby very stinky entities
            foreach (var entity in capi.World.GetEntitiesAround(
                capi.World.Player.Entity.Pos.XYZ,
                100.0f,
                100.0f,
                entity =>
                {
                    return (
                        entity.HasBehavior<EntityBehaviorStinky>()
                        && (entity.GetBehavior<EntityBehaviorStinky>()?.Stinkiness) > 0.9
                        && (double)flyShouldSpawn.nextFloat() < config.spawnChanceFlies
                        && entityParticleSystem.Count["matinggnats"] < config.maxFlies
                    );
                }
            ))
            {
                Room room = roomRegistry.GetRoomForPosition(entity.Pos.AsBlockPos);
                bool inRoom = room.ExitCount == 0;
                foreach (var _ in Enumerable.Range(0, (int)Math.Max(flySpawnCount.nextFloat(), 0)))
                {
                    EntityPos entityPos = entity.Pos;
                    if (inRoom && (room.Location.SizeXZ < 25))
                    {
                        gnatSpawnPos.Set(
                            room.Location.Start.AsBlockPos.X + flyShouldSpawn.nextFloat() * room.Location.SizeX,
                            room.Location.Start.AsBlockPos.Y + flyShouldSpawn.nextFloat() * room.Location.SizeY,
                            room.Location.Start.AsBlockPos.Z + flyShouldSpawn.nextFloat() * room.Location.SizeZ
                        );
                    }
                    else
                    {
                        gnatSpawnPos.Set(
                            entityPos.AsBlockPos.X + flySwarmPos[0].nextFloat(),
                            entityPos.AsBlockPos.Y + flySwarmPos[1].nextFloat(),
                            entityPos.AsBlockPos.Z + flySwarmPos[2].nextFloat()
                        );
                    }

                    entityParticleSystem?.SpawnParticle(
                        new EntityParticleMatingGnats(
                            capi,
                            flySwarmCohesion.nextFloat(),
                            gnatSpawnPos.X,
                            gnatSpawnPos.Y,
                            gnatSpawnPos.Z
                        )
                    );
                }
            }
        }
        catch (NullReferenceException exc)
        {
            capi.Logger.Error(exc);
            return;
        }
    }
}