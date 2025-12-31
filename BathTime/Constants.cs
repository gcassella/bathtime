using Vintagestory.API.MathTools;

namespace BathTime;

internal static class Constants
{
    public static string MOD_ID = "bathtime";
    public static string STINKINESS = "stinkiness";

    public static int[] stinkBaseColori = [108, 212, 60];
    public static double[] stinkBaseColord = [108.0 / 255.0, 212.0 / 255.0, 60.0 / 255.0];
    public static int[] hsvaStinkBaseColor = ColorUtil.RgbToHsvInts(
        stinkBaseColori[0],
        stinkBaseColori[1],
        stinkBaseColori[2]
    );
}