namespace BathTime;

public partial class BathtimeConfig : BathtimeBaseConfig<BathtimeConfig>, IHasConfigName
{
    public static string configName { get; } = Constants.CONFIG_NAME;
}
