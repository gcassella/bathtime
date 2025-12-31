using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using BathTime.HUD;
using Vintagestory.API.MathTools;

namespace BathTime;

#nullable disable

public class BathTimeModSystem : ModSystem
{
    StinkParticleSystem stinkParticleSystem;

    public override void Start(ICoreAPI api)
    {
        api.RegisterEntityBehaviorClass(Constants.MOD_ID + ".stinky", typeof(EntityBehaviorStinky));
    }

    public override void StartServerSide(ICoreServerAPI sapi)
    {
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.Gui.RegisterDialog(
            [
                new StinkBarHud(capi)
            ]
        );


        stinkParticleSystem = new StinkParticleSystem(capi);
        stinkParticleSystem.Initialize();
    }
}
