using System;
using BathTime.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace BathTime.HUD;

public class StinkBarHud : HudElement
{
    GuiElementStatbar? stinkBar;
    private long listenerId;

    private BathtimeClientConfig? config;


    private void LoadConfig()
    {
        try
        {
            config = capi.LoadModConfig<BathtimeClientConfig?>(Constants.UI_CONFIG_NAME);
            if (config is null)
            {
                capi.Logger.Warning(Constants.LOGGING_PREFIX + "Could not find Bathtime UI config. Writing default.");
                config = new BathtimeClientConfig();
                capi.StoreModConfig(config, Constants.UI_CONFIG_NAME);
            }
        }
        catch
        {
            capi.Logger.Warning(Constants.LOGGING_PREFIX + "Could not initialize Bathtime UI config. Did you make a typo? Falling back to default.");
            config = new BathtimeClientConfig();
        }
    }

    private void ReloadConfig(string eventname, ref EnumHandling handling, IAttribute data)
    {
        capi.Logger.Notification(Constants.LOGGING_PREFIX + "Reloading UI config.");

        ClearComposers();
        stinkBar = null;

        LoadConfig();
        ComposeGuis();
    }

    public StinkBarHud(ICoreClientAPI capi) : base(capi)
    {
        listenerId = capi.Event.RegisterGameTickListener(
            new Action<float>(OnGameTick),
            100,
            0
        );

        LoadConfig();
        capi.Event.RegisterEventBusListener(
            new EventBusListenerDelegate(ReloadConfig),
            0.5,
            Constants.RELOAD_COMMAND
        );
    }

    private void OnGameTick(float dt)
    {
        if (capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID)?.GetDouble(Constants.STINKINESS) is double stinkiness)
        {
            stinkBar?.SetLineInterval(0.05f);
            stinkBar?.SetValues((float)stinkiness, 0.0f, 1.0f);
        }
    }

    public override void OnOwnPlayerDataReceived()
    {
        ComposeGuis();
    }

    private void ComposeGuis()
    {
        if (config is null)
        {
            capi.Logger.Error(Constants.LOGGING_PREFIX + "Bathtime UI config is null!");
            return;
        }

        ElementBounds dialogBounds = new ElementBounds()
        {
            Alignment = EnumDialogArea.CenterBottom,
            BothSizing = ElementSizing.Fixed,
            fixedWidth = config.stinkBarWidth,
            fixedHeight = 50,
            fixedY = 10,
        }.WithFixedAlignmentOffset(config.stinkBarOffsetX, config.stinkBarOffsetY);
        ElementBounds stinkBarBounds = ElementStdBounds.Statbar(
            EnumDialogArea.CenterFixed,
            config.stinkBarWidth
        );

        string key = Constants.MOD_ID + ".stinkbar-" + capi.World.Player.Entity.EntityId;

        Composers["stinkbar"] =
            capi.Gui
            .CreateCompo(key, dialogBounds.FlatCopy().FixedGrow(0, 20))
            .BeginChildElements(dialogBounds)
                .AddIf(!config.stinkBarHidden)
                    .AddStatbar(stinkBarBounds, Constants.stinkBaseColord, true, Constants.MOD_ID + ".stinkbar")
                .EndIf()
            .EndChildElements()
            .Compose()
        ;

        if (!config.stinkBarHidden)
        {
            stinkBar = Composers["stinkbar"].GetStatbar(Constants.MOD_ID + ".stinkbar");
        }
        TryOpen();
    }

    public override void Dispose()
    {
        base.Dispose();

        capi.Event.UnregisterGameTickListener(listenerId);
    }
}
