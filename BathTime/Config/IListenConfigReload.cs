using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace BathTime;

public interface IListenConfigReload<T> where T : BathtimeBaseConfig<T>, IHasConfigName, new()
{
    abstract protected T config { get; set; }

    public void LoadConfig(ICoreAPI api)
    {
        config = BathtimeBaseConfig<T>.LoadStoredConfig(api);
    }

    public void ListenConfig(ICoreAPI api)
    {
        api.Event.RegisterEventBusListener(
            new EventBusListenerDelegate(
                (string eventname, ref EnumHandling handling, IAttribute data) =>
                {
                    LoadConfig(api);
                }
            ),
            0.5,
            Constants.RELOAD_COMMAND
        );
    }

    public void ListenConfig(ICoreAPI api, EventBusListenerDelegate onReload)
    {
        api.Event.RegisterEventBusListener(
            new EventBusListenerDelegate(onReload),
            0.5,
            Constants.RELOAD_COMMAND
        );
    }
}