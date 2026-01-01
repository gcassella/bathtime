using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace BathTime;

public interface IHasConfig<TConfig> where TConfig : BathtimeBaseConfig<TConfig>, IHasConfigName, new()
{
    abstract public TConfig config { get; set; }
}

public static class ConfigReloadExtensions
{
    public static void LoadConfig<TReloader, TConfig>(this TReloader configReloader, ICoreAPI api)
    where TReloader : IHasConfig<TConfig>
    where TConfig : BathtimeBaseConfig<TConfig>, IHasConfigName, new()
    {
        configReloader.config = BathtimeBaseConfig<TConfig>.LoadStoredConfig(api);
    }

    public static void ListenConfig<TReloader, TConfig>(this TReloader configReloader, ICoreAPI api)
    where TReloader : IHasConfig<TConfig>
    where TConfig : BathtimeBaseConfig<TConfig>, IHasConfigName, new()
    {
        api.Event.RegisterEventBusListener(
            new EventBusListenerDelegate(
                (string eventname, ref EnumHandling handling, IAttribute data) =>
                {
                    configReloader.LoadConfig<TReloader, TConfig>(api);
                }
            ),
            0.5,
            Constants.RELOAD_COMMAND
        );
    }

    public static void ListenConfig<TReloader, TConfig>(this TReloader configReloader, ICoreAPI api, EventBusListenerDelegate onReload)
    where TReloader : IHasConfig<TConfig>
    where TConfig : BathtimeBaseConfig<TConfig>, IHasConfigName, new()
    {
        api.Event.RegisterEventBusListener(
            new EventBusListenerDelegate(onReload),
            0.5,
            Constants.RELOAD_COMMAND
        );
    }
}
