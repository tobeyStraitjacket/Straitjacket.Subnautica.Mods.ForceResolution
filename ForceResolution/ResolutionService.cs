using SMLHelper.V2.Commands;
using System;
using System.Collections;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.ForceResolution
{
    internal class ResolutionService : MonoBehaviour
    {
        private static ResolutionService instance;
        internal static ResolutionService Instance => (instance == null) switch
        {
            true => instance = new GameObject("ResolutionService").AddComponent<ResolutionService>(),
            false => instance
        };

        private IEnumerator Start()
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSecondsRealtime(.25f);
                if (Application.isFocused)
                {
                    SetResolution(Main.Config.DesiredResolution, Main.Config.DesiredFullscreenMode);
                }
            }
        }

        public void SetResolution(Resolution resolution, Config.FullscreenMode fullScreenMode)
        {
            if (resolution.Equals(default(Resolution)))
            {
                return;
            }

            if (Screen.fullScreenMode != (FullScreenMode)fullScreenMode
                || Screen.currentResolution.width != resolution.width
                || Screen.currentResolution.height != resolution.height
                || Screen.currentResolution.refreshRate != resolution.refreshRate)
            {
                Screen.SetResolution(width: resolution.width,
                                     height: resolution.height,
                                     preferredRefreshRate: resolution.refreshRate,
                                     fullscreenMode: (FullScreenMode)fullScreenMode);
            }
        }

        [ConsoleCommand("forceres")]
        public static string SetResolutionCommand(string width, string height, string fullscreenMode = null, string refreshRate = null)
        {
            Resolution resolution = new Resolution
            {
                width = Convert.ToInt32(width),
                height = Convert.ToInt32(height),
                refreshRate = string.IsNullOrWhiteSpace(refreshRate) switch
                {
                    true when Main.Config.DesiredResolution.refreshRate == 0 => Screen.currentResolution.refreshRate,
                    true => Main.Config.DesiredResolution.refreshRate,
                    false => Convert.ToInt32(refreshRate)
                }
            };

            Main.Config.DesiredResolution = resolution;

            if (!string.IsNullOrWhiteSpace(fullscreenMode))
            {
                if (Enum.TryParse(fullscreenMode, true, out Config.FullscreenMode mode))
                {
                    Main.Config.DesiredFullscreenMode = mode;
                }
            }

            Main.Config.Save();
            Main.Config.ApplySavedSettings();

            return $"Set resolution: {Main.Config.DesiredResolution}, {Main.Config.DesiredFullscreenMode}";
        }

        [ConsoleCommand("forceres.service")]
        public static string SetServiceCommand(string serviceMode)
        {
            if (Enum.TryParse(serviceMode, true, out Config.ServiceMode mode))
            {
                Main.Config.ResolutionServiceMode = mode;
                Main.Config.Save();
                Main.Config.OnServiceModeChanged();
                return $"Set service mode: {Main.Config.ResolutionServiceMode}";
            }
            else
            {
                return $"Current service mode: { Main.Config.ResolutionServiceMode}{Environment.NewLine}" +
                       $"Valid options: {string.Join(", ", Enum.GetNames(typeof(Config.ServiceMode)))}";
            }
        }
    }
}
