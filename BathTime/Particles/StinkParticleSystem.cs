using System;
using System.IO;
using System.Threading;
using BathTime;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;


#nullable disable

internal class StinkParticles : IParticlePropertiesProvider
{
    public static ThreadLocal<Random> randTL = new ThreadLocal<Random>(() => new Random());

    public float MinQuantity;

    public float AddQuantity;

    public float WindAffectednes;

    public Vec3d MinPos;

    public Vec3d AddPos = new Vec3d();

    public Vec3f MinVelocity = new Vec3f();

    public Vec3f AddVelocity = new Vec3f();

    public float LifeLength;

    public float addLifeLength = 0f;

    public float MinSize = 1f;

    public float MaxSize = 1f;

    public int Color;

    public bool SelfPropelled;

    //
    // Summary:
    //     The block which can be used to get a random color when particle spawns are send
    //     from the server to the client
    public Block ColorByBlock;

    //
    // Summary:
    //     The item which can be used to get a random color when particle spawns are send
    //     from the server to the client
    public Item ColorByItem;

    //
    // Summary:
    //     The color map for climate color mapping. Leave null for no coloring by climate
    public string ClimateColorMap;

    //
    // Summary:
    //     The color map for season color mapping. Leave null for no coloring by season
    public string SeasonColorMap;

    protected Vec3d tmpPos = new Vec3d();

    private Vec3f tmpVelo = new Vec3f();

    public bool IgnoreUserConfig { get; set; }

    public static Random rand => randTL.Value;

    public Vec3f ParentVelocity { get; set; }

    public float ParentVelocityWeight { get; set; }

    public float GravityEffect { get; set; }

    public int LightEmission { get; set; }

    public int VertexFlags { get; set; }

    public bool Async { get; set; }

    public float Bounciness { get; set; }

    public bool ShouldDieInAir { get; set; }

    public bool ShouldDieInLiquid { get; set; }

    public bool ShouldSwimOnLiquid { get; set; }

    public bool WithTerrainCollision { get; set; } = true;

    public EvolvingNatFloat OpacityEvolve { get; set; }

    public EvolvingNatFloat RedEvolve { get; set; }

    public EvolvingNatFloat GreenEvolve { get; set; }

    public EvolvingNatFloat BlueEvolve { get; set; }

    public EvolvingNatFloat SizeEvolve { get; set; }

    public bool RandomVelocityChange { get; set; }

    public bool DieInAir => ShouldDieInAir;

    public bool DieInLiquid => ShouldDieInLiquid;

    public bool SwimOnLiquid => ShouldSwimOnLiquid;

    public float Quantity => MinQuantity + (float)rand.NextDouble() * AddQuantity;

    public float Size => MinSize + (float)rand.NextDouble() * (MaxSize - MinSize);

    float IParticlePropertiesProvider.LifeLength => LifeLength + addLifeLength * (float)rand.NextDouble();

    public EnumParticleModel ParticleModel { get; set; }

    public EvolvingNatFloat[] VelocityEvolve => null;

    bool IParticlePropertiesProvider.SelfPropelled => SelfPropelled;

    public bool TerrainCollision => WithTerrainCollision;

    public IParticlePropertiesProvider[] SecondaryParticles { get; set; }

    public IParticlePropertiesProvider[] DeathParticles { get; set; }

    public float SecondarySpawnInterval => 0f;

    public bool DieOnRainHeightmap { get; set; }

    public bool WindAffected { get; set; }

    public void Init(ICoreAPI api)
    {
    }

    public StinkParticles()
    {
    }

    public int GetRgbaColor(ICoreClientAPI capi)
    {
        if (ColorByBlock != null)
        {
            return ColorByBlock.GetRandomColor(capi, new ItemStack(ColorByBlock));
        }

        if (ColorByItem != null)
        {
            return ColorByItem.GetRandomColor(capi, new ItemStack(ColorByItem));
        }

        if (SeasonColorMap != null || ClimateColorMap != null)
        {
            return capi.World.ApplyColorMapOnRgba(ClimateColorMap, SeasonColorMap, Color, (int)MinPos.X, (int)MinPos.Y, (int)MinPos.Z);
        }

        return Color;
    }

    public bool UseLighting()
    {
        return true;
    }

    public void ToBytes(BinaryWriter writer)
    {
        writer.Write(MinQuantity);
        writer.Write(AddQuantity);
        MinPos.ToBytes(writer);
        AddPos.ToBytes(writer);
        MinVelocity.ToBytes(writer);
        AddVelocity.ToBytes(writer);
        writer.Write(LifeLength);
        writer.Write(GravityEffect);
        writer.Write(MinSize);
        writer.Write(MaxSize);
        writer.Write(Color);
        writer.Write(VertexFlags);
        writer.Write((int)ParticleModel);
        writer.Write(ShouldDieInAir);
        writer.Write(ShouldDieInLiquid);
        writer.Write(OpacityEvolve == null);
        if (OpacityEvolve != null)
        {
            OpacityEvolve.ToBytes(writer);
        }

        writer.Write(RedEvolve == null);
        if (RedEvolve != null)
        {
            RedEvolve.ToBytes(writer);
        }

        writer.Write(GreenEvolve == null);
        if (GreenEvolve != null)
        {
            GreenEvolve.ToBytes(writer);
        }

        writer.Write(BlueEvolve == null);
        if (BlueEvolve != null)
        {
            BlueEvolve.ToBytes(writer);
        }

        writer.Write(SizeEvolve == null);
        if (SizeEvolve != null)
        {
            SizeEvolve.ToBytes(writer);
        }

        writer.Write(SelfPropelled);
        writer.Write(ColorByBlock == null);
        if (ColorByBlock != null)
        {
            writer.Write(ColorByBlock.BlockId);
        }

        writer.Write(ColorByItem == null);
        if (ColorByItem != null)
        {
            writer.Write(ColorByItem.ItemId);
        }

        writer.Write(ClimateColorMap == null);
        if (ClimateColorMap != null)
        {
            writer.Write(ClimateColorMap);
        }

        writer.Write(SeasonColorMap == null);
        if (SeasonColorMap != null)
        {
            writer.Write(SeasonColorMap);
        }

        writer.Write(Bounciness);
        writer.Write(Async);
        writer.Write(LightEmission);
        writer.Write(IgnoreUserConfig);
    }

