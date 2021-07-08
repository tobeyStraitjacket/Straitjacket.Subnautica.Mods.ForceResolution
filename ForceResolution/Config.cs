using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Straitjacket.Subnautica.Mods.ForceResolution
{
    [Menu("Force Resolution", LoadOn = MenuAttribute.LoadEvents.MenuRegistered | MenuAttribute.LoadEvents.MenuOpened)]
    internal class Config : ConfigFile
    {
        public Config() : base()
        {
            OnFinishedLoading += OnLoaded;
        }

        public void OnLoaded(object sender, ConfigFileEventArgs e) => OnServiceModeChanged();

        public Resolution DesiredResolution { get; set; }

        [Choice(label: "Desired fullscreen mode",
                "Exclusive fullscreen", "Windowed fullscreen", "Windowed")]
        public FullscreenMode DesiredFullscreenMode { get; set; } = FullscreenMode.WindowedFullscreen;

        [Choice(label: "Resolution service mode",
                "Off", "Always on", "Main menu only",
                Tooltip = "Runs a background service that checks for changes to the game resolution, " +
                          "and automatically enforces your preferences.")]
        [OnChange(nameof(OnServiceModeChanged))]
        public ServiceMode ResolutionServiceMode { get; set; } = ServiceMode.MainMenuOnly;

        public void OnServiceModeChanged()
        {
            switch (ResolutionServiceMode)
            {
                case ServiceMode.AlwaysOn:
                    ResolutionService.Instance.gameObject.EnsureComponent<SceneCleanerPreserve>();
                    Object.DontDestroyOnLoad(ResolutionService.Instance.gameObject);
                    break;
                case ServiceMode.MainMenuOnly when Object.FindObjectOfType<Player>() is null:
                    foreach (var preserver in ResolutionService.Instance.GetComponents<SceneCleanerPreserve>().ToList())
                    {
                        Object.Destroy(preserver);
                    }
                    SceneManager.MoveGameObjectToScene(ResolutionService.Instance.gameObject, SceneManager.GetActiveScene());
                    break;
                case ServiceMode.MainMenuOnly:
                case ServiceMode.Off:
                    Object.Destroy(ResolutionService.Instance.gameObject);
                    break;
            }
        }

        [Button("Save current settings",
                Tooltip = "Saves the current resolution and fullscreen mode, to be automatically enforced " +
                          "by the resolution service or applied manually.")]
        public void SaveSettings()
        {
            string message = $"Saving resolution: {Screen.currentResolution}, {DesiredFullscreenMode}";
            Main.Logger.LogInfo(message);
            ErrorMessage.AddMessage(message);
            DesiredResolution = Screen.currentResolution;
            Save();
        }

        [Button("Apply saved settings",
                Tooltip = "Instantly sets your resolution and fullscreen mode to the saved settings. If you're running the service, you don't need to do this.")]
        public void ApplySavedSettings()
        {
            ResolutionService.Instance.SetResolution(DesiredResolution, DesiredFullscreenMode);
            string message = $"Set resolution: {Main.Config.DesiredResolution}, {Main.Config.DesiredFullscreenMode}";
            Main.Logger.LogInfo(message);
            ErrorMessage.AddMessage(message);
        }

        public enum FullscreenMode
        {
            ExclusiveFullscreen = 0,
            WindowedFullscreen = 1,
            Windowed = 3
        }

        public enum ServiceMode
        {
            Off, AlwaysOn, MainMenuOnly
        }
    }
}
