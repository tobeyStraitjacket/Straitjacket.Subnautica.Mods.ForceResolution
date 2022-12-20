using BepInEx.Configuration;
using UnityEngine;

namespace Tobey.ForceResolution;
public static class Config
{
    internal static ConfigFile Cfg => ForceResolution.Instance.Config;

    public static class General
    {
        public static ConfigEntry<Resolution> DesiredResolution { get; } =
            Cfg.Bind(
                section: nameof(General),
                key: "Desired resolution",
                defaultValue: Screen.currentResolution,
                configDescription: new(
                    description: "The desired resolution to force.",
                    tags: new[] { new ConfigurationManagerAttributes { ReadOnly = true, IsAdvanced = true, HideDefaultButton = true, Order = -50 } }
                )

            );

        public static ConfigEntry<FullscreenMode> DesiredFullscreenMode { get; } =
            Cfg.Bind(
                section: nameof(General),
                key: "Desired fullscreen mode",
                defaultValue: FullscreenMode.WindowedFullscreen,
                description: "The desired fullscreen mode."
            );

        public static ConfigEntry<ServiceMode> ResolutionServiceMode { get; } =
            Cfg.Bind(
                section: nameof(General),
                key: "Resolution service mode",
                defaultValue: ServiceMode.Startup,
                description: "Runs a background service that checks for changes to the game resolution, " +
                             "and attempts to enforce your preferences."
            );
    }

    public enum FullscreenMode
    {
        ExclusiveFullscreen = 0,
        WindowedFullscreen = 1,
        Windowed = 3
    }

    public enum ServiceMode
    {
        Disabled, Enabled, Startup
    }
}
