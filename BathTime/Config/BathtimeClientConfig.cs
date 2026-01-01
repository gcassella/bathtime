namespace BathTime.Config;

public class BathtimeClientConfig() : BathtimeBaseConfig<BathtimeClientConfig>, IHasConfigName
{
    public static string configName { get; } = Constants.UI_CONFIG_NAME;

    public float stinkBarWidth { get; set; } = 388;

    public bool stinkBarHidden { get; set; } = false;

    public double stinkBarOffsetX { get; set; } = -116;

    public double stinkBarOffsetY { get; set; } = -50;
}
