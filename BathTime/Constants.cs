using Vintagestory.API.MathTools;

namespace BathTime;

internal static class Constants
{
    public static string MOD_ID = "bathtime";

    public static string STINKINESS = "stinkiness";

    public static string SOAPY = "soapy";

    public static string LAST_SOAP_UPDATE = "last_soap_update";

    public static string SOAP_DURATION = "soap_duration";

    public static string CONFIG_NAME = MOD_ID + ".json";

    public static string CLIENT_CONFIG_NAME = MOD_ID + "_client.json";

    public static string LOGGING_PREFIX = "[" + MOD_ID + "] ";

    public static string RELOAD_COMMAND = "reload";

    public static string SET_COMMAND = "set";

    public static string HUD_COMMAND = "showhud";

    public static string STINK_PARTICLE_THRESHOLD = "stinkParticleThreshold";

    public static string FLIES_PARTICLE_THRESHOLD = "fliesParticleThreshold";

    public static double DEFAULT_STINK_PARTICLE_THRESHOLD = 0.25;

    public static double DEFAULT_FLIES_PARTICLE_THRESHOLD = 0.90;

    public static int[] stinkBaseColori = [108, 212, 60];
    public static double[] stinkBaseColord = [108.0 / 255.0, 212.0 / 255.0, 60.0 / 255.0];
    public static int[] hsvaStinkBaseColor = ColorUtil.RgbToHsvInts(
        stinkBaseColori[0],
        stinkBaseColori[1],
        stinkBaseColori[2]
    );

    public static int[] hsvaSoapBubbleBaseColor = ColorUtil.RgbToHsvInts(
        178,
        228,
        219
    );
}