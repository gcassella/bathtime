using System;
using Vintagestory.API.Client;

namespace BathTime.HUD;

public class StinkBarHud : HudElement
{
    GuiElementStatbar? stinkBar;
    private long listenerId;


    public StinkBarHud(ICoreClientAPI capi) : base(capi)
    {
        listenerId = capi.Event.RegisterGameTickListener(
            new Action<float>(OnGameTick),
            100,
            0
        );
    }

    private void OnGameTick(float dt)
    {
        if (capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID)?.GetDouble(Constants.STINKINESS) is double stinkiness)
        {
            stinkBar?.SetLineInterval(0.1f);
            stinkBar?.SetValues((float)stinkiness, 0.0f, 1.0f);
        }
    }

    public override void OnOwnPlayerDataReceived()
    {
        ComposeGuis();
    }

    private void ComposeGuis()
    {
        float width = 388;

        ElementBounds dialogBounds = new ElementBounds()
        {
            Alignment = EnumDialogArea.CenterBottom,
            BothSizing = ElementSizing.Fixed,
            fixedWidth = width,
            fixedHeight = 50,
            fixedY = 10,
        }.WithFixedAlignmentOffset(-116, -50);
        ElementBounds stinkBarBounds = ElementStdBounds.Statbar(
            EnumDialogArea.CenterFixed,
            width
        );

        string key = Constants.MOD_ID + ".stinkbar-" + capi.World.Player.Entity.EntityId;
        Composers["stinkbar"] =
            capi.Gui
            .CreateCompo(key, dialogBounds.FlatCopy().FixedGrow(0, 20))
            .BeginChildElements(dialogBounds)
                .AddStatbar(stinkBarBounds, Constants.stinkBaseColord, Constants.MOD_ID + ".stinkbar")
            .EndChildElements()
            .Compose()
        ;

        stinkBar = Composers["stinkbar"].GetStatbar(Constants.MOD_ID + ".stinkbar");
        TryOpen();
    }

    public override void Dispose()
    {
        base.Dispose();

        capi.Event.UnregisterGameTickListener(listenerId);
    }
}
