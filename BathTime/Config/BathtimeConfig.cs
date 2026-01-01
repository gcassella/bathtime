namespace BathTime.Config;

public class BathtimeConfig : BathtimeBaseConfig<BathtimeClientConfig>, IHasConfigName
{
    public static string configName { get; } = Constants.CONFIG_NAME;
}
