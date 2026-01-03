using Vintagestory.API.Common;

namespace BathTime;

public partial class BathtimeConfig : IConfig
{
    public static string configName { get; } = Constants.CONFIG_NAME;

    public static EnumAppSide Side { get; } = EnumAppSide.Server;
}
