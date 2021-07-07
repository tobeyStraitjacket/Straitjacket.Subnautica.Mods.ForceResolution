using BepInEx.Logging;
using SMLHelper.V2.Handlers;
using System.Diagnostics;
using QModManager.API;
using QModManager.API.ModLoading;

namespace Straitjacket.Subnautica.Mods.ForceResolution
{
    [QModCore]
    public static class Main
    {
        private static IQMod qMod;
        internal static IQMod QMod => qMod ??= QModServices.Main.GetMyMod();

        private static ManualLogSource logger;
        internal static ManualLogSource Logger => logger ??= BepInEx.Logging.Logger.CreateLogSource(QMod.DisplayName);

        internal static Config Config = OptionsPanelHandler.RegisterModOptions<Config>();

        [QModPatch]
        public static void Initialize()
        {
            Logger.LogInfo($"Initializing ForceResolution v{QMod.LoadedAssembly.GetName().Version}...");
            var stopwatch = Stopwatch.StartNew();

            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(ResolutionService));

            stopwatch.Stop();
            Logger.LogInfo($"Initialized in {stopwatch.ElapsedMilliseconds}ms.");
        }
    }
}
