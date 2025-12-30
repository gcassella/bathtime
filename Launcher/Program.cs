namespace Launcher
{
    public class LauncherEntry
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Launcher: Running Vintagestory.exe");
            Vintagestory.ClientWindows.Main(args);
        }
    }
}