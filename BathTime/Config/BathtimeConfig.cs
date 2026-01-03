using Vintagestory.API.Common;

namespace BathTime;

public partial class BathtimeConfig : BathtimeBaseConfig<BathtimeConfig>, IHasConfigName
{
    public static string configName { get; } = Constants.CONFIG_NAME;

    protected static new BathtimeConfig LoadInner(ICoreAPI api, string configName)
    {
        if (api.Side == EnumAppSide.Client)
        {
            api.Logger.Error("Server config being loaded on client. This is probably a mistake.");
        }
        return BathtimeBaseConfig<BathtimeConfig>.LoadInner(api, configName);
    }
}
