using Vintagestory.API.MathTools;

namespace BathTime;

internal static class Constants
{
    public static string MOD_ID = "bathtime";

    public static string STINKINESS_KEY = "stinkiness";

    public static string SOAPY_KEY = "soapy";

    public static string LAST_SOAP_UPDATE_KEY = "last_soap_update";

    public static string SOAP_DURATION_KEY = "soap_duration";

    public static string CONFIG_NAME = MOD_ID + ".json";

    public static string CLIENT_CONFIG_NAME = MOD_ID + "_client.json";

    public static string LOGGING_PREFIX = "[" + MOD_ID + "] ";

    public static string RELOAD_COMMAND = "reload";

    public static string SET_COMMAND = "set";

    public static string HUD_COMMAND = "showhud";

    public static string STINK_PARTICLE_THRESHOLD_KEY = "stink_particle_threshold";

    public static string FLIES_PARTICLE_THRESHOLD_KEY = "flies_particle_threshold";

    public static string LAST_STINKINESS_UPDATE_KEY = "last_stinkiness_update";

    public static double DEFAULT_STINK_PARTICLE_THRESHOLD = 0.4375;

    public static double DEFAULT_FLIES_PARTICLE_THRESHOLD = 0.90;

    public static double RATE_MULTIPLIER_ADDITIVE_PRIORITY = 0.0;

    public static double RATE_MULTIPLIER_MULTIPLICATIVE_PRIORITY = 0.5;

    public static double BATH_MULTIPLIER_DEFAULT_PRIOTIY = 1.0;

    public static double BATH_MULTIPLIER_ADDITIVE_PRIORITY = 1.5;

    public static double BATH_MULTIPLIER_MULTIPLICATIVE_PRIORITY = 2.0;

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