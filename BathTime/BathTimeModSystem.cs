using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using System.Linq;
using Vintagestory.API.Util;
using System;
using Vintagestory.API.Datastructures;

namespace BathTime;


public class BathTimeModSystem : ModSystem
{
    StinkParticleSystem? stinkParticleSystem;

    public override void Start(ICoreAPI api)
    {
        api.RegisterEntityBehaviorClass(Constants.MOD_ID + ".stinky", typeof(EntityBehaviorStinky));

        api.RegisterCollectibleBehaviorClass(Constants.MOD_ID + ".soap", typeof(CollectibleBehaviorSoap));
        api.RegisterCollectibleBehaviorClass(Constants.MOD_ID + ".perfume", typeof(CollectibleBehaviorPerfume));
    }

    private void SyncConfigToPlayer(IPlayer player, BathtimeConfig config)
    {
        player.Entity.SetDoubleAttribute(Constants.STINK_PARTICLE_THRESHOLD_KEY, config.stinkParticleThreshold);
        player.Entity.SetDoubleAttribute(Constants.FLIES_PARTICLE_THRESHOLD_KEY, config.fliesParticleThreshold);
    }

    public override void StartServerSide(ICoreServerAPI sapi)
    {
        BathtimeBaseConfig<BathtimeConfig>.LoadStoredConfig(sapi);

        // Add event bus listeners to propagate server config values that need to be exposed to client
        // via world attributes. Sync whenever a player joins or the config is reloaded.
        sapi.Event.PlayerJoin += new PlayerDelegate(player =>
        {
            var config = BathtimeBaseConfig<BathtimeConfig>.LoadStoredConfig(sapi);
            SyncConfigToPlayer(player, config);
        });

        sapi.Event.RegisterEventBusListener(
            new EventBusListenerDelegate(
                (string eventname, ref EnumHandling handling, IAttribute data) =>
                {
                    var config = BathtimeBaseConfig<BathtimeConfig>.LoadStoredConfig(sapi);
                    foreach (IPlayer player in sapi.World.AllOnlinePlayers)
                    {
                        SyncConfigToPlayer(player, config);
                    }
                }
            ),
            0.5,
            Constants.RELOAD_COMMAND
        );

        sapi.ChatCommands.Create(Constants.MOD_ID)
            .RequiresPrivilege(Privilege.controlserver)
            .WithDescription("Commands for controlling server side Bathtime mod.")
            .BeginSub(Constants.RELOAD_COMMAND)
                .WithDescription("Reload server side Bathtime config.")
                .HandleWith(
                    args =>
                    {
                        BathtimeBaseConfig<BathtimeConfig>.GloballyReloadStoredConfig(sapi);
                        return TextCommandResult.Success();
                    }
                )
            .EndSub()
            .BeginSub(Constants.SET_COMMAND)
                .WithDescription("Set server side Bathtime config value.")
                .WithArgs([
                    sapi.ChatCommands.Parsers.WordRange(
                        "valueName",
                        BathtimeBaseConfig<BathtimeConfig>.ValueNames
                    ),
                    sapi.ChatCommands.Parsers.Word("value"),
                ])
                .HandleWith(
                    (args) =>
                    {
                        string valueName = (string)(args[0] ?? throw new ArgumentNullException());
                        string value = (string)(args[1] ?? throw new ArgumentNullException());
                        bool success = BathtimeBaseConfig<BathtimeConfig>.UpdateStoredConfig(sapi, valueName, value);

                        if (success)
                        {
                            return TextCommandResult.Success("Set " + valueName + "=" + value + " succeeded.");
                        }
                        else
                        {
                            return TextCommandResult.Error("Set " + valueName + "=" + value + " failed.");
                        }
                    }
                )
            .EndSub()
            .BeginSub("hurtme")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .WithArgs(sapi.ChatCommands.Parsers.Float("damage"))
                .HandleWith(
                    args =>
                    {
                        DamageSource godDamage = new DamageSource()
                        {
                            Type = EnumDamageType.Injury,
                            SourceEntity = null,
                            KnockbackStrength = 0,
                        };
                        args.Caller.Player.Entity.ReceiveDamage(
                            godDamage,
                            (float)args[0]
                        );
                        return TextCommandResult.Success();
                    }
                )
            .EndSub();
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        BathtimeBaseConfig<BathtimeClientConfig>.LoadStoredConfig(capi);

        stinkParticleSystem = new StinkParticleSystem(capi);
        stinkParticleSystem.Initialize();

        capi.ChatCommands.Create(Constants.MOD_ID)
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.chat)
            .WithDescription("Commands for controlling client side Bathtime mod.")
            .BeginSub(Constants.HUD_COMMAND)
                .RequiresPrivilege(Privilege.gamemode)
                .HandleWith(
                    args =>
                    {
                        if (!capi.Gui.LoadedGuis.Any(gui => gui.GetType() == typeof(StinkBarHud)))
                        {
                            capi.Gui.RegisterDialog(
                                [
                                    new StinkBarHud(capi)
                                ]
                            );
                        }

                        return TextCommandResult.Success();
                    }
                )
            .EndSub()
            .BeginSub(Constants.RELOAD_COMMAND)
                .WithDescription("Reload client side Bathtime config.")
                .HandleWith(
                    args =>
                    {
                        BathtimeBaseConfig<BathtimeClientConfig>.GloballyReloadStoredConfig(capi);
                        return TextCommandResult.Success();
                    }
                )
            .EndSub()
            .BeginSub(Constants.SET_COMMAND)
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .WithDescription("Set client side Bathtime config value.")
                .WithArgs([
                    capi.ChatCommands.Parsers.WordRange(
                        "valueName",
                        BathtimeBaseConfig<BathtimeClientConfig>.ValueNames.Remove("configName")
                    ),
                    capi.ChatCommands.Parsers.Word("value"),
                ])
                .HandleWith(
                    (args) =>
                    {
                        string valueName = (string)(args[0] ?? throw new ArgumentNullException());
                        string value = (string)(args[1] ?? throw new ArgumentNullException());
                        bool success = BathtimeBaseConfig<BathtimeClientConfig>.UpdateStoredConfig(capi, valueName, value);

                        if (success)
                        {
                            return TextCommandResult.Success("Set " + valueName + "=" + value + " succeeded.");
                        }
                        else
                        {
                            return TextCommandResult.Error("Set " + valueName + "=" + value + " failed.");
                        }
                    }
                )
            .EndSub();
    }
}
