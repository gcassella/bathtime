using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace BathTime;

public class StinkBarHud : HudElement, IHasConfig<BathtimeClientConfig>
{
    GuiElementStatbar? stinkBar;
    private long listenerId;

    private BathtimeClientConfig _config = new();

    public BathtimeClientConfig config
    {
        get => _config;
        set => _config = value;
    }

    private void OnLoadConfig()
    {
        capi.Logger.Notification(Constants.LOGGING_PREFIX + "Reloading UI config.");
        ClearComposers();
        stinkBar = null;
        this.LoadConfig<StinkBarHud, BathtimeClientConfig>(capi);
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
        this.ListenConfig<StinkBarHud, BathtimeClientConfig>(
            capi,
            (string eventname, ref EnumHandling handling, IAttribute data) =>
            {
                OnLoadConfig();
            }
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
