using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace BathTime;

public class StinkBarHud : HudElement
{
    GuiElementStatbar? stinkBar;
    private long listenerId;

    private BathtimeClientConfig config
    {
        get => BathtimeBaseConfig<BathtimeClientConfig>.LoadStoredConfig(capi);
    }

    private void OnLoadConfig()
    {
        capi.Logger.Notification(Constants.LOGGING_PREFIX + "Reloading UI config.");
        ClearComposers();
        stinkBar = null;
        ComposeGuis();
    }

    public StinkBarHud(ICoreClientAPI capi) : base(capi)
    {
        listenerId = capi.Event.RegisterGameTickListener(
            new Action<float>(OnGameTick),
            100,
            0
        );

        OnLoadConfig();
        capi.Event.RegisterEventBusListener(
            new EventBusListenerDelegate(
                (string eventname, ref EnumHandling handling, IAttribute data) =>
                {
                    OnLoadConfig();
                }
            ),
            0.5,
            Constants.RELOAD_COMMAND
        );
    }

    private void OnGameTick(float dt)
    {
        stinkBar?.SetLineInterval(0.05f);
        stinkBar?.SetValues((float)capi.World.Player.Entity.GetDoubleAttribute(Constants.STINKINESS_KEY), 0.0f, 1.0f);
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
