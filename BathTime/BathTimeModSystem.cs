using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;

namespace BathTime;

#nullable disable

public class BathTimeModSystem : ModSystem
{

    StinkParticleSystem stinkParticleSystem;

    // Called on server and client
    // Useful for registering block/entity classes on both sides
    public override void Start(ICoreAPI api)
    {
        Mod.Logger.Notification("Hello from template mod: " + api.Side);

        api.RegisterEntityBehaviorClass(Constants.MOD_ID + ".stinky", typeof(EntityBehaviorStinky));
    }

    public override void StartServerSide(ICoreServerAPI sapi)
    {
        Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("bathtime:hello"));

        sapi.ChatCommands.Create("reset_stinky")
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.chat)
            .HandleWith(args =>
            {
                EntityBehaviorStinky behavior = args.Caller.Player.Entity.GetBehavior<EntityBehaviorStinky>();
                behavior.Stinkiness = 0;
                return TextCommandResult.Success("Your stinkiness has been reset");
            });
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        Mod.Logger.Notification("Hello from template mod client side: " + Lang.Get("bathtime:hello"));

        capi.ChatCommands.Create("stinky")
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.chat)
            .HandleWith(args =>
            {
                EntityBehaviorStinky behavior = args.Caller.Player.Entity.GetBehavior<EntityBehaviorStinky>();
                double stinkiness = behavior.Stinkiness;
                return TextCommandResult.Success("Your stinkiness is " + stinkiness);
            });

        stinkParticleSystem = new StinkParticleSystem(capi);
        stinkParticleSystem.Initialize();
    }
}