    public void FromBytes(BinaryReader reader, IWorldAccessor resolver)
    {
        MinQuantity = reader.ReadSingle();
        AddQuantity = reader.ReadSingle();
        MinPos = Vec3d.CreateFromBytes(reader);
        AddPos = Vec3d.CreateFromBytes(reader);
        MinVelocity = Vec3f.CreateFromBytes(reader);
        AddVelocity = Vec3f.CreateFromBytes(reader);
        LifeLength = reader.ReadSingle();
        GravityEffect = reader.ReadSingle();
        MinSize = reader.ReadSingle();
        MaxSize = reader.ReadSingle();
        Color = reader.ReadInt32();
        VertexFlags = reader.ReadInt32();
        ParticleModel = (EnumParticleModel)reader.ReadInt32();
        ShouldDieInAir = reader.ReadBoolean();
        ShouldDieInLiquid = reader.ReadBoolean();
        if (!reader.ReadBoolean())
        {
            OpacityEvolve = EvolvingNatFloat.CreateFromBytes(reader);
        }

        if (!reader.ReadBoolean())
        {
            RedEvolve = EvolvingNatFloat.CreateFromBytes(reader);
        }

        if (!reader.ReadBoolean())
        {
            GreenEvolve = EvolvingNatFloat.CreateFromBytes(reader);
        }

        if (!reader.ReadBoolean())
        {
            BlueEvolve = EvolvingNatFloat.CreateFromBytes(reader);
        }

        if (!reader.ReadBoolean())
        {
            SizeEvolve = EvolvingNatFloat.CreateFromBytes(reader);
        }

        SelfPropelled = reader.ReadBoolean();
        if (!reader.ReadBoolean())
        {
            ColorByBlock = resolver.Blocks[reader.ReadInt32()];
        }

        if (!reader.ReadBoolean())
        {
            ColorByItem = resolver.Items[reader.ReadInt32()];
        }

        if (!reader.ReadBoolean())
        {
            ClimateColorMap = reader.ReadString();
        }

        if (!reader.ReadBoolean())
        {
            SeasonColorMap = reader.ReadString();
        }

        Bounciness = reader.ReadSingle();
        Async = reader.ReadBoolean();
        LightEmission = reader.ReadInt32();
        IgnoreUserConfig = reader.ReadBoolean();
    }

    public void BeginParticle()
    {
        if (WindAffectednes > 0f)
        {
            ParentVelocityWeight = WindAffectednes;
            ParentVelocity = GlobalConstants.CurrentWindSpeedClient;
        }
    }

    public void PrepareForSecondarySpawn(ParticleBase particleInstance)
    {
        Vec3d position = particleInstance.Position;
        MinPos.X = position.X;
        MinPos.Y = position.Y;
        MinPos.Z = position.Z;
    }

    public Vec3d Pos
    {
        get
        {
            tmpPos.Set(MinPos.X + AddPos.X * (2 * rand.NextDouble() - 1), MinPos.Y + AddPos.Y * (2 * rand.NextDouble() - 1), MinPos.Z + AddPos.Z * (2 * rand.NextDouble() - 1));
            return tmpPos;
        }
    }

    public Vec3f GetVelocity(Vec3d pos)
    {
        tmpVelo.Set(MinVelocity.X + AddVelocity.X * (float)(2 * rand.NextDouble() - 1), MinVelocity.Y + AddVelocity.Y * (float)(2 * rand.NextDouble() - 1), MinVelocity.Z + AddVelocity.Z * (float)(2 * rand.NextDouble() - 1));
        return tmpVelo;
    }
}

#nullable enable

public class StinkParticleSystem
{
    readonly ICoreClientAPI capi;
    private static StinkParticles stinkParticles = new StinkParticles()
    {
        MinPos = new Vec3d(0.0, 1.0, 0.0),
        AddPos = new Vec3d(0.5, 0.0, 0.5),
        MinQuantity = 0,
        AddQuantity = 0,
        Color = ColorUtil.ToRgba(255, 108, 212, 60),
        GravityEffect = -0.02f,
        WithTerrainCollision = true,
        DieOnRainHeightmap = false,
        ShouldDieInLiquid = false,
        ShouldDieInAir = false,
        ShouldSwimOnLiquid = true,
        WindAffectednes = 0.5f,
        WindAffected = true,
        ParticleModel = EnumParticleModel.Quad,
        LifeLength = 0.75f,
        MinVelocity = new Vec3f(),
        AddVelocity = new Vec3f(-0.5f, 0.0f, 0.5f),
        MinSize = 0.2f,
        MaxSize = 0.7f,
        VertexFlags = 0
    };

    private static Vec3d stinkPosOffset = new Vec3d(0.5, 0.0, 0.5);


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
            ;

            // Spawn particles on stinky entities.
            EntityPos entityPos = entity.Pos;
            stinkParticles.MinPos = entityPos.XYZ + stinkPosOffset;
            stinkParticles.AddQuantity = (int)(3 * entityStinkiness);
            manager.Spawn(stinkParticles);
        }

        return true;
    }
}