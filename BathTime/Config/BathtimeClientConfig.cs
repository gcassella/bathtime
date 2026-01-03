using Vintagestory.API.Common;

namespace BathTime;

public class BathtimeClientConfig() : BathtimeBaseConfig<BathtimeClientConfig>, IHasConfigName
{
    public static string configName { get; } = Constants.CLIENT_CONFIG_NAME;

    public float stinkBarWidth { get; set; } = 388;

    public bool stinkBarHidden { get; set; } = false;

    public double stinkBarOffsetX { get; set; } = -116;

    public double stinkBarOffsetY { get; set; } = -50;

    protected static new BathtimeClientConfig LoadInner(ICoreAPI api, string configName)
    {
        if (api.Side == EnumAppSide.Server)
        {
            api.Logger.Error("Client config being loaded on server. This is probably a mistake.");
        }
        return BathtimeBaseConfig<BathtimeClientConfig>.LoadInner(api, configName);
    }
}
