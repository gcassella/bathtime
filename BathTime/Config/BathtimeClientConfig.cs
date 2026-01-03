using Vintagestory.API.Common;

namespace BathTime;

public partial class BathtimeClientConfig() : IConfig
{
    public static string configName { get; } = Constants.CLIENT_CONFIG_NAME;

    public static EnumAppSide Side { get; } = EnumAppSide.Client;

    public float stinkBarWidth { get; set; } = 388;

    public bool stinkBarHidden { get; set; } = false;

    public double stinkBarOffsetX { get; set; } = -116;

    public double stinkBarOffsetY { get; set; } = -50;
}
